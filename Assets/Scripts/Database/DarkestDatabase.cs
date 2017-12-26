using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DarkestJson;

public class DarkestDatabase : MonoBehaviour
{
    #region Data Paths

    private const string MonstersDirectory = "Data/Monsters/";
    private const string HeroesDirectory = "Data/Heroes/";
    private const string ItemDataPath = "Data/Inventory/Items";
    private const string DungeonGenerationDataPath = "Data/Mechanics/MapGenerator";
    private const string TownEventsDataPath = "Data/Mechanics/TownEvents";
    private const string EffectsDataPath = "Data/Mechanics/Effects";

    private const string CryptsEnviromentDataPath = "Data/Dungeons/Crypts";
    private const string WarrensEnviromentDataPath = "Data/Dungeons/Warrens";
    private const string WealdEnviromentDataPath = "Data/Dungeons/Weald";
    private const string CoveEnviromentDataPath = "Data/Dungeons/Cove";
    private const string DarkestEnviromentDataPath = "Data/Dungeons/Darkest";
    private const string TownEnviromentDataPath = "Data/Dungeons/Town";
    private const string SharedEnviromentDataPath = "Data/Dungeons/Shared";

    private const string CsvCurioDatabasePath = "Data/Curios/Curios";
    private const string JsonTrapDatabasePath = "Data/Curios/Traps";
    private const string JsonObstacleDatabasePath = "Data/Curios/Obstacles";
    private const string JsonCampaignGenerationPath = "Data/Mechanics/Campaign";
    private const string JsonExchangePath = "Data/Mechanics/HeirloomExchange";
    private const string JsonProvisionPath = "Data/Mechanics/Provision";
    private const string JsonBuffDatabasePath = "Data/JsonBuffs";
    private const string JsonNarrationDataPath = "Data/Narration";
    private const string JsonPartyNameDataPath = "Data/PartyNames";
    private const string JsonTraitDatabasePath = "Data/JsonTraits";
    private const string JsonCampingPath = "Data/JsonCamping";
    private const string JsonAIDatabasePath = "Data/JsonAI";
    private const string JsonTrinketDatabasePath = "Data/JsonTrinkets";
    private const string JsonQuestDatabasePath = "Data/JsonQuests";
    private const string JsonQuirkDatabasePath = "Data/JsonQuirks";
    private const string JsonLootDatabasePath = "Data/JsonLoot";
    private const string JsonHeroUpgradesPath = "Data/Upgrades/Heroes";
    private const string JsonBuildingUpgradesPath = "Data/Upgrades/Building";
    private const string JsonBuildingDataPath = "Data/Buildings/";

    #endregion

    public Dictionary<string, Building> Buildings { get; private set; }
    public Dictionary<string, UpgradeTree> UpgradeTrees { get; private set; }

    public Dictionary<string, Sprite> Sprites { get; private set; }
    public Dictionary<string, Sprite> DungeonSprites { get; private set; }
    public Dictionary<string, MonsterBrain> Brains { get; private set; }
    public Dictionary<string, MonsterData> Monsters { get; private set; }
    public Dictionary<string, HeroClass> HeroClasses { get; private set; }
    public Dictionary<string, Effect> Effects { get; private set; }
    public Dictionary<string, Buff> Buffs { get; private set; }
    public Dictionary<string, Quirk> Quirks { get; private set; }
    public Dictionary<string, Curio> Curios { get; private set; }
    public Dictionary<string, Obstacle> Obstacles { get; private set; }
    public Dictionary<string, Trap> Traps { get; private set; }
    public Dictionary<string, Dictionary<string, ItemData>> Items { get; private set; }   
    public Dictionary<string, DungeonEnviromentData> DungeonEnviromentData { get; private set; }

    public List<CampingSkill> CampingSkills { get; private set; }
    public List<Trait> Traits { get; private set; }

    public TownEventDatabase EventDatabase { get; private set; }
    public HeroSpriteDatabase HeroSprites { get; private set; }
    public QuestDatabase QuestDatabase { get; private set; }
    public LootDatabase LootDatabase { get; private set; }
    public ProvisionDatabase Provision { get; private set; }
    public CampaignGenerationData CampaignGeneration { get; private set; }
    public List<DungeonGenerationData> DungeonGenerationData { get; private set; }

    public Dictionary<string, string> HexColors { get; private set; }
    public Dictionary<AreaType, Sprite> MapRoomIconSet { get; private set; }
    public Dictionary<AreaType, Sprite> MapHallIconSet { get; private set; }
    public Dictionary<Knowledge, Sprite> MapRoomKnowledgeSet { get; private set; }
    public Dictionary<Knowledge, Sprite> MapHallKnowledgeSet { get; private set; }

    public Dictionary<string, NarrationEntry> Narration { get; private set; }
    public List<PartyNameEntry> PartyNames { get; private set; }
    public List<HeirloomExchange> HeirloomExchanges { get; private set; }

    public void Load()
    {
        LoadSprites();

        LoadJsonUpgrades();
        LoadJsonBuildings();
        
        LoadJsonBuffs();
        LoadJsonQuirks();
        LoadEffects();
        LoadTraits();
        LoadJsonAI();
        LoadJsonCampingSkills();
        LoadJsonHeroClasses();
        LoadJsonTrinkets();
        LoadJsonQuests();
        LoadJsonLoot();
        LoadJsonObstacles();
        LoadJsonTraps();
        LoadCsvCurios();

        LoadCampaignGenerationData();
        LoadItems();
        LoadProvision();
        LoadDungeons();
        LoadColours();
        LoadIconSets();
        LoadMonsters();
        LoadJsonTownEvents();
        LoadNarration();
        LoadPartyNames();
        LoadHeirloomExchanges();

        GC.Collect();
    }

    public void LoadDungeon(string dungeon, string quest = null)
    {
        DungeonSprites.Clear();
        switch (dungeon)
        {
            case "darkestdungeon":
                switch (quest)
                {
                    case "plot_darkest_dungeon_1":
                        foreach (var dungeonSprite in Resources.LoadAll<Sprite>("Dungeons/darkestdungeon/quest_1"))
                            DungeonSprites.Add(dungeonSprite.name, dungeonSprite);
                        break;
                    case "plot_darkest_dungeon_2":
                        foreach (var dungeonSprite in Resources.LoadAll<Sprite>("Dungeons/darkestdungeon/quest_2"))
                            DungeonSprites.Add(dungeonSprite.name, dungeonSprite);
                        break;
                    case "plot_darkest_dungeon_3":
                        foreach (var dungeonSprite in Resources.LoadAll<Sprite>("Dungeons/darkestdungeon/quest_3"))
                            DungeonSprites.Add(dungeonSprite.name, dungeonSprite);
                        break;
                    case "plot_darkest_dungeon_4":
                        foreach (var dungeonSprite in Resources.LoadAll<Sprite>("Dungeons/darkestdungeon/quest_4"))
                            DungeonSprites.Add(dungeonSprite.name, dungeonSprite);
                        break;
                    default:
                        goto case "plot_darkest_dungeon_1";
                }
                break;
            default:
                foreach (var dungeonSprite in Resources.LoadAll<Sprite>("Dungeons/" + dungeon))
                    DungeonSprites.Add(dungeonSprite.name, dungeonSprite);
                break;
        }
    }

    public bool ItemExists(ItemDefinition itemDefinition)
    {
        return DarkestDungeonManager.Data.Items.ContainsKey(itemDefinition.Type) &&
            DarkestDungeonManager.Data.Items[itemDefinition.Type].ContainsKey(itemDefinition.Id);
    }

    #region Json Convertation

    private List<MonsterBrain> GetJsonMonsterBrains()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonAIDatabasePath);
        List<JsonMonsterBrains> jsonBrains = JsonDarkestDeserializer.GetJsonAI(jsonText.text);
        List<MonsterBrain> brains = new List<MonsterBrain>();
        for (int i = 0; i < jsonBrains.Count; i++)
        {
            MonsterBrain brain = new MonsterBrain();
            brain.Id = jsonBrains[i].id;
            #region Skill Cooldowns
            foreach (var jsonCooldown in jsonBrains[i].skill_cooldowns)
                brain.SkillCooldowns.Add(new SkillCooldown(jsonCooldown.combat_skill_id, jsonCooldown.amount));
            #endregion
            #region Skill Desires
            foreach (var jsonSkillDesire in jsonBrains[i].skill_selection_desires)
            {
                switch(jsonSkillDesire.type)
                {
                    case "random_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionRandom(jsonSkillDesire.data));
                        break;
                    case "preferred_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionPreferred(jsonSkillDesire.data));
                        break;
                    case "heal_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionHeal(jsonSkillDesire.data));
                        break;
                    case "specific_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionSpecific(jsonSkillDesire.data));
                        break;
                    case "performing_turn_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionPerformingTurn(jsonSkillDesire.data));
                        break;
                    case "ally_alive_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionAllyAlive(jsonSkillDesire.data));
                        break;
                    case "fill_ally_captor_empty_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionFillEmptyCaptor(jsonSkillDesire.data));
                        break;
                    case "effect_key_status_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionStatus(jsonSkillDesire.data));
                        break;
                    case "ally_dead_skill":
                        brain.SkillDesireSet.Add(new SkillSelectionAllyDead(jsonSkillDesire.data));
                        break;
                    default:
                        Debug.LogError("Unknown skill selection desire: " + jsonSkillDesire.type);
                        break;
                }
            }
            #endregion
            #region Target Desires
            foreach (var jsonTargetDesire in jsonBrains[i].target_selection_desires)
            {
                switch (jsonTargetDesire.type)
                {
                    case "random_target":
                        brain.TargetDesireSet.Add(new TargetSelectionRandom(jsonTargetDesire.data));
                        break;
                    case "marked_target":
                        brain.TargetDesireSet.Add(new TargetSelectionMarked(jsonTargetDesire.data));
                        break;
                    case "health_target":
                        brain.TargetDesireSet.Add(new TargetSelectionHealth(jsonTargetDesire.data));
                        break;
                    case "stress_target":
                        brain.TargetDesireSet.Add(new TargetSelectionStress(jsonTargetDesire.data));
                        break;
                    case "fill_ally_captor_empty_target":
                        brain.TargetDesireSet.Add(new TargetSelectionFillCaptor(jsonTargetDesire.data));
                        break;
                    case "rank_target":
                        brain.TargetDesireSet.Add(new TargetSelectionRank(jsonTargetDesire.data));
                        break;
                    case "ally_class_target":
                        brain.TargetDesireSet.Add(new TargetSelectionAllyClass(jsonTargetDesire.data));
                        break;
                    case "resistance_target":
                        brain.TargetDesireSet.Add(new TargetSelectionResistance(jsonTargetDesire.data));
                        break;
                    default:
                        Debug.LogError("Unknown target selection desire: " + jsonTargetDesire.type);
                        break;
                }
            }
            #endregion
            #region Bonus Desires
            foreach (var jsonBonusDesire in jsonBrains[i].bonus_initiative_desires)
            {
                switch (jsonBonusDesire.type)
                {
                    case "hp_ratio_threshold":
                        brain.BonusDesireSet.Add(new BonusInitiativeHpRatio(jsonBonusDesire.data));
                        break;
                    case "death":
                        brain.BonusDesireSet.Add(new BonusInitiativeDeath(jsonBonusDesire.data));
                        break;
                    case "guaranteed":
                        brain.BonusDesireSet.Add(new BonusInitiativeGuaranteed(jsonBonusDesire.data));
                        break;
                    case "ally_last_damaged":
                        brain.BonusDesireSet.Add(new BonusInitiativeAllyLastDamaged(jsonBonusDesire.data));
                        break;
                    case "last_skill":
                        brain.BonusDesireSet.Add(new BonusInitiativeLastSkill(jsonBonusDesire.data));
                        break;
                    case "ally_actor_class_count":
                        brain.BonusDesireSet.Add(new BonusInitiativeAllyClassCount(jsonBonusDesire.data));
                        break;
                    default:
                        Debug.LogError("Unknown bonus desire: " + jsonBonusDesire.type);
                        break;
                }
            }
            #endregion
            brains.Add(brain);
        }
        return brains;
    }

    private List<Buff> GetJsonBuffLibrary()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonBuffDatabasePath);
        List<JsonBuff> jsonBuffs = JsonDarkestDeserializer.GetJsonBuffs(jsonText.text);
        List<Buff> buffs = new List<Buff>();
        for(int i = 0; i < jsonBuffs.Count; i++)
        {
            Buff buff = new Buff();
            buff.Id = jsonBuffs[i].id;
            buff.ModifierValue = jsonBuffs[i].amount;
            buff.IsFalseRule = jsonBuffs[i].is_false_rule;

            buff.DurationAmount = jsonBuffs[i].duration;
            switch(jsonBuffs[i].duration_type)
            {
                case "quest_end":
                    buff.DurationType = BuffDurationType.Raid;
                    break;
                case "activity_end":
                    buff.DurationType = BuffDurationType.Activity;
                    break;
                case "combat_end":
                    buff.DurationType = BuffDurationType.Combat;
                    break;
                case "quest_complete":
                    buff.DurationType = BuffDurationType.QuestComplete;
                    break;
                case "idle_start_town_visit":
                    buff.DurationType = BuffDurationType.IdleTownVisit;
                    break;
                case null:
                    buff.DurationType = BuffDurationType.Undefined;
                    break;
                default:
                    Debug.LogWarning("Unknown duration: " + jsonBuffs[i].duration_type);
                    break;
            }

            switch(jsonBuffs[i].stat_type)
            {
                case "resistance":
                case "upgrade_discount":
                    buff.Type = BuffType.StatAdd;
                    buff.AttributeType = CharacterHelper.StringToAttributeType(jsonBuffs[i].stat_sub_type);
                    break;
                case "combat_stat_add":
                case "combat_stat_multiply":
                    buff.Type = CharacterHelper.StringToBuffType(jsonBuffs[i].stat_type);
                    buff.AttributeType = CharacterHelper.StringToAttributeType(jsonBuffs[i].stat_sub_type);
                    break;
                case "hp_heal_amount":
                case "hp_heal_percent":
                case "damage_received_percent":
                case "hp_heal_received_percent":
                case "stress_dmg_percent":
                case "stress_heal_percent":
                case "stress_heal_received_percent":
                case "stress_dmg_received_percent":
                case "resolve_check_percent":
                case "resolve_xp_bonus_percent":
                case "stun_chance":
                case "poison_chance":
                case "bleed_chance":
                case "move_chance":
                case "debuff_chance":
                case "scouting_chance":
                case "party_surprise_chance":
                case "remove_negative_quirk_chance":
                case "food_consumption_percent":
                case "starving_damage_percent":
                case "monsters_surprise_chance":
                    buff.Type = BuffType.StatAdd;
                    buff.AttributeType = CharacterHelper.StringToAttributeType(jsonBuffs[i].stat_type);
                    break;
                default:
                    Debug.Log("Unexpected buff type " + jsonBuffs[i].stat_type);
                    continue;
            }

            buff.RuleType = CharacterHelper.StringToBuffRule(jsonBuffs[i].rule_type);
            buff.SingleParam = jsonBuffs[i].rule_data.ruleFloat;
            buff.StringParam = jsonBuffs[i].rule_data.ruleString;
            buffs.Add(buff);
        }
        return buffs;
    }

    private List<Quirk> GetJsonQuirkLibrary()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonQuirkDatabasePath);
        List<JsonQuirk> jsonQuirks = JsonDarkestDeserializer.GetJsonQuirks(jsonText.text);

        List<Quirk> quirks = new List<Quirk>();

        for (int i = 0; i < jsonQuirks.Count; i++)
        {
            Quirk quirk = new Quirk();
            quirk.Id = jsonQuirks[i].id;
            quirk.Classification = jsonQuirks[i].classification;
            quirk.ShowExplicitDescription = jsonQuirks[i].show_explicit_description;
            quirk.IsPositive = jsonQuirks[i].is_positive;
            quirk.IsDisease = jsonQuirks[i].is_disease;
            quirk.KeepLoot = jsonQuirks[i].keep_loot;
            quirk.CurioTag = jsonQuirks[i].curio_tag;
            quirk.CurioTagChance = jsonQuirks[i].curio_tag_chance;
            quirk.IncompatibleQuirks = jsonQuirks[i].incompatible_quirks;

            foreach (var buffName in jsonQuirks[i].buffs)
            {
                if (!Buffs.ContainsKey(buffName))
                    Debug.Log("Quirk buff " + buffName + " not found.");
                else
                    quirk.Buffs.Add(Buffs[buffName]);
            }
            quirks.Add(quirk);
        }
        return quirks;
    }

    private List<Trinket> GetJsonTrinketLibrary()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonTrinketDatabasePath);
        var trinketDatabase = JsonDarkestDeserializer.GetJsonTrinketDatabase(jsonText.text);
        List<JsonTrinket> jsonTrinkets = trinketDatabase.trinkets;
        List<Trinket> trinkets = new List<Trinket>();

        for (int i = 0; i < jsonTrinkets.Count; i++)
        {
            Trinket trinket = new Trinket();
            trinket.Id = jsonTrinkets[i].id;
            foreach(var buffName in jsonTrinkets[i].buffs.ToArray())
            {
                if (!Buffs.ContainsKey(buffName))
                    Debug.Log("Trinket buff " + buffName + " not found.");
                else
                    trinket.Buffs.Add(Buffs[buffName]);
            }
            trinket.ClassRequirements = jsonTrinkets[i].hero_class_requirements;
            trinket.PurchasePrice = jsonTrinkets[i].price;
            trinket.RarityId = jsonTrinkets[i].rarity;
            trinkets.Add(trinket);
        }
        return trinkets;
    }

    private List<Obstacle> GetJsonObstaclesLibrary()
    {
        List<Obstacle> obstacles = new List<Obstacle>();

        string obstacleText = Resources.Load<TextAsset>(JsonObstacleDatabasePath).text;
        var jsonObstacles = JsonDarkestDeserializer.GetJsonObstacles(obstacleText);

        for (int i = 0; i < jsonObstacles.Count; i++)
        {
            Obstacle obstacle = new Obstacle(jsonObstacles[i].name);
            for (int j = 0; j < jsonObstacles[i].fail_effects.Count; j++)
                obstacle.FailEffects.Add(Effects[jsonObstacles[i].fail_effects[j]]);

            obstacle.HealthPenalty = jsonObstacles[i].health;
            obstacle.TorchlightPenalty = jsonObstacles[i].torchlight;
            obstacle.AncestorTalk = jsonObstacles[i].ancestor_talk;
            obstacles.Add(obstacle);
        }

        return obstacles;
    }

    private List<Trap> GetJsonTrapLibrary()
    {
        List<Trap> traps = new List<Trap>();

        string trapText = Resources.Load<TextAsset>(JsonTrapDatabasePath).text;
        var jsonTraps = JsonDarkestDeserializer.GetJsonTraps(trapText);

        for (int i = 0; i < jsonTraps.Count; i++)
        {
            Trap trap = new Trap(jsonTraps[i].name);
            for (int j = 0; j < jsonTraps[i].success_effects.Count; j++ )
                trap.SuccessEffects.Add(Effects[jsonTraps[i].success_effects[j]]);
            for (int j = 0; j < jsonTraps[i].fail_effects.Count; j++)
                trap.FailEffects.Add(Effects[jsonTraps[i].fail_effects[j]]);

            trap.HealthPenalty = jsonTraps[i].health;
            foreach(var variant in jsonTraps[i].difficulty_variations)
            {
                var trapVariation = new TrapVariation();
                trapVariation.Level = variant.level;
                for (int j = 0; j < variant.success_effects.Count; j++)
                    trapVariation.SuccessEffects.Add(Effects[variant.success_effects[j]]);
                for (int j = 0; j < variant.fail_effects.Count; j++)
                    trapVariation.FailEffects.Add(Effects[variant.fail_effects[j]]);
                trapVariation.HealthPenalty = variant.health;
                trap.Variations.Add(trapVariation.Level, trapVariation);
            }
            traps.Add(trap);
        }

        return traps;
    }

    private QuestDatabase GetJsonQuestDatabase()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonQuestDatabasePath);
        JsonQuestDatabase jsonData = JsonDarkestDeserializer.GetJsonQuestDatabase(jsonText.text);
        QuestDatabase questData = new QuestDatabase();
        questData.FailStressPenalty = jsonData.stress_damage;
        questData.LevelRestrictions.AddRange(jsonData.restriction.difficulty.resolve_level_threshold_table);
        #region Quest Goals
        for (int i = 0; i < jsonData.goals.Count; i ++)
        {
            QuestGoal goal = new QuestGoal();
            goal.Id = jsonData.goals[i].id;
            goal.Type = jsonData.goals[i].type;
            for(int j = 0; j < jsonData.goals[i].starting_items.Count; j++)
            {
                ItemDefinition item = new ItemDefinition();
                item.Id = jsonData.goals[i].starting_items[j].id;
                item.Type = jsonData.goals[i].starting_items[j].type;
                item.Amount = jsonData.goals[i].starting_items[j].amount;
                goal.StartingItems.Add(item);
            }
            switch(goal.Type)
            {
                case "tutorial_room":
                    var questTutorData = new QuestTutorialData();
                    questTutorData.FinalRoomId = (string)jsonData.goals[i].data["room_id"];
                    goal.QuestData = questTutorData;
                    break;
                case "kill_monster":
                    var questKillData = new QuestKillMonsterData();
                    string monsterString = jsonData.goals[i].data["monster_class_ids"].ToString();
                    string[] monsters = monsterString.Split(
                        new[]{'[', ']', ',' ,'\r', '\n', '\"', ' '}, StringSplitOptions.RemoveEmptyEntries);
                    questKillData.MonsterNameIds = new List<string>(monsters);
                    questKillData.Amount = (int)(long)jsonData.goals[i].data["amount"];
                    goal.QuestData = questKillData;
                    break;
                case "explore_room":
                    var questVisitData = new QuestVisitedData();
                    questVisitData.Type = QuestVisitType.Explore;
                    double percentage;
                    if (double.TryParse(jsonData.goals[i].data["percentage"].ToString(), out percentage))
                        questVisitData.PercenageExplored = (float)percentage;
                    else
                        questVisitData.PercenageExplored = (long)jsonData.goals[i].data["percentage"];
                    goal.QuestData = questVisitData;
                    break;
                case "battle_room":
                case "battle":
                    var questBattleData = new QuestVisitedData();
                    questBattleData.Type = QuestVisitType.Battle;
                    double battlePercentage;
                    if (double.TryParse(jsonData.goals[i].data["percentage"].ToString(), out battlePercentage))
                        questBattleData.PercenageExplored = (float)battlePercentage;
                    else
                        questBattleData.PercenageExplored = (long)jsonData.goals[i].data["percentage"];
                    goal.QuestData = questBattleData;
                    break;
                case "gather":
                    var questGatherData = new QuestGatherData();
                    questGatherData.CurioName = (string)jsonData.goals[i].data["curio_name"];
                    JsonQuestItem jsonItem = JsonDarkestDeserializer.GetJsonQuestItem(jsonData.goals[i].data["item"].ToString());
                    ItemDefinition item = new ItemDefinition();
                    item.Id = jsonItem.id;
                    item.Type = jsonItem.type;
                    item.Amount = jsonItem.amount;
                    questGatherData.Item = item;
                    goal.QuestData = questGatherData;
                    break;
                case "activate":
                    var questActivateData = new QuestActivateData();
                    questActivateData.CurioName = (string)jsonData.goals[i].data["curio_name"];
                    questActivateData.Amount = (int)(long)jsonData.goals[i].data["amount"];
                    goal.QuestData = questActivateData;
                    break;
                case "trait_applied":
                    var questTraitData = new QuestTraitData();
                    questTraitData.Amount = (int)(long)jsonData.goals[i].data["amount"];
                    goal.QuestData = questTraitData;
                    break;
                case "deaths_door":
                    var questDeathDoorData = new QuestDeathDoorData();
                    questDeathDoorData.Amount = (int)(long)jsonData.goals[i].data["amount"];
                    goal.QuestData = questDeathDoorData;
                    break;
                default:
                    Debug.Log("Unknown quest goal type: " + goal.Type);
                    break;
            }
            questData.QuestGoals.Add(goal.Id, goal);
        }
        #endregion
        questData.TownProgressionGoalIds = jsonData.town_progression_goal_ids;
        #region Quest Types
        for (int i = 0; i < jsonData.types.Count; i++)
        {
            QuestType questType = new QuestType();
            questType.Id = jsonData.types[i].id;
            for(int j = 0; j < jsonData.types[i].goal_lists.Count; j++)
            {
                QuestGoalList goalList = new QuestGoalList();
                goalList.Dungeon = jsonData.types[i].goal_lists[j].dungeon;
                for(int k = 0; k < jsonData.types[i].goal_lists[j].goals.Count; k++)
                    foreach(var goal in jsonData.types[i].goal_lists[j].goals[k])
                        goalList.Goals.Add(goal);

                questType.GoalLists.Add(goalList);
            }
            questData.QuestTypes.Add(questType.Id, questType);
        }
        #endregion
        #region Plot Quest
        for (int i = 0; i < jsonData.plot_quests.Count; i++)
        {
            PlotQuest plotQuest = new PlotQuest();
            plotQuest.Id = jsonData.plot_quests[i].id;
            plotQuest.PlotDependency = jsonData.plot_quests[i].plot_quest_dependency;
            plotQuest.DungeonLevel = jsonData.plot_quests[i].dungeon_level;
            plotQuest.IsPlotQuest = jsonData.plot_quests[i].quest.is_plot_quest;
            plotQuest.Type = jsonData.plot_quests[i].quest.type;
            plotQuest.Dungeon = jsonData.plot_quests[i].quest.dungeon;
            plotQuest.Difficulty = jsonData.plot_quests[i].quest.difficulty;
            plotQuest.Length = jsonData.plot_quests[i].quest.length;
            plotQuest.RaidMap = jsonData.plot_quests[i].quest.map_name;
            if (jsonData.plot_quests[i].quest.goal_ids.Count > 1)
                Debug.LogError("Multiple goals in " + jsonData.plot_quests[i].id);
            plotQuest.Goal = questData.QuestGoals[jsonData.plot_quests[i].quest.goal_ids[0]];
            CompletionReward reward = new CompletionReward();
            reward.ResolveXP = jsonData.plot_quests[i].quest.completion_reward.resolve_xp;
            ItemDefinition item = new ItemDefinition();
            item.Id = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem0.id;
            item.Type = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem0.type;
            item.Amount = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem0.amount;
            reward.ItemDefinitions.Add(item);

            if (jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem1 != null)
            {
                item = new ItemDefinition();
                item.Id = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem1.id;
                item.Type = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem1.type;
                item.Amount = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem1.amount;
                reward.ItemDefinitions.Add(item);
            }
            if (jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem2 != null)
            {
                item = new ItemDefinition();
                item.Id = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem2.id;
                item.Type = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem2.type;
                item.Amount = jsonData.plot_quests[i].quest.completion_reward.items_definition.items.questItem2.amount;
                reward.ItemDefinitions.Add(item);
            }
            plotQuest.Reward = reward;

            if(jsonData.plot_quests[i].additional_trinket_completion_rewards.Count > 0)
            { 
                PlotTrinketReward trinketReward = new PlotTrinketReward();
                trinketReward.Rarity = jsonData.plot_quests[i].additional_trinket_completion_rewards[0].rarity;
                trinketReward.Amount = jsonData.plot_quests[i].additional_trinket_completion_rewards[0].amount;
                plotQuest.PlotTrinket = trinketReward;
            }

            plotQuest.IsProgression = jsonData.plot_quests[i].is_progression;
            plotQuest.HasStatueContents = jsonData.plot_quests[i].has_statue_contents;
            plotQuest.CompletionDungeonXp = jsonData.plot_quests[i].completion_dungeon_xp;
            plotQuest.CanRetreat = jsonData.plot_quests[i].can_retreat;
            plotQuest.AlwaysRetreatFromRaid = jsonData.plot_quests[i].retreat_always_from_raid;
            plotQuest.RetreatKillCount = jsonData.plot_quests[i].retreat_party_kill_count;
            plotQuest.IsSurpriseEnabled = jsonData.plot_quests[i].is_surprise_enabled;
            plotQuest.IsScoutingEnabled = jsonData.plot_quests[i].is_scouting_enabled;
            plotQuest.IsStressClearedOnCompletion = jsonData.plot_quests[i].is_roster_stress_cleared_on_completion;
            plotQuest.RosterBuffOnFailureMinimumPartyResolveLevel = 
                jsonData.plot_quests[i].roster_buff_on_failure_minimum_party_resolve_level;

            for(int j = 0; j < jsonData.plot_quests[i].roster_buffs_to_apply_on_failure.Count; j++)
                plotQuest.RosterBuffsOnFailure.Add(Buffs[jsonData.plot_quests[i].roster_buffs_to_apply_on_failure[j]]);
            for(int j = 0; j < jsonData.plot_quests[i].suggested_trinkets.Count; j++)
                plotQuest.SuggestedTrinkets.Add(new ItemDefinition("trinket",
                    jsonData.plot_quests[i].suggested_trinkets[j].trinket_id, jsonData.plot_quests[i].suggested_trinkets[j].amount));
            for (int j = 0; j < jsonData.plot_quests[i].upgrade_tags_to_remove_on_ignore.Count; j++)
                plotQuest.UpgradeTagsRemovedOnIgnore.Add(
                    new UpgradeTag(jsonData.plot_quests[i].upgrade_tags_to_remove_on_ignore[j].upgrade_tag,
                    jsonData.plot_quests[i].upgrade_tags_to_remove_on_ignore[j].amount));

            questData.PlotQuests.Add(plotQuest);
        }
        #endregion
        #region Quest Generation Data
        QuestGenerationData generationData = new QuestGenerationData();
        generationData.QuestsPerVisit = jsonData.generation.number.number_of_quests_per_town_visit_table;
        generationData.MaxQuestsPerDungeon = jsonData.generation.dungeon.max_number_of_generated_quests_per_dungeon;
        foreach(var genDungeon in jsonData.generation.dungeon.generated_dungeons)
        {
            GeneratedDungeon newGenDungeon = new GeneratedDungeon();
            newGenDungeon.Id = genDungeon.id;
            newGenDungeon.RequiredQuestsCompleted = genDungeon.required_number_of_quests_finished;
            generationData.Dungeons.Add(newGenDungeon.Id, newGenDungeon);
        }
        foreach(var genDifSet in jsonData.generation.difficulty.generated_resolve_level_difficulties)
        {
            GeneratedResolveDifficulty newResDif = new GeneratedResolveDifficulty();
            newResDif.ResolveLevels = genDifSet.resolve_levels;
            newResDif.Difficulty = genDifSet.difficulty;
            generationData.Difficulties.Add(newResDif);
        }
        foreach(var questTable in jsonData.generation.type.available_quests_table)
        {
            GeneratedDungeonQuestTypes questTypes = new GeneratedDungeonQuestTypes();
            questTypes.Dungeon = questTable.dungeon;
            foreach(var questSet in questTable.generated_quest_table)
            {
                List<GeneratedQuestType> newQuestSet = new List<GeneratedQuestType>();
                foreach(var questGenType in questSet)
                {
                    GeneratedQuestType newGenType = new GeneratedQuestType();
                    newGenType.Type = questGenType.type;
                    newGenType.Length = questGenType.length;
                    newGenType.Chance = questGenType.chance;
                    newQuestSet.Add(newGenType);
                }
                questTypes.QuestTypeSets.Add(newQuestSet);
            }
            generationData.QuestTypes.Add(questTypes.Dungeon, questTypes);
        }
        foreach(var heirloomTypeMap in jsonData.generation.rewards.heirloom_type_map)
            generationData.HeirloomTypes.Add(heirloomTypeMap.dungeon, heirloomTypeMap.types);
        foreach(var heirloomAmountSet in jsonData.generation.rewards.heirloom_amount_table)
            generationData.HeirloomAmounts.Add(heirloomAmountSet.type, heirloomAmountSet.amounts);

        generationData.ItemTable = new ItemDefinition[jsonData.generation.rewards.item_table.Length][][];
        for (int i = 0; i < jsonData.generation.rewards.item_table.Length; i++ )
        {
            generationData.ItemTable[i] = new ItemDefinition[jsonData.generation.rewards.item_table[i].Length][];
            for (int j = 0; j < generationData.ItemTable[i].Length; j++)
            {
                generationData.ItemTable[i][j] = new ItemDefinition[jsonData.generation.rewards.item_table[i][j].Length];
                for(int k = 0; k < generationData.ItemTable[i][j].Length; k++)
                {
                    generationData.ItemTable[i][j][k] = new ItemDefinition();
                    generationData.ItemTable[i][j][k].Type = jsonData.generation.rewards.item_table[i][j][k].type;
                    generationData.ItemTable[i][j][k].Id = jsonData.generation.rewards.item_table[i][j][k].id;
                    generationData.ItemTable[i][j][k].Amount = jsonData.generation.rewards.item_table[i][j][k].amount;
                }
            }
        }
  
        generationData.ResolveXpReward = jsonData.generation.rewards.resolve_xp_table;
        for (int i = 0; i < jsonData.generation.rewards.trinket_chance_table.Count; i++)
            generationData.TrinketChances.Add(jsonData.generation.rewards.trinket_chance_table[i].rarity,
                jsonData.generation.rewards.trinket_chance_table[i].chances);

        generationData.ResolveThreshold = jsonData.restriction.difficulty.resolve_level_threshold_table;
        questData.QuestGeneration = generationData;
        #endregion
        return questData;
    }

    private LootDatabase GetJsonLootDatabase()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonLootDatabasePath);
        var lootDatabase = JsonDarkestDeserializer.GetJsonLootDatabase(jsonText.text);

        LootDatabase newLootDatabase = new LootDatabase();
        foreach(var bonusType in lootDatabase.darkness_bonuses)
        {
            List<DarknessBonus> bonuses = new List<DarknessBonus>();
            foreach(var bonus in bonusType.bonuses)
            {
                DarknessBonus darkBonus = new DarknessBonus();
                darkBonus.DarknessLevel = bonus.darkness;
                darkBonus.Chance = bonus.chance;
                darkBonus.Codes = bonus.codes;
                bonuses.Add(darkBonus);
            }

            newLootDatabase.DarknessLoot.Add(bonusType.type, bonuses);
        }

        foreach(var table in lootDatabase.loot_tables)
        {
            LootTable lootTable = new LootTable();
            lootTable.Id = table.id;
            lootTable.Difficulty = table.difficulty;
            lootTable.Dungeon = table.dungeon;
            
            foreach(var entry in table.entries)
            {
                switch(entry.type)
                {
                    case "nothing":
                        LootEntry lootEntry = new LootEntry();
                        lootEntry.Chance = entry.chances;
                        lootTable.Entries.Add(lootEntry);
                        break;
                    case "table":
                        LootEntryTable lootEntryTable = new LootEntryTable();
                        lootEntryTable.Chance = entry.chances;
                        lootEntryTable.TableId = (string)entry.data["table"];
                        lootTable.Entries.Add(lootEntryTable);
                        break;
                    case "item":
                        LootEntryItem lootEntryItem = new LootEntryItem();
                        lootEntryItem.Chance = entry.chances;
                        lootEntryItem.ItemType = (string)entry.data["type"];
                        lootEntryItem.ItemId = (string)entry.data["id"];
                        lootEntryItem.ItemAmount = (int)(long)entry.data["amount"];
                        lootTable.Entries.Add(lootEntryItem);                        
                        break;
                    case "trinket":
                        LootEntryTrinket lootEntryTrinket = new LootEntryTrinket();
                        lootEntryTrinket.Chance = entry.chances;
                        lootEntryTrinket.Rarity = (string)entry.data["rarity"];
                        lootTable.Entries.Add(lootEntryTrinket);
                        break;
                    case "journal_page":
                        LootEntryJournal lootEntryJournal = new LootEntryJournal();
                        lootEntryJournal.Chance = entry.chances;
                        if (entry.data.ContainsKey("specific_page_index"))
                        {
                            lootEntryJournal.SpecificId = (int)(long)entry.data["specific_page_index"];
                        }
                        else
                        {
                            lootEntryJournal.MinIndex = (int)(long)entry.data["min_page_index"];
                            lootEntryJournal.MaxIndex = (int)(long)entry.data["max_page_index"];
                        }
                        lootTable.Entries.Add(lootEntryJournal);
                        break;
                    default:
                        Debug.LogError("Unknown loot entry type: " + entry.type);
                        break;
                }
            }
            if (!newLootDatabase.LootTables.ContainsKey(lootTable.Id))
                newLootDatabase.LootTables.Add(lootTable.Id, new List<LootTable>());

            newLootDatabase.LootTables[lootTable.Id].Add(lootTable);
        }
        return newLootDatabase;
    }

    private ProvisionDatabase GetJsonProvisionDatabase()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonProvisionPath);
        var jsonProvision = JsonDarkestDeserializer.GetJsonProvision(jsonText.text);
        ProvisionDatabase provisionDatabase = new ProvisionDatabase();

        foreach(var jsonStartLengthList in jsonProvision.raid_starting_length_inventory_item_lists)
        {
            List<ItemDefinition> newStartList = new List<ItemDefinition>();
            for (int j = 0; j < jsonStartLengthList.Count; j++)
                newStartList.Add(new ItemDefinition(jsonStartLengthList[j].type,
                    jsonStartLengthList[j].id, jsonStartLengthList[j].amount));
            provisionDatabase.StartingLengthInventories.Add(newStartList);
        }

        foreach(var jsonHeroItemList in jsonProvision.raid_starting_hero_class_item_lists)
        {
            List<ItemDefinition> newHeroList = new List<ItemDefinition>();
            for (int j = 0; j < jsonHeroItemList.item_lists.Count; j++)
                newHeroList.Add(new ItemDefinition(jsonHeroItemList.item_lists[j].type,
                    jsonHeroItemList.item_lists[j].id, jsonHeroItemList.item_lists[j].amount));

            provisionDatabase.HeroClassItemList.Add(jsonHeroItemList.hero_class, newHeroList);
        }

        foreach (var jsonShopLengthList in jsonProvision.default_store_inventory_item_lists)
        {
            List<ItemDefinition> newStartList = new List<ItemDefinition>();
            for (int j = 0; j < jsonShopLengthList.Count; j++)
                newStartList.Add(new ItemDefinition(jsonShopLengthList[j].type,
                    jsonShopLengthList[j].id, jsonShopLengthList[j].amount));
            provisionDatabase.ShopLengthInventories.Add(newStartList);
        }

        return provisionDatabase;
    }

    private List<UpgradeTree> GetJsonHeroUpgradeTree()
    {
        List<UpgradeTree> upgradeTrees = new List<UpgradeTree>();

        foreach (var upgradeAsset in Resources.LoadAll<TextAsset>(JsonHeroUpgradesPath))
        {
            var upgradeTreeSet = JsonDarkestDeserializer.GetJsonUpgradeTreeSet(upgradeAsset.text);

            foreach(var upgradeTree in upgradeTreeSet.trees)
            {
                UpgradeTree tree = new UpgradeTree();
                tree.Id = upgradeTree.id;
                tree.IsInstanced = upgradeTree.is_instanced;
                tree.Tags = new List<string>(upgradeTree.tags);

                foreach(var jsonUpgrade in upgradeTree.requirements)
                {
                    HeroUpgrade upgrade = new HeroUpgrade();
                    upgrade.Code = jsonUpgrade.code;
                    upgrade.PrerequisiteResolveLevel = jsonUpgrade.prerequisite_resolve_level;

                    foreach(var jsonCurrencyCost in jsonUpgrade.currency_cost)
                    {
                        CurrencyCost currencyCost = new CurrencyCost();
                        currencyCost.Type = jsonCurrencyCost.type;
                        currencyCost.Amount = jsonCurrencyCost.amount;
                        upgrade.Cost.Add(currencyCost);
                    }

                    foreach(var jsonPrereqs in jsonUpgrade.prerequisite_requirements)
                    {
                        PrerequisiteReqirement req = new PrerequisiteReqirement();
                        req.TreeId = jsonPrereqs.tree_id;
                        req.RequirementCode = jsonPrereqs.requirement_code;
                        upgrade.Prerequisites.Add(req);
                    }
                    tree.Upgrades.Add(upgrade);
                }

                upgradeTrees.Add(tree);
            }
        }

        return upgradeTrees;
    }

    private List<UpgradeTree> GetJsonBuildingUpgradeTree()
    {
        List<UpgradeTree> upgradeTrees = new List<UpgradeTree>();

        foreach (var upgradeAsset in Resources.LoadAll<TextAsset>(JsonBuildingUpgradesPath))
        {
            var upgradeTreeSet = JsonDarkestDeserializer.GetJsonUpgradeTreeSet(upgradeAsset.text);

            foreach (var upgradeTree in upgradeTreeSet.trees)
            {
                UpgradeTree tree = new UpgradeTree();
                tree.Id = upgradeTree.id;
                tree.IsInstanced = upgradeTree.is_instanced;
                tree.Tags = new List<string>(upgradeTree.tags);

                foreach (var jsonUpgrade in upgradeTree.requirements)
                {
                    TownUpgrade upgrade = new TownUpgrade();
                    upgrade.Code = jsonUpgrade.code;

                    foreach (var jsonCurrencyCost in jsonUpgrade.currency_cost)
                    {
                        CurrencyCost currencyCost = new CurrencyCost();
                        currencyCost.Type = jsonCurrencyCost.type;
                        currencyCost.Amount = jsonCurrencyCost.amount;
                        upgrade.Cost.Add(currencyCost);
                    }

                    foreach (var jsonPrereqs in jsonUpgrade.prerequisite_requirements)
                    {
                        PrerequisiteReqirement req = new PrerequisiteReqirement();
                        req.TreeId = jsonPrereqs.tree_id;
                        req.RequirementCode = jsonPrereqs.requirement_code;
                        upgrade.Prerequisites.Add(req);
                    }
                    tree.Upgrades.Add(upgrade);
                }

                upgradeTrees.Add(tree);
            }
        }

        return upgradeTrees;
    }

    private TownActivity GetJsonTownActivity(JsonActivity jsonActivity, string activityName)
    {
        TownActivity activity = new TownActivity(activityName);
        activity.SideEffectChance = jsonActivity.side_effects.chance;
        foreach (var badQuirk in jsonActivity.quirk_library_names)
            activity.IncompatiableQuirks.Add(badQuirk);

        foreach(var jsonTownEffect in jsonActivity.side_effects.results)
        {
            switch(jsonTownEffect.type)
            {
                case "activity_lock":
                    ActivityLockTownEffect lockEffect = new ActivityLockTownEffect();
                    lockEffect.Chance = jsonTownEffect.chance;
                    activity.SideEffects.Add(lockEffect);
                    break;
                case "go_missing":
                    MissingTownEffect missEffect = new MissingTownEffect();
                    missEffect.Chance = jsonTownEffect.chance;
                    foreach(var missDuration in jsonTownEffect.data)
                    {
                        MissingDuration newDuration = new MissingDuration();
                        newDuration.Chance = (int)(long)missDuration["chance"];
                        newDuration.Duration = (int)(long)missDuration["duration"];
                        missEffect.Durations.Add(newDuration);
                    }
                    activity.SideEffects.Add(missEffect);
                    break;
                case "add_quirk":
                    AddQuirkTownEffect addQuirkEffect = new AddQuirkTownEffect();
                    addQuirkEffect.Chance = jsonTownEffect.chance;
                    foreach (var quirkRec in jsonTownEffect.data)
                    {
                        TownActivityQuirk newQuirk = new TownActivityQuirk();
                        newQuirk.Chance = (int)(long)quirkRec["chance"];
                        newQuirk.QuirkName = (string)quirkRec["quirk_library_name"];
                        addQuirkEffect.QuirkSet.Add(newQuirk);
                    }
                    activity.SideEffects.Add(addQuirkEffect);
                    break;
                case "change_currency":
                    CurrencyTownEffect currencyEffect = new CurrencyTownEffect();
                    currencyEffect.Chance = jsonTownEffect.chance;
                    foreach(var currChange in jsonTownEffect.data)
                    {
                        TownActivityCurrency newCurrency = new TownActivityCurrency();
                        newCurrency.Chance = (int)(long)currChange["chance"];
                        newCurrency.Type = (string)currChange["type"];
                        newCurrency.Amount = (int)(long)currChange["amount"];
                        currencyEffect.Changes.Add(newCurrency);
                    }
                    activity.SideEffects.Add(currencyEffect);
                    break;
                case "apply_buff":
                    AddBuffTownEffect buffEffect = new AddBuffTownEffect();
                    buffEffect.Chance = jsonTownEffect.chance;
                    foreach(var buffSet in jsonTownEffect.data)
                    {
                        TownActivityBuff activityBuffSet = new TownActivityBuff();
                        activityBuffSet.Chance = (int)(long)buffSet["chance"];
                        string[] buffNames = buffSet["buff_library_ids"].ToString().Split(
                            new[] { '[', ']', ',', '\r', '\n', '\"', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        activityBuffSet.BuffNames = new List<string>(buffNames);
                        buffEffect.BuffSets.Add(activityBuffSet);
                    }
                    activity.SideEffects.Add(buffEffect);
                    break;
                case "add_trinket":
                    AddTrinketTownEffect addTrinketEffect = new AddTrinketTownEffect();
                    addTrinketEffect.Chance = jsonTownEffect.chance;
                    activity.SideEffects.Add(addTrinketEffect);
                    break;
                case "remove_trinket":
                    RemoveTrinketTownEffect removeTrinketEffect = new RemoveTrinketTownEffect();
                    removeTrinketEffect.Chance = jsonTownEffect.chance;
                    activity.SideEffects.Add(removeTrinketEffect);
                    break;
                default:
                    Debug.LogError("Missing activity town effect type: " + jsonTownEffect.type);
                    break;
            }
        }

        foreach(var jsonCostUpgrade in jsonActivity.cost_upgrades)
        {
            if (jsonCostUpgrade.upgrade_requirement_code == null)
            {
                activity.ActivityCost = new CurrencyCost(jsonCostUpgrade.cost_currency.type, jsonCostUpgrade.cost_currency.amount);
                activity.BaseCost = activity.ActivityCost;
            }
            else
            {
                CostUpgrade newCostUpgrade = new CostUpgrade();
                newCostUpgrade.Cost = new CurrencyCost(jsonCostUpgrade.cost_currency.type, jsonCostUpgrade.cost_currency.amount);
                newCostUpgrade.UpgradeCode = jsonCostUpgrade.upgrade_requirement_code;
                activity.CostUpgrades.Add(newCostUpgrade);
            }
        }

        foreach (var jsonSlotUpgrade in jsonActivity.slot_upgrades)
        {
            if (jsonSlotUpgrade.upgrade_requirement_code == null)
            {
                activity.NumberOfSlots = jsonSlotUpgrade.number_of_slots;
                activity.BaseSlots = activity.NumberOfSlots;
            }
            else
            {
                SlotUpgrade newSlotUpgrade = new SlotUpgrade();
                newSlotUpgrade.NumberOfSlots = jsonSlotUpgrade.number_of_slots;
                newSlotUpgrade.UpgradeCode = jsonSlotUpgrade.upgrade_requirement_code;
                activity.SlotUpgrades.Add(newSlotUpgrade);
            }
        }

        foreach (var jsonStressUpgrade in jsonActivity.stress_upgrades)
        {
            if (jsonStressUpgrade.upgrade_requirement_code == null)
            {
                activity.StressHealAmount = jsonStressUpgrade.heal_high;
                activity.BaseStressHeal = activity.StressHealAmount;
            }
            else
            {
                StressUpgrade newStressUpgrade = new StressUpgrade();
                newStressUpgrade.StressHeal = jsonStressUpgrade.heal_high;
                newStressUpgrade.UpgradeCode = jsonStressUpgrade.upgrade_requirement_code;
                activity.StressUpgrades.Add(newStressUpgrade);
            }
        }

        return activity;
    }

    private List<Building> GetJsonBuildings()
    {
        List<Building> buildings = new List<Building>();

        JsonAbbey jsonAbbey = JsonDarkestDeserializer.GetJsonAbbey(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "abbey.building").text);

        Abbey abbey = new Abbey();
        abbey.Activities.Add(GetJsonTownActivity(jsonAbbey.meditation, "abbey.meditation"));
        abbey.Activities.Add(GetJsonTownActivity(jsonAbbey.prayer, "abbey.prayer"));
        abbey.Activities.Add(GetJsonTownActivity(jsonAbbey.flagellation, "abbey.flagellation"));
        buildings.Add(abbey);

        JsonTavern jsonTavern = JsonDarkestDeserializer.GetJsonTavern(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "tavern.building").text);

        Tavern tavern = new Tavern();
        tavern.Activities.Add(GetJsonTownActivity(jsonTavern.bar, "tavern.bar"));
        tavern.Activities.Add(GetJsonTownActivity(jsonTavern.gambling, "tavern.gambling"));
        tavern.Activities.Add(GetJsonTownActivity(jsonTavern.brothel, "tavern.brothel"));
        buildings.Add(tavern);

        #region Sanitarium
        JsonSanitarium jsonSanitarium = JsonDarkestDeserializer.GetJsonSanitarium(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "sanitarium.building").text);

        Sanitarium sanitarium = new Sanitarium();
        sanitarium.QuirkActivity = new QuirkTreatmentActivity();
        sanitarium.QuirkActivity.QuirkTreatmentChance = jsonSanitarium.treatment.quirk_treatment_chance;

        foreach(var jsonUpgrade in jsonSanitarium.treatment.positive_quirk_cost_upgrades)
        {
            if(jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.QuirkActivity.BasePositiveQuirkCost = 
                    new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                sanitarium.QuirkActivity.PositiveQuirkCost = sanitarium.QuirkActivity.BasePositiveQuirkCost;
            }
            else
            {
                CostUpgrade newUpgrade = new CostUpgrade();
                newUpgrade.Cost = new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.QuirkActivity.PositiveQuirkUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonUpgrade in jsonSanitarium.treatment.negative_quirk_cost_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.QuirkActivity.BaseNegativeQuirkCost = 
                    new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                sanitarium.QuirkActivity.NegativeQuirkCost = sanitarium.QuirkActivity.BaseNegativeQuirkCost;
            }
            else
            {
                CostUpgrade newUpgrade = new CostUpgrade();
                newUpgrade.Cost = new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.QuirkActivity.NegativeQuirkUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonUpgrade in jsonSanitarium.treatment.permanent_negative_quirk_cost_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.QuirkActivity.BasePermNegativeCost = 
                    new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                sanitarium.QuirkActivity.PermNegativeQuirkCost = sanitarium.QuirkActivity.BasePermNegativeCost;
            }
            else
            {
                CostUpgrade newUpgrade = new CostUpgrade();
                newUpgrade.Cost = new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.QuirkActivity.PermNegativeUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonUpgrade in jsonSanitarium.treatment.slot_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.QuirkActivity.BaseQuirkSlots = jsonUpgrade.number_of_slots;
                sanitarium.QuirkActivity.QuirkSlots = sanitarium.QuirkActivity.BaseQuirkSlots;
            }
            else
            {
                SlotUpgrade newUpgrade = new SlotUpgrade();
                newUpgrade.NumberOfSlots = jsonUpgrade.number_of_slots;
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.QuirkActivity.SlotUpgrades.Add(newUpgrade);
            }
        }

        sanitarium.DiseaseActivity = new DiseaseTreatmentActivity();
        sanitarium.DiseaseActivity.DiseaseTreatmentChance = jsonSanitarium.disease_treatment.quirk_treatment_chance;

        foreach (var jsonUpgrade in jsonSanitarium.disease_treatment.disease_quirk_cost_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.DiseaseActivity.BaseDiseaseTreatmentCost = 
                    new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                sanitarium.DiseaseActivity.DiseaseTreatmentCost = sanitarium.DiseaseActivity.BaseDiseaseTreatmentCost;
            }
            else
            {
                CostUpgrade newUpgrade = new CostUpgrade();
                newUpgrade.Cost = new CurrencyCost(jsonUpgrade.cost_currency.type, jsonUpgrade.cost_currency.amount);
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.DiseaseActivity.DiseaseTreatmentUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonUpgrade in jsonSanitarium.disease_treatment.disease_quirk_cure_all_chance_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.DiseaseActivity.BaseCureAllChance = jsonUpgrade.chance;
                sanitarium.DiseaseActivity.CureAllChance = sanitarium.DiseaseActivity.BaseCureAllChance;
            }
            else
            {
                ChanceUpgrade newUpgrade = new ChanceUpgrade();
                newUpgrade.Chance = jsonUpgrade.chance;
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.DiseaseActivity.CureAllUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonUpgrade in jsonSanitarium.disease_treatment.slot_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                sanitarium.DiseaseActivity.BaseDiseaseSlots = jsonUpgrade.number_of_slots;
                sanitarium.DiseaseActivity.DiseaseSlots = sanitarium.DiseaseActivity.BaseDiseaseSlots;
            }
            else
            {
                SlotUpgrade newUpgrade = new SlotUpgrade();
                newUpgrade.NumberOfSlots = jsonUpgrade.number_of_slots;
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                sanitarium.DiseaseActivity.SlotUpgrades.Add(newUpgrade);
            }
        }

        buildings.Add(sanitarium);
        #endregion

        JsonBlacksmith jsonBlacksmith = JsonDarkestDeserializer.GetJsonBlacksmith(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "blacksmith.building").text);

        Blacksmith blacksmith = new Blacksmith();
        foreach(var jsonDiscUpgrade in jsonBlacksmith.equipment_cost_discount_upgrades)
        {
            DiscountUpgrade discountUpgrade = new DiscountUpgrade();
            discountUpgrade.Percent = jsonDiscUpgrade.discount_percent;
            discountUpgrade.TreeId = jsonDiscUpgrade.upgrade_tree_id;
            discountUpgrade.UpgradeCode = jsonDiscUpgrade.upgrade_requirement_code;
            blacksmith.DiscountUpgrades.Add(discountUpgrade);
        }
        buildings.Add(blacksmith);

        JsonGuild jsonGuild = JsonDarkestDeserializer.GetJsonGuild(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "guild.building").text);

        Guild guild = new Guild();
        foreach (var jsonDiscUpgrade in jsonGuild.combat_skill_cost_discount_upgrades)
        {
            DiscountUpgrade discountUpgrade = new DiscountUpgrade();
            discountUpgrade.Percent = jsonDiscUpgrade.discount_percent;
            discountUpgrade.TreeId = jsonDiscUpgrade.upgrade_tree_id;
            discountUpgrade.UpgradeCode = jsonDiscUpgrade.upgrade_requirement_code;
            guild.DiscountUpgrades.Add(discountUpgrade);
        }
        buildings.Add(guild);

        JsonCampingTrainer jsonCampingTrainer = JsonDarkestDeserializer.GetJsonCamp(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "camping_trainer.building").text);

        CampingTrainer campingTrainer = new CampingTrainer();
        foreach (var jsonDiscUpgrade in jsonCampingTrainer.camping_skill_cost_discount_upgrades)
        {
            DiscountUpgrade discountUpgrade = new DiscountUpgrade();
            discountUpgrade.Percent = jsonDiscUpgrade.discount_percent;
            discountUpgrade.TreeId = jsonDiscUpgrade.upgrade_tree_id;
            discountUpgrade.UpgradeCode = jsonDiscUpgrade.upgrade_requirement_code;
            campingTrainer.DiscountUpgrades.Add(discountUpgrade);
        }
        buildings.Add(campingTrainer);

        JsonNomadWagon jsonNomadWagon = JsonDarkestDeserializer.GetJsonWagon(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "nomad_wagon.building").text);

        NomadWagon nomadWagon = new NomadWagon();
        foreach (var jsonUpgrade in jsonNomadWagon.number_of_trinkets_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                nomadWagon.BaseTrinketSlots = jsonUpgrade.number_of_slots;
                nomadWagon.TrinketSlots = nomadWagon.BaseTrinketSlots;
            }
            else
            {
                SlotUpgrade newUpgrade = new SlotUpgrade();
                newUpgrade.NumberOfSlots = jsonUpgrade.number_of_slots;
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                nomadWagon.TrinketSlotUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonDiscUpgrade in jsonNomadWagon.trinket_cost_discount_upgrades)
        {
            DiscountUpgrade discountUpgrade = new DiscountUpgrade();
            discountUpgrade.Percent = jsonDiscUpgrade.discount_percent;
            discountUpgrade.TreeId = jsonDiscUpgrade.upgrade_tree_id;
            discountUpgrade.UpgradeCode = jsonDiscUpgrade.upgrade_requirement_code;
            nomadWagon.DiscountUpgrades.Add(discountUpgrade);
        }
        buildings.Add(nomadWagon);


        JsonStageCoach jsonStageCoach = JsonDarkestDeserializer.GetJsonCoach(
            Resources.Load<TextAsset>(JsonBuildingDataPath + "stage_coach.building").text);
        
        StageCoach stageCoach = new StageCoach();

        foreach (var jsonUpgrade in jsonStageCoach.number_of_recruits_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                stageCoach.BaseRecruitSlots = jsonUpgrade.number_of_slots;
                stageCoach.RecruitSlots = stageCoach.BaseRecruitSlots;
            }
            else
            {
                SlotUpgrade newUpgrade = new SlotUpgrade();
                newUpgrade.NumberOfSlots = jsonUpgrade.number_of_slots;
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                stageCoach.RecruitSlotUpgrades.Add(newUpgrade);
            }
        }


        foreach (var jsonUpgrade in jsonStageCoach.roster_size_upgrades)
        {
            if (jsonUpgrade.upgrade_requirement_code == null)
            {
                stageCoach.BaseRosterSlots = jsonUpgrade.number_of_slots;
                stageCoach.RosterSlots = stageCoach.BaseRosterSlots;
            }
            else
            {
                SlotUpgrade newUpgrade = new SlotUpgrade();
                newUpgrade.NumberOfSlots = jsonUpgrade.number_of_slots;
                newUpgrade.TreeId = jsonUpgrade.upgrade_tree_id;
                newUpgrade.UpgradeCode = jsonUpgrade.upgrade_requirement_code;
                stageCoach.RosterSlotUpgrades.Add(newUpgrade);
            }
        }

        foreach (var jsonUpgrade in jsonStageCoach.upgraded_recruits_upgrades)
        {
            RecruitUpgrade newUpgrade = new RecruitUpgrade
            {
                Level = jsonUpgrade.level,
                Chance = jsonUpgrade.chance,
                ExtraCampingSkills = jsonUpgrade.number_of_extra_camping_skills,
                ExtraCombatSkills = jsonUpgrade.number_of_extra_combat_skills,
                ExtraNegativeQuirks = jsonUpgrade.number_of_extra_negative_quirks,
                ExtraPositiveQuirks = jsonUpgrade.number_of_extra_positive_quirks,
                TreeId = jsonUpgrade.upgrade_tree_id,
                UpgradeCode = jsonUpgrade.upgrade_requirement_code,
            };
            stageCoach.RecruitExperienceUpgrades.Add(newUpgrade);
        }

        buildings.Add(stageCoach);

        return buildings;
    }

    #endregion

    private void LoadHeirloomExchanges()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonExchangePath);
        var jsonExchange = JsonDarkestDeserializer.GetJsonObject<JsonHeirloomExchange>(jsonText.text);
        HeirloomExchanges = new List<HeirloomExchange>();

        for (int i = 0; i < jsonExchange.markets[0].exchange_rates.Count; i++)
        {
            var newHeirloomExchange = new HeirloomExchange();
            newHeirloomExchange.FromType = jsonExchange.markets[0].exchange_rates[i].exchange_from_type;
            newHeirloomExchange.FromAmount = jsonExchange.markets[0].exchange_rates[i].exchange_from_amount;
            newHeirloomExchange.ToType = jsonExchange.markets[0].exchange_rates[i].exchange_to_type;
            newHeirloomExchange.ToAmount = jsonExchange.markets[0].exchange_rates[i].exchange_to_amount;
            HeirloomExchanges.Add(newHeirloomExchange);
        }
    }

    private void LoadPartyNames()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonPartyNameDataPath);
        var jsonPartyNames = JsonDarkestDeserializer.GetJsonPartyNames(jsonText.text);
        PartyNames = new List<PartyNameEntry>();
        for(int i = 0; i < jsonPartyNames.party_names.Count; i++)
        {
            var newPartyComp = new PartyNameEntry();
            newPartyComp.Id = jsonPartyNames.party_names[i].id;
            newPartyComp.ClassIds = jsonPartyNames.party_names[i].required_hero_class;
            PartyNames.Add(newPartyComp);
        }
    }

    private void LoadNarration()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(JsonNarrationDataPath);
        var jsonNarration = JsonDarkestDeserializer.GetJsonNarration(jsonText.text);
        Narration = new Dictionary<string, NarrationEntry>();
        foreach(var jsonNarrationEntry in jsonNarration.entries)
        {
            NarrationEntry narrationEntry = new NarrationEntry();
            narrationEntry.Id = jsonNarrationEntry.id;
            narrationEntry.Chance = jsonNarrationEntry.chance;
            narrationEntry.Tone = jsonNarrationEntry.tone;
            foreach(var jsonAudioEvent in jsonNarrationEntry.audio_events)
            {
                NarrationAudioEvent audioEvent = new NarrationAudioEvent();
                audioEvent.QueueOnlyOnEmpty = jsonAudioEvent.queue_only_on_empty;
                audioEvent.QueueWhilePlaying = jsonAudioEvent.queue_while_audio_playing;
                audioEvent.AudioEvent = jsonAudioEvent.audio_event;
                audioEvent.Chance = jsonAudioEvent.chance;
                audioEvent.Priority = jsonAudioEvent.priority;
                audioEvent.MaxRaidOccurrences = jsonAudioEvent.max_raid_occurrences;
                audioEvent.MaxTownVisitOccurrences = jsonAudioEvent.max_town_visit_occurrences;
                audioEvent.MaxCampaignOccurrences = jsonAudioEvent.max_campaign_occurrences;
                audioEvent.Filter = jsonAudioEvent.filter;
                audioEvent.CheckAllTags = jsonAudioEvent.check_all_tags;
                audioEvent.Tags = jsonAudioEvent.tags;
                narrationEntry.AudioEvents.Add(audioEvent);
            }
            Narration.Add(narrationEntry.Id, narrationEntry);
        }
    }

    private void LoadJsonAI()
    {
        Brains = new Dictionary<string, MonsterBrain>();

        foreach(var brain in GetJsonMonsterBrains())
        {
            if (Brains.ContainsKey(brain.Id))
                Debug.LogError("Same brains id: " + brain.Id);
            else
                Brains.Add(brain.Id, brain);
        }
    }

    private void LoadJsonBuffs()
    {
        Buffs = new Dictionary<string, Buff>();

        foreach (var buff in GetJsonBuffLibrary())
        {
            if (Buffs.ContainsKey(buff.Id))
                Debug.LogError("Same buff name: " + buff.Id);
            else
                Buffs.Add(buff.Id, buff);
        }
    }

    private void LoadJsonTrinkets()
    {
        Items = new Dictionary<string, Dictionary<string, ItemData>>();

        if (!Items.ContainsKey("trinket"))
            Items.Add("trinket", new Dictionary<string, ItemData>());

        foreach(var trinket in GetJsonTrinketLibrary())
        {
            if (Items[trinket.Type].ContainsKey(trinket.Id))
                Debug.Log("Same trinket: " + trinket.Id);
            else
                Items[trinket.Type].Add(trinket.Id, trinket);
        }
    }

    private void LoadJsonQuirks()
    {
        Quirks = new Dictionary<string, Quirk>();

        foreach (var quirk in GetJsonQuirkLibrary())
        {
            if (Quirks.ContainsKey(quirk.Id))
                Debug.LogError("Same quirk: " + quirk.Id);
            else
                Quirks.Add(quirk.Id, quirk);
        }
    }

    private void LoadJsonCampingSkills()
    {
        CampingSkills = new List<CampingSkill>();

        TextAsset jsonText = Resources.Load<TextAsset>(JsonCampingPath);

        foreach(var jsonCampSkill in JsonDarkestDeserializer.GetJsonCamping(jsonText.text).skills)
        {
            CampingSkill campSkill = new CampingSkill();
            campSkill.Id = jsonCampSkill.id;
            campSkill.TimeCost = jsonCampSkill.cost;
            campSkill.Limit = jsonCampSkill.use_limit;

            campSkill.Classes = new List<string>(jsonCampSkill.hero_classes);
            campSkill.CurrencyCost = new CurrencyCost(jsonCampSkill.upgrade_requirements[0].currency_cost[0].type,
                jsonCampSkill.upgrade_requirements[0].currency_cost[0].amount);

            foreach(var jsonCampEffect in jsonCampSkill.effects)
            {
                CampEffect campEffect = new CampEffect();

                campEffect.Selection = CampingSkillHelper.StringToCampTargetType(jsonCampEffect.selection);
                campEffect.Type = CampingSkillHelper.StringToCampEffectType(jsonCampEffect.type);
                campEffect.Subtype = jsonCampEffect.sub_type;
                campEffect.Amount = jsonCampEffect.amount;
                campEffect.Code = jsonCampEffect.chance.code;

                if (jsonCampEffect.requirements.Count > 0)
                    campEffect.Requirement = CampingSkillHelper.StringToCampEffectRequirement(jsonCampEffect.requirements[0]);
                else if (jsonCampEffect.requirements.Count > 1)
                    Debug.LogError("Multiple requirement in camping skill: " + campSkill.Id);
                campEffect.Chance = jsonCampEffect.chance.amount;

                campSkill.Effects.Add(campEffect);
            }
            CampingSkills.Add(campSkill);
        }
    }

    private void LoadJsonUpgrades()
    {
        UpgradeTrees = new Dictionary<string, UpgradeTree>();

        foreach (var tree in GetJsonBuildingUpgradeTree())
            UpgradeTrees.Add(tree.Id, tree);

        foreach (var tree in GetJsonHeroUpgradeTree())
            UpgradeTrees.Add(tree.Id, tree);
    }

    private void LoadJsonTownEvents()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(TownEventsDataPath);
        var jsonEvents = JsonDarkestDeserializer.GetJsonTownEvents(jsonText.text);

        EventDatabase = new TownEventDatabase();
        for(int i = 0; i < jsonEvents.settings.Count; i++)
        {
            var eventOption = new TownEventOption();
            eventOption.Id = jsonEvents.settings[i].id;
            eventOption.Frequency = jsonEvents.settings[i].event_chance_per_town_visits;
            EventDatabase.Settings.Add(eventOption);
        }

        for (int i = 0; i < jsonEvents.quest_type_event_guarantees.Count; i++)
        {
            var eventGuarantee = new TownEventGuarantee();
            eventGuarantee.Dungeon = jsonEvents.quest_type_event_guarantees[i].dungeon_type;
            eventGuarantee.QuestType = jsonEvents.quest_type_event_guarantees[i].quest_type;
            eventGuarantee.EventId = jsonEvents.quest_type_event_guarantees[i].event_id;
            EventDatabase.Guarantees.Add(eventGuarantee);
        }

        foreach(var jsonEvent in jsonEvents.events)
        {
            var townEvent = new TownEvent();

            townEvent.Id = jsonEvent.id;
            
            switch(jsonEvent.tone)
            {
                case "good":
                    townEvent.Tone = TownEventTone.Good;
                    break;
                case "bad":
                    townEvent.Tone = TownEventTone.Bad;
                    break;
                case "neutral":
                    townEvent.Tone = TownEventTone.Neutral;
                    break;
                default:
                    townEvent.Tone = TownEventTone.Neutral;
                    Debug.LogError("Unknown event tone: " + jsonEvent.tone);
                    break;
            }

            townEvent.Cooldown = jsonEvent.cooldown;
            townEvent.ChancePerNotRolled = jsonEvent.per_not_rolled_additional_chance;
            townEvent.Chance = jsonEvent.base_chance;

            townEvent.MinimumWeek = jsonEvent.requirements.minimum_week;
            townEvent.DeadHeroes = jsonEvent.requirements.dead_heroes;
            for (int i = 0; i < jsonEvent.requirements.hero_level_counts.Count; i++)
                townEvent.LevelHeroes.Add(jsonEvent.requirements.hero_level_counts[i].level,
                    jsonEvent.requirements.hero_level_counts[i].count);
            for (int i = 0; i < jsonEvent.requirements.upgrades_purchased.Count; i++)
                townEvent.Purchases.Add(jsonEvent.requirements.upgrades_purchased[i].tree_id,
                    jsonEvent.requirements.upgrades_purchased[i].requrement_code);

            townEvent.AmbienceParameters = jsonEvent.town_ambience_paramater_ids;
            townEvent.Sprite = jsonEvent.sprite;
            townEvent.SpriteAttachment = jsonEvent.sprite_attachment;

            foreach(var jsonEventData in jsonEvent.data)
            {
                var townEventData = new TownEventData();
                townEventData.StringData = jsonEventData.string_data;
                townEventData.NumberData = jsonEventData.number_data;

                switch(jsonEventData.type)
                {
                    case "embark_party_buff":
                        townEventData.Type = TownEventDataType.EmbarkPartyBuff;
                        break;
                    case "idle_resolve_level":
                        townEventData.Type = TownEventDataType.IdleResolve;
                        break;
                    case "bonus_recruit":
                        townEventData.Type = TownEventDataType.BonusRecruit;
                        break;
                    case "in_activity_buff":
                        townEventData.Type = TownEventDataType.InActivityBuff;
                        break;
                    case "activity_lock":
                        townEventData.Type = TownEventDataType.ActivityLock;
                        break;
                    case "activity_cost_change":
                        townEventData.Type = TownEventDataType.ActivityCostChange;
                        break;
                    case "provision_item_type_cost_change":
                        townEventData.Type = TownEventDataType.ProvisionTypeCostChange;
                        break;
                    case "provision_item_type_amount_change":
                        townEventData.Type = TownEventDataType.ProvisionTypeAmountChange;
                        break;
                    case "upgrade_tag_discount":
                        townEventData.Type = TownEventDataType.UpgradeTagDiscount;
                        break;
                    case "free_activity":
                        townEventData.Type = TownEventDataType.FreeActivity;
                        break;
                    case "dead_recruit":
                        townEventData.Type = TownEventDataType.DeadRecruit;
                        break;
                    case "remove_quest_hero_level_restriction":
                        townEventData.Type = TownEventDataType.NoLevelRestriction;
                        break;
                    case "upgrade_tag_free":
                        townEventData.Type = TownEventDataType.UpgradeTagFree;
                        break;
                    case "idle_buff":
                        townEventData.Type = TownEventDataType.IdleBuff;
                        break;
                    case "plot_quest":
                        townEventData.Type = TownEventDataType.PlotQuest;
                        break;
                    default:
                        Debug.LogError("Unknown event data type: " + jsonEventData.type);
                        break;
                }
                townEvent.Data.Add(townEventData);
            }

            EventDatabase.Events.Add(townEvent);
        }
    }

    private void LoadJsonBuildings()
    {
        Buildings = new Dictionary<string, Building>();

        foreach (var building in GetJsonBuildings())
            Buildings.Add(building.Name, building);
    }

    private void LoadJsonHeroClasses()
    {
        HeroClasses = new Dictionary<string, HeroClass>();

        foreach (var heroAsset in Resources.LoadAll<TextAsset>(HeroesDirectory + "Info/"))
        {
            List<string> heroData = heroAsset.text.Split(
                new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            HeroClass heroClass = new HeroClass(heroData);
            HeroClasses.Add(heroClass.StringId, heroClass);
        }
    }

    private void LoadJsonQuests()
    {
        QuestDatabase = GetJsonQuestDatabase();
    }

    private void LoadJsonLoot()
    {
        LootDatabase = GetJsonLootDatabase();
    }

    private void LoadJsonObstacles()
    {
        Obstacles = new Dictionary<string, Obstacle>();

        foreach (var obstacle in GetJsonObstaclesLibrary())
            Obstacles.Add(obstacle.StringId, obstacle);
    }

    private void LoadJsonTraps()
    {
        Traps = new Dictionary<string, Trap>();

        foreach (var trap in GetJsonTrapLibrary())
            Traps.Add(trap.StringId, trap);
    }

    private void LoadCsvCurios()
    {
        Curios = new Dictionary<string, Curio>();
        string[,] curioGrid = CsvReader.SplitCsvGrid(Resources.Load<TextAsset>(CsvCurioDatabasePath).text);
        for(int i = 2; i < curioGrid.GetLength(0); i += 15)
        {
            Curio curio = new Curio(curioGrid[i + 2, 2]);
            curio.ResultTypes = curioGrid[i, 4].ToLower();
            curio.RegionFound = curioGrid[i + 4, 2].ToLower();
            curio.IsFullCurio = curioGrid[i + 6, 2] == "Yes";
            if (curioGrid[i + 8, 2] != "")
                curio.Tags.Add(curioGrid[i + 8, 2].ToLower());
            if (curioGrid[i + 8, 3] != "")
                curio.Tags.Add(curioGrid[i + 8, 3].ToLower());
            if (curioGrid[i + 9, 2] != "")
                curio.Tags.Add(curioGrid[i + 9, 2].ToLower());
            if (curioGrid[i + 9, 3] != "")
                curio.Tags.Add(curioGrid[i + 9, 3].ToLower());

            #region Curio Results
            for (int resultIndex = 0; resultIndex < 8; resultIndex++ )
            {
                if (curioGrid[i + 2 + resultIndex, 5] != null 
                    && curioGrid[i + 2 + resultIndex, 5] != "")
                {
                    CurioInteraction interaction = new CurioInteraction();
                    interaction.ResultType = curioGrid[i + 2 + resultIndex, 4].ToLower();
                    interaction.Chance = int.Parse(curioGrid[i + 2 + resultIndex, 5]);

                    for (int typeIndex = 0; typeIndex < 3; typeIndex++)
                    {
                        if (curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3] != null
                            && curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3] != ""
                            && curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3] != "N/A")
                        {
                            CurioResult curioResult = new CurioResult();
                            curioResult.Item = curioGrid[i + 2 + resultIndex, 7 + typeIndex * 3];
                            curioResult.Chance = int.Parse(curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3]);
                            if (curioGrid[i + 2 + resultIndex, 9 + typeIndex * 3] == "<- # Draws")
                            {
                                curioResult.Draws = curioResult.Chance;
                                curioResult.IsCombined = true;
                            }
                            else
                                curioResult.Draws = 1;
                            interaction.Results.Add(curioResult);
                        }
                    }

                    curio.Results.Add(interaction);
                }
            }        
            #endregion

            #region Item Interactions
            for (int interactIndex = 0; interactIndex < 3 && i + 11 + interactIndex < curioGrid.GetLength(0); interactIndex++)
            {
                if (curioGrid[i + 11 + interactIndex, 4] != null && curioGrid[i + 11 + interactIndex, 4] != "")
                {
                    ItemInteraction itemInteraction = new ItemInteraction();
                    itemInteraction.ItemId = curioGrid[i + 11 + interactIndex, 4];
                    itemInteraction.ResultType = curioGrid[i + 11 + interactIndex, 5].ToLower();

                    for (int itemIndex = 0; itemIndex < 3; itemIndex++)
                    {
                        if (curioGrid[i + 11 + interactIndex, 7 + itemIndex * 3] != null &&
                            curioGrid[i + 11 + interactIndex, 7 + itemIndex * 3] != "")
                        {
                            CurioResult curioResult = new CurioResult();
                            curioResult.Item = curioGrid[i + 11 + interactIndex, 7 + itemIndex * 3];
                            curioResult.Chance = int.Parse(curioGrid[i + 11 + interactIndex, 8 + itemIndex * 3]);
                            if (curioGrid[i + 11 + interactIndex, 9 + itemIndex * 3] == "<- # Draws")
                            {
                                curioResult.Draws = curioResult.Chance;
                                curioResult.IsCombined = true;
                            }
                            else
                                curioResult.Draws = 1;

                            itemInteraction.Results.Add(curioResult);
                        }
                    }
                    curio.ItemInteractions.Add(itemInteraction);
                }
            }
            #endregion

            Curios.Add(curio.StringId, curio);
        }
    }

    private void LoadTraits()
    {
        Traits = new List<Trait>();
        var traitsAsset = Resources.Load<TextAsset>(JsonTraitDatabasePath);
        foreach(var jsonTrait in JsonDarkestDeserializer.GetJsonTraits(traitsAsset.text).traits)
        {
            Trait trait = new Trait();
            trait.Id = jsonTrait.id;
            trait.Type = CharacterHelper.StringToOverstress(jsonTrait.overstress_type);
            trait.CurioTag = jsonTrait.curio_tag;
            trait.TagChance = jsonTrait.curio_tag_chance;
            trait.KeepLoot = jsonTrait.keep_loot;

            for(int i = 0; i < jsonTrait.buff_ids.Count; i++)
                trait.Buffs.Add(Buffs[jsonTrait.buff_ids[i]]);

            foreach(var jsonStartAct in jsonTrait.combat_start_turn_act_outs)
            {
                CombatStartTurnActOut actOut = new CombatStartTurnActOut();
                actOut.ActType = CharacterHelper.StringToStartTurnAct(jsonStartAct.id);
                actOut.NumberParameter = jsonStartAct.data.number_value;
                actOut.StringParameter = jsonStartAct.data.string_value;
                actOut.Chance = jsonStartAct.chance;
                trait.StartTurnActs.Add(actOut);
            }

            foreach(var jsonReaction in jsonTrait.reaction_act_outs)
            {
                ReactionActOut reaction = new ReactionActOut();
                reaction.ActType = CharacterHelper.StringToReactionType(jsonReaction.id);
                reaction.Effect = jsonReaction.data.effect != "" ? Effects[jsonReaction.data.effect] : null;
                reaction.Chance = jsonReaction.chance;
                trait.Reactions.Add(reaction.ActType, reaction);
            }
            Traits.Add(trait);
        }
    }

    private void LoadEffects()
    {
        Effects = new Dictionary<string, Effect>();

        var effectsAsset = Resources.Load<TextAsset>(EffectsDataPath);

        List<string> effectsStrings = effectsAsset.text.Split(
            new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        effectsStrings.RemoveAll(item => !item.StartsWith("effect:"));
        
        foreach(var effectString in effectsStrings)
        {
            List<string> effectData = Regex.Matches(effectString.Replace("\t"," "),
                @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();
            for (int i = 0; i < effectData.Count; i++)
                effectData[i] = effectData[i].Replace("\"", "").Replace("%", "");
            Effect effect = new Effect(effectData);
            if (Effects.ContainsKey(effect.Name))
                Debug.LogError("Same effect: " + effect.Name);
            else
                Effects.Add(effect.Name, effect);
        }
    }

    private void LoadItems()
    {
        var itemData = Resources.Load<TextAsset>(ItemDataPath).text;
        foreach(var item in itemData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            List<string> splitItem = item.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ItemData newItemData = new ItemData();

            newItemData.Type = splitItem[2].Replace("\"", "");
            newItemData.Id = splitItem[4].Replace("\"", "");
            newItemData.StackLimit = int.Parse(splitItem[6]);
            newItemData.PurchasePrice = int.Parse(splitItem[8]);
            newItemData.SellPrice = int.Parse(splitItem[10]);

            if (!Items.ContainsKey(newItemData.Type))
                Items.Add(newItemData.Type, new Dictionary<string, ItemData>());
            Items[newItemData.Type].Add(newItemData.Id, newItemData);
        }
    }

    private void LoadProvision()
    {
        Provision = GetJsonProvisionDatabase();
    }

    private void LoadDungeons()
    {
        LoadDungeonGenerationData();

        DungeonEnviromentData = new Dictionary<string, DungeonEnviromentData>();
        LoadDungeonEnviroment("crypts", CryptsEnviromentDataPath);
        LoadDungeonEnviroment("warrens", WarrensEnviromentDataPath);
        LoadDungeonEnviroment("weald", WealdEnviromentDataPath);
        LoadDungeonEnviroment("cove", CoveEnviromentDataPath);
        LoadDungeonEnviroment("darkestdungeon", DarkestEnviromentDataPath);
        LoadDungeonEnviroment("town", TownEnviromentDataPath);
        LoadDungeonEnviroment("shared", SharedEnviromentDataPath);
    }

    private void LoadSprites()
    {
        DungeonSprites = new Dictionary<string, Sprite>();
        Sprites = new Dictionary<string, Sprite>();
    }

    private void LoadColours()
    {
        HexColors = new Dictionary<string,string>();

        HexColors.Add("neutral", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(154, 152, 143, 255)));
        HexColors.Add("notable", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(200, 180, 110, 255)));
        HexColors.Add("harmful", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(177, 25, 0, 255)));
        HexColors.Add("disease", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(121, 141, 69, 255)));

        HexColors.Add("kickstarter", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(77, 191, 122, 255)));
        HexColors.Add("trophy", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(222, 182, 105, 255)));
        HexColors.Add("darkest_dungeon", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(224, 39, 0, 255)));

        HexColors.Add("ancestral_shambler", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(224, 39, 0, 255)));
        HexColors.Add("ancestral", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(224, 39, 0, 255)));
        HexColors.Add("madman", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(224, 39, 0, 255)));
        HexColors.Add("collector", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(224, 39, 0, 255)));
        HexColors.Add("uncommon", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(96, 149, 75, 255)));
        HexColors.Add("common", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(168, 168, 168, 255)));
        HexColors.Add("very_common", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(168, 168, 168, 255)));
        HexColors.Add("rare", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(59, 97, 167, 255)));
        HexColors.Add("very_rare", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(224, 39, 0, 255)));

        HexColors.Add("town_quest_difficulty_1", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(169, 217, 102, 255)));
        HexColors.Add("town_quest_difficulty_3", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(233, 170, 98, 255)));
        HexColors.Add("town_quest_difficulty_5", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(255, 87, 49, 255)));
        HexColors.Add("town_quest_difficulty_6", HexColors["harmful"]);

        HexColors.Add("equipment_level_0", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(215, 213, 205, 255)));
        HexColors.Add("equipment_level_1", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(215, 213, 205, 255)));
        HexColors.Add("equipment_level_2", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(105, 190, 75, 255)));
        HexColors.Add("equipment_level_3", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(62, 114, 212, 255)));
        HexColors.Add("equipment_level_4", HexColors["harmful"]);

        HexColors.Add("equipment_level", HexColors["notable"]);
        HexColors.Add("equipment_tooltip_title", HexColors["notable"]);
        HexColors.Add("equipment_tooltip_body", HexColors["neutral"]);
        HexColors.Add("inventory_tooltip_gold_value", HexColors["notable"]);  
  
        HexColors.Add("pop_text_stress_reduce", "#fdf8c7");
        HexColors.Add("pop_text_outline_stress_reduce", "#7c663f");
        HexColors.Add("pop_text_stress_damage", "#000000");
        HexColors.Add("pop_text_outline_stress_damage", "#c2c2c2");
        HexColors.Add("pop_text_miss", "#000000");
        HexColors.Add("pop_text_outline_miss", "#9a988f");
        HexColors.Add("pop_text_no_damage", "#920400");
        HexColors.Add("pop_text_outline_no_damage", "#1e0400");
        HexColors.Add("pop_text_crit_damage", "#ff7100");
        HexColors.Add("pop_text_outline_crit_damage", "#480800");
        HexColors.Add("pop_text_damage", "#e30900");
        HexColors.Add("pop_text_outline_damage", "#390000");
        HexColors.Add("pop_text_deathblow", "#ff7100");
        HexColors.Add("pop_text_outline_deathblow", "#480800");
        HexColors.Add("pop_text_death_avoided", "#920400");
        HexColors.Add("pop_text_outline_death_avoided", "#1e0400");
        HexColors.Add("pop_text_resist", "#000000");
        HexColors.Add("pop_text_outline_resist", "#9a988f");
        HexColors.Add("pop_text_disease_resist", "#000000");
        HexColors.Add("pop_text_outline_disease_resist", "#" + ColorUtility.ToHtmlStringRGBA(ModColor(173, 201, 98, 255)));
        HexColors.Add("pop_text_pass", "#000000");
        HexColors.Add("pop_text_outline_pass", "#9a988f");
        HexColors.Add("pop_text_heal", "#4fcb4f");
        HexColors.Add("pop_text_outline_heal", "#2a4416");
        HexColors.Add("pop_text_heal_crit", "#54ff70");
        HexColors.Add("pop_text_outline_heal_crit", "#05644f");
        HexColors.Add("pop_text_buff", "#79f8ff");
        HexColors.Add("pop_text_outline_buff", "#127298");
        HexColors.Add("pop_text_debuff", "#e26826");
        HexColors.Add("pop_text_outline_debuff", "#7f290b");
        HexColors.Add("pop_text_stun", "#f0cf5a");
        HexColors.Add("pop_text_outline_stun", "#8e5b11");
        HexColors.Add("pop_text_stun_clear", "#f0cf5a");
        HexColors.Add("pop_text_outline_stun_clear", "#8e5b11");
        HexColors.Add("pop_text_poison", "#c6ff57");
        HexColors.Add("pop_text_outline_poison", "#545828");
        HexColors.Add("pop_text_bleed", "#e30900");
        HexColors.Add("pop_text_outline_bleed", "#390000");
        HexColors.Add("pop_text_cured", "#fdf8c7");
        HexColors.Add("pop_text_outline_cured", "#7c663f");
        HexColors.Add("pop_text_cure_failed", "#e26826");
        HexColors.Add("pop_text_outline_cure_failed", "#7f290b");
        HexColors.Add("pop_text_tagged", "#e30900");
        HexColors.Add("pop_text_outline_tagged", "#390000");
        HexColors.Add("pop_text_riposte", "#e30900");
        HexColors.Add("pop_text_outline_riposte", "#390000");
        HexColors.Add("pop_text_guard", "#79f8ff");
        HexColors.Add("pop_text_outline_guard", "#127298");
        HexColors.Add("pop_text_guard_failed", "#e26826");
        HexColors.Add("pop_text_outline_guard_failed", "#7f290b");
        HexColors.Add("pop_text_full", "#000000");
        HexColors.Add("pop_text_outline_full", "#9a988f");
        HexColors.Add("pop_text_heart_attack", "#000000");
        HexColors.Add("pop_text_outline_heart_attack", "#ffffff");
        HexColors.Add("pop_text_move_resist", "#000000");
        HexColors.Add("pop_text_outline_move_resist", "#79f8ff");
        HexColors.Add("pop_text_debuff_resist", "#000000");
        HexColors.Add("pop_text_outline_debuff_resist", "#e26826");
        HexColors.Add("pop_text_stun_resist", "#000000");
        HexColors.Add("pop_text_outline_stun_resist", "#f0cf5a");
        HexColors.Add("pop_text_poison_resist", "#000000");
        HexColors.Add("pop_text_outline_poison_resist", "#c6ff57");
        HexColors.Add("pop_text_bleed_resist", "#000000");
        HexColors.Add("pop_text_outline_bleed_resist", "#e30900");

        HexColors.Add("torch_centre", "#ff7a00");
        HexColors.Add("torch_ends", "#5f0400");
        HexColors.Add("torch_reduce_tip", HexColors["harmful"]);
    }

    private void LoadCampaignGenerationData()
    {
        CampaignGeneration = new CampaignGenerationData();
        TextAsset jsonText = Resources.Load<TextAsset>(JsonCampaignGenerationPath);
        JsonCampaignData jsonData = JsonDarkestDeserializer.GetJsonCampaign(jsonText.text);

        CampaignGeneration.DungeonXpTable = jsonData.quest_completion_xp_table.ToArray();
        CampaignGeneration.DungeonXpLevelThreshold = jsonData.level_threshold_table.ToArray();
        CampaignGeneration.HeroXpLevelThreshold = jsonData.resolve_level_thresholds.ToArray();
    }

    private void LoadMonsters()
    {
        Monsters = new Dictionary<string, MonsterData>();

        foreach (var monsterAsset in Resources.LoadAll<TextAsset>(MonstersDirectory))
        {
            List<string> monsterText = monsterAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            MonsterData monsterData = new MonsterData();
            monsterData.StringId = monsterText[0].Split(' ')[1];
            monsterData.TypeId = monsterText[1].Split(' ')[1];
            
            int index = 2;
            #region Art
            while (monsterText[index] != ".end")
            {
                //List<string> data = monsterText[index++].Replace("%", "").Replace("\"", "").Split(' ').ToList();
                List<string> data = Regex.Matches(monsterText[index++].Replace("\t", " "),
                    @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();
                for (int i = 0; i < data.Count; i++)
                    data[i] = data[i].Replace("\"", "").Replace("%", "");

                switch(data[0])
                {
                    case "rendering:":
                        monsterData.RenderingRankOverride = int.Parse(data[2]);
                        break;
                    case "defending_area_pos_offset:":
                        monsterData.DefendOffset = new Vector2(int.Parse(data[2]), int.Parse(data[3]));
                        break;
                    #region Common Effects
                    case "commonfx:":
                        if (monsterData.CommonEffects != null)
                            monsterData.CommonEffects.LoadData(data);
                        else
                            monsterData.CommonEffects = new CommonEffects(data);
                        break;
                    #endregion
                    #region Shape Shifter
                    case "shape_shifter:":
                        if (monsterData.Shapeshifter == null)
                            monsterData.Shapeshifter = new Shapeshifter(data);
                        else
                            monsterData.Shapeshifter.LoadData(data);               
                        break;
                    #endregion
                    #region Full Captor
                    case "captor_full:":
                        if (monsterData.FullCaptor == null)
                            monsterData.FullCaptor = new FullCaptor(data);
                        else
                            monsterData.FullCaptor.LoadData(data);
                        break;
                    #endregion                
                    #region Display Modifier
                    case "display_modifier:":
                        if (monsterData.DisplayModifier == null)
                            monsterData.DisplayModifier = new DisplayModifier(data);
                        else
                            monsterData.DisplayModifier.LoadData(data);
                        break;
                    #endregion
                    #region Healthbar Modifier
                    case "health_bar:":
                        if (monsterData.HealthbarModifier == null)
                            monsterData.HealthbarModifier = new HealthbarModifier(data);
                        else
                            monsterData.HealthbarModifier.LoadData(data);
                        break;
                    #endregion
                    #region Skill
                    case "skill:":
                        SkillArtInfo skillArt = new SkillArtInfo(data, true);
                        monsterData.SkillArtInfo.Add(skillArt);
                        break;
                    #endregion
                    case "riposte_skill:":
                        SkillArtInfo riposteArt = new SkillArtInfo(data, false);
                        monsterData.SkillArtInfo.Add(riposteArt);
                        break;
                    case "battle_stage:":
                        monsterData.BattleStage = data[2];
                        break;
                    case "audio_modifier:":
                        monsterData.AudioIntensity = int.Parse(data[2]);
                        break;
                    case "art:":
                        break;
                    default:
                        Debug.LogError(string.Format("Art unknown type: {0} Monster: {1}", data[0], monsterData.StringId));
                        break;
                }
            }
            #endregion
            index += 2;
            #region Info
            while (monsterText[index] != ".end")
            {
                //List<string> data = monsterText[index++].Replace("%", "").Replace("\"", "")
                //    .Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> data = Regex.Matches(monsterText[index++].
                    Replace("\t", " "), @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();
                for (int i = 0; i < data.Count; i++)
                    data[i] = data[i].Replace("\"", "").Replace("%", "");
                switch (data[0])
                {
                    case "display:":
                        monsterData.Size = int.Parse(data[2]);
                        break;
                    case "enemy_type:":
                        monsterData.EnemyTypes.Add(CharacterHelper.StringToMonsterType(data[2]));
                        break;
                    case "stats:":
                        monsterData.Attributes.Add(AttributeType.HitPoints, float.Parse(data[2]));
                        monsterData.Attributes.Add(AttributeType.DefenseRating, float.Parse(data[4]) / 100);
                        monsterData.Attributes.Add(AttributeType.ProtectionRating, float.Parse(data[6]));
                        monsterData.Attributes.Add(AttributeType.SpeedRating, float.Parse(data[8]));
                        monsterData.Attributes.Add(AttributeType.Stun, float.Parse(data[10]) / 100);
                        monsterData.Attributes.Add(AttributeType.Poison, float.Parse(data[12]) / 100);
                        monsterData.Attributes.Add(AttributeType.Bleed, float.Parse(data[14]) / 100);
                        monsterData.Attributes.Add(AttributeType.Debuff, float.Parse(data[16]) / 100);
                        monsterData.Attributes.Add(AttributeType.Move, float.Parse(data[18]) / 100);
                        break;
                    case "skill:":
                        List<string> combatData = new List<string>();
                        data = monsterText[index - 1].Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        bool isEffectData = false;
                        foreach (var item in data)
                        {
                            if (isEffectData)
                            {
                                if (item.Trim(' ').StartsWith("."))
                                    isEffectData = false;
                                else
                                {
                                    combatData.Add(item);
                                    continue;
                                }
                            }

                            string[] combatItems = item.Replace("%", "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (combatItems[combatItems.Length - 1] == ".effect")
                                isEffectData = true;
                            combatData.AddRange(combatItems);
                        }
                        monsterData.CombatSkills.Add(new CombatSkill(combatData, false));
                        break;
                    case "riposte_skill:":
                        List<string> riposteData = new List<string>();
                        data = monsterText[index - 1].Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        bool isRiposteData = false;
                        foreach (var item in data)
                        {
                            if (isRiposteData)
                            {
                                if (item.Trim(' ').StartsWith("."))
                                    isRiposteData = false;
                                else
                                {
                                    riposteData.Add(item);
                                    continue;
                                }
                            }

                            string[] combatItems = item.Replace("%", "").Split(
                                new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (combatItems[combatItems.Length - 1] == ".effect")
                                isRiposteData = true;
                            riposteData.AddRange(combatItems);
                        }
                        monsterData.RiposteSkill = new CombatSkill(riposteData, false);
                        break;
                    case "personality:":
                        monsterData.PreferableSkill = int.Parse(data[2]);
                        break;
                    #region Loot
                    case "loot:":
                        monsterData.Loot.Add(new LootDefinition(data));
                        break;
                    #endregion
                    #region Torchlight Modifier
                    case "torchlight_modifier:":
                        if (monsterData.TorchlightModifier == null)
                            monsterData.TorchlightModifier = new TorchlightModifier(data);
                        else
                            monsterData.TorchlightModifier.LoadData(data);
                        break;
                    #endregion
                    #region Initiative
                    case "initiative:":
                        if (monsterData.Initiative != null)
                            monsterData.Initiative.LoadData(data);
                        else
                            monsterData.Initiative = new Initiative(data);
                        break;
                    #endregion
                    #region Monster Brains
                    case "monster_brain:":
                        monsterData.MonsterBrainId = data[2];
                        break;
                    #endregion
                    #region Tags
                    case "tag:":
                        monsterData.Tags.Add(data[2]);
                        break;
                    #endregion
                    #region Shared Health
                    case "shared_health:":
                        if (monsterData.SharedHealth != null)
                            monsterData.SharedHealth.LoadData(data);
                        else
                            monsterData.SharedHealth = new SharedHealth(data);
                        break;
                    #endregion
                    #region Shape Shifter
                    case "shape_shifter:":
                        if (monsterData.Shapeshifter == null)
                            monsterData.Shapeshifter = new Shapeshifter(data);
                        else
                            monsterData.Shapeshifter.LoadData(data);
                        break;
                    #endregion
                    #region Death Class
                    case "death_class:":
                        if (monsterData.DeathClass == null)
                            monsterData.DeathClass = new DeathClass(data);
                        else
                            monsterData.DeathClass.LoadData(data);
                        break;
                    #endregion
                    #region Death Damage
                    case "death_damage:":
                        if (monsterData.DeathDamage == null)
                            monsterData.DeathDamage = new DeathDamage(data);
                        else
                            monsterData.DeathDamage.LoadData(data);
                        break;
                    #endregion
                    #region Spawn
                    case "spawn:":
                        if (monsterData.Spawn == null)
                            monsterData.Spawn = new Spawn(data);
                        else
                            monsterData.Spawn.LoadData(data);
                        break;
                    #endregion
                    #region Skill Reaction
                    case "skill_reaction:":
                        if (monsterData.SkillReaction == null)
                            monsterData.SkillReaction = new SkillReaction(data);
                        else
                            monsterData.SkillReaction.LoadData(data);
                        break;
                    #endregion

                    #region Battle Modifier
                    case "battle_modifier:":
                        if (monsterData.Modifiers == null)
                            monsterData.Modifiers = new BattleModifier(data);
                        else
                            monsterData.Modifiers.LoadData(data);
                        break;
                    #endregion  
                    #region Companion
                    case "companion:":
                        if (monsterData.Companion == null)
                            monsterData.Companion = new Companion(data);
                        else
                            monsterData.Companion.LoadData(data);
                        break;
                    #endregion
                    #region Empty Captor
                    case "captor_empty:":
                        if (monsterData.EmptyCaptor == null)
                            monsterData.EmptyCaptor = new EmptyCaptor(data);
                        else
                            monsterData.EmptyCaptor.LoadData(data);
                        break;
                    #endregion
                    #region Full Captor
                    case "captor_full:":
                        if (monsterData.FullCaptor == null)
                            monsterData.FullCaptor = new FullCaptor(data);
                        else
                            monsterData.FullCaptor.LoadData(data);
                        break;
                    #endregion
                    #region Controller Captor
                    case "controller:":
                        if (monsterData.ControllerCaptor == null)
                            monsterData.ControllerCaptor = new Controller(data);
                        else
                            monsterData.ControllerCaptor.LoadData(data);
                        break;
                    #endregion
                    #region Life Link
                    case "life_link:":
                        if (monsterData.LifeLink == null)
                            monsterData.LifeLink = new LifeLink(data);
                        else
                            monsterData.LifeLink.LoadData(data);
                        break;
                    #endregion
                    #region Life Time
                    case "life_time:":
                        if (monsterData.LifeTime == null)
                            monsterData.LifeTime = new LifeTime(data);
                        else
                            monsterData.LifeTime.LoadData(data);
                        break;
                    #endregion
                    case "battle_backdrop:":
                        monsterData.BattleBackdrop = data[2];
                        break;
                    default:
                        Debug.LogError(string.Format("Info unknown type: {0} Monster: {1}", data[0], monsterData.StringId));
                        break;
                }
            }
            #endregion
            Monsters.Add(monsterData.StringId, monsterData);

            #region Check
            if (monsterData.Initiative == null)
                Debug.LogError("Monster " + monsterData.StringId + " has no initiative.");
            if (monsterData.Modifiers == null)
                monsterData.Modifiers = new BattleModifier();
            #endregion
        }
    }

    private void LoadIconSets()
    {
        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/"))
            Sprites.Add(sprite.name, sprite);

        #region Room Icons

        MapRoomIconSet = new Dictionary<AreaType, Sprite>
        {
            {AreaType.Empty, Sprites["room_empty"]},
            {AreaType.Tresure, Sprites["room_treasure"]},
            {AreaType.BattleTresure, Sprites["room_treasure"]},
            {AreaType.Entrance, Sprites["room_entrance"]},
            {AreaType.Curio, Sprites["room_curio"]},
            {AreaType.BattleCurio, Sprites["room_curio"]},
            {AreaType.Boss, Sprites["room_boss"]},
            {AreaType.Battle, Sprites["room_battle"]}
        };

        MapRoomKnowledgeSet = new Dictionary<Knowledge, Sprite>
        {
            {Knowledge.Completed, Sprites["marker_room_visited"]},
            {Knowledge.Hidden, Sprites["room_unknown"]}
        };

        MapHallIconSet = new Dictionary<AreaType, Sprite>
        {
            {AreaType.Empty, Sprites["hall_dim"]},
            {AreaType.Trap, Sprites["marker_trap"]},
            {AreaType.Obstacle, Sprites["marker_obstacle"]},
            {AreaType.Hunger, Sprites["marker_hunger"]},
            {AreaType.Curio, Sprites["marker_curio"]},
            {AreaType.Battle, Sprites["marker_battle"]}
        };

        MapHallKnowledgeSet = new Dictionary<Knowledge, Sprite>
        {
            {Knowledge.Completed, Sprites["hall_clear"]},
            {Knowledge.Hidden, Sprites["hall_dark"]},
            {Knowledge.Scouted, Sprites["hall_dim"]}
        };

        #endregion

        #region Hero Icons

        HeroSprites = new HeroSpriteDatabase();
        foreach(var hero in HeroClasses.Values)
        {
            HeroSpriteInfo heroInfo = new HeroSpriteInfo();

            heroInfo.Header = Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + "_guild_header");

            heroInfo.Skills.Add("one", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.one"));
            heroInfo.Skills.Add("two", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.two"));
            heroInfo.Skills.Add("three", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.three"));
            heroInfo.Skills.Add("four", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.four"));
            heroInfo.Skills.Add("five", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.five"));
            heroInfo.Skills.Add("six", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.six"));
            heroInfo.Skills.Add("seven", Resources.Load<Sprite>(HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + hero.StringId + ".ability.seven"));

            foreach(string outfit in new[] { "A", "B", "C", "D"})
            {
                HeroOutfit heroOutfit = new HeroOutfit();
                string portraitString = HeroesDirectory + "Sprites/"
                + hero.StringId + "/" + outfit + "/" + hero.StringId + "_portrait_roster";
                heroOutfit.Portrait = Resources.Load<Sprite>(portraitString);
                heroInfo.Outfits.Add(outfit, heroOutfit);
            }

            foreach (var weapon in hero.Weapons)
                heroInfo.Equip.Add(weapon.Name, Resources.Load<Sprite>(HeroesDirectory + "Sprites/" + hero.StringId
                    + "/Equipment/eqp_weapon_" + weapon.Name[weapon.Name.Length - 1]));
            foreach (var armor in hero.Armors)
                heroInfo.Equip.Add(armor.Name, Resources.Load<Sprite>(HeroesDirectory + "Sprites/" + hero.StringId
                    + "/Equipment/eqp_armour_" + armor.Name[armor.Name.Length - 1]));
            
            HeroSprites.HeroClassInfo.Add(hero.StringId, heroInfo);
        }

        #endregion        
    }

    private void LoadDungeonGenerationData()
    {
        DungeonGenerationData = new List<DungeonGenerationData>();
        string dungeonText = Resources.Load<TextAsset>(DungeonGenerationDataPath).text;
        List<string> dungeonList = dungeonText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        for (int i = 0; i < dungeonList.Count; i++)
        {
            if (dungeonList[i] != "map:")
                continue;

            DungeonGenerationData dungeonData = new DungeonGenerationData
            {
                Length = dungeonList[++i].Split(' ')[1],
                QuestType = dungeonList[++i].Split(' ')[1],
                Dungeon = dungeonList[++i].Split(' ')[1],
                BaseRoomNumber = int.Parse(dungeonList[++i].Split(' ')[1]),
                BaseCorridorNumber = int.Parse(dungeonList[++i].Split(' ')[1]),
                GridSizeX = int.Parse(dungeonList[++i].Split(' ')[1]),
                GridSizeY = int.Parse(dungeonList[i].Split(' ')[2]),
                Spacing = int.Parse(dungeonList[++i].Split(' ')[1]),
                GoalRoomNumber = int.Parse(dungeonList[++i].Split(' ')[1]),
                Connectivity = float.Parse(dungeonList[++i].Split(' ')[1]),
                MinFinalDistance = int.Parse(dungeonList[++i].Split(' ')[1]),
                HallwayBattleMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                HallwayBattleMax = int.Parse(dungeonList[i].Split(' ')[2]),
                HallwayTrapMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                HallwayTrapMax = int.Parse(dungeonList[i].Split(' ')[2]),
                HallwayObstacleMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                HallwayObstacleMax = int.Parse(dungeonList[i].Split(' ')[2]),
                HallwayCurioMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                HallwayCurioMax = int.Parse(dungeonList[i].Split(' ')[2]),
                HallwayHungerMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                HallwayHungerMax = int.Parse(dungeonList[i].Split(' ')[2]),
                TotalRoomBattleMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                TotalRoomBattleMax = int.Parse(dungeonList[i].Split(' ')[2]),
                RoomBattleMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                RoomBattleMax = int.Parse(dungeonList[i].Split(' ')[2]),
                RoomGuardedCurioMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                RoomGuardedCurioMax = int.Parse(dungeonList[i].Split(' ')[2]),
                RoomCurioMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                RoomCurioMax = int.Parse(dungeonList[i].Split(' ')[2]),
                RoomGuardedTresureMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                RoomGuardedTresureMax = int.Parse(dungeonList[i].Split(' ')[2]),
                RoomTresureMin = int.Parse(dungeonList[++i].Split(' ')[1]),
                RoomTresureMax = int.Parse(dungeonList[i].Split(' ')[2])
            };

            DungeonGenerationData.Add(dungeonData);
        }
    }

    private void LoadDungeonEnviroment(string dungeon, string dataPath)
    {
        var envData = new DungeonEnviromentData();
        string dungeonText = Resources.Load<TextAsset>(dataPath).text;
        List<string> dungeonList = dungeonText.Split(new[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
        envData.HallVariations = int.Parse(dungeonList[2].Split(' ')[1]);

        List<string> items = dungeonList[3].Split(' ').ToList();
        for (int j = 1; j < items.Count; j++)
            envData.RoomVariations.Add(items[j]);

        for (int i = 4; i < dungeonList.Count; i++)
        {
            if(dungeonList[i] == "mash:")
            {
                DungeonBattleMash mash = new DungeonBattleMash {MashId = int.Parse(dungeonList[++i].Split(' ')[1])};

                while(dungeonList[++i] != ".end")
                {
                    items = dungeonList[i].Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    switch(items[0])
                    {
                        case "hall:":
                            mash.HallEncounters.Add(new DungeonBattleEncounter
                                (int.Parse(items[2]), items.GetRange(4, items.Count - 4)));
                            break;
                        case "room:":
                            mash.RoomEncounters.Add(new DungeonBattleEncounter
                                (int.Parse(items[2]), items.GetRange(4, items.Count - 4)));
                            break;
                        case "stall:":
                            mash.StallEncounters.Add(new DungeonBattleEncounter
                                (int.Parse(items[2]), items.GetRange(4, items.Count - 4)));
                            break;
                        case "boss:":
                            mash.BossEncounters.Add(new DungeonBattleEncounter
                                (int.Parse(items[2]), items.GetRange(4, items.Count - 4)));
                            break;
                        case "named:":
                            if(!mash.NamedEncounters.ContainsKey(items[2]))
                                mash.NamedEncounters.Add(items[2], new List<DungeonBattleEncounter>());

                            mash.NamedEncounters[items[2]].Add(new DungeonBattleEncounter
                                (int.Parse(items[4]), items.GetRange(6, items.Count - 6)));
                            break;
                    }
                }
                envData.BattleMashes.Add(mash);
            }
            if(dungeonList[i] == "props:")
            {
                while(dungeonList[++i] != ".end")
                {
                    items = dungeonList[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    switch(items[0])
                    {
                        case "hall_curios:":
                            envData.HallCurios.Add(new DungeonPropsEncounter(int.Parse(items[2]), items[4]));
                            break;
                        case "room_curios:":
                            envData.RoomCurios.Add(new DungeonPropsEncounter(int.Parse(items[2]), items[4]));
                            break;
                        case "room_treasures:":
                            envData.RoomTresures.Add(new DungeonPropsEncounter(int.Parse(items[2]), items[4]));
                            break;
                        case "secret_room_treasures:":
                            envData.SecretTresures.Add(new DungeonPropsEncounter(int.Parse(items[2]), items[4]));
                            break;
                        case "traps:":
                            envData.Traps.Add(new DungeonPropsEncounter(int.Parse(items[2]), items[4]));
                            break;
                        case "obstacles:":
                            envData.Obstacles.Add(new DungeonPropsEncounter(int.Parse(items[2]), items[4]));
                            break;
                    }
                }
            }
        }
        DungeonEnviromentData.Add(dungeon, envData);
    }

    private Color ModColor(float r, float g, float b, float a)
    {
        return new Color(r / 255, g / 255, b / 255, a / 255);
    }
}