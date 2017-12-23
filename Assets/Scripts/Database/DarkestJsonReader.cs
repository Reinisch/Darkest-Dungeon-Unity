using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable InconsistentNaming

namespace DarkestJson
{
    #region Heirloom Trading
    public class JsonHeirloomExchange
    {
        public List<JsonHeirLoomMarket> markets;
    }
    public class JsonHeirLoomMarket
    {
        public string id;
        public List<JsonHeirloomExchangeEntry> exchange_rates;
    }
    public class JsonHeirloomExchangeEntry
    {
        public string exchange_from_type;
        public int exchange_from_amount;
        public string exchange_to_type;
        public int exchange_to_amount;
    }
    #endregion

    #region Party Names
    public class JsonPartyNameDictionary
    {
        public List<JsonPartyNameEntry> party_names;
    }
    public class JsonPartyNameEntry
    {
        public string id;
        public List<string> required_hero_class;
    }
    #endregion

    #region Narration
    public class JsonNarration
    {
        public List<string> filters;
        public List<JsonNarrationEntry> entries;
    }

    public class JsonNarrationEntry
    {
        public string id;
        public string tone;
        public float chance;
        public List<JsonNarrationEntryAudioEvent> audio_events;
    }

    public class JsonNarrationEntryAudioEvent
    {
        public bool queue_only_on_empty;
        public bool queue_while_audio_playing;
        public string audio_event;
        public float chance;
        public float priority;
        public int max_raid_occurrences;
        public int max_town_visit_occurrences;
        public int max_campaign_occurrences;
        public string filter;
        public bool check_all_tags;
        public List<string> tags;
    }
    #endregion

    #region Town Events
    public class JsonTownEventDatabase
    {
        public List<JsonTownEventOption> settings;
        public List<JsonTownEvent> events;
        public List<JsonTownGuarantee> quest_type_event_guarantees;
    }
    public class JsonTownGuarantee
    {
        public string dungeon_type;
        public string quest_type;
        public string event_id;
    }
    public class JsonTownEventOption
    {
        public string id;
        public List<float> event_chance_per_town_visits;
    }
    public class JsonTownEvent
    {
        public string id;
        public float base_chance;
        public float per_not_rolled_additional_chance;
        public int cooldown;
        public JsonTownEventRequirements requirements;
        public List<string> town_ambience_paramater_ids;
        public string tone;
        public string sprite;
        public string sprite_attachment;
        public List<JsonTownEventData> data;
    }
    public class JsonTownEventData
    {
        public string type;
        public string string_data;
        public float number_data;
    }
    public class JsonTownEventRequirements
    {
        public int minimum_week;
        public int dead_heroes;
        public List<JsonTownEventHeroCount> hero_level_counts;
        public List<JsonTownEventUpgrade> upgrades_purchased;
    }
    public class JsonTownEventHeroCount
    {
        public int level;
        public int count;
    }
    public class JsonTownEventUpgrade
    {
        public string tree_id;
        public string requrement_code;
    }
    #endregion

    #region Provision
    public class JsonProvision
    {
        public List<List<JsonItem>> raid_starting_length_inventory_item_lists;
        public List<JsonHeroItems> raid_starting_hero_class_item_lists;
        public List<List<JsonItem>> default_store_inventory_item_lists;
    }

    public class JsonHeroItems
    {
        public string hero_class;
        public List<JsonItem> item_lists;
    }

    public class JsonItem
    {
        public string type;
        public string id;
        public int amount;
    }
    #endregion

    #region Traits
    public class JsonTraits
    {
        public List<JsonTrait> traits;
    }
    public class JsonTrait
    {
        public string id;
        public string overstress_type;
        public string curio_tag;
        public float curio_tag_chance;
        public bool keep_loot;
        public List<string> buff_ids;
        public List<JsonCombatStartTurnActOut> combat_start_turn_act_outs;
        public List<JsonReactionActOut> reaction_act_outs;
    }
    public class JsonCombatStartTurnActOut
    {
        public string id;
        public JsonStartTurnData data;
        public int chance;
    }
    public class JsonStartTurnData
    {
        public float number_value;
        public string string_value;
    }
    public class JsonReactionActOut
    {
        public string id;
        public JsonReactionData data;
        public int chance;
    }
    public class JsonReactionData
    {
        public string effect;
    }
    #endregion

    #region Camping Skills
    public class JsonCamping
    {
        public List<JsonCampingSkill> skills;
    }

    public class JsonCampingSkill
    {
        public string id;
        public int level;
        public int cost;
        public int use_limit;

        public List<JsonCampingEffect> effects;
        public List<string> hero_classes;
        public List<JsonCampingUpgradeRequirement> upgrade_requirements;
    }

    public class JsonCampingUpgradeRequirement
    {
        public string code;
        public List<JsonCurrencyCost> currency_cost;
    }

    public class JsonCampingEffect
    {
        public string selection;
        public List<string> requirements;
        public JsonCampingChance chance;
        public string type;
        public string sub_type;
        public float amount;
    }

    public class JsonCampingChance
    {
        public string code;
        public float amount;
    }
    #endregion

    #region Buildings
    public class JsonStageCoach
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public List<JsonSlotUpgrade> number_of_recruits_upgrades;
        public List<JsonSlotUpgrade> roster_size_upgrades;
        public List<JsonRecruitUpgrade> upgraded_recruits_upgrades;
    }

    public class JsonNomadWagon
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public List<JsonSlotUpgrade> number_of_trinkets_upgrades;
        public List<JsonDiscountUpgrade> trinket_cost_discount_upgrades;
    }


    public class JsonCampingTrainer
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public List<JsonDiscountUpgrade> camping_skill_cost_discount_upgrades;
    }

    public class JsonGuild
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public List<JsonDiscountUpgrade> combat_skill_cost_discount_upgrades;
    }

    public class JsonBlacksmith
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public List<JsonDiscountUpgrade> equipment_cost_discount_upgrades;
    }

    public class JsonSanitarium
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public JsonQuirkTreatment treatment;
        public JsonDiseaseTreatment disease_treatment;
    }

    public class JsonTavern
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public JsonActivity bar;
        public JsonActivity gambling;
        public JsonActivity brothel;
    }

    public class JsonAbbey
    {
        public int on_start_town_visit_priority;
        public int number_of_quests_finished;
        public int highest_dungeon_level;

        public JsonActivity meditation;
        public JsonActivity prayer;
        public JsonActivity flagellation;
    }

    public class JsonQuirkTreatment
    {
        public List<JsonCostUpgrade> positive_quirk_cost_upgrades;
        public List<JsonCostUpgrade> negative_quirk_cost_upgrades;
        public List<JsonCostUpgrade> permanent_negative_quirk_cost_upgrades;
        public List<JsonSlotUpgrade> slot_upgrades;

        public float quirk_treatment_chance;
    }

    public class JsonDiseaseTreatment
    {
        public List<JsonCostUpgrade> disease_quirk_cost_upgrades;
        public List<JsonChanceUpgrade> disease_quirk_cure_all_chance_upgrades;
        public List<JsonSlotUpgrade> slot_upgrades;

        public float quirk_treatment_chance;
    }

    public class JsonActivity
    {
        public JsonActivitySideEffects side_effects;
        public List<string> quirk_library_names;
        public bool caretaker_friendly;
        public List<JsonCostUpgrade> cost_upgrades;
        public List<JsonSlotUpgrade> slot_upgrades;
        public List<JsonStressUpgrade> stress_upgrades;
        public List<JsonChanceUpgrade> affliction_cure_upgrades;
    }

    public class JsonActivitySideEffects
    {
        public float chance;
        public List<JsonActivityResult> results;
    }

    public class JsonActivityResult
    {
        public string type;
        public int chance;
        public List<Dictionary<string, object>> data;
    }

    public class JsonCostUpgrade
    {
        public JsonCurrencyCost cost_currency;
        public string upgrade_tree_id;
        public string upgrade_requirement_code;
    }

    public class JsonRecruitUpgrade
    {
        public int level;
        public float chance;
        public int number_of_extra_positive_quirks;
        public int number_of_extra_negative_quirks;
        public int number_of_extra_combat_skills;
        public int number_of_extra_camping_skills;
        public List<int> guaranteed_previous_raid_dead_hero_levels;
        public string upgrade_tree_id;
        public string upgrade_requirement_code;
    }

    public class JsonSlotUpgrade
    {
        public int number_of_slots;
        public string upgrade_tree_id;
        public string upgrade_requirement_code;
    }

    public class JsonStressUpgrade
    {
        public int heal_low;
        public int heal_high;
        public string upgrade_tree_id;
        public string upgrade_requirement_code;
    }

    public class JsonDiscountUpgrade
    {
        public float discount_percent;
        public string upgrade_tree_id;
        public string upgrade_requirement_code;
    }

    public class JsonChanceUpgrade
    {
        public float chance;
        public string upgrade_tree_id;
        public string upgrade_requirement_code;
    }
    #endregion

    #region Updates
    public class JsonHeroUpgradeTreeSet
    {
        public List<JsonHeroUpgradeTree> trees;
    }

    public class JsonBuildupgradeTreeSet
    {
        public List<JsonBuildUpgradeTree> trees;
    }

    public class JsonBuildUpgradeTree
    {
        public string id;
        public bool is_instanced;
        public List<string> tags;
        public List<JsonBuildUpgrade> requirements;
    }

    public class JsonHeroUpgradeTree
    {
        public string id;
        public bool is_instanced;
        public List<string> tags;
        public List<JsonHeroUpgrade> requirements;
    }

    public class JsonBuildUpgrade
    {
        public string code;
        public List<JsonCurrencyCost> currency_cost;
        public List<JsonPrerequisiteReqirement> prerequisite_requirements;
    }

    public class JsonHeroUpgrade
    {
        public string code;
        public List<JsonCurrencyCost> currency_cost;
        public List<JsonPrerequisiteReqirement> prerequisite_requirements;
        public int prerequisite_resolve_level;
    }

    public class JsonCurrencyCost
    {
        public string type;
        public int amount;
    }

    public class JsonPrerequisiteReqirement
    {
        public string tree_id;
        public string requirement_code;
    }
    #endregion

    #region AI
    public class JsonMonsterBrainsDatabase
    {
        public List<JsonMonsterBrains> monster_brains;
    }

    public class JsonMonsterBrains
    {
        public string id;
        public List<JsonSkillCooldown> skill_cooldowns;
        public List<JsonSelectionDesire> skill_selection_desires;
        public List<JsonSelectionDesire> target_selection_desires;
        public List<JsonSelectionDesire> bonus_initiative_desires;
    }
    public class JsonSkillCooldown
    {
        public string combat_skill_id;
        public int amount;
    }
    public class JsonSelectionDesire
    {
        public string type;
        public Dictionary<string, object> data;
    }
    #endregion

    #region Traps
    public class JsonTrapDatabase
    {
        public List<JsonTrap> props;
    }

    public class JsonTrap
    {
        public string name;
        public List<string> success_effects;
        public List<string> fail_effects;
        public float health;

        public List<JsonTrapVariation> difficulty_variations;
    }

    public class JsonTrapVariation
    {
        public int level;
        public List<string> success_effects;
        public List<string> fail_effects;
        public float health;
    }
    #endregion

    #region Obstacles
    public class JsonObstacleDatabase
    {
        public List<JsonObstacle> props;
    }

    public class JsonObstacle
    {
        public string name;
        public List<string> fail_effects;
        public float health;
        public float torchlight;
        public bool ancestor_talk;
    }
    #endregion

    #region Loot
    public class JsonLootDatabase
    {
        public List<JsonDarknessBonusSet> darkness_bonuses;
        public List<JsonLootTable> loot_tables;
    }

    public class JsonDarknessBonusSet
    {
        public string type;
        public List<JsonDarknessBonus> bonuses;
    }

    public class JsonDarknessBonus
    {
        public int darkness;
        public float chance;
        public List<string> codes;
    }

    public class JsonLootTable
    {
        public string id;
        public int difficulty;
        public string dungeon;
        public List<JsonLootEntry> entries;
    }

    public class JsonLootEntry
    {
        public string type;
        public float chances;
        public Dictionary<string, object> data;
    }
    #endregion

    #region Quirks
    public class JsonQuirkData
    {
        public List<JsonQuirk> quirks;
    }

    public class JsonQuirk
    {
        public string id;
        public bool show_explicit_description;
        public bool is_positive;
        public bool is_disease;
        public string classification;
        public List<string> incompatible_quirks;
        public string curio_tag;
        public float curio_tag_chance;
        public bool keep_loot;
        public List<string> buffs;
    }
    #endregion

    #region Campaign
    public class JsonCampaignData
    {
        public List<int> quest_completion_xp_table;
        public List<int> level_threshold_table;
        public List<int> resolve_level_thresholds;
        public List<int> gold_icon_thresholds;
        public List<int> provision_icon_thresholds;
    }

    public class JsonItemIconThreshold
    {
        public List<int> icon_thresholds;
    }
    #endregion

    #region Buffs
    public class JsonBuffData
    {
        public List<JsonBuff> buffs;
    }

    public class JsonBuff
    {
        public string id;
        public string stat_type;
        public string stat_sub_type;
        public float amount;
        public string duration_type;
        public int duration;
        public string rule_type;
        public bool is_false_rule;
        public JsonBuffRuleData rule_data;
    }

    public class JsonBuffRuleData
    {
        [JsonProperty("float")]
        public float ruleFloat;
        [JsonProperty("string")]
        public string ruleString;
    }
    #endregion

    #region Trinkets
    public class JsonTrinket
    {
        public string id;
        public List<string> buffs;
        public List<string> hero_class_requirements;
        public string rarity;
        public int price;
        public int limit;
        public string origin_dungeon;

        public JsonTrinket()
        {
            buffs = new List<string>();
            hero_class_requirements = new List<string>();
        }
    }

    public class JsonTrinketDatabase
    {
        public List<string> rarities;
        public List<JsonTrinket> trinkets;
    }
    #endregion

    #region Quests
    public class JsonQuestDatabase
    {
        public int stress_damage;
        public List<JsonQuestGoal> goals;
        public List<string> town_progression_goal_ids;
        public List<JsonQuestType> types;
        public List<JsonPlotQuest> plot_quests;
        public JsonQuestGeneration generation;
        public JsonQuestRestrictions restriction;
    }

    #region Json Quest Goal
    public class JsonQuestGoal
    {
        public string id;
        public string type;
        public List<JsonQuestItem> starting_items;
        public bool ignore_fog_of_war;
        public bool show_as_quest;
        public Dictionary<string, object> data;
    }
    public class JsonQuestItem
    {
        public string type;
        public string id;
        public int amount;
    }
    #endregion

    #region Json Quest Type
    public class JsonQuestType
    {
        public string id;
        public List<JsonQuestTypeGoal> goal_lists;
    }
    public class JsonQuestTypeGoal
    {
        public string dungeon;
        public List<List<string>> goals;
    }
    #endregion

    #region Json Quest Plot
    public class JsonPlotQuest
    {
        public string id;
        public string plot_quest_dependency;
        public int dungeon_level;
        public JsonPlotQuestData quest;
        public List<JsonPlotAddTrinketData> additional_trinket_completion_rewards;
        public bool is_progression;
        public bool has_statue_contents;
        public bool completion_dungeon_xp;
        public bool can_retreat;
        public bool retreat_always_from_raid;
        public int retreat_party_kill_count;
        public bool is_surprise_enabled;
        public bool is_scouting_enabled;
        public bool is_roster_stress_cleared_on_completion;
        public int roster_buff_on_failure_minimum_party_resolve_level;
        public List<JsonUpgradeTag> upgrade_tags_to_remove_on_ignore;
        public List<JsonUpgradeTag> upgrade_tags_to_remove_on_failure;
        public List<string> roster_buffs_to_apply_on_failure;
        public List<JsonSuggestedTrinket> suggested_trinkets;
    }
    public class JsonPlotQuestData
    {
        public bool is_plot_quest;
        public string type;
        public string dungeon;
        public int difficulty;
        public int length;
        public string map_name;
        public List<string> goal_ids;
        public JsonCompletionReward completion_reward;
    }
    public class JsonPlotAddTrinketData
    {
        public string rarity;
        public int amount;
    }
    public class JsonSuggestedTrinket
    {
        public string trinket_id;
        public int amount;
    }
    public class JsonCompletionReward
    {
        public int resolve_xp;
        public JsonItemsDefinition items_definition;
    }
    public class JsonItemsDefinition
    {
        public string system_config_type;
        public JsonRewardItems items;
    }
    public class JsonUpgradeTag
    {
        public string upgrade_tag;
        public int amount;
    }
    public class JsonRewardItems
    {
        [JsonProperty("0")]
        public JsonQuestItem questItem0;
        [JsonProperty("1")]
        public JsonQuestItem questItem1;
        [JsonProperty("2")]
        public JsonQuestItem questItem2;
    }
    #endregion

    #region Json Quest Generation
    public class JsonQuestGeneration
    {
        public JsonQuestGenerationNumbers number;
        public JsonQuestGenerationDungeons dungeon;
        public JsonQuestGenerationDifficulties difficulty;
        public JsonQuestGenerationTypes type;
        public JsonQuestGenerationRewards rewards;
    }

    #region Json Quest Generation Numbers And Dungeons
    public class JsonQuestGenerationNumbers
    {
        public List<int> number_of_quests_per_town_visit_table;
    }
    public class JsonQuestGenerationDungeons
    {
        public int max_number_of_generated_quests_per_dungeon;
        public List<JsonQuestGenerationDungeonData> generated_dungeons;
    }
    public class JsonQuestGenerationDungeonData
    {
        public string id;
        public int required_number_of_quests_finished;
    }
    #endregion

    #region Json Quest Generation Difficulties
    public class JsonQuestGenerationDifficulties
    {
        public List<JsonQuestGenerationResolveDifficulty> generated_resolve_level_difficulties;
    }
    public class JsonQuestGenerationResolveDifficulty
    {
        public List<int> resolve_levels;
        public int difficulty;
    }
    #endregion

    #region Json Quest Generation Types
    public class JsonQuestGenerationTypes
    {
        public List<JsonQuestGenerationTypeData> available_quests_table;
    }
    public class JsonQuestGenerationTypeData
    {
        public string dungeon;
        public List<List<JsonGeneratedQuestTypeItem>> generated_quest_table;
    }
    public class JsonGeneratedQuestTypeItem
    {
        public string type;
        public int chance;
        public int length;
    }
    #endregion

    #region Json Quest Generation Rewards
    public class JsonQuestGenerationRewards
    {
        public List<JsonHeirloomTypeMapGeneration> heirloom_type_map;
        public List<JsonHeirloomGeneratedAmounts> heirloom_amount_table;
        public JsonQuestGeneratedItemTableData[][][] item_table;
        public int[][] resolve_xp_table;
        public List<JsonTrinketTable> trinket_chance_table;
    }
    public class JsonTrinketTable
    {
        public string rarity;
        public int[][] chances;
    }
    public class JsonHeirloomTypeMapGeneration
    {
        public string dungeon;
        public List<string> types;
    }
    public class JsonHeirloomGeneratedAmounts
    {
        public string type;
        public int[][] amounts;
    }
    public class JsonQuestGeneratedItemTableData
    {
        public string type;
        public string id;
        public int amount;
    }
    #endregion
    #endregion

    #region Json Quest Restrictions
    public class JsonQuestRestrictions
    {
        public JsonQuestRestrictionsDifficulty difficulty;
    }
    public class JsonQuestRestrictionsDifficulty
    {
        public List<int> resolve_level_threshold_table;
    }
    #endregion
    #endregion

    public static class JsonDarkestDeserializer
    {
        public static JsonClass GetJsonObject<JsonClass>(string jsonString)
        {
            return JsonConvert.DeserializeObject<JsonClass>(jsonString);
        }

        public static JsonHeirloomExchange GetJsonHeirloomExchange(string exchangeString)
        {
            return JsonConvert.DeserializeObject<JsonHeirloomExchange>(exchangeString);
        }

        public static JsonPartyNameDictionary GetJsonPartyNames(string partyString)
        {
            return JsonConvert.DeserializeObject<JsonPartyNameDictionary>(partyString);
        }

        public static JsonNarration GetJsonNarration(string narrationString)
        {
            return JsonConvert.DeserializeObject<JsonNarration>(narrationString);
        }

        public static JsonTownEventDatabase GetJsonTownEvents(string eventString)
        {
            return JsonConvert.DeserializeObject<JsonTownEventDatabase>(eventString);
        }

        public static JsonProvision GetJsonProvision(string provisionString)
        {
            return JsonConvert.DeserializeObject<JsonProvision>(provisionString);
        }

        public static JsonTraits GetJsonTraits(string traitsString)
        {
            return JsonConvert.DeserializeObject<JsonTraits>(traitsString);
        }

        public static JsonCamping GetJsonCamping(string campingString)
        {
            return JsonConvert.DeserializeObject<JsonCamping>(campingString);
        }

        public static JsonStageCoach GetJsonCoach(string coachString)
        {
            return JsonConvert.DeserializeObject<JsonStageCoach>(coachString);
        }

        public static JsonNomadWagon GetJsonWagon(string wagonString)
        {
            return JsonConvert.DeserializeObject<JsonNomadWagon>(wagonString);
        }

        public static JsonCampingTrainer GetJsonCamp(string campString)
        {
            return JsonConvert.DeserializeObject<JsonCampingTrainer>(campString);
        }

        public static JsonGuild GetJsonGuild(string guildString)
        {
            return JsonConvert.DeserializeObject<JsonGuild>(guildString);
        }

        public static JsonBlacksmith GetJsonBlacksmith(string blackString)
        {
            return JsonConvert.DeserializeObject<JsonBlacksmith>(blackString);
        }

        public static JsonSanitarium GetJsonSanitarium(string sanitariumString)
        {
            return JsonConvert.DeserializeObject<JsonSanitarium>(sanitariumString);
        }

        public static JsonTavern GetJsonTavern(string tavernString)
        {
            return JsonConvert.DeserializeObject<JsonTavern>(tavernString);
        }

        public static JsonAbbey GetJsonAbbey(string abbeyString)
        {
            return JsonConvert.DeserializeObject<JsonAbbey>(abbeyString);
        }

        public static List<JsonMonsterBrains> GetJsonAI(string aiString)
        {
            return JsonConvert.DeserializeObject<JsonMonsterBrainsDatabase>(aiString).monster_brains;
        }

        public static List<JsonTrap> GetJsonTraps(string trapString)
        {
            return JsonConvert.DeserializeObject<JsonTrapDatabase>(trapString).props;
        }

        public static List<JsonObstacle> GetJsonObstacles(string obstacleString)
        {
            return JsonConvert.DeserializeObject<JsonObstacleDatabase>(obstacleString).props;
        }

        public static List<JsonBuff> GetJsonBuffs(string buffString)
        {
            return JsonConvert.DeserializeObject<JsonBuffData>(buffString).buffs;
        }

        public static List<JsonQuirk> GetJsonQuirks(string quirkString)
        {
            return JsonConvert.DeserializeObject<JsonQuirkData>(quirkString).quirks;
        }

        public static JsonLootDatabase GetJsonLootDatabase(string lootString)
        {
            return JsonConvert.DeserializeObject<JsonLootDatabase>(lootString);
        }

        public static JsonTrinketDatabase GetJsonTrinketDatabase(string trinketData)
        {
            return JsonConvert.DeserializeObject<JsonTrinketDatabase>(trinketData);
        }

        public static JsonQuestDatabase GetJsonQuestDatabase(string questData)
        {
            return JsonConvert.DeserializeObject<JsonQuestDatabase>(questData);
        }

        public static JsonQuestItem GetJsonQuestItem(string itemString)
        {
            return JsonConvert.DeserializeObject<JsonQuestItem>(itemString);
        }

        public static JsonCampaignData GetJsonCampaign(string itemString)
        {
            return JsonConvert.DeserializeObject<JsonCampaignData>(itemString);
        }

        public static JsonHeroUpgradeTreeSet GetJsonUpgradeTreeSet(string upgradeString)
        {
            return JsonConvert.DeserializeObject<JsonHeroUpgradeTreeSet>(upgradeString);
        }
    }
}