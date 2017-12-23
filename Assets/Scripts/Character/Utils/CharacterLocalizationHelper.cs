using UnityEngine;

public static class CharacterLocalizationHelper
{
    public static string BaseStatsAddBonusString(AttributeType stat)
    {
        switch (stat)
        {
            case AttributeType.AttackRating:
                return "buff_stat_tooltip_combat_stat_add_attack_rating";
            case AttributeType.DamageHigh:
                return "buff_stat_tooltip_combat_stat_add_damage_high";
            case AttributeType.DamageLow:
                return "buff_stat_tooltip_combat_stat_add_damage_low";
            case AttributeType.CritChance:
                return "buff_stat_tooltip_combat_stat_add_crit_chance";
            case AttributeType.DefenseRating:
                return "buff_stat_tooltip_combat_stat_add_defense_rating";
            case AttributeType.HpHealReceivedPercent:
                return "buff_stat_tooltip_hp_heal_received_percent";
            case AttributeType.ProtectionRating:
                return "buff_stat_tooltip_combat_stat_add_protection_rating";
            case AttributeType.SpeedRating:
                return "buff_stat_tooltip_combat_stat_add_speed_rating";
        }
        return "";
    }

    public static string BaseStatsMultBonusString(AttributeType stat)
    {
        switch (stat)
        {
            case AttributeType.AttackRating:
                return "buff_stat_tooltip_combat_stat_multiply_attack_rating";
            case AttributeType.DamageHigh:
                return "buff_stat_tooltip_combat_stat_multiply_damage_high";
            case AttributeType.DamageLow:
                return "buff_stat_tooltip_combat_stat_multiply_damage_low";
            case AttributeType.CritChance:
                return "buff_stat_tooltip_combat_stat_multiply_crit_chance";
            case AttributeType.DefenseRating:
                return "buff_stat_tooltip_combat_stat_multiply_defense_rating";
            case AttributeType.HpHealReceivedPercent:
                return "buff_stat_tooltip_hp_heal_received_percent";
            case AttributeType.ProtectionRating:
                return "buff_stat_tooltip_combat_stat_multiply_protection_rating";
            case AttributeType.SpeedRating:
                return "buff_stat_tooltip_combat_stat_multiply_speed_rating";
        }
        return "";
    }

    public static string StatusString(StatusType status)
    {
        switch (status)
        {
            case StatusType.Bleeding:
                return "buff_rule_data_tooltip_bleeding";
            case StatusType.Poison:
                return "buff_rule_data_tooltip_poisoned";
            case StatusType.Stun:
                return "buff_rule_data_tooltip_stunned";
            case StatusType.Marked:
                return "buff_rule_data_tooltip_tagged";
            default:
                return "";
        }
    }

    public static string MonsterTypeString(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Man:
                return "buff_rule_data_tooltip_man";
            case MonsterType.Beast:
                return "buff_rule_data_tooltip_beast";
            case MonsterType.Eldritch:
                return "buff_rule_data_tooltip_eldritch";
            case MonsterType.Unholy:
                return "buff_rule_data_tooltip_unholy";
            default:
                return "";
        }
    }

    public static string MonsterTooltipTypeString(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Man:
                return "enemy_type_name_man";
            case MonsterType.Beast:
                return "enemy_type_name_beast";
            case MonsterType.Eldritch:
                return "enemy_type_name_eldritch";
            case MonsterType.Unholy:
                return "enemy_type_name_unholy";
            case MonsterType.Cauldron:
                return "enemy_type_name_cauldron";
            case MonsterType.Ironwork:
                return "enemy_type_name_ironwork";
            case MonsterType.Carpentry:
                return "enemy_type_name_carpentry";
            case MonsterType.Corpse:
                return "enemy_type_name_corpse";
            case MonsterType.Cosmic:
                return "enemy_type_name_cosmic";
            default:
                return "";
        }
    }

    public static string BuffRuleTooltipString(BuffRule buffRule, bool isFalse)
    {
        if (isFalse)
        {
            switch (buffRule)
            {
                case BuffRule.Always:
                    return "buff_rule_tooltip_always_false";
                case BuffRule.Size:
                    return "buff_rule_tooltip_monsterSize_false";
                case BuffRule.LightBelow:
                    return "buff_rule_tooltip_lightbelow_false";
                case BuffRule.LightAbove:
                    return "buff_rule_tooltip_lightabove_false";
                case BuffRule.HpBelow:
                    return "buff_rule_tooltip_hpbelow_false";
                case BuffRule.HpAbove:
                    return "buff_rule_tooltip_hpabove_false";
                case BuffRule.Afflicted:
                    return "buff_rule_tooltip_afflicted_false";
                case BuffRule.Virtued:
                    return "buff_rule_tooltip_virtued_false";
                case BuffRule.Melee:
                    return "buff_rule_tooltip_meleeonly_false";
                case BuffRule.Ranged:
                    return "buff_rule_tooltip_rangedonly_false";
                case BuffRule.FirstRound:
                    return "buff_rule_tooltip_firstroundonly_false";
                case BuffRule.Status:
                    return "buff_rule_tooltip_actorStatus_false";
                case BuffRule.EnemyType:
                    return "buff_rule_tooltip_monsterType_false";
                case BuffRule.DeathsDoor:
                    return "buff_rule_tooltip_at_deaths_door_false";
                case BuffRule.InRank:
                    return "buff_rule_tooltip_in_rank_false";
                case BuffRule.InCamp:
                    return "buff_rule_tooltip_in_camp_false";
                case BuffRule.InDungeon:
                    return "buff_rule_tooltip_in_dungeon_false";
                case BuffRule.InMode:
                    return "buff_rule_tooltip_in_mode_false";
                case BuffRule.StressAbove:
                    return "buff_rule_tooltip_stress_above_false";
                case BuffRule.StressBelow:
                    return "buff_rule_tooltip_stress_below_false";
                case BuffRule.WalkBack:
                    return "buff_rule_tooltip_walking_backwards_false";
                case BuffRule.InActivity:
                    return "buff_rule_tooltip_in_activity_false";
                case BuffRule.InCorridor:
                    return "buff_rule_tooltip_in_corridor_false";
                case BuffRule.Riposting:
                    return "buff_rule_tooltip_riposte_false";
                case BuffRule.Skill:
                    return "buff_rule_tooltip_skill_false";
                default:
                    Debug.LogError("Unknown rule type: " + buffRule);
                    return "buff_rule_tooltip_always_false";
            }
        }
        else
        {
            switch (buffRule)
            {
                case BuffRule.Always:
                    return "buff_rule_tooltip_always";
                case BuffRule.Size:
                    return "buff_rule_tooltip_monsterSize";
                case BuffRule.LightBelow:
                    return "buff_rule_tooltip_lightbelow";
                case BuffRule.LightAbove:
                    return "buff_rule_tooltip_lightabove";
                case BuffRule.HpBelow:
                    return "buff_rule_tooltip_hpbelow";
                case BuffRule.HpAbove:
                    return "buff_rule_tooltip_hpabove";
                case BuffRule.Afflicted:
                    return "buff_rule_tooltip_afflicted";
                case BuffRule.Virtued:
                    return "buff_rule_tooltip_virtued";
                case BuffRule.Melee:
                    return "buff_rule_tooltip_meleeonly";
                case BuffRule.Ranged:
                    return "buff_rule_tooltip_rangedonly";
                case BuffRule.FirstRound:
                    return "buff_rule_tooltip_firstroundonly";
                case BuffRule.Status:
                    return "buff_rule_tooltip_actorStatus";
                case BuffRule.EnemyType:
                    return "buff_rule_tooltip_monsterType";
                case BuffRule.DeathsDoor:
                    return "buff_rule_tooltip_at_deaths_door";
                case BuffRule.InRank:
                    return "buff_rule_tooltip_in_rank";
                case BuffRule.InCamp:
                    return "buff_rule_tooltip_in_camp";
                case BuffRule.InDungeon:
                    return "buff_rule_tooltip_in_dungeon";
                case BuffRule.InMode:
                    return "buff_rule_tooltip_in_mode_false";
                case BuffRule.StressAbove:
                    return "buff_rule_tooltip_stress_above";
                case BuffRule.StressBelow:
                    return "buff_rule_tooltip_stress_below";
                case BuffRule.WalkBack:
                    return "buff_rule_tooltip_walking_backwards";
                case BuffRule.InActivity:
                    return "buff_rule_tooltip_in_activity";
                case BuffRule.InCorridor:
                    return "buff_rule_tooltip_in_corridor";
                case BuffRule.Riposting:
                    return "buff_rule_tooltip_riposte";
                case BuffRule.Skill:
                    return "buff_rule_tooltip_skill";
                default:
                    Debug.LogError("Unknown rule type: " + buffRule);
                    return "buff_rule_tooltip_always";
            }
        }
    }
}