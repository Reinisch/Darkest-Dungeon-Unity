using UnityEngine;
using System.Collections.Generic;

public static class BattleSolver
{
    public static SkillResult SkillResult { get { return SkillExecutionResult; } }
    public static HeroActionInfo HeroActionInfo { get { return HeroSkillExecutionInfo; } }

    private static readonly SkillResult SkillExecutionResult = new SkillResult();
    private static readonly HeroActionInfo HeroSkillExecutionInfo = new HeroActionInfo();

    public static bool IsSkillUsable(FormationUnit performer, CombatSkill skill)
    {
        FormationParty friends;
        FormationParty enemies;
        if (performer.Team == Team.Heroes)
        {
            friends = RaidSceneManager.BattleGround.HeroParty;
            enemies = RaidSceneManager.BattleGround.MonsterParty;
        }
        else
        {
            friends = RaidSceneManager.BattleGround.MonsterParty;
            enemies = RaidSceneManager.BattleGround.HeroParty;
        }

        return skill.LaunchRanks.IsLaunchableFrom(performer.Rank, performer.Size) &&
            skill.HasAvailableTargets(performer, friends, enemies);
    }

    public static bool IsCampingSkillUsable(FormationUnit performer, CampingSkill skill)
    {
        int skillUsageCount = 0;
        for (int i = 0; i < performer.CombatInfo.SkillsUsedThisTurn.Count; i++)
            if (performer.CombatInfo.SkillsUsedThisTurn[i] == skill.Id)
                skillUsageCount++;

        if (skillUsageCount >= skill.Limit)
            return false;

        if (skill.TimeCost > RaidSceneManager.Raid.CampingTimeLeft)
            return false;

        for (int i = 0; i < skill.Effects.Count; i++)
        {
            switch(skill.Effects[i].Selection)
            {
                case CampTargetType.Self:
                    return true;
                case CampTargetType.Individual:
                case CampTargetType.PartyOther:
                    if (RaidSceneManager.HeroParty.Units.Count > 1)
                        return true;
                    break;
            }
        }

        return false;
    }

    public static bool IsRequirementFulfilled(FormationUnit target, CampEffectRequirement requirement)
    {
        switch(requirement)
        {
            case CampEffectRequirement.Afflicted:
                return target.Character.IsAfflicted;
            case CampEffectRequirement.DeathRecovery:
                return target.Character[StatusType.DeathRecovery].IsApplied;
            case CampEffectRequirement.Nonreligious:
                return target.Character.IsMonster == false && (target.Character as Hero).HeroClass.Tags.Contains("non-religious");
            case CampEffectRequirement.Religious:
                return target.Character.IsMonster == false && (target.Character as Hero).HeroClass.Tags.Contains("religious");
            default:
                return true;
        }
    }

    public static bool IsPerformerSkillTargetable(CombatSkill skill, BattleFormation allies, BattleFormation enemies, FormationUnit performer)
    {
        if (skill.TargetRanks.IsSelfTarget)
        {
            if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.Contains(performer.CombatInfo.CombatId))
                return false;
            if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.Contains(performer.CombatInfo.CombatId))
                return false;
            return true;
        }

        if (skill.TargetRanks.IsSelfFormation)
        {
            if (skill.IsSelfValid)
            {
                if (skill.Heal != null)
                {
                    if (performer.CombatInfo.BlockedHealUnitIds.Contains(performer.CombatInfo.CombatId) == false)
                        return true;
                }
                else if (skill.IsBuffSkill)
                {
                    if (performer.CombatInfo.BlockedBuffUnitIds.Contains(performer.CombatInfo.CombatId) == false)
                        return true;
                }
                else
                    return true;
            }

            for (int i = 0; i < allies.Party.Units.Count; i++)
            {
                if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.
                    Contains(allies.Party.Units[i].CombatInfo.CombatId))
                    continue;
                if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.
                    Contains(allies.Party.Units[i].CombatInfo.CombatId))
                    continue;

                if (allies.Party.Units[i] != performer && skill.TargetRanks.IsTargetableUnit(allies.Party.Units[i]))
                    return true;
            }
        }
        else
        {
            for (int i = 0; i < enemies.Party.Units.Count; i++)
                if (skill.TargetRanks.IsTargetableUnit(enemies.Party.Units[i]))
                    return true;
        }

        return false;
    }

    public static void FindTargets(FormationUnit performer, FormationUnit primaryTarget, CampEffect effect, List<FormationUnit> finalTargets)
    {
        finalTargets.Clear();

        switch (effect.Selection)
        {
            case CampTargetType.Individual:
                if (primaryTarget != null)
                    finalTargets.Add(primaryTarget);
                break;
            case CampTargetType.PartyOther:
                for (int j = 0; j < RaidSceneManager.HeroParty.Units.Count; j++)
                    if (RaidSceneManager.HeroParty.Units[j] != performer)
                        finalTargets.Add(RaidSceneManager.HeroParty.Units[j]);
                break;
            case CampTargetType.Self:
                finalTargets.Add(performer);
                break;
        }
    }

    public static List<FormationUnit> GetSkillAvailableTargets(FormationUnit performer, CombatSkill skill)
    {
        if (performer.Team == Team.Heroes)
            return skill.GetAvailableTargets(performer, RaidSceneManager.BattleGround.HeroParty,
                RaidSceneManager.BattleGround.MonsterParty);
        else
            return skill.GetAvailableTargets(performer, RaidSceneManager.BattleGround.MonsterParty,
                RaidSceneManager.BattleGround.HeroParty);
    }

    public static MonsterBrainDecision UseMonsterBrain(FormationUnit performer, string combatSkillOverride = null)
    {
        if (performer.Character.IsMonster)
        {
            var monster = performer.Character as Monster;

            if(string.IsNullOrEmpty(combatSkillOverride))
            {
                var skillDesires = new List<SkillSelectionDesire>(monster.Brain.SkillDesireSet);
                var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);

                while (skillDesires.Count != 0)
                {
                    SkillSelectionDesire desire = RandomSolver.ChooseByRandom(skillDesires);
                    if (desire != null && desire.SelectSkill(performer, monsterBrainDecision))
                    {
                        var cooldown = monster.Brain.SkillCooldowns.Find(cd => cd.SkillId == monsterBrainDecision.SelectedSkill.Id);
                        if (cooldown != null) performer.CombatInfo.SkillCooldowns.Add(cooldown.Copy());
                        RaidSceneManager.BattleGround.LastSkillUsed = monsterBrainDecision.SelectedSkill.Id;
                        return monsterBrainDecision;
                    }
                    else
                        skillDesires.Remove(desire);
                }
                return new MonsterBrainDecision(BrainDecisionType.Pass);
            }
            else
            {
                var availableSkill = monster.Data.CombatSkills.Find(skill => skill.Id == combatSkillOverride);

                if (availableSkill != null && IsSkillUsable(performer, availableSkill))
                {
                    var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);
                    monsterBrainDecision.Decision = BrainDecisionType.Perform;
                    monsterBrainDecision.SelectedSkill = availableSkill;
                    monsterBrainDecision.TargetInfo.Targets = GetSkillAvailableTargets(performer, monsterBrainDecision.SelectedSkill);
                    monsterBrainDecision.TargetInfo.Type = monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfTarget ?
                        SkillTargetType.Self : monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfFormation ?
                        SkillTargetType.Party : SkillTargetType.Enemy;

                    var availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet);

                    while (availableTargetDesires.Count > 0)
                    {
                        TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                        if (desire.SelectTarget(performer, monsterBrainDecision))
                            return monsterBrainDecision;
                        else
                            availableTargetDesires.Remove(desire);
                    }
                    return new MonsterBrainDecision(BrainDecisionType.Pass);
                }
                return new MonsterBrainDecision(BrainDecisionType.Pass);
            }
        }
        else
        {
            var hero = performer.Character as Hero;

            var availableSkills = hero.Mode == null ? new List<CombatSkill>(hero.CurrentCombatSkills).FindAll(skill =>
                skill != null && IsSkillUsable(performer, skill)) : new List<CombatSkill>(hero.CurrentCombatSkills).FindAll(skill =>
                skill.ValidModes.Contains(hero.CurrentMode.Id) && IsSkillUsable(performer, skill));

            if (availableSkills.Count != 0)
            {
                var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);
                monsterBrainDecision.Decision = BrainDecisionType.Perform;
                monsterBrainDecision.SelectedSkill = availableSkills[RandomSolver.Next(availableSkills.Count)];
                monsterBrainDecision.TargetInfo.Targets = GetSkillAvailableTargets(performer, monsterBrainDecision.SelectedSkill);
                monsterBrainDecision.TargetInfo.Type = monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfTarget ?
                    SkillTargetType.Self : monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfFormation ?
                    SkillTargetType.Party : SkillTargetType.Enemy;

                var availableTargets = new List<FormationUnit>(monsterBrainDecision.TargetInfo.Targets);
                if (availableTargets.Count > 0)
                {
                    monsterBrainDecision.TargetInfo.Targets.Clear();

                    if (monsterBrainDecision.SelectedSkill.TargetRanks.IsMultitarget)
                    {
                        monsterBrainDecision.TargetInfo.Targets.AddRange(availableTargets);
                        return monsterBrainDecision;
                    }
                    else
                    {
                        int index = RandomSolver.Next(availableTargets.Count);
                        monsterBrainDecision.TargetInfo.Targets.Add(availableTargets[index]);
                        availableTargets.RemoveAt(index);
                        return monsterBrainDecision;
                    }
                }
                return new MonsterBrainDecision(BrainDecisionType.Pass);
            }
            return new MonsterBrainDecision(BrainDecisionType.Pass);
        }
    }

    public static SkillTargetInfo SelectSkillTargets(FormationUnit performer, FormationUnit primaryTarget, CombatSkill skill)
    {
        if (skill.TargetRanks.IsSelfTarget)
            return new SkillTargetInfo(performer, SkillTargetType.Self);

        if (skill.TargetRanks.IsSelfFormation)
        {
            if(skill.TargetRanks.IsMultitarget)
            {
                var targets = performer.Team == Team.Heroes ?
                    new List<FormationUnit>(RaidSceneManager.BattleGround.HeroParty.Units) :
                    new List<FormationUnit>(RaidSceneManager.BattleGround.MonsterParty.Units);

                if (!skill.IsSelfValid)
                    targets.Remove(performer);

                for (int i = targets.Count - 1; i >= 0; i--)
                    if (!skill.TargetRanks.IsTargetableUnit(targets[i]))
                        targets.Remove(targets[i]);

                return new SkillTargetInfo(targets, SkillTargetType.Party);
            }
            else
                return new SkillTargetInfo(primaryTarget, SkillTargetType.Party);
        }
        else
        {
            if (skill.TargetRanks.IsMultitarget)
            {
                var targets = performer.Team == Team.Heroes ?
                    new List<FormationUnit>(RaidSceneManager.BattleGround.MonsterParty.Units) :
                    new List<FormationUnit>(RaidSceneManager.BattleGround.HeroParty.Units);

                for (int i = targets.Count - 1; i >= 0; i--)
                    if (!skill.TargetRanks.IsTargetableUnit(targets[i]))
                        targets.Remove(targets[i]);

                return new SkillTargetInfo(targets, SkillTargetType.Enemy);
            }
            else
                return new SkillTargetInfo(primaryTarget, SkillTargetType.Enemy);
        }
    }

    public static void ExecuteSkill(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill, SkillArtInfo artInfo)
    {
        SkillResult.Skill = skill;
        SkillResult.ArtInfo = artInfo;

        var target = targetUnit.Character;
        var performer = performerUnit.Character;

        ApplyConditions(performerUnit, targetUnit, skill);

        if (skill.Move != null && !performerUnit.CombatInfo.IsImmobilized)
        {
            if (skill.Move.Pullforward > 0)
                performerUnit.Pull(skill.Move.Pullforward, false);
            else if (skill.Move.Pushback > 0)
                performerUnit.Push(skill.Move.Pushback, false);
        }

        if (skill.Category == SkillCategory.Heal || skill.Category == SkillCategory.Support)
        {
            #region Heal

            if (skill.Heal != null)
            {
                float initialHeal = RandomSolver.Next(skill.Heal.MinAmount, skill.Heal.MaxAmount + 1) *
                    (1 + performer.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

                if (skill.IsCritValid)
                {
                    float critChance = performer[AttributeType.CritChance].ModifiedValue + skill.CritMod / 100;
                    if (RandomSolver.CheckSuccess(critChance))
                    {
                        int critHeal = target.Heal(initialHeal * 1.5f, true);
                        targetUnit.OverlaySlot.UpdateOverlay();
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critHeal, SkillResultType.CritHeal));
                        
                        ApplyEffects(performerUnit, targetUnit, skill);
                        if (targetUnit.Character.IsMonster == false)
                            DarkestDungeonManager.Data.Effects["crit_heal_stress_heal"].ApplyIndependent(targetUnit);
                        return;
                    }
                }

                int heal = target.Heal(initialHeal, true);
                targetUnit.OverlaySlot.UpdateOverlay();

                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, heal, SkillResultType.Heal));
                ApplyEffects(performerUnit, targetUnit, skill);
            }
            else
            {
                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Utility));
                ApplyEffects(performerUnit, targetUnit, skill);
            }

            #endregion
        }
        else
        {
            #region Damage

            float accuracy = skill.Accuracy + performer.Accuracy;
            float hitChance = Mathf.Clamp(accuracy - target.Dodge, 0, 0.95f);
            float roll = (float)RandomSolver.NextDouble();
            if (target.BattleModifiers != null && target.BattleModifiers.CanBeHit == false)
                roll = float.MaxValue;

            if (roll > hitChance)
            {
                if (!(skill.CanMiss == false || (target.BattleModifiers != null && target.BattleModifiers.CanBeMissed == false)))
                {
                    if (roll > Mathf.Min(accuracy, 0.95f))
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Miss));
                    else
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Dodge));

                    ApplyEffects(performerUnit, targetUnit, skill);
                    return;
                }
            }

            float initialDamage = performer is Hero ?
                Mathf.Lerp(performer.MinDamage, performer.MaxDamage, (float)RandomSolver.NextDouble()) * (1 + skill.DamageMod) :
                Mathf.Lerp(skill.DamageMin, skill.DamageMax, (float)RandomSolver.NextDouble()) * performer.DamageMod;

            int damage = Mathf.CeilToInt(initialDamage * (1 - target.Protection));
            if(damage < 0)
                damage = 0;

            if (target.BattleModifiers != null && target.BattleModifiers.CanBeDamagedDirectly == false)
                damage = 0;

            if (skill.IsCritValid)
            {
                float critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod;
                if (RandomSolver.CheckSuccess(critChance))
                {
                    int critDamage = target.TakeDamage(damage * 1.5f);
                    targetUnit.OverlaySlot.UpdateOverlay();

                    if (target.HasZeroHealth)
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critDamage, true, SkillResultType.Crit));
                    else
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critDamage, SkillResultType.Crit));

                    ApplyEffects(performerUnit, targetUnit, skill);
                    if (targetUnit.Character.IsMonster == false)
                        DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(targetUnit);
                    return;
                }
            }
            damage = target.TakeDamage(damage);
            targetUnit.OverlaySlot.UpdateOverlay();
            if (target.HasZeroHealth)
                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, damage, true, SkillResultType.Hit));
            else
                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, damage, SkillResultType.Hit));

            ApplyEffects(performerUnit, targetUnit, skill);

            #endregion
        }
    }

    public static void CalculateSkillPotential(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill)
    {
        var target = targetUnit.Character;
        var performer = performerUnit.Character;

        if (skill.Category == SkillCategory.Heal || skill.Category == SkillCategory.Support)
            HeroActionInfo.UpdateInfo(false, 0, 0, 0, 0);
        else
        {
            ApplyConditions(performerUnit, targetUnit, skill);
            float accuracy = skill.Accuracy + performer.Accuracy;
            float hitChance = Mathf.Clamp(accuracy - target.Dodge, 0, 0.95f);
            if (skill.CanMiss == false)
                hitChance = 1;
            else if (target.BattleModifiers != null && target.BattleModifiers.CanBeMissed == false)
                hitChance = 1;

            float initialMinDamage = performer is Hero ?
                performer.MinDamage * (1 + skill.DamageMod) :
                skill.DamageMin * performer.DamageMod;
            float initialMaxDamage = performer is Hero ?
                performer.MaxDamage * (1 + skill.DamageMod) :
                skill.DamageMax * performer.DamageMod;

            int minDamage = Mathf.CeilToInt(initialMinDamage * (1 - target.Protection));
            if (minDamage < 0)
                minDamage = 0;
            int maxDamage = Mathf.CeilToInt(initialMaxDamage * (1 - target.Protection));
            if (maxDamage < 0)
                maxDamage = 0;

            if (target.BattleModifiers != null && target.BattleModifiers.CanBeDamagedDirectly == false)
            {
                minDamage = 0;
                maxDamage = 0;
            }

            float critChance = 0;
            if (skill.IsCritValid)
                critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod;

            RemoveConditions(performerUnit);
            RemoveConditions(targetUnit);
            HeroActionInfo.UpdateInfo(true, hitChance, critChance, minDamage, maxDamage);
        }
    }

    public static void ApplyEffects(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill)
    {
        if(skill.ValidModes.Count > 1 && performerUnit.Character.Mode != null)
            foreach (var effect in skill.ModeEffects[performerUnit.Character.Mode.Id])
                effect.Apply(performerUnit, targetUnit, SkillResult);

        foreach (var effect in skill.Effects)
            effect.Apply(performerUnit, targetUnit, SkillResult);
    }

    public static void ApplyConditions(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill)
    {
        performerUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.
            GetCombatUnitRules(performerUnit, targetUnit, skill, performerUnit.Character.RiposteSkill == skill));
        targetUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.
            GetCombatUnitRules(targetUnit, performerUnit, skill, false));

        foreach (var effect in skill.Effects)
            effect.ApplyTargetConditions(performerUnit, targetUnit);
    }

    public static void RemoveConditions(FormationUnit targetUnit)
    {
        targetUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(targetUnit));
        targetUnit.Character.RemoveConditionalBuffs();
    }
}