using UnityEngine;
using System.Collections.Generic;
using System;

public class Effect
{
    public string Name { get; private set; }
    public EffectTargetType TargetType { get; private set; }

    public Dictionary<EffectBoolParams, bool?> BooleanParams { get; private set; }
    public Dictionary<EffectIntParams, int?> IntegerParams { get; private set; }
    public List<SubEffect> SubEffects { get; private set; }

    public Effect(List<string> data)
    {
        SubEffects = new List<SubEffect>();
        BooleanParams = new Dictionary<EffectBoolParams,bool?>();
        IntegerParams = new Dictionary<EffectIntParams, int?>();

        foreach(EffectBoolParams effectBool in Enum.GetValues(typeof(EffectBoolParams)))
            BooleanParams.Add(effectBool, null);
        foreach (EffectIntParams effectInteger in Enum.GetValues(typeof(EffectIntParams)))
            IntegerParams.Add(effectInteger, null);

        LoadData(data);
    }

    public void Apply(FormationUnit performer, FormationUnit target, SkillResult skillResult)
    {
        if (BooleanParams[EffectBoolParams.ApplyOnce].HasValue)
            if(BooleanParams[EffectBoolParams.ApplyOnce].Value)
                if (skillResult.AppliedEffects.Contains(this))
                    return;

        if (BooleanParams[EffectBoolParams.OnMiss] == false)
            if (skillResult.Current.Type == SkillResultType.Miss || skillResult.Current.Type == SkillResultType.Dodge)
                return;

        if (BooleanParams[EffectBoolParams.CanApplyAfterDeath] == false)
            if (skillResult.Current.IsZeroed)
                return;

        switch(TargetType)
        {
            case EffectTargetType.Performer:
                foreach (var subEffect in SubEffects)
                    subEffect.Apply(performer, performer, this);
                break;
            case EffectTargetType.Target:
                foreach (var subEffect in SubEffects)
                    subEffect.Apply(performer, target, this);
                break;
            case EffectTargetType.PerformersOther:
                foreach(var unit in performer.Party.Units)
                {
                    if(unit != performer)
                        foreach (var subEffect in SubEffects)
                            subEffect.Apply(performer, unit, this);
                }
                break;
            case EffectTargetType.TargetGroup:
                foreach(var unit in target.Party.Units)
                {
                    foreach (var subEffect in SubEffects)
                        subEffect.Apply(performer, unit, this);
                }
                break;
            case EffectTargetType.Global:
                if(IntegerParams[EffectIntParams.Torch].HasValue)
                {
                    if (IntegerParams[EffectIntParams.Torch] < 0)
                        RaidSceneManager.TorchMeter.DecreaseTorch(-IntegerParams[EffectIntParams.Torch].Value);
                    else
                        RaidSceneManager.TorchMeter.IncreaseTorch(IntegerParams[EffectIntParams.Torch].Value);
                }
                foreach (var subEffect in SubEffects)
                    subEffect.Apply(performer, target, this);
                break;
        }

        skillResult.AppliedEffects.Add(this);
    }

    public void ApplyTargetConditions(FormationUnit performer, FormationUnit target)
    {
        switch (TargetType)
        {
            case EffectTargetType.Performer:
                foreach (var subEffect in SubEffects)
                    subEffect.ApplyTargetConditions(performer, performer, target, this);
                break;
            case EffectTargetType.Target:
                foreach (var subEffect in SubEffects)
                    subEffect.ApplyTargetConditions(performer, target, target, this);
                break;
            case EffectTargetType.PerformersOther:
                foreach (var unit in performer.Party.Units)
                {
                    if (unit != performer)
                        foreach (var subEffect in SubEffects)
                            subEffect.ApplyTargetConditions(performer, unit, unit, this);
                }
                break;
            case EffectTargetType.TargetGroup:
                foreach (var unit in target.Party.Units)
                {
                    foreach (var subEffect in SubEffects)
                        subEffect.ApplyTargetConditions(performer, unit, unit, this);
                }
                break;
            case EffectTargetType.Global:
                foreach (var subEffect in SubEffects)
                    subEffect.ApplyTargetConditions(performer, target, performer, this);
                break;
        }
    }

    public void ApplyIndependent(FormationUnit performer, FormationUnit target)
    {
        SubEffects.ForEach(sub => sub.Apply(performer, target, this));
    }

    public void ApplyIndependent(FormationUnit target)
    {
        SubEffects.ForEach(sub => sub.Apply(null, target, this));
    }

    public string Tooltip()
    {
        string toolTip = "";
        switch(TargetType)
        {
            case EffectTargetType.Performer:
                foreach (var subEffect in SubEffects)
                {
                    string subTooltip = subEffect.Tooltip(this);
                    if (subTooltip.Length > 0)
                    {
                        if (toolTip.Length > 0)
                            toolTip += "\n" + subTooltip;
                        else
                            toolTip += subTooltip;
                    }
                }
                break;
            case EffectTargetType.Target:
                foreach(var subEffect in SubEffects)
                {
                    string subTooltip = subEffect.Tooltip(this);
                    if (subTooltip.Length > 0)
                    {
                        if(toolTip.Length > 0)
                            toolTip += "\n" + subTooltip;
                        else
                            toolTip += subTooltip;
                    }
                }
                break;
            case EffectTargetType.Global:
                if(IntegerParams[EffectIntParams.Torch].HasValue)
                    toolTip += string.Format(LocalizationManager.GetString(
                        "effect_tooltip_torch_format"), IntegerParams[EffectIntParams.Torch].Value);
                break;
            case EffectTargetType.PerformersOther:
                foreach (var subEffect in SubEffects)
                {
                    string subTooltip = subEffect.Tooltip(this);
                    if (subTooltip.Length > 0)
                    {
                        if (toolTip.Length > 0)
                            toolTip += "\n" + subTooltip;
                        else
                            toolTip += subTooltip;
                    }
                }
                break;
            case EffectTargetType.TargetGroup:
                foreach (var subEffect in SubEffects)
                {
                    string subTooltip = subEffect.Tooltip(this);
                    if (subTooltip.Length > 0)
                    {
                        if (toolTip.Length > 0)
                            toolTip += "\n" + subTooltip;
                        else
                            toolTip += subTooltip;
                    }
                }
                break;
        }
        return toolTip;
    }

    private void LoadData(List<string> data)
    {
        CombatStatBuffEffect statEffect = null;
        RiposteEffect riposteEffect = null;
        SummonMonstersEffect summonEffect = null;
        ClearGuardEffect clearGuardEffect = null;
        GuardEffect guardEffect = null;

        try
        {
            for (int i = 1; i < data.Count; i++)
            {
                float parseFloat;
                bool parseBool;
                int parseInt;

                switch (data[i])
                {
                    case ".name":
                        Name = data[++i];
                        break;
                    case ".target":
                        switch (data[++i])
                        {
                            case "target":
                                TargetType = EffectTargetType.Target;
                                break;
                            case "performer":
                                TargetType = EffectTargetType.Performer;
                                break;
                            case "global":
                                TargetType = EffectTargetType.Global;
                                break;
                            case "performer_group_other":
                                TargetType = EffectTargetType.PerformersOther;
                                break;
                            case "target_group":
                                TargetType = EffectTargetType.TargetGroup;
                                break;
                            default:
                                Debug.LogError("Unknown effect target type: " + data[i]);
                                break;
                        }
                        break;
                    case ".on_hit":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.OnHit] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in on_hit effect: {1}", data[i], Name);
                        break;
                    case ".on_miss":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.OnMiss] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in on_miss effect: {1}", data[i], Name);
                        break;
                    case ".can_apply_on_death":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.CanApplyAfterDeath] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in can_apply_on_death effect: {1}", data[i], Name);
                        break;
                    case ".apply_once":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.ApplyOnce] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in apply_once effect: {1}", data[i], Name);
                        break;
                    case ".apply_with_result":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.ApplyWithResult] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in apply_with_result effect: {1}", data[i], Name);
                        break;
                    case ".crit_doesnt_apply_to_roll":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.CritDoesntApplyToRoll] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in crit_apply effect: {1}", data[i], Name);
                        break;
                    case ".queue":
                        if (bool.TryParse(data[++i], out parseBool))
                            BooleanParams[EffectBoolParams.Queue] = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in queue effect: {1}", data[i], Name);
                        break;
                    case ".curio_result_type":
                        BooleanParams[EffectBoolParams.CurioResult] = data[++i] == "positive";
                        break;
                    case ".chance":
                        IntegerParams[EffectIntParams.Chance] = int.Parse(data[++i]);
                        break;
                    case ".curio":
                        IntegerParams[EffectIntParams.Curio] = int.Parse(data[++i]);
                        break;
                    case ".item":
                        IntegerParams[EffectIntParams.Item] = int.Parse(data[++i]);
                        break;
                    case ".duration":
                        IntegerParams[EffectIntParams.Duration] = int.Parse(data[++i]);
                        break;
                    case ".torch_increase":
                        IntegerParams[EffectIntParams.Torch] = int.Parse(data[++i]);
                        break;
                    case ".torch_decrease":
                        IntegerParams[EffectIntParams.Torch] = -int.Parse(data[++i]);
                        break;
                    case ".pull":
                        SubEffects.Add(new PullEffect(int.Parse(data[++i])));
                        break;
                    case ".push":
                        SubEffects.Add(new PushEffect(int.Parse(data[++i])));
                        break;
                    case ".kill":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .kill effect: {1}", data[i], Name);
                        SubEffects.Add(new KillEffect());
                        break;
                    case ".kill_enemy_types":
                        switch (data[++i])
                        {
                            case "unholy":
                                SubEffects.Add(new KillEnemyTypeEffect(MonsterType.Unholy));
                                break;
                            case "man":
                                SubEffects.Add(new KillEnemyTypeEffect(MonsterType.Man));
                                break;
                            case "beast":
                                SubEffects.Add(new KillEnemyTypeEffect(MonsterType.Beast));
                                break;
                            case "eldritch":
                                SubEffects.Add(new KillEnemyTypeEffect(MonsterType.Eldritch));
                                break;
                            case "corpse":
                                SubEffects.Add(new KillEnemyTypeEffect(MonsterType.Corpse));
                                break;
                            default:
                                Debug.LogError("Unknown monster type in kill_types: " + data[i] + " in effect " + Name);
                                break;
                        }
                        break;
                    case ".stress":
                        SubEffects.Add(new StressEffect(int.Parse(data[++i])));
                        break;
                    case ".dotPoison":
                        SubEffects.Add(new PoisonEffect(int.Parse(data[++i])));
                        break;
                    case ".dotBleed":
                        SubEffects.Add(new BleedEffect(int.Parse(data[++i])));
                        break;
                    case ".stun":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .unstun effect: {1}", data[i], Name);
                        SubEffects.Add(new StunEffect());
                        break;
                    case ".unstun":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .unstun effect: {1}", data[i], Name);
                        SubEffects.Add(new UnstunEffect());
                        break;
                    case ".tag":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .tag effect: {1}", data[i], Name);
                        SubEffects.Add(new TagEffect());
                        break;
                    case ".buff_duration_type":
                        var turboTag = SubEffects.Find(sub => sub is TagEffect) as TagEffect;
                        turboTag.DurationType = data[++i] == "combat_end" ? DurationType.Combat : DurationType.Round;
                        break;
                    case ".untag":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .untag effect: {1}", data[i], Name);
                        SubEffects.Add(new UntagEffect());
                        break;
                    case ".immobilize":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .immobilize effect: {1}", data[i], Name);
                        SubEffects.Add(new ImmobilizeEffect());
                        break;
                    case ".unimmobilize":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .unimmobilize effect: {1}", data[i], Name);
                        SubEffects.Add(new UnimmobilizeEffect());
                        break;
                    case ".heal":
                        SubEffects.Add(new HealEffect(int.Parse(data[++i])));
                        break;
                    case ".healstress":
                        SubEffects.Add(new StressHealEffect(int.Parse(data[++i])));
                        break;
                    case ".cure":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .cure effect: {1}", data[i], Name);
                        SubEffects.Add(new CureEffect());
                        break;
                    case ".guard":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .guard effect: {1}", data[i], Name);
                        guardEffect = new GuardEffect();
                        break;
                    case ".clearguarding":
                        if (clearGuardEffect == null)
                            clearGuardEffect = new ClearGuardEffect();
                        ++i;
                        clearGuardEffect.ClearGuarding = true;
                        break;
                    case ".clearguarded":
                        if (clearGuardEffect == null)
                            clearGuardEffect = new ClearGuardEffect();
                        ++i;
                        clearGuardEffect.ClearGuarded = true;
                        break;
                    case ".swap_source_and_target":
                        if (guardEffect != null)
                            guardEffect.SwapTargets = bool.Parse(data[++i]);
                        break;
                    case ".shuffletarget":
                        SubEffects.Add(new ShuffleTargetEffect(false));
                        break;
                    case ".performer_rank_target":
                        if (!int.TryParse(data[++i], out parseInt))
                            Debug.LogErrorFormat("Failed to parse {0} to int in .performer_rank_target effect: {1}", data[i], Name);
                        SubEffects.Add(new PerformerRankTargetEffect());
                        break;
                    case ".clear_rank_target":
                        ++i;
                        SubEffects.Add(new ClearRankTargetEffect());
                        break;
                    case ".shuffleparty":
                        SubEffects.Add(new ShuffleTargetEffect(true));
                        break;
                    case ".set_mode":
                        SubEffects.Add(new SetModeEffect(data[++i]));
                        break;
                    case ".control":
                        SubEffects.Add(new ControlEffect(int.Parse(data[++i])));
                        break;
                    case ".riposte":
                        ++i;
                        riposteEffect = new RiposteEffect();
                        break;
                    case ".riposte_chance_add":
                        if (!float.TryParse(data[++i], out parseFloat))
                            Debug.LogErrorFormat("Failed to parse {0} to float in .riposte_chance_add effect: {1}", data[i], Name);

                        break;
                    case ".disease":
                        if (data[++i] == "any")
                            SubEffects.Add(new DiseaseEffect(null, true));
                        else
                        {
                            if (DarkestDungeonManager.Data.Quirks.ContainsKey(data[i]))
                                SubEffects.Add(new DiseaseEffect(DarkestDungeonManager.Data.Quirks[data[i]], false));
                            else
                                Debug.LogErrorFormat("Missing disease {0} in effect: {1}", data[i], Name);
                        }
                        break;
                    case ".keyStatus":
                        if (statEffect == null)
                            statEffect = new CombatStatBuffEffect();

                        switch (data[++i])
                        {
                            case "tagged":
                                statEffect.TargetStatus = StatusType.Marked;
                                break;
                            case "poisoned":
                                statEffect.TargetStatus = StatusType.Poison;
                                break;
                            case "bleeding":
                                statEffect.TargetStatus = StatusType.Bleeding;
                                break;
                            case "stunned":
                                statEffect.TargetStatus = StatusType.Stun;
                                break;
                            default:
                                Debug.LogError("Unknown key status in effect: " + data[i]);
                                break;
                        }
                        break;
                    case ".monsterType":
                        if (statEffect == null)
                            statEffect = new CombatStatBuffEffect();

                        switch (data[++i])
                        {
                            case "unholy":
                                statEffect.TargetMonsterType = MonsterType.Unholy;
                                break;
                            case "man":
                                statEffect.TargetMonsterType = MonsterType.Man;
                                break;
                            case "beast":
                                statEffect.TargetMonsterType = MonsterType.Beast;
                                break;
                            case "eldritch":
                                statEffect.TargetMonsterType = MonsterType.Eldritch;
                                break;
                            default:
                                Debug.LogError("Unknown monster type in effect: " + data[i]);
                                break;
                        }
                        break;
                    case ".buff_type":
                        if (statEffect == null)
                            statEffect = new CombatStatBuffEffect();

                        switch (data[++i])
                        {
                            case "hp_heal_received_percent":
                                i += 2;
                                if (float.TryParse(data[i], out parseFloat))
                                    statEffect.StatAddBuffs.Add(AttributeType.HpHealReceivedPercent, parseFloat / 100);
                                else
                                    Debug.LogErrorFormat("Failed to parse {0} to float in hp_heal_received effect: {1}", data[i], Name);

                                break;
                            default:
                                Debug.LogError("Unknown buff type in effect: " + data[i]);
                                break;
                        }
                        break;
                    case ".summon_count":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        summonEffect.SummonCount = int.Parse(data[++i]);
                        break;
                    case ".summon_can_spawn_loot":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        if (bool.TryParse(data[++i], out parseBool))
                            summonEffect.CanSpawnLoot = parseBool;
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to bool in summon_loot effect: {1}", data[i], Name);
                        break;
                    case ".summon_erase_data_on_roll":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        if (!bool.TryParse(data[++i], out parseBool))
                            Debug.LogErrorFormat("Failed to parse {0} to bool in summon_erase_data effect: {1}", data[i], Name);
                        break;
                    case ".summon_monsters":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        while (++i < data.Count && data[i--][0] != '.')
                            summonEffect.SummonMonsters.Add(data[++i]);
                        break;
                    case ".summon_chances":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        while (++i < data.Count && data[i--][0] != '.')
                        {
                            if (float.TryParse(data[++i], out parseFloat))
                                summonEffect.SummonChances.Add(parseFloat);
                            else
                                Debug.LogErrorFormat("Failed to parse {0} to float in .summon_chances effect: {1}", data[i], Name);
                        }

                        break;
                    case ".summon_limits":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        while (++i < data.Count && data[i--][0] != '.')
                            summonEffect.SummonLimits.Add(int.Parse(data[++i]));
                        break;
                    case ".summon_ranks":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        while (++i < data.Count && data[i--][0] != '.')
                            summonEffect.SummonRanks.Add(new FormationSet(data[++i]));
                        break;
                    case ".summon_does_roll_initiatives":
                        if (summonEffect == null)
                            summonEffect = new SummonMonstersEffect();

                        while (++i < data.Count && data[i--][0] != '.')
                            summonEffect.SummonRollInitiatives.Add(int.Parse(data[++i]));
                        break;
                    case ".capture":
                        ++i;
                        SubEffects.Add(new CaptureEffect());
                        break;
                    case ".virtue_blockable_chance":
                        ++i;
                        var captureEffectBlock = SubEffects.Find(effect => effect is CaptureEffect) as CaptureEffect;
                        if (captureEffectBlock != null)
                            captureEffectBlock.VirtueBlockable = true;
                        else
                            Debug.LogError("Missing capture effect in effect" + Name);
                        break;
                    case ".capture_remove_from_party":
                        ++i;
                        var captureEffectRemove = SubEffects.Find(effect => effect is CaptureEffect) as CaptureEffect;
                        if (captureEffectRemove != null)
                            captureEffectRemove.RemoveFromParty = true;
                        else
                            Debug.LogError("Missing capture effect in effect" + Name);
                        break;
                    case ".combat_stat_buff":
                        ++i;
                        if (statEffect == null)
                            statEffect = new CombatStatBuffEffect();
                        break;
                    case ".protection_rating_add":
                        if (riposteEffect != null)
                        {
                            if (float.TryParse(data[++i], out parseFloat))
                                riposteEffect.StatAddBuffs.Add(AttributeType.ProtectionRating, parseFloat / 100);
                            else
                                Debug.LogErrorFormat("Failed to parse {0} to float in .protection_rating_add: {1}", data[i], Name);

                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }

                        if (float.TryParse(data[++i], out parseFloat))
                            statEffect.StatAddBuffs.Add(AttributeType.ProtectionRating, parseFloat / 100);
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to float in .protection_rating_add: {1}", data[i], Name);

                        break;
                    case ".speed_rating_add":
                    case ".speed_rating":
                        if (riposteEffect != null)
                        {
                            if (float.TryParse(data[++i], out parseFloat))
                                riposteEffect.StatAddBuffs.Add(AttributeType.SpeedRating, parseFloat);
                            else
                                Debug.LogErrorFormat("Failed to parse {0} to float in .speed_rating : {1}", data[i], Name);

                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        if (float.TryParse(data[++i], out parseFloat))
                            statEffect.StatAddBuffs.Add(AttributeType.SpeedRating, parseFloat);
                        else
                            Debug.LogErrorFormat("Failed to parse {0} to float in .speed_rating : {1}", data[i], Name);

                        break;
                    case ".crit_rating_add":
                    case ".critical_rating":
                        if (riposteEffect != null)
                        {
                            riposteEffect.StatAddBuffs.Add(AttributeType.CritChance, float.Parse(data[++i]) / 100);
                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        statEffect.StatAddBuffs.Add(AttributeType.CritChance, float.Parse(data[++i]) / 100);
                        break;
                    case ".crit_chance_add":
                        if (riposteEffect != null)
                        {
                            riposteEffect.StatAddBuffs.Add(AttributeType.CritChance, float.Parse(data[++i]) / 100);
                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        statEffect.StatAddBuffs.Add(AttributeType.CritChance, float.Parse(data[++i]) / 100);
                        break;
                    case ".defense_rating_add":
                        if (riposteEffect != null)
                        {
                            riposteEffect.StatAddBuffs.Add(AttributeType.DefenseRating, float.Parse(data[++i]) / 100);
                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        statEffect.StatAddBuffs.Add(AttributeType.DefenseRating, float.Parse(data[++i]) / 100);
                        break;
                    case ".attack_rating_add":
                        if (riposteEffect != null)
                        {
                            riposteEffect.StatAddBuffs.Add(AttributeType.AttackRating, float.Parse(data[++i]) / 100);
                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        statEffect.StatAddBuffs.Add(AttributeType.AttackRating, float.Parse(data[++i]) / 100);
                        break;
                    case ".damage_low_multiply":
                        if (riposteEffect != null)
                        {
                            riposteEffect.StatMultBuffs.Add(AttributeType.DamageLow, float.Parse(data[++i]) / 100);
                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        statEffect.StatMultBuffs.Add(AttributeType.DamageLow, float.Parse(data[++i]) / 100);
                        break;
                    case ".damage_high_multiply":
                        if (riposteEffect != null)
                        {
                            riposteEffect.StatMultBuffs.Add(AttributeType.DamageHigh, float.Parse(data[++i]) / 100);
                            break;
                        }

                        if (statEffect == null)
                        {
                            statEffect = new CombatStatBuffEffect();
                            Debug.LogError("Early stat in " + Name);
                        }
                        statEffect.StatMultBuffs.Add(AttributeType.DamageHigh, float.Parse(data[++i]) / 100);
                        break;
                    case ".buff_ids":
                        BuffEffect buffEffect = new BuffEffect();
                        while (++i < data.Count && data[i--][0] != '.')
                        {
                            if (!DarkestDungeonManager.Data.Buffs.ContainsKey(data[++i]))
                            {
                                Debug.LogError("Missing buff in effect: " + data[i]);
                            }
                            else
                                buffEffect.Buffs.Add(DarkestDungeonManager.Data.Buffs[data[i]]);
                        }
                        SubEffects.Add(buffEffect);
                        break;
                    case "combat_skill:":
                        break;
                    default:
                        Debug.LogError("Unexpected token in effect: " + data[i]);
                        break;
                }
            }
            if (statEffect != null)
                SubEffects.Add(statEffect);
            if (riposteEffect != null)
                SubEffects.Add(riposteEffect);
            if (summonEffect != null)
                SubEffects.Add(summonEffect);
            if (guardEffect != null)
                SubEffects.Add(guardEffect);
            if (clearGuardEffect != null)
                SubEffects.Add(clearGuardEffect);
        }
        catch
        {
            Debug.LogError("Error in effect: " + Name);
        }
    }
}