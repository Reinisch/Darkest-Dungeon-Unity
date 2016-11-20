using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public enum EffectBoolParams : byte 
{
    OnHit, OnMiss, ApplyOnce,
    CanApplyAfterDeath,
    CritDoesntApplyToRoll,
    ApplyWithResult, Queue,
    CurioResult, SpawnsLoot,
}
public enum EffectIntParams : byte 
{
    Chance, Duration, Torch, Summons,
    VirtueBlockChance, Curio, Item,
}
public enum EffectTargetType : byte 
{ 
    Target, Performer, Global,
    PerformersOther, TargetGroup,
}
public enum EffectSubType : byte 
{
    None, Kill, KillType, Control, Buff,
    Immobilize, Unimmobilize, Pull, Push,
    Stress, StressHeal, StatBuff, Debuff,
    Stun, Unstun, Poison, Bleeding, Heal,
    Cure, LifeDrain, Tag, Untag, Shuffle,
    Summon, Disease, Mode, Capture, Rank,
    ClearTargetRanks, GuardAlly, Riposte,
    ClearGuard
}

public class Effect
{
    public string Name
    {
        get;
        private set;
    }
    public EffectTargetType TargetType
    { 
        get;
        private set;
    }

    public Dictionary<EffectBoolParams, bool?> BooleanParams
    {
        get;
        private set;
    }
    public Dictionary<EffectIntParams, int?> IntegerParams
    {
        get;
        private set;
    }
    
    public List<SubEffect> SubEffects { get; set; }

    void LoadData(List<string> data)
    {
        CombatStatBuffEffect statEffect = null;
        RiposteEffect riposteEffect = null;
        SummonMonstersEffect summonEffect = null;
        ClearGuardEffect clearGuardEffect = null;
        GuardEffect guardEffect = null;

        bool parseBool = false;
        float parseFloat = 0;

        try
        {
            for (int i = 1; i < data.Count; i++)
            {
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
                        BooleanParams[EffectBoolParams.CurioResult] = data[++i] == "positive" ? true : false;
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
                        SubEffects.Add(new KillEffect(int.Parse(data[++i])));
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
                        SubEffects.Add(new StunEffect(int.Parse(data[++i])));
                        break;
                    case ".unstun":
                        SubEffects.Add(new UnstunEffect(int.Parse(data[++i])));
                        break;
                    case ".tag":
                        SubEffects.Add(new TagEffect(int.Parse(data[++i])));
                        break;
                    case ".buff_duration_type":
                        var turboTag = SubEffects.Find(sub => sub is TagEffect) as TagEffect;
                        turboTag.DurationType = data[++i] == "combat_end" ? DurationType.Combat : DurationType.Round;
                        break;
                    case ".untag":
                        SubEffects.Add(new UntagEffect(int.Parse(data[++i])));
                        break;
                    case ".immobilize":
                        SubEffects.Add(new ImmobilizeEffect(int.Parse(data[++i])));
                        break;
                    case ".unimmobilize":
                        SubEffects.Add(new UnimmobilizeEffect(int.Parse(data[++i])));
                        break;
                    case ".heal":
                        SubEffects.Add(new HealEffect(int.Parse(data[++i])));
                        break;
                    case ".healstress":
                        SubEffects.Add(new StressHealEffect(int.Parse(data[++i])));
                        break;
                    case ".cure":
                        SubEffects.Add(new CureEffect(int.Parse(data[++i])));
                        break;
                    case ".guard":
                        guardEffect = new GuardEffect(int.Parse(data[++i]));
                        break;
                    case ".clearguarding":
                        if (clearGuardEffect == null)
                            clearGuardEffect = new ClearGuardEffect(1);
                        ++i;
                        clearGuardEffect.ClearGuarding = true;
                        break;
                    case ".clearguarded":
                        if (clearGuardEffect == null)
                            clearGuardEffect = new ClearGuardEffect(1);
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
                        SubEffects.Add(new PerformerRankTargetEffect(int.Parse(data[++i])));
                        break;
                    case ".clear_rank_target":
                        SubEffects.Add(new ClearRankTargetEffect(data[++i]));
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
                        if (float.TryParse(data[++i], out parseFloat))
                            riposteEffect.RiposteChance = parseFloat;
                        else
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
                            statEffect = new CombatStatBuffEffect(1);

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
                            statEffect = new CombatStatBuffEffect(1);

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
                            statEffect = new CombatStatBuffEffect(1);

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

                        if (bool.TryParse(data[++i], out parseBool))
                            summonEffect.EraseDataOnRoll = parseBool;
                        else
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
                        if (statEffect == null)
                            statEffect = new CombatStatBuffEffect(int.Parse(data[++i]));
                        else
                            statEffect.CombatStatParam = int.Parse(data[++i]);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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
                            statEffect = new CombatStatBuffEffect(1);
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

        if (BooleanParams[EffectBoolParams.OnMiss].HasValue)
            if (BooleanParams[EffectBoolParams.OnMiss].Value == false)
                if (skillResult.Current.Type == SkillResultType.Miss || skillResult.Current.Type == SkillResultType.Dodge)
                    return;

        if (BooleanParams[EffectBoolParams.CanApplyAfterDeath].HasValue)
            if (BooleanParams[EffectBoolParams.CanApplyAfterDeath].Value == false)
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
        for (int i = 0; i < SubEffects.Count; i++)
            SubEffects[i].Apply(performer, target, this);
    }
    public void ApplyIndependent(FormationUnit target)
    {
        for (int i = 0; i < SubEffects.Count; i++)
            SubEffects[i].Apply(null, target, this);
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
}

public class SubEffect
{
    public virtual EffectSubType Type
    {
        get
        {
            return EffectSubType.None;
        }
    }
    public virtual bool Fusable
    {
        get
        {
            return false;
        }
    }
    public virtual string Tooltip(Effect effect)
    {
        return "";
    }

    public virtual void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return;
    }
    public virtual void ApplyTargetConditions(FormationUnit performer, FormationUnit target, FormationUnit primaryTarget, Effect effect)
    {
        return;
    }
    public virtual bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return false;
    }
    public virtual bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return false;
    }
    public virtual bool ApplyFused(FormationUnit performer, FormationUnit target, Effect effect, int fuseParameter)
    {
        return false;
    }
    public virtual int Fuse(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return 0;
    }
}

public class SetModeEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Mode;
        }
    }

    public string Mode { get; set; }

    public SetModeEffect(string mode)
    {
        Mode = mode;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character.IsMonster)
            return false;

        var heroTarget = target.Character as Hero;

        target.SetCombatAnimation(false);

        heroTarget.CurrentMode = heroTarget.HeroClass.Modes.Find(mode => mode.Id == Mode);

        target.SetCombatAnimation(true);

        if (RaidSceneManager.RaidInterface.RaidPanel.SelectedUnit == target)
            RaidSceneManager.RaidInterface.RaidPanel.bannerPanel.skillPanel.UpdateSkillPanel();

        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
            return true;
        else
            return false;
    }

    public override string Tooltip(Effect effect)
    {
        var modName = LocalizationManager.GetString("actor_mode_name_" + Mode);
        return string.Format(LocalizationManager.GetString("effect_tooltip_set_actor_mode_format"), modName);
    }
}

public class ControlEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Control;
        }
    }

    public int Duration { get; set; }

    public ControlEffect(int duration)
    {
        Duration = duration;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (!(performer.Character.IsMonster))
            return false;

        if ((performer.Character as Monster).Data.ControllerCaptor == null)
            return false;

        float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        debuffChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            debuffChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

        debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

        if (RandomSolver.CheckSuccess(debuffChance))
        {
            RaidSceneManager.BattleGround.ControlUnit(target, performer, Duration);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            return true;
        }
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
        return false;
    }
}

public class SummonMonstersEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Summon;
        }
    }

    public int SummonCount { get; set; }
    public bool CanSpawnLoot { get; set; }
    public bool EraseDataOnRoll { get; set; }

    public List<string> SummonMonsters { get; set; }
    public List<float> SummonChances { get; set; }
    public List<int> SummonLimits { get; set; }
    public List<FormationSet> SummonRanks { get; set; }
    public List<int> SummonRollInitiatives { get; set; }

    public SummonMonstersEffect()
    {
        SummonMonsters = new List<string>();
        SummonChances = new List<float>();
        SummonLimits = new List<int>();
        SummonRanks = new List<FormationSet>();
        SummonRollInitiatives = new List<int>();
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        List<int> summonPool = new List<int>();
        List<float> chancePool = new List<float>(SummonChances);
        for(int i = 0; i < SummonMonsters.Count; i++)
            summonPool.Add(i);

        for(int i = 0; i < SummonCount; i++)
        {
            if(summonPool.Count == 0)
                break;

            int rolledIndex = RandomSolver.ChooseRandomIndex(chancePool);
            int summonIndex = summonPool[rolledIndex];
            if(SummonLimits.Count > 0)
            {
                if(SummonLimits[summonIndex] <= performer.Party.Units.FindAll(unit => 
                    unit.Character.Name == SummonMonsters[summonIndex]).Count)
                {
                    i--;
                    summonPool.RemoveAt(rolledIndex);
                    chancePool.RemoveAt(rolledIndex);
                    continue;
                }
            }
            if(performer.Formation.AvailableSummonSpace < DarkestDungeonManager.Data.Monsters[SummonMonsters[summonIndex]].Size)
            {
                i--;
                summonPool.RemoveAt(rolledIndex);
                chancePool.RemoveAt(rolledIndex);
                continue;
            }
            MonsterData summonData = DarkestDungeonManager.Data.Monsters[SummonMonsters[summonIndex]];
            GameObject summonObject = Resources.Load("Prefabs/Monsters/" + summonData.TypeId) as GameObject;
            bool rollInitiative = SummonRollInitiatives.Count > 0 ? true : false;
            if (SummonRanks.Count > 0)
                RaidSceneManager.BattleGround.SummonUnit(summonData, summonObject, SummonRanks[summonIndex].
                    Ranks[RandomSolver.Next(SummonRanks[summonIndex].Ranks.Count)], rollInitiative, CanSpawnLoot);
            else
                RaidSceneManager.BattleGround.SummonUnit(summonData, summonObject, 1, rollInitiative, CanSpawnLoot);
        }
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            return true;
        }
        return false;
    }
}

public class CaptureEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Capture;
        }
    }

    public bool RemoveFromParty { get; set; }
    public bool VirtueBlockable { get; set; }

    public CaptureEffect()
    {
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var emptyCaptorUnit = performer.Party.Units.Find(unit => unit.Character.IsMonster
            && (unit.Character as Monster).Data.EmptyCaptor != null
            && (unit.Character as Monster).Data.EmptyCaptor.PerformerBaseClass == performer.Character.Class);

        if (emptyCaptorUnit == null)
            return false;

        MonsterData fullCaptorData = DarkestDungeonManager.Data.
            Monsters[(emptyCaptorUnit.Character as Monster).Data.EmptyCaptor.FullMonsterClass];
        GameObject unitObject = Resources.Load("Prefabs/Monsters/" + fullCaptorData.TypeId) as GameObject;
        FormationUnit fullCaptorUnit = RaidSceneManager.BattleGround.ReplaceUnit(fullCaptorData, emptyCaptorUnit, unitObject);

        RaidSceneManager.BattleGround.CaptureUnit(target, fullCaptorUnit, RemoveFromParty);

        if(RemoveFromParty == false)
        {
            var emptyCaptorMonster = emptyCaptorUnit.Character as Monster;
            if (emptyCaptorMonster.Data.EmptyCaptor.CaptureEffects.Count > 0)
            {
                foreach (var captorEffectString in emptyCaptorMonster.Data.EmptyCaptor.CaptureEffects)
                {
                    var captorEffect = DarkestDungeonManager.Data.Effects[captorEffectString];
                    foreach (var subEffect in captorEffect.SubEffects)
                        subEffect.ApplyInstant(emptyCaptorUnit, target, captorEffect);
                }
            }
            target.SetCaptureEffect(fullCaptorUnit);
        }
              
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            return true;
        }
        return false;
    }
}

public class BuffEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Buff;
        }
    }

    public List<Buff> Buffs { get; set; }

    public BuffEffect()
    {
        Buffs = new List<Buff>();
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = "";
        foreach (var buff in Buffs)
            if (toolTip.Length > 0)
                toolTip += "\n" + buff.ToolTip;
            else
                toolTip += buff.ToolTip;

        return toolTip;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    void ApplyBuff(FormationUnit target, Effect effect)
    {
        if (effect.IntegerParams[EffectIntParams.Curio].HasValue)
            foreach (var buff in Buffs)
                target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Camp, BuffSourceType.Adventure));
        else if (effect.IntegerParams[EffectIntParams.Duration].HasValue)
        {
            if (effect.IntegerParams[EffectIntParams.Duration].Value == -1)
                foreach (var buff in Buffs)
                    target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Camp, BuffSourceType.Adventure));
            else
                foreach (var buff in Buffs)
                    target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Round,
                        BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
        }
        else
        {
            foreach (var buff in Buffs)
                target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Round,
                    BuffSourceType.Adventure, 3));
        }
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (Buffs.Count == 0)
            return false;

        if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
            {
                ApplyBuff(target, effect);
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                return false;
            }
        }
        else
        {
            if (Buffs[0].IsPositive())
            {
                ApplyBuff(target, effect);
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                return false;
            }
        }
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;
        if (Buffs.Count == 0)
            return false;

        if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
            {
                ApplyBuff(target, effect);
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Debuff);
                    target.OverlaySlot.UpdateOverlay();
                    return true;
                }
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
                return false;
            }
        }
        else
        {
            if (Buffs[0].IsPositive())
            {
                ApplyBuff(target, effect);
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Debuff);
                    target.OverlaySlot.UpdateOverlay();
                    return true;
                }
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
                return false;
            }
        }
    }
}

public class PullEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Pull;
        }
    }

    public int PullParam { get; set; }

    public PullEffect(int pullParam)
    {
        PullParam = pullParam;
    }

    public override string Tooltip(Effect effect)
    {
        return string.Format(LocalizationManager.GetString(
                        "effect_tooltip_move_forward"), PullParam);
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

        moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(moveChance))
        {
            target.Pull(PullParam);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
            return true;
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.MoveResist);
            return false;
        }
    }
}

public class PushEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Push;
        }
    }

    public int PushParam { get; set; }

    public PushEffect(int pushParam)
    {
        PushParam = pushParam;
    }

    public override string Tooltip(Effect effect)
    {
        return string.Format(LocalizationManager.GetString(
                        "effect_tooltip_move_backward"), PushParam);
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

        moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(moveChance))
        {
            target.Push(PushParam);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
            return true;
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.MoveResist);
            return false;
        }
    }
}

public class StunEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Stun;
        }
    }

    public int Duration { get; set; }

    public StunEffect(int duration)
    {
        Duration = duration;
    }

    public override string Tooltip(Effect effect)
    {
        string stunString = LocalizationManager.GetString("effect_tooltip_stun");
        string chanceFormat = string.Format(LocalizationManager.GetString(
                        "effect_base_chance_format"), effect.IntegerParams[EffectIntParams.Chance].Value);
        string toolTip = stunString + " " + chanceFormat;
        return toolTip;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var stunStatus = target.Character.GetStatusEffect(StatusType.Stun) as StunStatusEffect;
        if (stunStatus.IsApplied)
            return true;

        float stunChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        stunChance -= target.Character.GetSingleAttribute(AttributeType.Stun).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            stunChance += performer.Character.GetSingleAttribute(AttributeType.StunChance).ModifiedValue;

        stunChance = Mathf.Clamp(stunChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(stunChance))
        {
            stunStatus.StunApplied = true;
            target.SetHalo("stunned");
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Stunned);
            return true;
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.StunResist);
            return false;
        }
    }
}

public class UnstunEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Unstun;
        }
    }

    public int UnstunParam { get; set; }

    public UnstunEffect(int unstanParam)
    {
        UnstunParam = unstanParam;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_unstun");
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var markStatus = target.Character.GetStatusEffect(StatusType.Stun) as StunStatusEffect;
        if (markStatus.IsApplied)
        {
            markStatus.StunApplied = false;
            target.ResetHalo();
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Unstun);
            return true;
        }
        return false;
    }
}

public class PoisonEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Poison;
        }
    }

    public int DotPoison { get; set; }

    public PoisonEffect(int dotAmount)
    {
        DotPoison = dotAmount;
    }

    public override string Tooltip(Effect effect)
    {
        string poisonString = LocalizationManager.GetString("effect_tooltip_dot_poison");
        string chanceFormat = string.Format(LocalizationManager.GetString(
                        "effect_base_chance_format"), effect.IntegerParams[EffectIntParams.Chance].Value);
        string toolTip = poisonString + " " + chanceFormat;
        toolTip += "\n" + string.Format(LocalizationManager.GetString(
                        "effect_tooltip_dot_format"), DotPoison, effect.IntegerParams[EffectIntParams.Duration].Value);
        return toolTip;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        float poisonChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        poisonChance -= target.Character.GetSingleAttribute(AttributeType.Poison).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            poisonChance += performer.Character.GetSingleAttribute(AttributeType.PoisonChance).ModifiedValue;

        poisonChance = Mathf.Clamp(poisonChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(poisonChance))
        {
            var poisonStatus = target.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect;
            var newDot = new DamageOverTimeInstanse();
            newDot.TickDamage = DotPoison;
            newDot.TicksAmount = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
                effect.IntegerParams[EffectIntParams.Duration].Value : 3;
            newDot.TicksLeft = newDot.TicksAmount;
            poisonStatus.AddInstanse(newDot);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Poison);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.PoisonResist);
            return false;
        }
    }
}

public class BleedEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Bleeding;
        }
    }

    public int DotBleed { get; set; }

    public BleedEffect(int dotAmount)
    {
        DotBleed = dotAmount;
    }

    public override string Tooltip(Effect effect)
    {
        string poisonString = LocalizationManager.GetString("effect_tooltip_dot_bleed");
        string chanceFormat = string.Format(LocalizationManager.GetString(
                        "effect_base_chance_format"), effect.IntegerParams[EffectIntParams.Chance].Value);
        string toolTip = poisonString + " " + chanceFormat;
        toolTip += "\n" + string.Format(LocalizationManager.GetString(
                        "effect_tooltip_dot_format"), DotBleed, effect.IntegerParams[EffectIntParams.Duration].Value);
        return toolTip;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        float bleedChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        bleedChance -= target.Character.GetSingleAttribute(AttributeType.Bleed).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            bleedChance += performer.Character.GetSingleAttribute(AttributeType.BleedChance).ModifiedValue;

        bleedChance = Mathf.Clamp(bleedChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(bleedChance))
        {
            var poisonStatus = target.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
            var newDot = new DamageOverTimeInstanse();
            newDot.TickDamage = DotBleed;
            newDot.TicksAmount = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
                effect.IntegerParams[EffectIntParams.Duration].Value : 3;
            newDot.TicksLeft = newDot.TicksAmount;
            poisonStatus.AddInstanse(newDot);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if(ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Bleed);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.BleedResist);
            return false;
        }
    }
}

public class HealEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Heal;
        }
    }

    public int HealAmount { get; set; }

    public HealEffect(int amount)
    {
        HealAmount = amount;
    }

    public override string Tooltip(Effect effect)
    {
        return string.Format(LocalizationManager.GetString(
                        "effect_tooltip_heal_format"), HealAmount);
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if(effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = HealAmount;
        if(performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.HpHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;
        if (target.Character.AtDeathsDoor)
            (target.Character as Hero).RevertDeathsDoor();
        if (performer != null && RandomSolver.CheckSuccess(performer.Character.Crit))
        {
            int critHeal = Mathf.RoundToInt(heal * 1.5f);
            target.Character.Health.IncreaseValue(critHeal);
            target.OverlaySlot.healthBar.UpdateHealth(target.Character);
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.CritHeal, critHeal.ToString());
        }
        else
        {
            target.Character.Health.IncreaseValue(heal);
            target.OverlaySlot.healthBar.UpdateHealth(target.Character);
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Heal, heal.ToString());
        }
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if(effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = HealAmount;
        if(performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.HpHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;
        if (target.Character.AtDeathsDoor)
            (target.Character as Hero).RevertDeathsDoor();
        if (performer != null && RandomSolver.CheckSuccess(performer.Character.Crit))
        {
            int critHeal = Mathf.RoundToInt(heal * 1.5f);
            target.Character.Health.IncreaseValue(critHeal);
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.CritHeal, critHeal.ToString());
            if(target.Character is Hero)
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally_crit");
            else
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_enemy_crit");

            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
        {
            target.Character.Health.IncreaseValue(heal);
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Heal, heal.ToString());
            if(target.Character is Hero)
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
            else
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_enemy");

            target.OverlaySlot.UpdateOverlay();
            return true;
        }
    }
}

public class CureEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Cure;
        }
    }

    public int CureParam { get; set; }

    public CureEffect(int cureParam)
    {
        CureParam = cureParam;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_cure");
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        (target.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect).RemoveDoT();
        (target.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect).RemoveDoT();
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        bool cureEffective = false;
        var poisonStatus = target.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect;
        var bleedStatus = target.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
        if(poisonStatus.IsApplied)
        {
            poisonStatus.RemoveDoT();
            cureEffective = true;
        }
        if(bleedStatus.IsApplied)
        {
            bleedStatus.RemoveDoT();
            cureEffective = true;
        }
        if (cureEffective)
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Cured);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
            return false;
    }
}

public class TagEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Tag;
        }
    }

    public int TagParam { get; set; }
    public DurationType DurationType { get; set; }

    public TagEffect(int tagParam)
    {
        TagParam = tagParam;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_tag");
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var markStatus = target.Character.GetStatusEffect(StatusType.Marked) as MarkStatusEffect;
        markStatus.MarkDuration = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
            effect.IntegerParams[EffectIntParams.Duration].Value : 3;
        markStatus.DurationType = DurationType;
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if(ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Tagged);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }
}

public class ClearGuardEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.ClearGuard;
        }
    }

    public int ClearParam { get; set; }
    public bool ClearGuarding { get; set; }
    public bool ClearGuarded { get; set; }

    public ClearGuardEffect(int clearParam)
    {
        ClearParam = clearParam;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            ApplyInstant(performer, target, effect);
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if(ClearGuarding)
            target.Character[StatusType.Guard].ResetStatus();
        if (ClearGuarded)
            target.Character[StatusType.Guarded].ResetStatus();

        target.OverlaySlot.UpdateOverlay();

        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
            return true;
        return false;
    }
}

public class UntagEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Untag;
        }
    }

    public int UntagParam { get; set; }

    public UntagEffect(int untagParam)
    {
        UntagParam = untagParam;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_untag");
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var markStatus = target.Character.GetStatusEffect(StatusType.Marked) as MarkStatusEffect;
        if(markStatus.IsApplied)
        {
            markStatus.MarkDuration = 0;
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if(ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Untagged);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }
}

public class CombatStatBuffEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.StatBuff;
        }
    }

    public int CombatStatParam { get; set; }
    public StatusType TargetStatus { get; set; }
    public MonsterType TargetMonsterType { get; set; }

    public Dictionary<AttributeType, float> StatAddBuffs { get; set; }
    public Dictionary<AttributeType, float> StatMultBuffs { get; set; }

    public CombatStatBuffEffect(int combatStatParam)
    {
        CombatStatParam = combatStatParam;
        StatAddBuffs = new Dictionary<AttributeType, float>();
        StatMultBuffs = new Dictionary<AttributeType, float>();
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = "";
        foreach (var item in StatAddBuffs)
        {
            if (item.Key == AttributeType.DamageLow || item.Value == 0)
                continue;

            string newStat = toolTip.Length > 0 ?
                "\n" + string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsAddBonusString(item.Key)), item.Value) :
                string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsAddBonusString(item.Key)), item.Value);
            if (TargetStatus != StatusType.None)
            {
                string statusFormat = LocalizationManager.GetString("buff_rule_tooltip_actorStatus");
                toolTip += string.Format(statusFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.StatusString(TargetStatus)), newStat);
            }
            else if (!(TargetMonsterType == MonsterType.None || TargetMonsterType == MonsterType.Unknown))
            {
                string monsterFormat = LocalizationManager.GetString("buff_rule_tooltip_monsterType");
                toolTip += string.Format(monsterFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.MonsterTypeString(TargetMonsterType)), newStat);
            }
            else
                toolTip += newStat;
        }

        foreach (var item in StatMultBuffs)
        {
            if (item.Key == AttributeType.DamageLow || item.Value == 0)
                continue;
            string newStat = toolTip.Length > 0 ?
                "\n" + string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsMultBonusString(item.Key)), item.Value) :
                string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsMultBonusString(item.Key)), item.Value);

            if (TargetStatus != StatusType.None)
            {
                string statusFormat = LocalizationManager.GetString("buff_rule_tooltip_actorStatus");
                toolTip += string.Format(statusFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.StatusString(TargetStatus)), newStat);
            }
            else if (!(TargetMonsterType == MonsterType.None || TargetMonsterType == MonsterType.Unknown))
            {
                string monsterFormat = LocalizationManager.GetString("buff_rule_tooltip_monsterType");
                toolTip += string.Format(monsterFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.MonsterTypeString(TargetMonsterType)), newStat);
            }
            else
                toolTip += newStat;
        }

        return toolTip;
    }

    public override void ApplyTargetConditions(FormationUnit performer, FormationUnit target, FormationUnit primaryTarget, Effect effect)
    {
        if ((TargetStatus == StatusType.None && TargetMonsterType == MonsterType.None) == false)
        {
            if (primaryTarget == null)
                return;

            if (TargetMonsterType != MonsterType.None)
            {
                if (primaryTarget.Character.IsMonster == false)
                    return;
                else
                {
                    if (!(primaryTarget.Character as Monster).Types.Contains(TargetMonsterType))
                        return;
                }
            }

            if (TargetStatus != StatusType.None)
            {
                if (!primaryTarget.Character.GetStatusEffect(TargetStatus).IsApplied)
                    return;
            }
            ApplyConditional(target, effect);
        }
    }
    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (TargetStatus == StatusType.None && TargetMonsterType == MonsterType.None)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                    ApplyInstant(performer, target, effect);
                else
                    target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
            }
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
    }

    void ApplyBuff(FormationUnit target, Effect effect)
    {
        if(effect.IntegerParams[EffectIntParams.Curio].HasValue)
        {
            foreach (var statInfo in StatAddBuffs)
            {
                target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatAdd,
                    AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                    BuffDurationType.Camp, BuffSourceType.Adventure));
            }
            foreach (var statInfo in StatMultBuffs)
            {
                target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatMultiply,
                    AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                    BuffDurationType.Camp, BuffSourceType.Adventure));
            }
        }
        else if (effect.IntegerParams[EffectIntParams.Duration].HasValue)
        {
            if (effect.IntegerParams[EffectIntParams.Duration].Value == -1)
            {
                foreach (var statInfo in StatAddBuffs)
                {
                    target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatAdd,
                        AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                        BuffDurationType.Camp, BuffSourceType.Adventure));
                }
                foreach (var statInfo in StatMultBuffs)
                {
                    target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatMultiply,
                        AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                        BuffDurationType.Camp, BuffSourceType.Adventure));
                }
            }
            else
            {
                foreach (var statInfo in StatAddBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatAdd,
                        AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                        BuffDurationType.Round, BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
                foreach (var statInfo in StatMultBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatMultiply,
                        AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                        BuffDurationType.Round, BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
            }
        }
        else
        {
            foreach (var statInfo in StatAddBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatAdd,
                    AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                    BuffDurationType.Round, BuffSourceType.Adventure, 3));
            foreach (var statInfo in StatMultBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatMultiply,
                    AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                    BuffDurationType.Round, BuffSourceType.Adventure, 3));
        }
    }
    void ApplyConditional(FormationUnit target, Effect effect)
    {
        foreach (var statInfo in StatAddBuffs)
            target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatAdd,
                AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                BuffDurationType.Round, BuffSourceType.Condition));
        foreach (var statInfo in StatMultBuffs)
            target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatMultiply,
                AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                BuffDurationType.Round, BuffSourceType.Condition));
    }
    public bool IsPositive()
    {
        KeyValuePair<AttributeType, float> buff;

        if (StatAddBuffs.Count > 0)
            buff = StatAddBuffs.First();
        else if (StatMultBuffs.Count > 0)
            buff = StatMultBuffs.First();
        else
            return false;

        if (buff.Key == AttributeType.StressDmgPercent || buff.Key == AttributeType.StressDmgReceivedPercent)
        {
            if (buff.Value > 0)
                return false;
            return true;
        }
        else if (buff.Value >= 0)
            return true;
        return false;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (StatAddBuffs.Count == 0 && StatMultBuffs.Count == 0)
            return false;

        if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
            {
                ApplyBuff(target, effect);
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;
                

                debuffChance = Mathf.Clamp(debuffChance, 0, 0.95f);
                if (performer == target)
                    debuffChance = 1;

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                return false;
            }
        }
        else
        {
            if (IsPositive())
            {
                ApplyBuff(target, effect);
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = Mathf.Clamp(debuffChance, 0, 0.95f);
                if (performer == target)
                    debuffChance = 1;

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                return false;
            }
        }
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (StatAddBuffs.Count == 0 && StatMultBuffs.Count == 0)
            return false;

        if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
            {
                ApplyBuff(target, effect);
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Debuff);
                    target.OverlaySlot.UpdateOverlay();
                    return true;
                }
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
                return false;
            }
        }
        else
        {
            if (IsPositive())
            {
                ApplyBuff(target, effect);
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Debuff);
                    target.OverlaySlot.UpdateOverlay();
                    return true;
                }
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
                return false;
            }
        }
    }
}

public class DiseaseEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Disease;
        }
    }

    public bool IsRandom { get; set; }
    public Quirk Disease { get; set; }

    public DiseaseEffect(Quirk disease, bool isRandom)
    {
        Disease = disease;
        IsRandom = isRandom;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null || target.Character is Hero == false)
            return false;

        float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
        if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
            return false;

        float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;

        if (RandomSolver.CheckSuccess(diseaseChance))
        {
            var hero = target.Character as Hero;
            if (IsRandom == false && Disease != null)
            {
                if (hero.AddQuirk(Disease))
                    return true;
            }
            else
            {
                hero.AddRandomDisease();
                return true;
            }
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null || target.Character is Hero == false)
            return false;

        float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
        if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
            return false;

        float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;

        if (RandomSolver.CheckSuccess(diseaseChance))
        {
            var hero = target.Character as Hero;
            if (IsRandom == false && Disease != null)
            {
                if (hero.AddQuirk(Disease))
                {
                    target.SetHalo("disease");
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Disease,
                        LocalizationManager.GetString("str_quirk_name_" + Disease.Id));
                    return true;
                }
                return false;
            }
            else
            {
                var disease = hero.AddRandomDisease();
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Disease,
                    LocalizationManager.GetString("str_quirk_name_" + disease.Id));
                target.SetHalo("disease");
                return true;
            }
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DiseaseResist);
            return false;
        }
    }
}

public class PerformerRankTargetEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Rank;
        }
    }

    public int ParamRank { get; set; }

    public PerformerRankTargetEffect(int rankParam)
    {
        ParamRank = rankParam;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null || performer == null)
            return false;

        target.Formation.rankHolder.MarkRank(target.Rank);
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}

public class ClearRankTargetEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.ClearTargetRanks;
        }
    }

    public FormationSet Ranks { get; set; }

    public ClearRankTargetEffect(string ranks)
    {
        Ranks = new FormationSet(ranks);
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (performer == null)
            return false;

        if (target.Team == Team.Heroes)
            RaidSceneManager.BattleGround.MonsterFormation.rankHolder.ClearMarks();
        else
            RaidSceneManager.BattleGround.HeroFormation.rankHolder.ClearMarks();

        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}

public class ImmobilizeEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Immobilize;
        }
    }

    public int ImmobilizeParam { get; set; }

    public ImmobilizeEffect(int immobilizeParam)
    {
        ImmobilizeParam = immobilizeParam;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (!target.CombatInfo.IsImmobilized)
        {
            target.CombatInfo.IsImmobilized = true;
            target.SetDefendAnimation(true);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}

public class UnimmobilizeEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Unimmobilize;
        }
    }

    public int UnimmobilizeParam { get; set; }

    public UnimmobilizeEffect(int unimmobilizeParam)
    {
        UnimmobilizeParam = unimmobilizeParam;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.CombatInfo.IsImmobilized)
        {
            target.CombatInfo.IsImmobilized = false;
            target.SetDefendAnimation(false);
            return true;
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}

public class KillEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Kill;
        }
    }

    public int KillParam { get; set; }

    public KillEffect(int killParam)
    {
        KillParam = killParam;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        target.Character.Health.CurrentValue = 0;
        target.CombatInfo.MarkedForDeath = true;
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}

public class KillEnemyTypeEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.KillType;
        }
    }

    public MonsterType EnemyType { get; set; }

    public KillEnemyTypeEffect(MonsterType monsterType)
    {
        EnemyType = monsterType;
    }

    public override string Tooltip(Effect effect)
    {
        switch (EnemyType)
        {
            case MonsterType.Man:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_man"));
            case MonsterType.Beast:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_beast"));
            case MonsterType.Unholy:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_unholy"));
            case MonsterType.Eldritch:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_eldritch"));
            case MonsterType.Corpse:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_corpse"));
            default:
                return "";
        }
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character.IsMonster)
        {
            if ((target.Character as Monster).Types.Contains(EnemyType))
            {
                target.Character.Health.CurrentValue = 0;
                target.CombatInfo.MarkedForDeath = true;
                return true;
            }
        }
        return false;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}

public class StressEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Stress;
        }
    }
    public override bool Fusable
    {
        get
        {
            return true;
        }
    }
    public int StressAmount { get; set; }

    public StressEffect(int amount)
    {
        StressAmount = amount;
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = string.Format(LocalizationManager.GetString(
                        "effect_tooltip_stress_format"), StressAmount);
        return toolTip;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character is Hero == false)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialDamage = StressAmount;
        if(performer != null)
            initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

        int damage = Mathf.RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
        if (damage < 1) damage = 1;

        target.Character.Stress.IncreaseValue(damage);
        if(target.Character.IsOverstressed)
        {
            if (target.Character.IsVirtued)
                target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                    RaidSceneManager.Instanse.AddResolveCheck(target);

            if (target.Character.Stress.CurrentValue == 200)
                RaidSceneManager.Instanse.AddHeartAttackCheck(target);
        }
        target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character is Hero == false)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialDamage = StressAmount;
        if(performer != null)
            initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

        int damage = Mathf.RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
        if (damage < 1) damage = 1;

        target.Character.Stress.IncreaseValue(damage);
        if (target.Character.IsOverstressed)
        {
            if (target.Character.IsVirtued)
                target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                RaidSceneManager.Instanse.AddResolveCheck(target);

            if (target.Character.Stress.CurrentValue == 200)
                RaidSceneManager.Instanse.AddHeartAttackCheck(target);
        }

        target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Stress, damage.ToString());
        target.SetHalo("afflicted");
        return true;
    }

    public override bool ApplyFused(FormationUnit performer, FormationUnit target, Effect effect, int fuseParameter)
    {
        if (target == null || fuseParameter <= 0)
            return false;

        if (target.Character is Hero == false)
            return false;

        target.Character.Stress.IncreaseValue(fuseParameter);
        if (target.Character.IsOverstressed)
        {
            if (target.Character.IsVirtued)
                target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                RaidSceneManager.Instanse.AddResolveCheck(target);

            if (target.Character.Stress.CurrentValue == 200)
                RaidSceneManager.Instanse.AddHeartAttackCheck(target);
        }

        target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Stress, fuseParameter.ToString());
        target.SetHalo("afflicted");
        return true;
    }
    public override int Fuse(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return 0;

        if (target.Character is Hero == false)
            return 0;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return 0;

        float initialDamage = StressAmount;
        if (performer != null)
            initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

        int damage = Mathf.RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
        if (damage < 1) damage = 1;

        return damage;
    }
}

public class StressHealEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.StressHeal;
        }
    }
    public override bool Fusable
    {
        get
        {
            return true;
        }
    }

    public int StressHealAmount { get; set; }

    public StressHealEffect(int amount)
    {
        StressHealAmount = amount;
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = string.Format(LocalizationManager.GetString(
                        "effect_tooltip_stress_heal_format"), StressHealAmount);
        return toolTip;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var hero = target.Character as Hero;

        if (hero == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = StressHealAmount;
        if(performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;

        target.Character.Stress.DecreaseValue(heal);
        if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
            hero.RevertTrait();
        target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var hero = target.Character as Hero;

        if (hero == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = StressHealAmount;
        if(performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;

        target.Character.Stress.DecreaseValue(heal);
        if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
            hero.RevertTrait();
        target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.StressHeal, heal.ToString());
        target.SetHalo("heroic");
        return true;
    }
    public override bool ApplyFused(FormationUnit performer, FormationUnit target, Effect effect, int fuseParameter)
    {
        if (target == null || fuseParameter <= 0)
            return false;

        var hero = target.Character as Hero;

        if (hero == null)
            return false;

        target.Character.Stress.DecreaseValue(fuseParameter);
        if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
            hero.RevertTrait();
        target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.StressHeal, fuseParameter.ToString());
        target.SetHalo("heroic");
        return true;
    }
    public override int Fuse(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return 0;

        if (target.Character is Hero == false)
            return 0;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return 0;

        float initialHeal = StressHealAmount;
        if (performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;

        return heal;
    }
}

public class ShuffleTargetEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Shuffle;
        }
    }

    public bool IsPartyShuffle { get; set; }

    public ShuffleTargetEffect(bool isPartyShuffle)
    {
        IsPartyShuffle = isPartyShuffle;
    }
    public override string Tooltip(Effect effect)
    {
        if (IsPartyShuffle)
            return LocalizationManager.GetString("effect_tooltip_shuffle_party");
        else
            return LocalizationManager.GetString("effect_tooltip_shuffle_single");
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Party.Units.Count < 2)
            return false;

        if (IsPartyShuffle)
        {
            var shuffleUnits = new List<FormationUnit>(target.Party.Units);
            foreach (var unit in shuffleUnits)
            {
                var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < unit.Rank)
                    unit.Pull(unit.Rank - shuffleRoll.Rank);
                else
                    unit.Push(shuffleRoll.Rank - unit.Rank);
            }
            shuffleUnits.Clear();
            return true;
        }
        else
        {
            float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
            if (performer != null && performer.Character is Hero)
                moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

            moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
            if (RandomSolver.CheckSuccess(moveChance))
            {
                var shuffleTargets = target.Party.Units.FindAll(unit => unit != target);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < target.Rank)
                    target.Pull(target.Rank - shuffleRoll.Rank);
                else
                    target.Push(shuffleRoll.Rank - target.Rank);
                return true;
            }
            return false;
        }

    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Party.Units.Count < 2)
            return false;

        if (IsPartyShuffle)
        {
            foreach (var unit in target.Party.Units)
            {
                float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                moveChance -= unit.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
                if (RandomSolver.CheckSuccess(moveChance))
                {
                    var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                    var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                    if (shuffleRoll.Rank < unit.Rank)
                        unit.Pull(unit.Rank - shuffleRoll.Rank);
                    else
                        unit.Push(shuffleRoll.Rank - unit.Rank);
                    return true;
                }
            }
            return true;
        }
        else
        {
            float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
            if (performer != null && performer.Character is Hero)
                moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

            moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
            if (RandomSolver.CheckSuccess(moveChance))
            {
                var shuffleTargets = target.Party.Units.FindAll(unit => unit != target);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < target.Rank)
                    target.Pull(target.Rank - shuffleRoll.Rank);
                else
                    target.Push(shuffleRoll.Rank - target.Rank);
                return true;
            }
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.MoveResist);
            return false;
        }

    }
}

public class GuardEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.GuardAlly;
        }
    }

    public int GuardParam { get; set; }
    public bool SwapTargets { get; set; }

    public GuardEffect(int guardParam)
    {
        GuardParam = guardParam;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
            {
                if (SwapTargets)
                    ApplyInstant(target, performer, effect);
                else
                    ApplyInstant(performer, target, effect);
            }
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var targetGuardStatus = target.Character.GetStatusEffect(StatusType.Guard) as GuardStatusEffect;
        var targetGuardedStatus = target.Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;

        var performerGuardStatus = performer.Character.GetStatusEffect(StatusType.Guard) as GuardStatusEffect;
        var performerGuardedStatus = performer.Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;

        if (performerGuardedStatus.IsApplied)
            performerGuardedStatus.ResetStatus();

        if(performerGuardStatus.IsApplied)
        {
            if(performerGuardStatus.Targets.Contains(target))
            {
                targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
                    effect.IntegerParams[EffectIntParams.Duration].Value : 1;
                if (targetGuardedStatus.Guard != performer)
                    Debug.LogError("Alien guard: " + targetGuardedStatus.Guard.name + ". Expected guard: " + performer.name);
            }
            else
            {
                if (targetGuardStatus.IsApplied)
                    targetGuardStatus.ResetStatus();

                if (targetGuardedStatus.IsApplied)
                    targetGuardedStatus.ResetStatus();

                targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
                    effect.IntegerParams[EffectIntParams.Duration].Value : 1;
                targetGuardedStatus.Guard = performer;
                performerGuardStatus.Targets.Add(target);
                target.OverlaySlot.UpdateOverlay();
            }
        }
        else
        {
            if (targetGuardStatus.IsApplied)
                targetGuardStatus.ResetStatus();

            if (targetGuardedStatus.IsApplied)
                targetGuardedStatus.ResetStatus();

            targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
                effect.IntegerParams[EffectIntParams.Duration].Value : 1;
            targetGuardedStatus.Guard = performer;
            performerGuardStatus.Targets.Add(target);
            performer.OverlaySlot.UpdateOverlay();
            target.OverlaySlot.UpdateOverlay();
        }

        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if(SwapTargets)
        {
            if (ApplyInstant(target, performer, effect))
            {
                RaidSceneManager.RaidEvents.ShowPopupMessage(performer, PopupMessageType.Guard);
                performer.OverlaySlot.UpdateOverlay();
                return true;
            }
        }
        else
        {
            if (ApplyInstant(performer, target, effect))
            {
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Guard);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_guard");
    }
}

public class RiposteEffect : SubEffect
{
    public override EffectSubType Type
    {
        get
        {
            return EffectSubType.Riposte;
        }
    }

    public float RiposteChance { get; set; }

    public Dictionary<AttributeType, float> StatAddBuffs { get; set; }
    public Dictionary<AttributeType, float> StatMultBuffs { get; set; }

    public RiposteEffect()
    {
        StatAddBuffs = new Dictionary<AttributeType, float>();
        StatMultBuffs = new Dictionary<AttributeType, float>();
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_riposte");
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }
    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var riposteStatus = target.Character.GetStatusEffect(StatusType.Riposte) as RiposteStatusEffect;
        int duration = effect.IntegerParams[EffectIntParams.Duration].HasValue ?
            effect.IntegerParams[EffectIntParams.Duration].Value : 1;

        if (duration == -1)
        {
            riposteStatus.DurationType = DurationType.Combat;
            duration = 1;
        }

        riposteStatus.RiposteDuration = duration;

        foreach (var statInfo in StatAddBuffs)
            if(statInfo.Value != 0)
                target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatAdd,
                    RuleType = BuffRule.Riposting, AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                    BuffDurationType.Round, BuffSourceType.Adventure, duration));
        foreach (var statInfo in StatMultBuffs)
            if (statInfo.Value != 0)
                target.Character.AddBuff(new BuffInfo(new Buff() { Id = "", Type = BuffType.StatMultiply,
                    RuleType = BuffRule.Riposting, AttributeType = statInfo.Key, ModifierValue = statInfo.Value },
                    BuffDurationType.Round, BuffSourceType.Adventure, duration));

        return true;
    }
    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Riposte);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }
}