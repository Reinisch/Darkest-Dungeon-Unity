using UnityEngine;
using System.Collections.Generic;

public enum SkillResultType { Hit, Miss, Crit, Dodge, Heal, CritHeal, Utility }
public enum SkillTargetType { Self, Party, Enemy }
public enum BrainDecisionType { Pass, Perform }

public class HeroActionInfo
{
    public bool IsValid { get; private set; }
    public float ChanceToHit { get; private set; }
    public float ChanceToCrit { get; private set; }
    public int MinDamage { get; private set; }
    public int MaxDamage { get; private set; }

    public HeroActionInfo()
    {

    }

    public void UpdateInfo(bool valid, float hit, float crit, int minDamage, int maxDamage)
    {
        IsValid = valid;
        ChanceToCrit = crit;
        ChanceToHit = hit;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
    }
}
public class SkillTargetInfo
{
    public List<FormationUnit> Targets { get; set; }
    public SkillTargetType Type { get; set; }

    public CharacterMode Mode { get; private set; }
    public CombatSkill Skill { get; private set; }
    public SkillArtInfo SkillArtInfo { get; private set; }

    public SkillTargetInfo UpdateSkillInfo(FormationUnit performer, CombatSkill skill)
    {
        Mode = performer.Character.Mode;
        Skill = skill;
        SkillArtInfo = performer.Character.SkillArtInfo.Find(info => info.SkillId == skill.Id);

        if (skill.LimitPerBattle.HasValue)
            performer.CombatInfo.SkillsUsedInBattle.Add(skill.Id);
        if (skill.LimitPerTurn.HasValue)
            performer.CombatInfo.SkillsUsedThisTurn.Add(skill.Id);

        return this;
    }

    public SkillTargetInfo(List<FormationUnit> targets, SkillTargetType type)
    {
        Targets = targets;
        Type = type;
    }
    public SkillTargetInfo(FormationUnit target, SkillTargetType type)
    {
        Targets = new List<FormationUnit>();
        Targets.Add(target);
        Type = type;
    }
    public SkillTargetInfo(SkillTargetType type)
    {
        Targets = new List<FormationUnit>();
        Type = type;
    }
}
public class MonsterBrainDecision
{
    public BrainDecisionType Decision { get; set; }
    public CombatSkill SelectedSkill { get; set; }
    public SkillTargetInfo TargetInfo { get; set; }

    public MonsterBrainDecision(BrainDecisionType decision)
    {
        Decision = decision;
        TargetInfo = new SkillTargetInfo(SkillTargetType.Self);
    }
    public MonsterBrainDecision(BrainDecisionType decision, CombatSkill skill, SkillTargetInfo targetInfo)
    {
        Decision = decision;
        SelectedSkill = skill;
        TargetInfo = targetInfo;
    }
}
public class SkillResult
{
    public CombatSkill Skill { get; set; }
    public SkillArtInfo ArtInfo { get; set; }
    public SkillResultEntry Current { get; set; }
    public bool HasCritEffect
    {
        get
        {
            for (int i = 0; i < SkillEntries.Count; i++)
                if (SkillEntries[i].Type == SkillResultType.Crit && SkillEntries[i].CanCritReleaf)
                    return true;
            return false;
        }
    }
    public bool HasDeadEffect
    {
        get
        {
            for (int i = 0; i < SkillEntries.Count; i++)
                if (SkillEntries[i].IsZeroed && SkillEntries[i].CanKillReleaf)
                    return true;
            return false;
        }
    }
    public bool HasHit { get; set; }
    public bool HasZeroHealth { get; set; }
    public List<Effect> AppliedEffects { get; private set; }
    public List<SkillResultEntry> SkillEntries { get; private set; }

    public SkillResult()
    {
        AppliedEffects = new List<Effect>();
        SkillEntries = new List<SkillResultEntry>();
    }
    public void Reset()
    {
        Current = null;
        HasHit = false;
        HasZeroHealth = false;
        AppliedEffects.Clear();
        SkillEntries.Clear();
    }
    public SkillResult Copy()
    {
        SkillResult copy = new SkillResult();
        copy.Skill = Skill;
        copy.ArtInfo = ArtInfo;
        copy.Current = Current;
        copy.HasHit = HasHit;
        copy.HasZeroHealth = HasZeroHealth;
        copy.AppliedEffects = new List<Effect>(AppliedEffects);
        copy.SkillEntries = new List<SkillResultEntry>(SkillEntries);
        return copy;
    }

    public void AddResultEntry(SkillResultEntry entry)
    {
        Current = entry;
        SkillEntries.Add(entry);
        if (entry.Type != SkillResultType.Dodge && entry.Type != SkillResultType.Miss)
            HasHit = true;
        if (entry.IsZeroed)
            HasZeroHealth = true;
    }
    public void AddEffectEntry(Effect entry)
    {
        AppliedEffects.Add(entry);
    }
}
public class SkillResultEntry
{
    public int Amount { get; set; }
    public bool IsZeroed { get; set; }
    public bool IsTargetHit { get; set; }
    public bool IsHarmful { get; set; }
    public bool CanCritReleaf { get; set; }
    public bool CanKillReleaf { get; set; }
    public SkillResultType Type { get; set; }
    public FormationUnit Target { get; set; }

    public SkillResultEntry(FormationUnit target, SkillResultType result)
    {
        Type = result;
        Target = target;
        IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
        IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        CanCritReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromCrit;
        CanKillReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromKills;
    }
    public SkillResultEntry(FormationUnit target, int skillDamage, SkillResultType result)
    {
        Amount = skillDamage;
        Type = result;
        Target = target;
        IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
        IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        CanCritReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromCrit;
        CanKillReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromKills;
    }
    public SkillResultEntry(FormationUnit target, int skillDamage, bool isTargetZeroed, SkillResultType result)
    {
        Amount = skillDamage;
        Type = result;
        Target = target;
        IsZeroed = isTargetZeroed;
        IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
        IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        CanCritReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromCrit;
        CanKillReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromKills;
    }
}

public static class BattleSolver
{
    public static SkillResult SkillResult
    { 
        get
        {
            return skillResult;
        }
    }
    public static HeroActionInfo HeroActionInfo
    {
        get
        {
            return heroActionInfo;
        }
    }

    private static SkillResult skillResult = new SkillResult();
    private static HeroActionInfo heroActionInfo = new HeroActionInfo();

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
                default:
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
    public static bool IsPerformerSkillTargetable(CombatSkill skill,
        BattleFormation allies, BattleFormation enemies, FormationUnit performer)
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

            for (int i = 0; i < allies.party.Units.Count; i++)
            {
                if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.
                    Contains(allies.party.Units[i].CombatInfo.CombatId))
                    continue;
                if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.
                    Contains(allies.party.Units[i].CombatInfo.CombatId))
                    continue;

                if (allies.party.Units[i] != performer && skill.TargetRanks.IsTargetableUnit(allies.party.Units[i]))
                    return true;
            }
        }
        else
        {
            for (int i = 0; i < enemies.party.Units.Count; i++)
                if (skill.TargetRanks.IsTargetableUnit(enemies.party.Units[i]))
                    return true;
        }

        return false;
    }
    public static void GetTargetsForCampEffect(FormationUnit performer,
        FormationUnit target, CampEffect effect, List<FormationUnit> finalTargets)
    {
        finalTargets.Clear();

        switch (effect.Selection)
        {
            case CampTargetType.Individual:
                if (target != null)
                    finalTargets.Add(target);
                break;
            case CampTargetType.PartyOther:
                for (int j = 0; j < RaidSceneManager.HeroParty.Units.Count; j++)
                    if (RaidSceneManager.HeroParty.Units[j] != performer)
                        finalTargets.Add(RaidSceneManager.HeroParty.Units[j]);
                break;
            case CampTargetType.Self:
                finalTargets.Add(performer);
                break;
            default:
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

            if(combatSkillOverride == null)
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

                if (availableSkill != null && BattleSolver.IsSkillUsable(performer, availableSkill))
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
                skill.ValidModes.Contains(hero.CurrentMode.Id) && skill != null && IsSkillUsable(performer, skill));

            if (availableSkills.Count != 0)
            {
                var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);
                monsterBrainDecision.Decision = BrainDecisionType.Perform;
                monsterBrainDecision.SelectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
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
                        int index = Random.Range(0, availableTargets.Count);
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
                float initialHeal = Random.Range(skill.Heal.MinAmount, skill.Heal.MaxAmount + 1) *
                            (1 + performer.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

                int heal = Mathf.CeilToInt(initialHeal * (1 + target[AttributeType.HpHealReceivedPercent].ModifiedValue));
                if (heal < 1) heal = 1;
                if (target.AtDeathsDoor)
                    (target as Hero).RevertDeathsDoor();

                if (skill.IsCritValid)
                {
                    float critChance = performer[AttributeType.CritChance].ModifiedValue + skill.CritMod / 100;
                    if (RandomSolver.CheckSuccess(critChance))
                    {
                        int critHeal = Mathf.CeilToInt(heal * 1.5f);
                        target.Health.IncreaseValue(critHeal);
                        targetUnit.OverlaySlot.healthBar.UpdateHealth(target);
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critHeal, SkillResultType.CritHeal));
                        
                        ApplyEffects(performerUnit, targetUnit, skill);
                        if (targetUnit.Character.IsMonster == false)
                            DarkestDungeonManager.Data.Effects["crit_heal_stress_heal"].ApplyIndependent(targetUnit);
                        RemoveConditions(performerUnit, targetUnit);
                        return;
                    }
                }
                target.Health.IncreaseValue(heal);
                targetUnit.OverlaySlot.healthBar.UpdateHealth(target);

                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, heal, SkillResultType.Heal));
                ApplyEffects(performerUnit, targetUnit, skill);
                RemoveConditions(performerUnit, targetUnit);
            }
            else
            {
                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Utility));
                ApplyEffects(performerUnit, targetUnit, skill);
                RemoveConditions(performerUnit, targetUnit);
            }
            #endregion
        }
        else
        {
            #region Damage
            float accuracy = skill.Accuracy + performer.Accuracy;
            float hitChance = Mathf.Clamp(accuracy - target.Dodge, 0, 0.95f);
            float roll = Random.value;
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
                    RemoveConditions(performerUnit, targetUnit);
                    return;
                }
            }

            float initialDamage = performer is Hero ?
                Mathf.Lerp(performer.MinDamage, performer.MaxDamage, Random.value) * (1 + skill.DamageMod) :
                Mathf.Lerp(skill.DamageMin, skill.DamageMax, Random.value) * performer.DamageMod;

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
                    int critDamage = Mathf.CeilToInt(damage * 1.5f);
                    target.Health.DecreaseValue(critDamage);
                    targetUnit.OverlaySlot.healthBar.UpdateHealth(target);

                    if (Mathf.CeilToInt(target.Health.CurrentValue) == 0)
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critDamage, true, SkillResultType.Crit));
                    else
                        SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critDamage, SkillResultType.Crit));

                    ApplyEffects(performerUnit, targetUnit, skill);
                    if (targetUnit.Character.IsMonster == false)
                        DarkestDungeonManager.Data.Effects["BarkStress"].ApplyIndependent(targetUnit);
                    RemoveConditions(performerUnit, targetUnit);
                    return;
                }
            }
            target.Health.DecreaseValue(damage);
            targetUnit.OverlaySlot.healthBar.UpdateHealth(target);
            if (Mathf.CeilToInt(target.Health.CurrentValue) == 0)
                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, damage, true, SkillResultType.Hit));
            else
                SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, damage, SkillResultType.Hit));

            ApplyEffects(performerUnit, targetUnit, skill);
            RemoveConditions(performerUnit, targetUnit);
            return;
            #endregion
        }
    }
    public static HeroActionInfo CalculateSkillPotential(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill)
    {
        var target = targetUnit.Character;
        var performer = performerUnit.Character;

        if (skill.Category == SkillCategory.Heal || skill.Category == SkillCategory.Support)
        {
            HeroActionInfo.UpdateInfo(false, 0, 0, 0, 0);
            return HeroActionInfo;
        }
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

            RemoveConditions(performerUnit, targetUnit);
            HeroActionInfo.UpdateInfo(true, hitChance, critChance, minDamage, maxDamage);
            return HeroActionInfo;
        }
    }

    public static void ApplyEffects(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill)
    {
        if(skill.ValidModes.Count > 1 && performerUnit.Character.Mode != null)
        {
            foreach (var effect in skill.ModeEffects[performerUnit.Character.Mode.Id])
                effect.Apply(performerUnit, targetUnit, SkillResult);
        }
        foreach (var effect in skill.Effects)
            effect.Apply(performerUnit, targetUnit, SkillResult);
    }
    public static void ApplyConditions(FormationUnit performerUnit, FormationUnit targetUnit, CombatSkill skill)
    {
        performerUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.
            GetCombatUnitRules(performerUnit, targetUnit, skill, performerUnit.Character.RiposteSkill == skill));
        targetUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.
            GetCombatUnitRules(targetUnit, performerUnit, null, false));

        foreach (var effect in skill.Effects)
            effect.ApplyTargetConditions(performerUnit, targetUnit);
    }
    public static void RemoveConditions(FormationUnit performerUnit, FormationUnit targetUnit)
    {
        performerUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(performerUnit));
        targetUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(targetUnit));

        performerUnit.Character.RemoveConditionalBuffs();
        targetUnit.Character.RemoveConditionalBuffs();
    }
}