using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AttributeType
{
    Undefined, HitPoints, Stress, HpHealAmount, HpHealPercent, DmgReceivedPercent, HpHealReceivedPercent,
    StressDmgReceivedPercent, StressDmgPercent, StressHealPercent, StressHealReceivedPercent,
    ResolveCheckPercent, ResolveXpPercent, StunChance, PoisonChance, BleedChance, MoveChance,
    DebuffChance, ScoutingChance, PartySurpriseChance, MonsterSurpirseChance, RemoveQuirkChance,
    FoodConsumption, StarvingDamagePercent, DefenseRating, ProtectionRating, SpeedRating,
    AttackRating, CritChance, DamageLow, DamageHigh, ArmorDiscount, WeaponDiscount, Stun,
    Poison, Disease, DeathBlow, Move, Bleed, Debuff, Trap,
}

public enum AttributeCategory
{
    Undefined, CombatStat, Modifier, Discount, Resistance
}

public static class CharacterHelper
{
    public static AttributeType StringToAttributeType(string typeString)
    {
        switch(typeString)
        {
            case "max_hp": return AttributeType.HitPoints;
            case "stress": return AttributeType.Stress;
            case "defense_rating": return AttributeType.DefenseRating;
            case "protection_rating": return AttributeType.ProtectionRating;
            case "speed_rating": return AttributeType.SpeedRating;
            case "attack_rating": return AttributeType.AttackRating;
            case "crit_chance": return AttributeType.CritChance;
            case "damage_low": return AttributeType.DamageLow;
            case "damage_high": return AttributeType.DamageHigh;

            case "armour": return AttributeType.ArmorDiscount;
            case "weapon": return AttributeType.WeaponDiscount;

            case "stun": return AttributeType.Stun;
            case "poison": return AttributeType.Poison;
            case "disease": return AttributeType.Disease;
            case "death_blow": return AttributeType.DeathBlow;
            case "move": return AttributeType.Move;
            case "bleed": return AttributeType.Bleed;
            case "debuff": return AttributeType.Debuff;
            case "trap": return AttributeType.Trap;

            case "hp_heal_amount": return AttributeType.HpHealAmount;
            case "hp_heal_percent": return AttributeType.HpHealPercent;
            case "stress_dmg_percent": return AttributeType.StressDmgPercent;
            case "stress_heal_percent": return AttributeType.StressHealPercent;
            case "damage_received_percent": return AttributeType.DmgReceivedPercent;
            case "hp_heal_received_percent": return AttributeType.HpHealReceivedPercent;
            case "stress_dmg_received_percent": return AttributeType.StressDmgReceivedPercent;
            case "stress_heal_received_percent": return AttributeType.StressHealReceivedPercent;
            case "stun_chance": return AttributeType.StunChance;
            case "poison_chance": return AttributeType.PoisonChance;
            case "bleed_chance": return AttributeType.BleedChance;
            case "move_chance": return AttributeType.MoveChance;
            case "debuff_chance": return AttributeType.DebuffChance;
            case "scouting_chance": return AttributeType.ScoutingChance;
            case "party_surprise_chance": return AttributeType.PartySurpriseChance;
            case "monsters_surprise_chance": return AttributeType.MonsterSurpirseChance;
            case "remove_negative_quirk_chance": return AttributeType.RemoveQuirkChance;
            case "food_consumption_percent": return AttributeType.FoodConsumption;
            case "starving_damage_percent": return AttributeType.StarvingDamagePercent;
            case "resolve_check_percent": return AttributeType.ResolveCheckPercent;
            case "resolve_xp_bonus_percent": return AttributeType.ResolveXpPercent;

            default:
                Debug.LogError("Undefined character attribute type: " + typeString);
                return AttributeType.Undefined;
        }
    }
    public static string AttributeTypeToString(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.HitPoints:
                return "max_hp";
            case AttributeType.Stress:
                return "stress";
            case AttributeType.DefenseRating:
                return "defense_rating";
            case AttributeType.ProtectionRating:
                return "protection_rating";
            case AttributeType.SpeedRating:
                return "speed_rating";
            case AttributeType.AttackRating:
                return "attack_rating";
            case AttributeType.CritChance:
                return "crit_chance";
            case AttributeType.DamageLow:
                return "damage_low";
            case AttributeType.DamageHigh:
                return "damage_high";
            case AttributeType.ArmorDiscount:
                return "armour";
            case AttributeType.WeaponDiscount:
                return "weapon";
            case AttributeType.Stun:
                return "stun";
            case AttributeType.Poison:
                return "poison";
            case AttributeType.Disease:
                return "disease";
            case AttributeType.DeathBlow:
                return "death_blow";
            case AttributeType.Move:
                return "move";
            case AttributeType.Bleed:
                return "bleed";
            case AttributeType.Debuff:
                return "debuff";
            case AttributeType.Trap:
                return "trap";
            case AttributeType.HpHealAmount:
                return "hp_heal_amount";
            case AttributeType.HpHealPercent:
                return "hp_heal_percent";
            case AttributeType.StressDmgPercent:
                return "stress_dmg_percent";
            case AttributeType.StressHealPercent:
                return "stress_heal_percent";
            case AttributeType.DmgReceivedPercent:
                return "damage_received_percent";
            case AttributeType.HpHealReceivedPercent:
                return "hp_heal_received_percent";
            case AttributeType.StressDmgReceivedPercent:
                return "stress_dmg_received_percent";
            case AttributeType.StressHealReceivedPercent:
                return "stress_heal_received_percent";
            case AttributeType.StunChance:
                return "stun_chance";
            case AttributeType.PoisonChance:
                return "poison_chance";
            case AttributeType.BleedChance:
                return "bleed_chance";
            case AttributeType.MoveChance:
                return "move_chance";
            case AttributeType.DebuffChance:
                return "debuff_chance";
            case AttributeType.ScoutingChance:
                return "scouting_chance";
            case AttributeType.PartySurpriseChance:
                return "party_surprise_chance";
            case AttributeType.MonsterSurpirseChance:
                return "monsters_surprise_chance";
            case AttributeType.RemoveQuirkChance:
                return "remove_negative_quirk_chance";
            case AttributeType.FoodConsumption:
                return "food_consumption_percent";
            case AttributeType.StarvingDamagePercent:
                return "starving_damage_percent";
            case AttributeType.ResolveCheckPercent:
                return "resolve_check_percent";
            case AttributeType.ResolveXpPercent:
                return "resolve_xp_bonus_percent";
            default:
                return "";
        }
    }
    public static AttributeCategory StringToAttributeCategory(string categoryString)
    {
        switch(categoryString)
        {
            case "combat_stat": return AttributeCategory.CombatStat;
            case "modifier": return AttributeCategory.Modifier;
            case "upgrade_discount": return AttributeCategory.Discount;
            case "resistance": return AttributeCategory.Resistance;
            default:
                Debug.LogError("Undefined character attribute category: " + categoryString);
                return AttributeCategory.Undefined;
        }
    }
    public static string AttributeCategoryToString(AttributeCategory category)
    {
        switch(category)
        {
            case AttributeCategory.CombatStat:
                return "combat_stat";
            case AttributeCategory.Modifier:
                return "modifier";
            case AttributeCategory.Discount:
                return "upgrade_discount";
            case AttributeCategory.Resistance:
                return "resistance";
            default:
                return "";
        }
    }
    public static AttributeCategory GetAttributeCategory(AttributeType type)
    {
        switch(type)
        {
            case AttributeType.HitPoints:
            case AttributeType.Stress:
            case AttributeType.DefenseRating:
            case AttributeType.ProtectionRating:
            case AttributeType.SpeedRating:
            case AttributeType.AttackRating:
            case AttributeType.CritChance:
            case AttributeType.DamageLow:
            case AttributeType.DamageHigh:
                return AttributeCategory.CombatStat;
            case AttributeType.ArmorDiscount:
            case AttributeType.WeaponDiscount:
                return AttributeCategory.Discount;
            case AttributeType.Stun:
            case AttributeType.Poison:
            case AttributeType.Disease:
            case AttributeType.DeathBlow:
            case AttributeType.Move:
            case AttributeType.Bleed:
            case AttributeType.Debuff:
            case AttributeType.Trap:
                return AttributeCategory.Resistance;
            case AttributeType.HpHealAmount:
            case AttributeType.HpHealPercent:
            case AttributeType.StressDmgPercent:
            case AttributeType.StressHealPercent:
            case AttributeType.DmgReceivedPercent:
            case AttributeType.HpHealReceivedPercent:
            case AttributeType.StressDmgReceivedPercent:
            case AttributeType.StressHealReceivedPercent:
            case AttributeType.StunChance:
            case AttributeType.PoisonChance:
            case AttributeType.BleedChance:
            case AttributeType.MoveChance:
            case AttributeType.DebuffChance:
            case AttributeType.ScoutingChance:
            case AttributeType.PartySurpriseChance:
            case AttributeType.MonsterSurpirseChance:
            case AttributeType.RemoveQuirkChance:
            case AttributeType.FoodConsumption:
            case AttributeType.StarvingDamagePercent:
            case AttributeType.ResolveCheckPercent:
            case AttributeType.ResolveXpPercent:
                return AttributeCategory.Modifier;
            default:
                return AttributeCategory.Undefined;
        }
    }
    public static MonsterType StringToMonsterType(string monsterType)
    {
        switch(monsterType)
        {
            case "corpse":
                return MonsterType.Corpse;
            case "man":
                return MonsterType.Man;
            case "unholy":
                return MonsterType.Unholy;
            case "eldritch":
                return MonsterType.Eldritch;
            case "beast":
                return MonsterType.Beast;
            case "ironwork":
                return MonsterType.Ironwork;
            case "carpentry":
                return MonsterType.Carpentry;
            case "cauldron":
                return MonsterType.Cauldron;
            case "cosmic":
                return MonsterType.Cosmic;
            default:
                Debug.LogError("Unknown monster type: " + monsterType);
                return MonsterType.Unknown;
        }
    }
    public static BuffType StringToBuffType(string buffType)
    {
        switch(buffType)
        {
            case "combat_stat_add":
                return BuffType.StatAdd;
            case "combat_stat_multiply":
                return BuffType.StatMultiply;
            default:
                Debug.LogError("Unknown buff type: " + buffType);
                return BuffType.StatAdd;
        }
    }
    public static string BuffTypeToString(BuffType buffType)
    {
        switch(buffType)
        {
            case BuffType.StatAdd:
                return "combat_stat_add";
            case BuffType.StatMultiply:
                return "combat_stat_multiply";
            default:
                return "";
        }
    }
    public static StatusType StringToStatusType(string statusType)
    {
        switch (statusType)
        {
            case "tagged":
                return StatusType.Marked;
            case "poisoned":
                return StatusType.Poison;
            case "bleeding":
                return StatusType.Bleeding;
            case "stunned":
                return StatusType.Stun;
            default:
                Debug.LogError("Unknown key status in effect: " + statusType);
                return StatusType.None;
        }
    }
    public static BuffRule StringToBuffRule(string buffRule)
    {
        switch(buffRule)
        {
            case "always":
                return BuffRule.Always;
            case "monsterSize":
                return BuffRule.Size;
            case "lightbelow":
                return BuffRule.LightBelow;
            case "lightabove":
                return BuffRule.LightAbove;
            case "hpbelow":
                return BuffRule.HpBelow;
            case "hpabove":
                return BuffRule.HpAbove;
            case "afflicted":
                return BuffRule.Afflicted;
            case "virtued":
                return BuffRule.Virtued;
            case "meleeonly":
                return BuffRule.Melee;
            case "rangedonly":
                return BuffRule.Ranged;
            case "firstroundonly":
                return BuffRule.FirstRound;
            case "actorStatus":
                return BuffRule.Status;
            case "monsterType":
                return BuffRule.EnemyType;
            case "at_deaths_door":
                return BuffRule.DeathsDoor;
            case "in_rank":
                return BuffRule.InRank;
            case "in_camp":
                return BuffRule.InCamp;
            case "in_mode":
                return BuffRule.InMode;
            case "in_dungeon":
                return BuffRule.InDungeon;
            case "stress_above":
                return BuffRule.StressAbove;
            case "stress_below":
                return BuffRule.StressBelow;
            case "walking_backwards":
                return BuffRule.WalkBack;
            case "in_activity":
                return BuffRule.InActivity;
            case "in_corridor":
                return BuffRule.InCorridor;
            case "riposte":
                return BuffRule.Riposting;
            case "skill":
                return BuffRule.Skill;
            default:
                Debug.LogError("Unknown rule type: " + buffRule);
                return BuffRule.Always;
        }
    }
    public static string BuffRuleToString(BuffRule buffRule)
    {
        switch (buffRule)
        {
            case BuffRule.Always:
                return "always";
            case BuffRule.Size:
                return "monsterSize";
            case BuffRule.LightBelow:
                return "lightbelow";
            case BuffRule.LightAbove:
                return "lightabove";
            case BuffRule.HpBelow:
                return "hpbelow";
            case BuffRule.HpAbove:
                return "hpabove";
            case BuffRule.Afflicted:
                return "afflicted";
            case BuffRule.Virtued:
                return "virtued";
            case BuffRule.Melee:
                return "meleeonly";
            case BuffRule.Ranged:
                return "rangedonly";
            case BuffRule.FirstRound:
                return "firstroundonly";
            case BuffRule.Status:
                return "actorStatus";
            case BuffRule.EnemyType:
                return "monsterType";
            case BuffRule.DeathsDoor:
                return "at_deaths_door";
            case BuffRule.InRank:
                return "in_rank";
            case BuffRule.InCamp:
                return "in_camp";
            case BuffRule.InMode:
                return "in_mode";
            case BuffRule.InDungeon:
                return "in_dungeon";
            case BuffRule.StressAbove:
                return "stress_above";
            case BuffRule.StressBelow:
                return "stress_below";
            case BuffRule.WalkBack:
                return "walking_backwards";
            case BuffRule.InActivity:
                return "in_activity";
            case BuffRule.InCorridor:
                return "in_corridor";
            case BuffRule.Riposting:
                return "riposte";
            case BuffRule.Skill:
                return "skill";
            default:
                Debug.LogError("Unknown rule type: " + buffRule);
                return "";
        }
    }
    public static OverstressType StringToOverstress(string overstressString)
    {
        switch(overstressString)
        {
            case "affliction":
                return OverstressType.Affliction;
            case "virtue":
                return OverstressType.Virtue;
            default:
                Debug.LogError("Unknown oversterss type " + overstressString);
                return OverstressType.Affliction;
        }
    }
    public static string OverstressToString(OverstressType overstress)
    {
        switch (overstress)
        {
            case OverstressType.Affliction:
                return "affliction";
            case OverstressType.Virtue:
                return "virtue";
            default:
                return "affliction";
        }
    }
    public static StartTurnActType StringToStartTurnAct(string actString)
    {
        switch(actString)
        {
            case "nothing":
                return StartTurnActType.Nothing;
            case "bark_stress":
                return StartTurnActType.BarkStress;
            case "change_pos":
                return StartTurnActType.ChangePosition;
            case "ignore_command":
                return StartTurnActType.IgnoreCommand;
            case "random_command":
                return StartTurnActType.RandomCommand;
            case "retreat_from_combat":
                return StartTurnActType.RetreatFromCombat;
            case "attack_friendly":
                return StartTurnActType.AttackFriendly;
            case "attack_self":
                return StartTurnActType.AttackSelf;
            case "mark_self":
                return StartTurnActType.MarkSelf;
            case "stress_heal_self":
                return StartTurnActType.StressHealSelf;
            case "stress_heal_party":
                return StartTurnActType.StressHealParty;
            case "buff_random_party_member":
                return StartTurnActType.BuffAlly;
            case "buff_party":
                return StartTurnActType.BuffParty;
            case "heal_self":
                return StartTurnActType.HealSelf;
            default:
                Debug.LogError("Unknown start turn act type " + actString);
                return StartTurnActType.Nothing;
        }
    }
    public static string StartTurnActToString(StartTurnActType actType)
    {
        switch (actType)
        {
            case StartTurnActType.Nothing:
                return "nothing";
            case StartTurnActType.BarkStress:
                return "bark_stress";
            case StartTurnActType.ChangePosition:
                return "change_pos";
            case StartTurnActType.IgnoreCommand:
                return "ignore_command";
            case StartTurnActType.RandomCommand:
                return "random_command";
            case StartTurnActType.RetreatFromCombat:
                return "retreat_from_combat";
            case StartTurnActType.AttackFriendly:
                return "attack_friendly";
            case StartTurnActType.AttackSelf:
                return "attack_self";
            case StartTurnActType.MarkSelf:
                return "mark_self";
            case StartTurnActType.StressHealSelf:
                return "stress_heal_self";
            case StartTurnActType.StressHealParty:
                return "stress_heal_party";
            case StartTurnActType.BuffAlly:
                return "buff_random_party_member";
            case StartTurnActType.BuffParty:
                return "buff_party";
            case StartTurnActType.HealSelf:
                return "heal_self";
            default:
                return "nothing";
        }
    }
    public static ReactionType StringToReactionType(string actString)
    {
        switch (actString)
        {
            case "block_move":
                return ReactionType.BlockMove;
            case "block_heal":
                return ReactionType.BlockHeal;
            case "block_buff":
                return ReactionType.BlockBuff;
            case "block_item":
                return ReactionType.BlockItem;
            case "block_combat_retreat":
                return ReactionType.BlockRetreat;
            case "comment_self_hit":
                return ReactionType.CommentSelfHit;
            case "comment_self_missed":
                return ReactionType.CommentSelfMissed;
            case "comment_ally_hit":
                return ReactionType.CommentAllyHit;
            case "comment_ally_missed":
                return ReactionType.CommentAllyMissed;
            case "comment_ally_attack_hit":
                return ReactionType.CommentAllyAttackHit;
            case "comment_ally_attack_missed":
                return ReactionType.CommentAllyAttackMiss;
            case "comment_move":
                return ReactionType.CommentMove;
            case "comment_curio_interaction":
                return ReactionType.CommentCurioInteraction;
            case "comment_trap_triggered":
                return ReactionType.CommentTrapTriggered;
            case "block_effect":
                return ReactionType.BlockEffect;
            default:
                Debug.LogError("Unknown reaction type " + actString);
                return ReactionType.BlockMove;
        }
    }
    public static string ReactionTypeToString(ReactionType actString)
    {
        switch (actString)
        {
            case ReactionType.BlockMove:
                return "block_move";
            case ReactionType.BlockHeal:
                return "block_heal";
            case ReactionType.BlockBuff:
                return "block_buff";
            case ReactionType.BlockItem:
                return "block_item";
            case ReactionType.BlockRetreat:
                return "block_combat_retreat";
            case ReactionType.CommentSelfHit:
                return "comment_self_hit";
            case ReactionType.CommentSelfMissed:
                return "comment_self_missed";
            case ReactionType.CommentAllyHit:
                return "comment_ally_hit";
            case ReactionType.CommentAllyMissed:
                return "comment_ally_missed";
            case ReactionType.CommentAllyAttackHit:
                return "comment_ally_attack_hit";
            case ReactionType.CommentAllyAttackMiss:
                return "comment_ally_attack_missed";
            case ReactionType.CommentMove:
                return "comment_move";
            case ReactionType.CommentCurioInteraction:
                return "comment_curio_interaction";
            case ReactionType.CommentTrapTriggered:
                return "comment_trap_triggered";
            case ReactionType.BlockEffect:
                return "block_effect";
            default:
                return "block_move";
        }
    }
}

public static class LocalizationHelper
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
        if(isFalse)
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

public class Character
{
    protected List<BuffInfo> buffInfo;
    protected Dictionary<StatusType, StatusEffect> statusEffects;
    protected Dictionary<AttributeType, SingleAttribute> singleAttributes;
    protected Dictionary<AttributeType, PairedAttribute> pairedAttributes;

    #region Basic Stats
    private static AttributeType[] SingleStats = new AttributeType[]
    {
        AttributeType.DefenseRating, AttributeType.ProtectionRating, AttributeType.SpeedRating,
        AttributeType.AttackRating, AttributeType.CritChance, AttributeType.DamageLow, AttributeType.DamageHigh,
    };
    #endregion
    #region Modifiers
    private static AttributeType[] Modifiers = new AttributeType[]
    {
        AttributeType.HpHealAmount, AttributeType.HpHealPercent, AttributeType.MoveChance, AttributeType.DebuffChance,
        AttributeType.StressHealPercent, AttributeType.DmgReceivedPercent, AttributeType.HpHealReceivedPercent,
        AttributeType.StressDmgReceivedPercent, AttributeType.StressHealReceivedPercent, AttributeType.StunChance,
        AttributeType.PoisonChance, AttributeType.BleedChance, AttributeType.ResolveCheckPercent, AttributeType.StressDmgPercent, 
        AttributeType.ScoutingChance, AttributeType.PartySurpriseChance, AttributeType.MonsterSurpirseChance,
        AttributeType.RemoveQuirkChance, AttributeType.FoodConsumption, AttributeType.StarvingDamagePercent,
    };
    #endregion
    #region Hero Discounts
    private static AttributeType[] HeroDiscounts = new AttributeType[]
    {
        AttributeType.ArmorDiscount,
        AttributeType.WeaponDiscount,
    };
    #endregion
    #region Hero Resistances
    private static AttributeType[] HeroResistances = new AttributeType[]
    {
        AttributeType.Stun, AttributeType.Poison, AttributeType.Disease,
        AttributeType.DeathBlow, AttributeType.Move, AttributeType.Bleed,
        AttributeType.Debuff, AttributeType.Trap,
    };
    #endregion
    #region Monster Resistances
    private static AttributeType[] MonsterResistances = new AttributeType[]
    {
        AttributeType.Stun, AttributeType.Poison, AttributeType.Move,
        AttributeType.Bleed, AttributeType.Debuff,
    };
    #endregion

    public virtual List<SkillArtInfo> SkillArtInfo
    {
        get
        {
            return new List<SkillArtInfo>();
        }
    }
    public virtual CombatSkill RiposteSkill
    {
        get
        {
            return null;
        }
    }
    public virtual int Size
    {
        get
        {
            return 1;
        }
    }
    public virtual string Name
    {
        get
        {
            return "Character";
        }
    }
    public virtual string Class
    {
        get
        {
            return "Class";
        }
    }
    public virtual bool AtDeathsDoor
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsStressed
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsOverstressed
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsVirtued
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsAfflicted
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsMonster
    {
        get
        {
            return false;
        }
    }
    public virtual int RenderRankOverride
    {
        get
        {
            return 0;
        }
    }
    public virtual bool InMode
    {
        get
        {
            return false;
        }
    }
    public virtual CharacterMode Mode
    {
        get
        {
            return null;
        }
    }
    public virtual Trait Trait
    {
        get
        {
            return null;
        }
        protected set
        {

        }
    }

    public virtual CommonEffects CommonEffects
    {
        get
        {
            return null;
        }
    }
    public virtual Initiative Initiative
    {
        get
        {
            return null;
        }
    }
    public virtual DisplayModifier DisplayModifier
    {
        get
        {
            return null;
        }
    }
    public virtual TorchlightModifier TorchlightModifier
    {
        get
        {
            return null;
        }
    }
    public virtual HealthbarModifier HealthbarModifier
    { 
        get
        {
            return null;
        }
    }
    public virtual DeathClass DeathClass
    { 
        get
        {
            return null;
        }
    }
    public virtual DeathDamage DeathDamage
    {
        get
        {
            return null;
        }
    }
    public virtual BattleModifier BattleModifiers
    {
        get
        {
            return null;
        }
    }
    public virtual Companion Companion
    {
        get
        {
            return null;
        }
    }
    public virtual EmptyCaptor EmptyCaptor
    {
        get
        {
            return null;
        }
    }
    public virtual FullCaptor FullCaptor
    {
        get
        {
            return null;
        }
    }
    public virtual Controller ControllerCaptor
    {
        get
        {
            return null;
        }
    }
    public virtual LifeTime LifeTime
    {
        get
        {
            return null;
        }
    }
    public virtual LifeLink LifeLink
    {
        get
        {
            return null;
        }
    }
    public virtual SharedHealth SharedHealth
    {
        get
        {
            return null;
        }
    }
    public virtual Shapeshifter Shapeshifter
    {
        get
        {
            return null;
        }
    }
    public virtual Spawn Spawn
    {
        get
        {
            return null;
        }
    }
    public virtual SkillReaction SkillReaction
    {
        get
        {
            return null;
        }
    }
    public virtual List<MonsterType> MonsterTypes
    {
        get
        {
            return null;
        }
    }
    public virtual List<LootDefinition> Loot
    {
        get
        {
            return null;
        }
    }

    public float FoodConsumption
    {
        get
        {
            var food = GetSingleAttribute(AttributeType.FoodConsumption);
            if (food != null)
                return Mathf.Clamp(food.ModifiedValue, -1f, float.MaxValue);
            else
                return 0;
        }
    }
    public float Speed
    {
        get
        {
            var speed = GetSingleAttribute(AttributeType.SpeedRating);
            if (speed != null)
                return Mathf.Clamp(speed.ModifiedValue, 0, float.MaxValue);
            else
                return 0;
        }
    }
    public float Crit
    {
        get
        {
            var crit = GetSingleAttribute(AttributeType.CritChance);
            if (crit != null)
                return Mathf.Clamp(crit.ModifiedValue, 0, 1);
            else
                return 0;
        }
    }
    public float Accuracy
    {
        get
        {
            var acc = GetSingleAttribute(AttributeType.AttackRating);
            if (acc != null)
                return Mathf.Clamp(acc.ModifiedValue, -1, 2);
            else
                return 0;
        }
    }
    public float Dodge
    {
        get
        {
            var dodge = GetSingleAttribute(AttributeType.DefenseRating);
            if (dodge != null)
                return Mathf.Clamp(dodge.ModifiedValue, 0, Mathf.Max(3, dodge.RawValue));
            else
                return 0;
        }
    }
    public float Protection
    {
        get
        {
            var prot = GetSingleAttribute(AttributeType.ProtectionRating);
            if (prot != null)
                return Mathf.Clamp(prot.ModifiedValue, 0, Mathf.Max(0.85f, prot.RawValue));
            else
                return 0;
        }
    }
    public float MinDamage
    {
        get
        {
            var dmgLow = GetSingleAttribute(AttributeType.DamageLow);
            if (dmgLow != null)
                return Mathf.Clamp(dmgLow.ModifiedValue, 0, float.MaxValue);
            else
                return 0;
        }
    }
    public float MaxDamage
    {
        get
        {
            var dmgHigh = GetSingleAttribute(AttributeType.DamageHigh);
            if (dmgHigh != null)
                return Mathf.Clamp(dmgHigh.ModifiedValue, MinDamage, float.MaxValue);
            else
                return 0;
        }
    }
    public float DamageMod
    {
        get
        {
            var dmg = GetSingleAttribute(AttributeType.DamageHigh);
            if (dmg != null)
                return Mathf.Clamp(dmg.Multiplier, -0.95f, float.MaxValue);
            else
                return 1;
        }
    }

    public static void InitializeBasicStatuses(Dictionary<StatusType, StatusEffect> targetDictionary)
    {
        targetDictionary.Clear();

        targetDictionary.Add(StatusType.Stun, new StunStatusEffect());
        targetDictionary.Add(StatusType.Marked, new MarkStatusEffect());
        targetDictionary.Add(StatusType.Riposte, new RiposteStatusEffect());
        targetDictionary.Add(StatusType.Bleeding, new BleedingStatusEffect());
        targetDictionary.Add(StatusType.Poison, new PoisonStatusEffect());
        targetDictionary.Add(StatusType.Guard, new GuardStatusEffect());
        targetDictionary.Add(StatusType.Guarded, new GuardedStatusEffect());
        targetDictionary.Add(StatusType.DeathsDoor, new DeathsDoorStatusEffect());
        targetDictionary.Add(StatusType.DeathRecovery, new DeathRecoveryStatusEffect());
    }

    protected void AddSingleAttribute(AttributeType stat, SingleAttribute attribute)
    {
        singleAttributes.Add(stat, attribute);
    }
    protected void AddPairedAttribute(AttributeType stat, PairedAttribute attribute)
    {
        pairedAttributes.Add(stat, attribute);
    }

    public Character(HeroClass heroClass, int level)
    {
        buffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute(AttributeCategory.Discount));

        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                AddSingleAttribute(HeroResistances[i], new SingleAttribute(heroClass.Resistanses[HeroResistances[i]], AttributeCategory.Resistance));
            else
                AddSingleAttribute(HeroResistances[i], new SingleAttribute(heroClass.Resistanses[HeroResistances[i]]
                    + level * 0.1f, AttributeCategory.Resistance));
    }
    public Character(HeroClass heroClass)
    {
        buffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute(AttributeCategory.Discount));

        for (int i = 0; i < HeroResistances.Length; i++)
            AddSingleAttribute(HeroResistances[i], new SingleAttribute(heroClass.Resistanses[HeroResistances[i]], AttributeCategory.Resistance));
    }
    public Character(SaveHeroData saveHeroData)
    {
        buffInfo = saveHeroData.buffs;
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        HeroClass heroClass = DarkestDungeonManager.Data.HeroClasses[saveHeroData.heroClass];

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute(AttributeCategory.Discount));

        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                AddSingleAttribute(HeroResistances[i], new SingleAttribute(heroClass.Resistanses[HeroResistances[i]], AttributeCategory.Resistance));
            else
                AddSingleAttribute(HeroResistances[i], new SingleAttribute(heroClass.Resistanses[HeroResistances[i]]
                    + saveHeroData.resolveLevel * 0.1f, AttributeCategory.Resistance));
    }
    public Character(MonsterData monsterData)
    {
        buffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(monsterData.Attributes[AttributeType.HitPoints],
            monsterData.Attributes[AttributeType.HitPoints], true, AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            if(monsterData.Attributes.ContainsKey(SingleStats[i]))
                AddSingleAttribute(SingleStats[i], new SingleAttribute(monsterData.Attributes[SingleStats[i]], AttributeCategory.CombatStat));
            else
                AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < MonsterResistances.Length; i++)
            AddSingleAttribute(MonsterResistances[i], new SingleAttribute(monsterData.Attributes[MonsterResistances[i]], AttributeCategory.Resistance));
    }
    public Character(FormationUnitSaveData unitSaveData, MonsterData monsterData)
    {
        buffInfo = unitSaveData.Buffs;
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = unitSaveData.Statuses;

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(unitSaveData.CurrentHp,
            monsterData.Attributes[AttributeType.HitPoints], true, AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            if (monsterData.Attributes.ContainsKey(SingleStats[i]))
                AddSingleAttribute(SingleStats[i], new SingleAttribute(monsterData.Attributes[SingleStats[i]], AttributeCategory.CombatStat));
            else
                AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < MonsterResistances.Length; i++)
            AddSingleAttribute(MonsterResistances[i], new SingleAttribute(monsterData.Attributes[MonsterResistances[i]], AttributeCategory.Resistance));
    }

    protected void UpdateResolve(int level, HeroClass heroClass)
    {
        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                GetSingleAttribute(HeroResistances[i]).RawValue = heroClass.Resistanses[HeroResistances[i]];
            else
                GetSingleAttribute(HeroResistances[i]).RawValue = heroClass.Resistanses[HeroResistances[i]] + level * 0.1f;
    }

    public void LoadStatusEffects(Dictionary<StatusType, StatusEffect> newStatusEffects)
    {
        statusEffects = newStatusEffects;
    }
    public void UpdateDurations(BuffDurationType durationType)
    {
        foreach (var buffEntry in buffInfo.FindAll(roundBuff => roundBuff.DurationType == durationType))
            if (--buffEntry.Duration <= 0)
                RemoveBuff(buffEntry);
    }
    public void UpdateRound()
    {
        foreach (var effect in statusEffects)
            effect.Value.UpdateNextTurn();

        UpdateDurations(BuffDurationType.Round);
    }

    public PairedAttribute Health
    {
        get
        {
            return GetPairedAttribute(AttributeType.HitPoints);
        }
    }
    public PairedAttribute Stress
    {
        get
        {
            return GetPairedAttribute(AttributeType.Stress);
        }
    }

    public bool HasBuffs()
    {
        return buffInfo.Find(info => info.SourceType == BuffSourceType.Adventure && info.Buff.IsPositive()) != null;
    }
    public bool HasDebuffs()
    {
        return buffInfo.Find(info => info.SourceType == BuffSourceType.Adventure && !info.Buff.IsPositive()) != null;
    }
    public void ApplyStunRecovery()
    {
        var recoveryBuff = DarkestDungeonManager.Data.Buffs["STUNRECOVERYBUFF"];
        int recoveryStackCount = 0;

        for(int i = 0; i < buffInfo.Count; i++)
        {
            if (buffInfo[i].Buff == recoveryBuff)
                recoveryStackCount++;
        }

        recoveryStackCount++;

        for(int i = 0; i < recoveryStackCount; i++)
            AddBuff(new BuffInfo(recoveryBuff, BuffDurationType.Round, BuffSourceType.Adventure, 2));
    }
    public void RemoveConditionalBuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
        {
            if (buffInfo[i].SourceType == BuffSourceType.Condition)
                RemoveBuff(buffInfo[i]);
        }
    }
    public void RemoveCampingBuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
        {
            if (buffInfo[i].DurationType == BuffDurationType.Camp)
                RemoveBuff(buffInfo[i]);
        }
    }
    public void RemoveLightBuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
        {
            if (buffInfo[i].SourceType == BuffSourceType.Light)
                RemoveBuff(buffInfo[i]);
        }
    }

    public string DeathsDoorBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.DeathsDoor
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach(var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }
    public string MortalityBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Mortality
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }
    public string TraitBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Trait
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }
    public string CombatBuffTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Combat && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_duration_combat_end_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Camp && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {

            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_until_camp_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Round && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_duration_round_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Raid && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_until_end_of_raid_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }
        return toolTip.TrimStart('\n');
    }
    public string CombatDebuffTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Combat && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_duration_combat_end_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Camp && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_until_camp_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Round && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_duration_round_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Raid && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_until_end_of_raid_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }
        return toolTip.TrimStart('\n');
    }

    protected void ApplyBuff(BuffInfo buffEntry)
    {
        if (buffEntry.IsApplied)
            return;
        buffEntry.IsApplied = true;

        if(buffEntry.Buff.Type == BuffType.StatAdd)
            GetAttribute(buffEntry.Buff.AttributeType).FlatAddition += buffEntry.ModifierValue;
        else if(buffEntry.Buff.Type == BuffType.StatMultiply)
            GetAttribute(buffEntry.Buff.AttributeType).Multiplier += buffEntry.ModifierValue;
    }
    protected void RevertBuff(BuffInfo buffEntry)
    {
        if (!buffEntry.IsApplied)
            return;
        buffEntry.IsApplied = false;

        if (buffEntry.Buff.Type == BuffType.StatAdd)
            GetAttribute(buffEntry.Buff.AttributeType).FlatAddition -= buffEntry.ModifierValue;
        else if (buffEntry.Buff.Type == BuffType.StatMultiply)
            GetAttribute(buffEntry.Buff.AttributeType).Multiplier -= buffEntry.ModifierValue;
    }
    protected void RemoveBuff(BuffInfo buffEntry)
    {
        buffInfo.Remove(buffEntry);
        RevertBuff(buffEntry);
    }
    protected void ApplyBuffRule(BuffInfo buffEntry, RaidRuleInfo raidRuleInfo)
    {
        switch (buffEntry.Buff.RuleType)
        {
            case BuffRule.Afflicted: // done
                #region Afflicted
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.IsAfflicted)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.IsAfflicted)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Always: // done
                #region Always
                if (buffEntry.Buff.IsFalseRule)
                {
                    RevertBuff(buffEntry);
                }
                else
                {
                    ApplyBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.DeathsDoor:  // done
                #region DeathsDoor
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.AtDeathsDoor)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.AtDeathsDoor)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.EnemyType: // done
                #region EnemyType
                if (raidRuleInfo.Target == null || raidRuleInfo.Target.Character.IsMonster == false)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Target.Character.MonsterTypes.Contains(CharacterHelper.StringToMonsterType(buffEntry.Buff.StringParam)))
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Target.Character.MonsterTypes.Contains(CharacterHelper.StringToMonsterType(buffEntry.Buff.StringParam)))
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.FirstRound: // done
                #region FirstRound
                if (raidRuleInfo.BattleGround.BattleStatus != BattleStatus.Fighting)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.BattleGround.Round.RoundNumber == 0)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.BattleGround.Round.RoundNumber == 0)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.HpAbove: // done
                #region HpAbove
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio > buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.HpBelow:  // done
                #region HpAbove
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio < buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InActivity:
                #region InActivity
                RevertBuff(buffEntry);
                break;
                #endregion
            case BuffRule.InCamp:
                #region InCamp
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.IsDoingCamping)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.IsDoingCamping)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InCorridor:  // done
                #region InCorridor
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InDungeon:  // done
                #region InDungeon
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Dungeon == buffEntry.Buff.StringParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Dungeon == buffEntry.Buff.StringParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InMode:  // done
                #region InMode
                if (raidRuleInfo.Unit.Character.InMode == false)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Mode.Id == buffEntry.Buff.StringParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Mode.Id == buffEntry.Buff.StringParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InRank: // done
                #region InRank
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Rank == buffEntry.Buff.SingleParam + 1)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Rank == buffEntry.Buff.SingleParam + 1)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.LightAbove: // done
                #region LightAbove
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount > buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.LightBelow: // done
                #region LightBelow
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount < buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Melee:  // done
                #region Melee
                if (raidRuleInfo.Skill == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Skill.Type == "melee")
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Skill.Type == "melee")
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Ranged:  // done
                #region Ranged
                if (raidRuleInfo.Skill == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Skill.Type == "ranged")
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Skill.Type == "ranged")
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Riposting:  // done
                #region Riposting
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.IsRiposting)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.IsRiposting)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Size:  // done
                #region Size
                if (raidRuleInfo.Target == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Target.Size == buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Target.Size == buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Skill:  // done
                #region Skill
                if (raidRuleInfo.Skill == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Skill.Id == buffEntry.Buff.StringParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Skill.Id == buffEntry.Buff.StringParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Status:  // done
                #region Status
                if (raidRuleInfo.Target == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }
                var targetStatus = CharacterHelper.StringToStatusType(buffEntry.Buff.StringParam);
                if (targetStatus == StatusType.None)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Target.Character[targetStatus].IsApplied)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Target.Character[targetStatus].IsApplied)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.StressAbove:  // done
                #region StressAbove
                if (raidRuleInfo.Unit.Character.IsMonster)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue > buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.StressBelow:  // done
                #region StressBelow
                if (raidRuleInfo.Unit.Character.IsMonster)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue < buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Virtued: // done
                #region Virtued
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.IsVirtued)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.IsVirtued)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.WalkBack: // done
                #region WalkBack
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.IsWalkingBack)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.IsWalkingBack)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            default:
                break;
        }
    }

    public void AddBuff(BuffInfo newBuffInfo)
    {
        buffInfo.Add(newBuffInfo);
        if(newBuffInfo.Buff.RuleType == BuffRule.Always)
            ApplyBuff(newBuffInfo);
    }
    public bool ContainsBuff(Buff buff, BuffSourceType sourceType)
    {
        return buffInfo.Find(item => item.Buff == buff && item.SourceType == sourceType) != null;
    }

    public void RemoveSourceBuff(Buff revertBuff, BuffSourceType sourceType)
    {
        var revertBuffInfo = buffInfo.Find(item => item.Buff == revertBuff && item.SourceType == sourceType);
        if (revertBuffInfo != null)
            RemoveBuff(revertBuffInfo);
    }
    public void RemoveCombatDebuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
            if (buffInfo[i].SourceType == BuffSourceType.Adventure && buffInfo[i].Buff.IsPositive() == false)
                RemoveBuff(buffInfo[i]);
    }

    public void ApplySingleBuffRule(RaidRuleInfo raidRuleInfo, BuffRule rule)
    {
        for (int i = 0; i < buffInfo.Count; i++)
            if(buffInfo[i].Buff.RuleType == rule)
                ApplyBuffRule(buffInfo[i], raidRuleInfo);
    }
    public void ApplyAllBuffRules(RaidRuleInfo raidRuleInfo)
    {
        for (int i = 0; i < buffInfo.Count; i++)
            ApplyBuffRule(buffInfo[i], raidRuleInfo);
    }

    public SingleAttribute GetSingleAttribute(AttributeType stat)
    {
        if (singleAttributes.ContainsKey(stat))
            return singleAttributes[stat];
        else
            return null;
    }
    public PairedAttribute GetPairedAttribute(AttributeType stat)
    {
        if (pairedAttributes.ContainsKey(stat))
            return pairedAttributes[stat];
        else
            return null;
    }
    public BaseAttribute GetAttribute(AttributeType stat)
    {
        if (singleAttributes.ContainsKey(stat))
            return singleAttributes[stat];
        else if (pairedAttributes.ContainsKey(stat))
            return pairedAttributes[stat];
        else
        {
            Debug.Log("Attribute not found: " + stat.ToString());
            return null;
        }
    }
    public StatusEffect GetStatusEffect(StatusType type)
    {
        return statusEffects[type];
    }

    public StatusEffect this[StatusType type]
    {
        get
        {
            if (type != StatusType.None)
                return statusEffects[type];
            else
                return null;
        }
    }
    public SingleAttribute this[AttributeType stat]
    {
        get
        {
            if (singleAttributes.ContainsKey(stat))
                return singleAttributes[stat];
            else
                return null;
        }
    }
    public PairedAttribute this[AttributeType stat, bool paired]
    {
        get
        {
            if (pairedAttributes.ContainsKey(stat))
                return pairedAttributes[stat];
            else
                return null;
        }
        set
        {
            if (pairedAttributes.ContainsKey(stat))
                pairedAttributes[stat] = value;
            else
                pairedAttributes.Add(stat, value);
        }
    }

    public virtual void UpdateSaveData(FormationUnitSaveData saveUnitData)
    {
        saveUnitData.IsHero = false;
        saveUnitData.Class = Class;
        saveUnitData.Name = Name;
        saveUnitData.CurrentHp = Health.CurrentValue;
        saveUnitData.Buffs = buffInfo;
        saveUnitData.Statuses = statusEffects;
    }
}