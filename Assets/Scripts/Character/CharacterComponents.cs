using UnityEngine;
using System.Collections.Generic;

public enum CharacterComponentType
{
    CommonEffects, Initiative, SharedHealth,
    Shapeshifter, EmptyCaptor, FullCaptor,
    Controller, Spawn, LifeLink, LifeTime,
    Companion, DeathClass, DeathDamage,
    SkillReaction, DisplayModifier,
    HealthBarModifier, BattleModifier,
    LootDefinition, TorchlightModifier
}
public enum DeathClassType
{
    Replacement, Corpse
}

public abstract class CharacterComponent
{
    public abstract CharacterComponentType ComponentType
    {
        get;
    }
    public abstract void LoadData(List<string> data);
}

public class CommonEffects : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.CommonEffects;
        }
    }

    public string DeathEffect { get; private set; }

    public CommonEffects(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".deathfx":
                    DeathEffect = data[++i];
                    break;
                default:
                    Debug.LogError("Unexpected token in common effects: " + data[i]);
                    break;
            }
        }
    }
}
public class DeathClass : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.DeathClass;
        }
    }

    public string CorpseClass { get; set; }
    public DeathClassType Type { get; set; }

    public bool? IsValidOnCrit { get; set; }
    public bool? IsValidOnBleedDot { get; set; }
    public bool? IsValidOnBlightDot { get; set; }
    public bool? CanDieFromDamage { get; set; }

    public float? CarryOverHpMinPercent { get; set; }
    public List<Effect> DeathChangeEffects { get; set; }

    public DeathClass(List<string> data)
    {
        DeathChangeEffects = new List<Effect>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".monster_class_id":
                    CorpseClass = data[++i];
                    break;
                case ".is_valid_on_crit":
                    IsValidOnCrit = bool.Parse(data[++i].ToLower());
                    break;
                case ".is_valid_on_bleed_dot":
                    IsValidOnBleedDot = bool.Parse(data[++i].ToLower());
                    break;
                case ".is_valid_on_blight_dot":
                    IsValidOnBlightDot = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_die_from_damage":
                    CanDieFromDamage = bool.Parse(data[++i].ToLower());
                    break;
                case ".carry_over_hp_min_percent":
                    CarryOverHpMinPercent = float.Parse(data[++i].ToLower());
                    break;
                case ".change_class_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            DeathChangeEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in death class");
                    }
                    break;
                case ".type":
                    if (data[++i] == "corpse")
                        Type = DeathClassType.Corpse;
                    break;
                default:
                    Debug.LogError("Unexpected token in death class: " + data[i]);
                    break;
            }
        }
    }
}
public class Initiative : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.Initiative;
        }
    }

    public int NumberOfTurns { get; private set; }

    public bool HideIndicator { get; private set; }

    public Initiative(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".number_of_turns_per_round":
                    NumberOfTurns = int.Parse(data[++i]);
                    break;
                case ".hide_indicator":
                    HideIndicator = bool.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in initiative: " + data[i]);
                    break;
            }
        }
    }
}

public class SharedHealth : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.SharedHealth;
        }
    }

    public string SharedId { get; private set; }

    public SharedHealth(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".id":
                    SharedId = data[++i];
                    break;
                default:
                    Debug.LogError("Unexpected token in shared health: " + data[i]);
                    break;
            }
        }
    }
}
public class Shapeshifter : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.Shapeshifter;
        }
    }

    public string EffectName { get; set; }
    public List<string> MonsterClassIds { get; set; }
    public List<int> MonsterClassChances { get; set; }
    public List<FormationSet> MonsterClassValidRanks { get; set; }
    public int RoundFrequency { get; set; }

    public Shapeshifter(List<string> data)
    {
        MonsterClassIds = new List<string>();
        MonsterClassChances = new List<int>();
        MonsterClassValidRanks = new List<FormationSet>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".fx_name":
                    EffectName = data[++i];
                    break;
                case ".round_frequency":
                    RoundFrequency = int.Parse(data[++i]);
                    break;
                case ".monster_class_ids":
                    while (++i < data.Count && data[i--][0] != '.')
                        MonsterClassIds.Add(data[++i]);
                    break;
                case ".monster_class_chances":
                    while (++i < data.Count && data[i--][0] != '.')
                        MonsterClassChances.Add(int.Parse(data[++i]));
                    break;
                case ".monster_class_valid_ranks":
                    while (++i < data.Count && data[i--][0] != '.')
                        MonsterClassValidRanks.Add(new FormationSet(data[++i]));
                    break;
                default:
                    Debug.LogError("Unexpected token in shapeshifter: " + data[i]);
                    break;
            }
        }
    }
}

public class EmptyCaptor : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.EmptyCaptor;
        }
    }

    public string FullMonsterClass { get; set; }
    public string PerformerBaseClass { get; set; }

    public List<string> CaptureEffects { get; set; }

    public EmptyCaptor(List<string> data)
    {
        CaptureEffects = new List<string>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".performing_monster_captor_base_class":
                    PerformerBaseClass = data[++i];
                    break;
                case ".captor_full_monster_class":
                    FullMonsterClass = data[++i];
                    break;
                case ".capture_effects":
                    while (++i < data.Count && data[i][0] != '.')
                        CaptureEffects.Add(data[i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in empty captor: " + data[i]);
                    break;
            }
        }
    }
}
public class FullCaptor : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.FullCaptor;
        }
    }

    public string EmptyMonsterClass { get; set; }

    public bool SwitchClassOnDeath { get; set; }
    public bool ReleaseOnDeath { get; set; }
    public bool ReleasePrisonerAtDeathDoor { get; set; }
    public bool ReleaseOnPrisonerAffliction { get; set; }

    public bool HasPrisonerOverlay { get; set; }

    public float PerTurnDamagePercent { get; set; }
    public float PerTurnStress { get; set; }

    public List<string> ReleaseEffects { get; set; }

    public FullCaptor(List<string> data)
    {
        ReleaseEffects = new List<string>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".switch_class_on_death":
                    SwitchClassOnDeath = bool.Parse(data[++i].ToLower());
                    break;
                case ".captor_empty_monster_class":
                    EmptyMonsterClass = data[++i];
                    break;
                case ".release_on_death":
                    ReleaseOnDeath = bool.Parse(data[++i].ToLower());
                    break;
                case ".release_on_prisoner_at_deaths_door":
                    ReleasePrisonerAtDeathDoor = bool.Parse(data[++i].ToLower());
                    break;
                case ".release_on_prisoner_affliction":
                    ReleaseOnPrisonerAffliction = bool.Parse(data[++i].ToLower());
                    break;
                case ".per_turn_damage_percent":
                    PerTurnDamagePercent = float.Parse(data[++i]);
                    break;
                case ".per_turn_stress_damage":
                    PerTurnStress = float.Parse(data[++i]);
                    break;
                case ".has_prisoner_overlay":
                    HasPrisonerOverlay = bool.Parse(data[++i].ToLower());
                    break;
                case ".release_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                        ReleaseEffects.Add(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in full captor: " + data[i]);
                    break;
            }
        }
    }
}
public class Controller : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.Controller;
        }
    }

    public int StressPerTurn { get; set; }

    public List<string> UncontrollEffects { get; set; }

    public Controller(List<string> data)
    {
        UncontrollEffects = new List<string>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".stress_per_controlled_turn":
                    StressPerTurn = int.Parse(data[++i]);
                    break;
                case ".uncontrol_effects":
                    while (++i < data.Count && data[i][0] != '.')
                        UncontrollEffects.Add(data[i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in controller captor: " + data[i]);
                    break;
            }
        }
    }
}

public class LifeTime : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.LifeTime;
        }
    }

    public int AliveRoundLimit { get; set; }
    public bool CheckForLoot { get; set; }

    public LifeTime(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".alive_round_limit":
                    AliveRoundLimit = int.Parse(data[++i]);
                    break;
                case ".does_check_for_loot":
                    CheckForLoot = bool.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in life time: " + data[i]);
                    break;
            }
        }
    }
}
public class LifeLink : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.LifeLink;
        }
    }

    public string LinkBaseClass { get; set; }

    public bool CheckForLoot { get; set; }

    public LifeLink(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".base_class":
                    LinkBaseClass = data[++i];
                    break;
                case ".does_spawn_loot":
                    CheckForLoot = bool.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in life link: " + data[i]);
                    break;
            }
        }
    }
}
public class Companion : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.Companion;
        }
    }

    public string MonsterClass { get; set; }
    public float HealPerTurn { get; set; }

    public List<Buff> Buffs { get; set; }

    public Companion(List<string> data)
    {
        Buffs = new List<Buff>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".monster_class":
                    MonsterClass = data[++i];
                    break;
                case ".heal_per_turn_percent":
                    HealPerTurn = float.Parse(data[++i]);
                    break;
                case ".buffs":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        i++;
                        if (DarkestDungeonManager.Data.Buffs.ContainsKey(data[i]))
                            Buffs.Add(DarkestDungeonManager.Data.Buffs[data[i]]);
                        else
                            Debug.LogError("Missing buff " + data[i] + " in Companion " + MonsterClass);
                    }
                    break;
                default:
                    Debug.LogError("Unexpected token in companion: " + data[i]);
                    break;
            }
        }
    }
}

public class DeathDamage : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.DeathDamage;
        }
    }

    public string TargetBaseClass { get; set; }
    public int TargetDamage { get; set; }

    public DeathDamage(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".target_base_class_id":
                    TargetBaseClass = data[++i];
                    break;
                case ".target_damage":
                    TargetDamage = int.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in death damage: " + data[i]);
                    break;
            }
        }
    }
}
public class SkillReaction : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.SkillReaction;
        }
    }

    public List<Effect> WasHitPerformerEffects { get; set; }
    public List<Effect> WasKilledOtherMonstersEffects { get; set; }

    public SkillReaction(List<string> data)
    {
        WasHitPerformerEffects = new List<Effect>();
        WasKilledOtherMonstersEffects = new List<Effect>();
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".was_hit_performer_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            WasHitPerformerEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill reaction");
                    }
                    break;
                case ".was_killed_other_monsters_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            WasKilledOtherMonstersEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill reaction");
                    }
                    break;
                default:
                    Debug.LogError("Unexpected token in skill reaction: " + data[i]);
                    break;
            }
        }
    }
}
public class Spawn : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.Spawn;
        }
    }

    public List<Effect> Effects { get; set; }

    public Spawn(List<string> data)
    {
        Effects = new List<Effect>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            Effects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in spawn");
                    }
                    break;
                default:
                    Debug.LogError("Unexpected token in spawn: " + data[i]);
                    break;
            }
        }
    }
}

public class DisplayModifier : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.DisplayModifier;
        }
    }

    public bool DisableHalos { get; set; }
    public bool UseCentreSkillAnnouncment { get; set; }

    public List<string> DisabledPopups { get; set; }

    public DisplayModifier(List<string> data)
    {
        DisabledPopups = new List<string>();

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".disable_halos":
                    DisableHalos = bool.Parse(data[++i].ToLower());
                    break;
                case ".use_centre_skill_announcement":
                    UseCentreSkillAnnouncment = bool.Parse(data[++i].ToLower());
                    break;
                case ".disabled_popup_text_types":
                    while (++i < data.Count && data[i--][0] != '.')
                        DisabledPopups.Add(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in display modifier: " + data[i]);
                    break;
            }
        }
    }
}
public class HealthbarModifier : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.HealthBarModifier;
        }
    }

    public string Type { get; set; }

    public HealthbarModifier(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".type":
                    Type = data[++i];
                    break;
                default:
                    Debug.LogError("Unexpected token in health bar: " + data[i]);
                    break;
            }
        }
    }
}

public class BattleModifier : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.BattleModifier;
        }
    }

    public bool DisableStallPenalty { get; set; }
    public bool CanSurprise { get; set; }
    public bool CanBeSurprised { get; set; }
    public bool AlwaysSurprise { get; set; }
    public bool AlwaysBeSurprised { get; set; }
    public bool IsValidFriendlyTarget { get; set; }
    public bool CanRelieveStressFromKills { get; set; }
    public bool CanRelieveStressFromCrit { get; set; }
    public bool CanBeSummonRank { get; set; }
    public bool CountsAsSizeForMonsterBrains { get; set; }
    public bool CountsTowardsStallPenalty { get; set; }
    public bool CanBeMissed { get; set; }
    public bool CanBeRandomTarget { get; set; }

    public bool? CanBeHit { get; set; }
    public bool? CanBeDamagedDirectly { get; set; }

    public BattleModifier()
    {
        DisableStallPenalty = false;            // ok
        CanSurprise = true;                     // ok
        CanBeSurprised = true;                  // ok
        AlwaysSurprise = false;                 // ok
        AlwaysBeSurprised = false;              // ok
        IsValidFriendlyTarget = true;           // ok
        CanRelieveStressFromKills = true;       // ok
        CanRelieveStressFromCrit = true;        // ok
        CanBeSummonRank = false;                // ok
        CountsAsSizeForMonsterBrains = true;    // ok
        CountsTowardsStallPenalty = true;       // ok
        CanBeMissed = true;                     // ok
        CanBeRandomTarget = true;

        CanBeHit = true;                        // ok
        CanBeDamagedDirectly = true;            // ok
    }
    public BattleModifier(List<string> data)
    {
        DisableStallPenalty = false;            // ok
        CanSurprise = true;                     // ok
        CanBeSurprised = true;                  // ok
        AlwaysSurprise = false;                 // ok
        AlwaysBeSurprised = false;              // ok
        IsValidFriendlyTarget = true;           // ok
        CanRelieveStressFromKills = true;       // ok
        CanRelieveStressFromCrit = true;        // ok
        CanBeSummonRank = false;                // ok
        CountsAsSizeForMonsterBrains = true;    // ok
        CountsTowardsStallPenalty = true;       // ok
        CanBeMissed = true;                     // ok
        CanBeRandomTarget = true;

        CanBeHit = true;                        // ok
        CanBeDamagedDirectly = true;            // ok

        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".disable_stall_penalty":
                    DisableStallPenalty = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_surprise":
                    CanSurprise = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_surprised":
                    CanBeSurprised = bool.Parse(data[++i].ToLower());
                    break;
                case ".always_surprise":
                    AlwaysSurprise = bool.Parse(data[++i].ToLower());
                    break;
                case ".always_be_surprised":
                    AlwaysBeSurprised = bool.Parse(data[++i].ToLower());
                    break;
                case ".is_valid_friendly_target":
                    IsValidFriendlyTarget = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_relieve_stress_from_killing_blow":
                    CanRelieveStressFromKills = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_summon_rank":
                    CanBeSummonRank = bool.Parse(data[++i].ToLower());
                    break;
                case ".does_count_as_monster_size_for_monster_brain":
                    CountsAsSizeForMonsterBrains = bool.Parse(data[++i].ToLower());
                    break;
                case ".does_count_towards_stall_penalty":
                    CountsTowardsStallPenalty = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_relieve_stress_from_crit":
                    CanRelieveStressFromCrit = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_missed":
                    CanBeMissed = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_random_target":
                    CanBeRandomTarget = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_hit":
                    CanBeHit = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_damaged_directly":
                    CanBeDamagedDirectly = bool.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in battle modifiers: " + data[i]);
                    break;
            }
        }
    }
}
public class LootDefinition : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.LootDefinition;
        }
    }

    public string Code { get; set; }
    public int Count { get; set; }

    public LootDefinition()
    {

    }
    public LootDefinition(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".code":
                    Code = data[++i];
                    break;
                case ".count":
                    Count = int.Parse(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in loot: " + data[i]);
                    break;
            }
        }
    }
}
public class TorchlightModifier : CharacterComponent
{
    public override CharacterComponentType ComponentType
    {
        get
        {
            return CharacterComponentType.TorchlightModifier;
        }
    }

    public int Min { get; set; }
    public int Max { get; set; }

    public TorchlightModifier(int min, int max)
    {
        Min = min;
        Max = max;
    }
    public TorchlightModifier(List<string> data)
    {
        LoadData(data);
    }

    public override void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".min":
                    Min = int.Parse(data[++i]);
                    break;
                case ".max":
                    Max = int.Parse(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in torchlight modifier: " + data[i]);
                    break;
            }
        }
    }
}