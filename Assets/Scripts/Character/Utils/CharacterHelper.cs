using UnityEngine;

public static class CharacterHelper
{
    public static AttributeType StringToAttributeType(string typeString)
    {
        switch (typeString)
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
        switch (categoryString)
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
        switch (category)
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
        switch (type)
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
        switch (monsterType)
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
        switch (buffType)
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
        switch (buffType)
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
        switch (buffRule)
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
        switch (overstressString)
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
        switch (actString)
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
    public static Rarity StringToRarity(string rarityString)
    {
        switch(rarityString)
        {
            case "darkest_dungeon":
                return Rarity.DarkestDungeon;
            case "ancestral_shambler":
                return Rarity.AncestralShambler;
            case "ancestral":
                return Rarity.Ancestral;
            case "collector":
                return Rarity.Collector;
            case "madman":
                return Rarity.Madman;
            case "very_rare":
                return Rarity.VeryRare;
            case "rare":
                return Rarity.Rare;
            case "uncommon":
                return Rarity.Uncommon;
            case "common":
                return Rarity.Common;
            case "very_common":
                return Rarity.VeryCommon;
            case "trophy":
                return Rarity.Trophy;
            case "kickstarter":
                return Rarity.KickStarter;
            default:
                Debug.LogError("Unknown rarity: " + rarityString);
                return Rarity.Common;
        }
    }
}