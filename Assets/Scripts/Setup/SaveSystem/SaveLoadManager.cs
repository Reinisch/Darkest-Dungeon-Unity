using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class SaveLoadManager
{
    static void WriteHero(BinaryWriter bw, SaveHeroData saveHeroData)
    {
        bw.Write((int)saveHeroData.status);
        bw.Write(saveHeroData.missingDuration);
        bw.Write(saveHeroData.inActivity == null ? "" : saveHeroData.inActivity);
        bw.Write(saveHeroData.trait);
        bw.Write(saveHeroData.rosterId);

        bw.Write(saveHeroData.name);
        bw.Write(saveHeroData.heroClass);

        bw.Write(saveHeroData.resolveLevel);
        bw.Write(saveHeroData.resolveXP);

        bw.Write(saveHeroData.currentHp);
        bw.Write(saveHeroData.stressLevel);

        bw.Write(saveHeroData.weaponLevel);
        bw.Write(saveHeroData.armorLevel);

        bw.Write(saveHeroData.leftTrinketId);
        bw.Write(saveHeroData.rightTrinketId);

        bw.Write(saveHeroData.quirks.Count);
        for (int i = 0; i < saveHeroData.quirks.Count; i++)
        {
            bw.Write(saveHeroData.quirks[i].Quirk.Id);
            bw.Write(saveHeroData.quirks[i].IsLocked);
            bw.Write(saveHeroData.quirks[i].IsNew);
            bw.Write(saveHeroData.quirks[i].IsReplaced);
            bw.Write(saveHeroData.quirks[i].Longetivity);
            bw.Write(saveHeroData.quirks[i].ReplacedQuirk);
        }
        bw.Write(saveHeroData.buffs.Count);
        for (int i = 0; i < saveHeroData.buffs.Count; i++)
        {
            bw.Write((int)saveHeroData.buffs[i].SourceType);

            if(saveHeroData.buffs[i].SourceType == BuffSourceType.Adventure || saveHeroData.buffs[i].SourceType == BuffSourceType.Estate)
            {
                bw.Write((int)saveHeroData.buffs[i].DurationType);
                bw.Write(saveHeroData.buffs[i].OverridenValue);
                bw.Write(saveHeroData.buffs[i].Duration);

                bw.Write(saveHeroData.buffs[i].Buff.Id);
                if (saveHeroData.buffs[i].Buff.Id == "")
                {
                    bw.Write(saveHeroData.buffs[i].Buff.ModifierValue);
                    bw.Write((int)saveHeroData.buffs[i].Buff.Type);
                    bw.Write((int)saveHeroData.buffs[i].Buff.AttributeType);
                    bw.Write((int)saveHeroData.buffs[i].Buff.RuleType);
                }
            }
        }

        bw.Write(saveHeroData.selectedCombatSkillIndexes.Count);
        for (int i = 0; i < saveHeroData.selectedCombatSkillIndexes.Count; i++)
            bw.Write(saveHeroData.selectedCombatSkillIndexes[i]);

        bw.Write(saveHeroData.selectedCampingSkillIndexes.Count);
        for (int i = 0; i < saveHeroData.selectedCampingSkillIndexes.Count; i++)
            bw.Write(saveHeroData.selectedCampingSkillIndexes[i]);
    }
    static SaveHeroData ReadHero(BinaryReader br)
    {
        SaveHeroData saveHeroData = new SaveHeroData();

        saveHeroData.status = (HeroStatus)br.ReadInt32();
        saveHeroData.missingDuration = br.ReadInt32();
        saveHeroData.inActivity = br.ReadString();
        saveHeroData.trait = br.ReadString();
        saveHeroData.rosterId = br.ReadInt32();

        saveHeroData.name = br.ReadString();
        saveHeroData.heroClass = br.ReadString();

        saveHeroData.resolveLevel = br.ReadInt32();
        saveHeroData.resolveXP = br.ReadInt32();

        saveHeroData.currentHp = br.ReadSingle();
        saveHeroData.stressLevel = br.ReadSingle();

        saveHeroData.weaponLevel = br.ReadInt32();
        saveHeroData.armorLevel = br.ReadInt32();

        saveHeroData.leftTrinketId = br.ReadString();
        saveHeroData.rightTrinketId = br.ReadString();

        int quirkCount = br.ReadInt32();
        for (int i = 0; i < quirkCount; i++)
        {
            var newQuirkInfo = new QuirkInfo(br.ReadString())
            {
                IsLocked = br.ReadBoolean(),
                IsNew = br.ReadBoolean(),
                IsReplaced = br.ReadBoolean(),
                Longetivity = br.ReadInt32(),
                ReplacedQuirk = br.ReadString(),
            };
            saveHeroData.quirks.Add(newQuirkInfo);
        }
        int buffEntryCount = br.ReadInt32();
        for (int i = 0; i < buffEntryCount; i++)
        {
            BuffSourceType buffSourceType =  (BuffSourceType)br.ReadInt32();

            if (buffSourceType == BuffSourceType.Adventure || buffSourceType == BuffSourceType.Estate)
            {
                BuffInfo newBuffInfo = new BuffInfo((BuffDurationType)br.ReadInt32(), buffSourceType);

                newBuffInfo.OverridenValue = br.ReadSingle();
                newBuffInfo.Duration = br.ReadInt32();

                string buffId = br.ReadString();
                if (buffId == "")
                {
                    newBuffInfo.Buff = new Buff()
                    {
                        Id = "",
                        ModifierValue = br.ReadSingle(),
                        Type = (BuffType)br.ReadInt32(),
                        AttributeType = (AttributeType)br.ReadInt32(),
                        RuleType = (BuffRule)br.ReadInt32(),
                    };
                }
                else
                    newBuffInfo.Buff = DarkestDungeonManager.Data.Buffs[buffId];

                saveHeroData.buffs.Add(newBuffInfo);
            }
        }

        int selectedCombatCount = br.ReadInt32();
        for (int i = 0; i < selectedCombatCount; i++)
            saveHeroData.selectedCombatSkillIndexes.Add(br.ReadInt32());

        int selectedCampingCount = br.ReadInt32();
        for (int i = 0; i < selectedCampingCount; i++)
            saveHeroData.selectedCampingSkillIndexes.Add(br.ReadInt32());

        return saveHeroData;
    }

    public static void WriteSave(SaveCampaignData saveData)
    {
        try
        {
            using (var fs = new FileStream(Application.persistentDataPath + "\\Saves\\DarkestSave" + saveData.saveId +
                    ".darkestsave", FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    #region Estate
                    bw.Write(saveData.isFirstStart);
                    bw.Write(saveData.gameVersion);
                    bw.Write(saveData.hamletTitle);
                    bw.Write(saveData.locationName);
                    bw.Write(saveData.questsCompleted);
                    bw.Write(saveData.currentWeek);
                    bw.Write(saveData.saveId);
                    // Currencies
                    bw.Write(saveData.goldAmount);
                    bw.Write(saveData.bustsAmount);
                    bw.Write(saveData.deedsAmount);
                    bw.Write(saveData.portraitsAmount);
                    bw.Write(saveData.crestsAmount);
                    // Heroes
                    bw.Write(saveData.saveHeroData.Length);
                    for (int i = 0; i < saveData.saveHeroData.Length; i++)
                        WriteHero(bw, saveData.saveHeroData[i]);
                    bw.Write(saveData.stageCoachData.Length);
                    for (int i = 0; i < saveData.stageCoachData.Length; i++)
                        WriteHero(bw, saveData.stageCoachData[i]);
                    // Trinkets
                    bw.Write(saveData.trinketData.Count);
                    for (int i = 0; i < saveData.trinketData.Count; i++)
                        bw.Write(saveData.trinketData[i]);
                    bw.Write(saveData.wagonData.Count);
                    for (int i = 0; i < saveData.wagonData.Count; i++)
                        bw.Write(saveData.wagonData[i]);
                    // Dungeon infos
                    bw.Write(saveData.saveDungeonData.Count);
                    foreach(var dungeonProgress in saveData.saveDungeonData)
                    {
                        bw.Write(dungeonProgress.Value.DungeonName);
                        bw.Write(dungeonProgress.Value.MasteryLevel);
                        bw.Write(dungeonProgress.Value.CurrentXP);
                        bw.Write(dungeonProgress.Value.IsUnlocked);
                        bw.Write(dungeonProgress.Value.PlotQuestCompleted);
                    }
                    // Hero deaths
                    bw.Write(saveData.deathRecords.Count);
                    for (int i = 0; i < saveData.deathRecords.Count; i++)
                    {
                        bw.Write(saveData.deathRecords[i].HeroName);
                        bw.Write(saveData.deathRecords[i].HeroClassIndex);
                        bw.Write(saveData.deathRecords[i].ResolveLevel);
                        bw.Write((int)saveData.deathRecords[i].Factor);
                        bw.Write(saveData.deathRecords[i].KillerName);
                    }
                    // Building upgrade trees
                    bw.Write(saveData.buildingUpgrades.Count);
                    foreach(var buildingTreeEntry in saveData.buildingUpgrades)
                    {
                        bw.Write(buildingTreeEntry.Key);
                        bw.Write(buildingTreeEntry.Value.PurchasedUpgrades.Count);
                        for (int i = 0; i < buildingTreeEntry.Value.PurchasedUpgrades.Count; i++)
                            bw.Write(buildingTreeEntry.Value.PurchasedUpgrades[i]);
                    }
                    // Hero upgrade trees
                    bw.Write(saveData.instancedPurchases.Count);
                    foreach(var inst in saveData.instancedPurchases)
                    {
                        bw.Write(inst.Key);
                        bw.Write(inst.Value.Count);
                        foreach(var tree in inst.Value)
                        {
                            bw.Write(tree.Key);
                            bw.Write(tree.Value.PurchasedUpgrades.Count);
                            for (int i = 0; i < tree.Value.PurchasedUpgrades.Count; i++)
                                bw.Write(tree.Value.PurchasedUpgrades[i]);
                        }
                    }
                    #region Activity Log
                    bw.Write(saveData.activityLog.Count);
                    for(int i = 0; i < saveData.activityLog.Count; i++)
                    {
                        bw.Write(saveData.activityLog[i].WeekNumber);
                        bw.Write(saveData.activityLog[i].ReturnRecord != null ? true : false);
                        bw.Write(saveData.activityLog[i].EmbarkRecord != null ? true : false);
                        if(saveData.activityLog[i].ReturnRecord != null)
                        {
                            bw.Write((int)saveData.activityLog[i].ReturnRecord.PartyActionType);
                            bw.Write(saveData.activityLog[i].ReturnRecord.QuestType);
                            bw.Write(saveData.activityLog[i].ReturnRecord.QuestDifficulty);
                            bw.Write(saveData.activityLog[i].ReturnRecord.QuestLength);
                            bw.Write(saveData.activityLog[i].ReturnRecord.Dungeon);

                            bw.Write(saveData.activityLog[i].ReturnRecord.Names.Count);
                            for(int j = 0; j < saveData.activityLog[i].ReturnRecord.Names.Count; j++)
                                bw.Write(saveData.activityLog[i].ReturnRecord.Names[j]);

                            bw.Write(saveData.activityLog[i].ReturnRecord.Classes.Count);
                            for(int j = 0; j < saveData.activityLog[i].ReturnRecord.Classes.Count; j++)
                                bw.Write(saveData.activityLog[i].ReturnRecord.Classes[j]);

                            bw.Write(saveData.activityLog[i].ReturnRecord.Alive.Count);
                            for(int j = 0; j < saveData.activityLog[i].ReturnRecord.Alive.Count; j++)
                                bw.Write(saveData.activityLog[i].ReturnRecord.Alive[j]);

                            bw.Write(saveData.activityLog[i].ReturnRecord.IsSuccessfull);
                        }
                        if (saveData.activityLog[i].EmbarkRecord != null)
                        {
                            bw.Write((int)saveData.activityLog[i].EmbarkRecord.PartyActionType);
                            bw.Write(saveData.activityLog[i].EmbarkRecord.QuestType);
                            bw.Write(saveData.activityLog[i].EmbarkRecord.QuestDifficulty);
                            bw.Write(saveData.activityLog[i].EmbarkRecord.QuestLength);
                            bw.Write(saveData.activityLog[i].EmbarkRecord.Dungeon);

                            bw.Write(saveData.activityLog[i].EmbarkRecord.Names.Count);
                            for (int j = 0; j < saveData.activityLog[i].EmbarkRecord.Names.Count; j++)
                                bw.Write(saveData.activityLog[i].EmbarkRecord.Names[j]);

                            bw.Write(saveData.activityLog[i].EmbarkRecord.Classes.Count);
                            for (int j = 0; j < saveData.activityLog[i].EmbarkRecord.Classes.Count; j++)
                                bw.Write(saveData.activityLog[i].EmbarkRecord.Classes[j]);

                            bw.Write(saveData.activityLog[i].EmbarkRecord.Alive.Count);
                            for (int j = 0; j < saveData.activityLog[i].EmbarkRecord.Alive.Count; j++)
                                bw.Write(saveData.activityLog[i].EmbarkRecord.Alive[j]);

                            bw.Write(saveData.activityLog[i].EmbarkRecord.IsSuccessfull);
                        }
                    }
                    #endregion

                    #region Quests
                    bw.Write(saveData.completedPlot.Count);
                    for (int i = 0; i < saveData.completedPlot.Count; i++)
                        bw.Write(saveData.completedPlot[i]);
                    bw.Write(saveData.generatedQuests.Count);
                    for (int i = 0; i < saveData.generatedQuests.Count; i++)
                    {
                        bw.Write(saveData.generatedQuests[i].IsPlotQuest ? (saveData.generatedQuests[i] as PlotQuest).Id : "");
                        bw.Write(saveData.generatedQuests[i].Type);
                        bw.Write(saveData.generatedQuests[i].Dungeon);
                        bw.Write(saveData.generatedQuests[i].Difficulty);
                        bw.Write(saveData.generatedQuests[i].Length);
                        bw.Write(saveData.generatedQuests[i].Goal.Id);
                        bw.Write(saveData.generatedQuests[i].Reward.ResolveXP);
                        bw.Write(saveData.generatedQuests[i].Reward.ItemDefinitions.Count);
                        for (int j = 0; j < saveData.generatedQuests[i].Reward.ItemDefinitions.Count; j++)
                        {
                            bw.Write(saveData.generatedQuests[i].Reward.ItemDefinitions[j].Id);
                            bw.Write(saveData.generatedQuests[i].Reward.ItemDefinitions[j].Type);
                            bw.Write(saveData.generatedQuests[i].Reward.ItemDefinitions[j].Amount);
                        }

                        bw.Write(saveData.generatedQuests[i].IsProgression);
                        bw.Write(saveData.generatedQuests[i].HasStatueContents);
                        bw.Write(saveData.generatedQuests[i].CompletionDungeonXp);
                        bw.Write(saveData.generatedQuests[i].CanRetreat);
                        bw.Write(saveData.generatedQuests[i].AlwaysRetreatFromRaid);
                        bw.Write(saveData.generatedQuests[i].RetreatKillCount);
                        bw.Write(saveData.generatedQuests[i].IsSurpriseEnabled);
                        bw.Write(saveData.generatedQuests[i].IsScoutingEnabled);
                        bw.Write(saveData.generatedQuests[i].IsStressClearedOnCompletion);
                        bw.Write(saveData.generatedQuests[i].RosterBuffOnFailureMinimumPartyResolveLevel);
                        bw.Write(saveData.generatedQuests[i].RosterBuffsOnFailure.Count);
                        for (int j = 0; j < saveData.generatedQuests[i].RosterBuffsOnFailure.Count; j++)
                            bw.Write(saveData.generatedQuests[i].RosterBuffsOnFailure[j].Id);
                        bw.Write(saveData.generatedQuests[i].SuggestedTrinkets.Count);
                        for (int j = 0; j < saveData.generatedQuests[i].SuggestedTrinkets.Count; j++)
                        {
                            bw.Write(saveData.generatedQuests[i].SuggestedTrinkets[j].Id);
                            bw.Write(saveData.generatedQuests[i].SuggestedTrinkets[j].Amount);
                        }
                        bw.Write(saveData.generatedQuests[i].UpgradeTagsRemovedOnIgnore.Count);
                        for (int j = 0; j < saveData.generatedQuests[i].UpgradeTagsRemovedOnIgnore.Count; j++)
                        {
                            bw.Write(saveData.generatedQuests[i].UpgradeTagsRemovedOnIgnore[j].Tag);
                            bw.Write(saveData.generatedQuests[i].UpgradeTagsRemovedOnIgnore[j].Amount);
                        }
                    }
                    #endregion

                    #region Activity Slots
                    bw.Write(saveData.abbeyActivitySlots.Count);
                    for(int i = 0; i < saveData.abbeyActivitySlots.Count; i++)
                    {
                        bw.Write(saveData.abbeyActivitySlots[i].Count);
                        for(int j = 0; j < saveData.abbeyActivitySlots[i].Count; j++)
                        {
                            bw.Write(saveData.abbeyActivitySlots[i][j].HeroRosterId);
                            bw.Write((int)saveData.abbeyActivitySlots[i][j].Status);
                        }
                    }
                    bw.Write(saveData.tavernActivitySlots.Count);
                    for (int i = 0; i < saveData.tavernActivitySlots.Count; i++)
                    {
                        bw.Write(saveData.tavernActivitySlots[i].Count);
                        for (int j = 0; j < saveData.tavernActivitySlots[i].Count; j++)
                        {
                            bw.Write(saveData.tavernActivitySlots[i][j].HeroRosterId);
                            bw.Write((int)saveData.tavernActivitySlots[i][j].Status);
                        }
                    }
                    bw.Write(saveData.sanitariumActivitySlots.Count);
                    for (int i = 0; i < saveData.sanitariumActivitySlots.Count; i++)
                    {
                        bw.Write(saveData.sanitariumActivitySlots[i].Count);
                        for (int j = 0; j < saveData.sanitariumActivitySlots[i].Count; j++)
                        {
                            bw.Write(saveData.sanitariumActivitySlots[i][j].HeroRosterId);
                            bw.Write((int)saveData.sanitariumActivitySlots[i][j].Status);
                            bw.Write(saveData.sanitariumActivitySlots[i][j].TargetDiseaseQuirk == null ?
                                "" : saveData.sanitariumActivitySlots[i][j].TargetDiseaseQuirk);
                            bw.Write(saveData.sanitariumActivitySlots[i][j].TargetNegativeQuirk == null ?
                                "" : saveData.sanitariumActivitySlots[i][j].TargetNegativeQuirk);
                            bw.Write(saveData.sanitariumActivitySlots[i][j].TargetPositiveQuirk == null ?
                                "" : saveData.sanitariumActivitySlots[i][j].TargetPositiveQuirk);
                        }
                    }
                    #endregion
                    #endregion

                    #region Raid
                    bw.Write(saveData.InRaid);
                    if (saveData.InRaid == false)
                        return;

                    #region Quest
                    bw.Write(saveData.QuestCompleted);
                    bw.Write(saveData.Quest.IsPlotQuest ? (saveData.Quest as PlotQuest).Id : "");
                    bw.Write(saveData.Quest.Type);
                    bw.Write(saveData.Quest.Dungeon);
                    bw.Write(saveData.Quest.Difficulty);
                    bw.Write(saveData.Quest.Length);
                    bw.Write(saveData.Quest.Goal.Id);
                    bw.Write(saveData.Quest.Reward.ResolveXP);
                    bw.Write(saveData.Quest.Reward.ItemDefinitions.Count);
                    for (int i = 0; i < saveData.Quest.Reward.ItemDefinitions.Count; i++)
                    {
                        bw.Write(saveData.Quest.Reward.ItemDefinitions[i].Id);
                        bw.Write(saveData.Quest.Reward.ItemDefinitions[i].Type);
                        bw.Write(saveData.Quest.Reward.ItemDefinitions[i].Amount);
                    }

                    bw.Write(saveData.Quest.IsProgression);
                    bw.Write(saveData.Quest.HasStatueContents);
                    bw.Write(saveData.Quest.CompletionDungeonXp);
                    bw.Write(saveData.Quest.CanRetreat);
                    bw.Write(saveData.Quest.AlwaysRetreatFromRaid);
                    bw.Write(saveData.Quest.RetreatKillCount);
                    bw.Write(saveData.Quest.IsSurpriseEnabled);
                    bw.Write(saveData.Quest.IsScoutingEnabled);
                    bw.Write(saveData.Quest.IsStressClearedOnCompletion);
                    bw.Write(saveData.Quest.RosterBuffOnFailureMinimumPartyResolveLevel);
                    bw.Write(saveData.Quest.RosterBuffsOnFailure.Count);
                    for (int i = 0; i < saveData.Quest.RosterBuffsOnFailure.Count; i++)
                        bw.Write(saveData.Quest.RosterBuffsOnFailure[i].Id);
                    bw.Write(saveData.Quest.SuggestedTrinkets.Count);
                    for (int i = 0; i < saveData.Quest.SuggestedTrinkets.Count; i++)
                    {
                        bw.Write(saveData.Quest.SuggestedTrinkets[i].Id);
                        bw.Write(saveData.Quest.SuggestedTrinkets[i].Amount);
                    }
                    bw.Write(saveData.Quest.UpgradeTagsRemovedOnIgnore.Count);
                    for (int i = 0; i < saveData.Quest.UpgradeTagsRemovedOnIgnore.Count; i++)
                    {
                        bw.Write(saveData.Quest.UpgradeTagsRemovedOnIgnore[i].Tag);
                        bw.Write(saveData.Quest.UpgradeTagsRemovedOnIgnore[i].Amount);
                    }
                    #endregion

                    #region Dungeon
                    bw.Write(saveData.Dungeon.Name);
                    bw.Write(saveData.Dungeon.GridSizeX);
                    bw.Write(saveData.Dungeon.GridSizeY);
                    bw.Write(saveData.Dungeon.StartingRoomId);

                    #region Rooms
                    bw.Write(saveData.Dungeon.Rooms.Count);
                    foreach(var room in saveData.Dungeon.Rooms.Values)
                    {
                        bw.Write(room.Id);
                        bw.Write(room.Doors.Count);
                        for(int j = 0; j < room.Doors.Count; j++)
                        {
                            bw.Write(room.Doors[j].TargetArea);
                            bw.Write((int)room.Doors[j].Direction);
                        }
                        bw.Write(room.TextureId);
                        bw.Write((int)room.Type);
                        bw.Write((int)room.Knowledge);
                        bw.Write(room.Prop != null ? true : false);
                        if(room.Prop != null)
                        {
                            bw.Write((int)room.Prop.Type);
                            switch(room.Prop.Type)
                            {
                                case AreaType.Door:
                                    var door = room.Prop as Door;
                                    bw.Write(door.TargetArea);
                                    bw.Write((int)door.Direction);
                                    break;
                                case AreaType.Curio:
                                    bw.Write((room.Prop as Curio).IsQuestCurio);
                                    bw.Write(room.Prop.StringId);
                                    break;
                                case AreaType.Obstacle:
                                case AreaType.Trap:
                                    bw.Write(room.Prop.StringId);
                                    break;
                            }
                        }
                        bw.Write(room.BattleEncounter != null ? true : false);
                        if (room.BattleEncounter != null)
                        {
                            bw.Write(room.BattleEncounter.Monsters.Count);
                            for (int j = 0; j < room.BattleEncounter.Monsters.Count; j++)
                                bw.Write(room.BattleEncounter.Monsters[j].Data.StringId);
                            bw.Write(room.BattleEncounter.Cleared);
                        }
                    }
                    #endregion

                    #region Hallways
                    bw.Write(saveData.Dungeon.Hallways.Count);
                    foreach (var hallway in saveData.Dungeon.Hallways.Values)
                    {
                        bw.Write(hallway.Id);
                        bw.Write(hallway.RoomA.Id);
                        bw.Write(hallway.RoomB.Id);
                        bw.Write(hallway.Halls.Count);
                        for (int j = 0; j < hallway.Halls.Count; j++)
                        {
                            bw.Write(hallway.Halls[j].Id);
                            bw.Write(hallway.Halls[j].TextureId);
                            bw.Write((int)hallway.Halls[j].Type);
                            bw.Write((int)hallway.Halls[j].Knowledge);
                            bw.Write(hallway.Halls[j].Prop != null ? true : false);
                            if (hallway.Halls[j].Prop != null)
                            {
                                bw.Write((int)hallway.Halls[j].Prop.Type);
                                switch (hallway.Halls[j].Prop.Type)
                                {
                                    case AreaType.Door:
                                        var door = hallway.Halls[j].Prop as Door;
                                        bw.Write(door.TargetArea);
                                        bw.Write((int)door.Direction);
                                        break;
                                    case AreaType.Curio:
                                        bw.Write((hallway.Halls[j].Prop as Curio).IsQuestCurio);
                                        bw.Write(hallway.Halls[j].Prop.StringId);
                                        break;
                                    case AreaType.Obstacle:
                                    case AreaType.Trap:
                                        bw.Write(hallway.Halls[j].Prop.StringId);
                                        break;
                                }
                            }
                            bw.Write(hallway.Halls[j].BattleEncounter != null ? true : false);
                            if (hallway.Halls[j].BattleEncounter != null)
                            {
                                bw.Write(hallway.Halls[j].BattleEncounter.Monsters.Count);
                                for (int i = 0; i < hallway.Halls[j].BattleEncounter.Monsters.Count; i++)
                                    bw.Write(hallway.Halls[j].BattleEncounter.Monsters[i].Data.StringId);
                                bw.Write(hallway.Halls[j].BattleEncounter.Cleared);
                            }
                        }
                    }
                    #endregion

                    bw.Write(saveData.Dungeon.SharedMashExecutionIds.Count);
                    for (int i = 0; i < saveData.Dungeon.SharedMashExecutionIds.Count; i++)
                        bw.Write(saveData.Dungeon.SharedMashExecutionIds[i]);
                    #endregion

                    #region Raid Party
                    bw.Write(saveData.RaidParty.IsMovingLeft);
                    bw.Write(saveData.RaidParty.HeroInfo.Count);
                    for (int i = 0; i < saveData.RaidParty.HeroInfo.Count; i++)
                    {
                        bw.Write(saveData.RaidParty.HeroInfo[i].HeroRosterId);
                        bw.Write(saveData.RaidParty.HeroInfo[i].IsAlive);
                        if (saveData.RaidParty.HeroInfo[i].IsAlive == false)
                        {
                            bw.Write((int)saveData.RaidParty.HeroInfo[i].Factor);
                            bw.Write(saveData.RaidParty.HeroInfo[i].Killer);
                        }
                    }
                    #endregion

                    #region Raid Info
                    bw.Write((int)saveData.CampingPhase);
                    bw.Write(saveData.CampingTimeLeft);
                    bw.Write(saveData.NightAmbushReduced);
                    bw.Write(saveData.HungerCooldown);

                    bw.Write(saveData.ExploredRoomCount);
                    bw.Write(saveData.CurrentLocation);
                    bw.Write(saveData.LastRoom);
                    bw.Write(saveData.PreviousLastSector);
                    bw.Write(saveData.LastSector);

                    bw.Write(saveData.KilledMonsters.Count);
                    for (int i = 0; i < saveData.KilledMonsters.Count; i++)
                        bw.Write(saveData.KilledMonsters[i]);

                    bw.Write(saveData.InvestigatedCurios.Count);
                    for (int i = 0; i < saveData.InvestigatedCurios.Count; i++)
                        bw.Write(saveData.InvestigatedCurios[i]);

                    bw.Write(saveData.TorchAmount);
                    bw.Write(saveData.MaxTorchAmount);
                    bw.Write(saveData.ModifiedMinTorch);
                    bw.Write(saveData.ModifiedMaxTorch);
                    #endregion

                    #region Hero Formation
                    saveData.HeroFormationData.WriteFormationData(bw);
                    #endregion

                    #region Inventory
                    bw.Write(saveData.InventoryItems.Count);
                    for (int i = 0; i < saveData.InventoryItems.Count; i++)
                    {
                        bw.Write(saveData.InventoryItems[i] != null);
                        if (saveData.InventoryItems[i] != null)
                        {
                            bw.Write(saveData.InventoryItems[i].Item.Id);
                            bw.Write(saveData.InventoryItems[i].Item.Type);
                            bw.Write(saveData.InventoryItems[i].Item.Amount);
                        }
                    }
                    #endregion

                    #region Battle
                    bw.Write(saveData.inBattle);
                    if (saveData.inBattle == false)
                        return;

                    saveData.battleGroundSaveData.WriteBattlegroundData(bw);
                    #endregion
                    #endregion
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            int x = 0;
            x++;
        }
    }
    public static SaveCampaignData ReadSave(int slotId)
    {
        try
        {
            SaveCampaignData saveData = new SaveCampaignData();
            using (var fs = new FileStream(Application.persistentDataPath + "\\Saves\\DarkestSave" + slotId +
                    ".darkestsave", FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    #region Estate
                    saveData.isFirstStart = br.ReadBoolean(); 
                    saveData.gameVersion = br.ReadString();
                    saveData.hamletTitle = br.ReadString();
                    saveData.locationName = br.ReadString();
                    saveData.questsCompleted = br.ReadInt32();
                    saveData.currentWeek = br.ReadInt32();

                    saveData.saveId = br.ReadInt32();

                    saveData.goldAmount = br.ReadInt32();
                    saveData.bustsAmount = br.ReadInt32();
                    saveData.deedsAmount = br.ReadInt32();
                    saveData.portraitsAmount = br.ReadInt32();
                    saveData.crestsAmount = br.ReadInt32();

                    saveData.saveHeroData = new SaveHeroData[br.ReadInt32()];
                    for (int i = 0; i < saveData.saveHeroData.Length; i++)
                        saveData.saveHeroData[i] = ReadHero(br);

                    saveData.stageCoachData = new SaveHeroData[br.ReadInt32()];
                    for (int i = 0; i < saveData.stageCoachData.Length; i++)
                        saveData.stageCoachData[i] = ReadHero(br);

                    int trinketCount = br.ReadInt32();
                    for (int i = 0; i < trinketCount; i++)
                        saveData.trinketData.Add(br.ReadString());

                    int wagonCount = br.ReadInt32();
                    for (int i = 0; i < wagonCount; i++)
                        saveData.wagonData.Add(br.ReadString());

                    int dungeonProgressCount = br.ReadInt32();
                    for (int i = 0; i < dungeonProgressCount; i++)
                    {
                        var newProgress = new DungeonProgress(br.ReadString(), br.ReadInt32(), br.ReadInt32(), br.ReadBoolean(), br.ReadBoolean());
                        saveData.saveDungeonData.Add(newProgress.DungeonName, newProgress);;
                    }

                    int deathRecordCount = br.ReadInt32();
                    for (int i = 0; i < deathRecordCount; i++)
                    {
                        var newRecord = new DeathRecord();
                        newRecord.HeroName = br.ReadString();
                        newRecord.HeroClassIndex = br.ReadInt32();
                        newRecord.ResolveLevel = br.ReadInt32();
                        newRecord.Factor = (DeathFactor)br.ReadInt32();
                        newRecord.KillerName = br.ReadString();
                        saveData.deathRecords.Add(newRecord);
                    }

                    // Building upgrade trees
                    int buildingTreeCount = br.ReadInt32();
                    saveData.buildingUpgrades = new Dictionary<string, UpgradePurchases>(buildingTreeCount);
                    for (int i = 0; i < buildingTreeCount; i++)
                    {
                        var newPurchases = new UpgradePurchases(br.ReadString());
                        int treeCodesCount = br.ReadInt32();
                        for (int j = 0; j < treeCodesCount; j++)
                            newPurchases.PurchasedUpgrades.Add(br.ReadString());
                        saveData.buildingUpgrades.Add(newPurchases.TreeId, newPurchases);
                    }
                    // Hero upgrade trees
                    saveData.instancedPurchases = new Dictionary<int, Dictionary<string, UpgradePurchases>>();
                    int instancesCount = br.ReadInt32();

                    for (int i = 0; i < instancesCount; i++)
                    {
                        var newInstance = new Dictionary<string, UpgradePurchases>();
                        saveData.instancedPurchases.Add(br.ReadInt32(), newInstance);
                        int instanceTreeCount = br.ReadInt32();
                        for (int j = 0; j < instanceTreeCount; j++)
                        {
                            var newPurchases = new UpgradePurchases(br.ReadString());
                            int treeCodesCount = br.ReadInt32();
                            for (int k = 0; k < treeCodesCount; k++)
                                newPurchases.PurchasedUpgrades.Add(br.ReadString());
                            newInstance.Add(newPurchases.TreeId, newPurchases);
                        }
                    }
                    #region Activity Log
                    int activityLogCount = br.ReadInt32();
                    for (int i = 0; i < activityLogCount; i++)
                    {
                        var newLog = new WeekActivityLog(br.ReadInt32());
                        bool hasReturn = br.ReadBoolean();
                        bool hasEmbark = br.ReadBoolean();
                        if (hasReturn)
                        {
                            newLog.ReturnRecord = new PartyActivityRecord();
                            newLog.ReturnRecord.PartyActionType = (PartyActionType)br.ReadInt32();
                            newLog.ReturnRecord.QuestType = br.ReadString();
                            newLog.ReturnRecord.QuestDifficulty = br.ReadString();
                            newLog.ReturnRecord.QuestLength = br.ReadString();
                            newLog.ReturnRecord.Dungeon = br.ReadString();

                            int namesCount = br.ReadInt32();
                            for (int j = 0; j < namesCount; j++)
                                newLog.ReturnRecord.Names.Add(br.ReadString());

                            int classesCount = br.ReadInt32();
                            for (int j = 0; j < classesCount; j++)
                                newLog.ReturnRecord.Classes.Add(br.ReadString());

                            int aliveCount = br.ReadInt32();
                            for (int j = 0; j < aliveCount; j++)
                                newLog.ReturnRecord.Alive.Add(br.ReadBoolean());

                            newLog.ReturnRecord.IsSuccessfull = br.ReadBoolean();
                        }
                        if (hasEmbark)
                        {
                            newLog.EmbarkRecord = new PartyActivityRecord();
                            newLog.EmbarkRecord.PartyActionType = (PartyActionType)br.ReadInt32();
                            newLog.EmbarkRecord.QuestType = br.ReadString();
                            newLog.EmbarkRecord.QuestDifficulty = br.ReadString();
                            newLog.EmbarkRecord.QuestLength = br.ReadString();
                            newLog.EmbarkRecord.Dungeon = br.ReadString();

                            int namesCount = br.ReadInt32();
                            for (int j = 0; j < namesCount; j++)
                                newLog.EmbarkRecord.Names.Add(br.ReadString());

                            int classesCount = br.ReadInt32();
                            for (int j = 0; j < classesCount; j++)
                                newLog.EmbarkRecord.Classes.Add(br.ReadString());

                            int aliveCount = br.ReadInt32();
                            for (int j = 0; j < aliveCount; j++)
                                newLog.EmbarkRecord.Alive.Add(br.ReadBoolean());

                            newLog.EmbarkRecord.IsSuccessfull = br.ReadBoolean();
                        }
                        saveData.activityLog.Add(newLog);
                    }
                    #endregion

                    #region Quests
                    int completedPlotCount = br.ReadInt32();
                    saveData.completedPlot.Clear();
                    for (int i = 0; i < completedPlotCount; i++)
                        saveData.completedPlot.Add(br.ReadString());

                    int generatedQuestCount = br.ReadInt32();
                    saveData.generatedQuests.Clear();
                    for (int i = 0; i < generatedQuestCount; i++)
                    {
                        Quest quest;
                        string plotGenId = br.ReadString();
                        if (plotGenId == "tutorial")
                        {
                            quest = new PlotQuest()
                            {
                                Id = plotGenId,
                                PlotTrinket = new PlotTrinketReward() { Amount = 0, Rarity = "very_common" },
                            };
                        }
                        else if (plotGenId != "")
                            quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(plQuest => plQuest.Id == plotGenId).Copy();
                        else
                            quest = new Quest();

                        quest.Type = br.ReadString();
                        quest.Dungeon = br.ReadString();
                        quest.Difficulty = br.ReadInt32();
                        quest.Length = br.ReadInt32();
                        quest.Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals[br.ReadString()];
                        quest.Reward = new CompletionReward();
                        quest.Reward.ResolveXP = br.ReadInt32();
                        int questRewardGenCount = br.ReadInt32();
                        quest.Reward.ItemDefinitions.Clear();
                        for (int j = 0; j < questRewardGenCount; j++)
                        {
                            quest.Reward.ItemDefinitions.Add(new ItemDefinition()
                            {
                                Id = br.ReadString(),
                                Type = br.ReadString(),
                                Amount = br.ReadInt32(),
                            });
                        }

                        quest.IsProgression = br.ReadBoolean();
                        quest.HasStatueContents = br.ReadBoolean();
                        quest.CompletionDungeonXp = br.ReadBoolean();
                        quest.CanRetreat = br.ReadBoolean();
                        quest.AlwaysRetreatFromRaid = br.ReadBoolean();
                        quest.RetreatKillCount = br.ReadInt32();
                        quest.IsSurpriseEnabled = br.ReadBoolean();
                        quest.IsScoutingEnabled = br.ReadBoolean();
                        quest.IsStressClearedOnCompletion = br.ReadBoolean();
                        quest.RosterBuffOnFailureMinimumPartyResolveLevel = br.ReadInt32();
                        int rostBuffsFailGenCount = br.ReadInt32();
                        quest.RosterBuffsOnFailure.Clear();
                        for (int j = 0; j < rostBuffsFailGenCount; j++)
                            quest.RosterBuffsOnFailure.Add(DarkestDungeonManager.Data.Buffs[br.ReadString()]);

                        int suggestedGenCount = br.ReadInt32();
                        quest.SuggestedTrinkets.Clear();
                        for (int j = 0; j < suggestedGenCount; j++)
                            quest.SuggestedTrinkets.Add(new ItemDefinition("trinket", br.ReadString(), br.ReadInt32()));

                        int tagGenCount = br.ReadInt32();
                        quest.UpgradeTagsRemovedOnIgnore.Clear();
                        for (int j = 0; j < tagGenCount; j++)
                            quest.UpgradeTagsRemovedOnIgnore.Add(new UpgradeTag(br.ReadString(), br.ReadInt32()));

                        saveData.generatedQuests.Add(quest);
                    }
                    #endregion

                    #region Activity Slots
                    int abbeyActivityCount = br.ReadInt32();
                    for (int i = 0; i < abbeyActivityCount; i++)
                    {
                        int abbeySlotCount = br.ReadInt32();
                        var newActivitySlotList = new List<SaveActivitySlot>();

                        for (int j = 0; j < abbeySlotCount; j++)
                        {
                            var newSaveActivity = new SaveActivitySlot()
                            {
                                HeroRosterId = br.ReadInt32(),
                                Status = (ActivitySlotStatus)br.ReadInt32(),
                            };
                            newActivitySlotList.Add(newSaveActivity);
                        }
                        saveData.abbeyActivitySlots.Add(newActivitySlotList);
                    }
                    int tavernActivityCount = br.ReadInt32();
                    for (int i = 0; i < tavernActivityCount; i++)
                    {
                        int tavernSlotCount = br.ReadInt32();
                        var newActivitySlotList = new List<SaveActivitySlot>();

                        for (int j = 0; j < tavernSlotCount; j++)
                        {
                            var newSaveActivity = new SaveActivitySlot()
                            {
                                HeroRosterId = br.ReadInt32(),
                                Status = (ActivitySlotStatus)br.ReadInt32(),
                            };
                            newActivitySlotList.Add(newSaveActivity);
                        }
                        saveData.tavernActivitySlots.Add(newActivitySlotList);
                    }

                    int sanitariumActivityCount = br.ReadInt32();
                    for (int i = 0; i < sanitariumActivityCount; i++)
                    {
                        int sanitariumSlotCount = br.ReadInt32();
                        var newActivitySlotList = new List<SaveActivitySlot>();

                        for (int j = 0; j < sanitariumSlotCount; j++)
                        {
                            var newSaveActivity = new SaveActivitySlot()
                            {
                                HeroRosterId = br.ReadInt32(),
                                Status = (ActivitySlotStatus)br.ReadInt32(),
                                TargetDiseaseQuirk = br.ReadString(),
                                TargetNegativeQuirk = br.ReadString(),
                                TargetPositiveQuirk = br.ReadString(),
                            };
                            newActivitySlotList.Add(newSaveActivity);
                        }
                        saveData.sanitariumActivitySlots.Add(newActivitySlotList);
                    }
                    #endregion
                    #endregion

                    #region Raid
                    saveData.InRaid = br.ReadBoolean();
                    if (saveData.InRaid == false)
                        return saveData;

                    #region Quest
                    saveData.QuestCompleted = br.ReadBoolean();
                    string plotId = br.ReadString();
                    if (plotId == "tutorial")
                    {
                        saveData.Quest = new PlotQuest()
                        {
                            Id = plotId,
                            PlotTrinket = new PlotTrinketReward() { Amount = 0, Rarity = "very_common" },
                        };
                    }
                    else if (plotId != "")
                        saveData.Quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(plQuest => plQuest.Id == plotId).Copy();
                    else
                        saveData.Quest = new Quest();

                    saveData.Quest.Type = br.ReadString();
                    saveData.Quest.Dungeon = br.ReadString();
                    saveData.Quest.Difficulty = br.ReadInt32();
                    saveData.Quest.Length = br.ReadInt32();
                    saveData.Quest.Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals[br.ReadString()];
                    saveData.Quest.Reward = new CompletionReward();
                    saveData.Quest.Reward.ResolveXP = br.ReadInt32();
                    int questRewardCount = br.ReadInt32();
                    saveData.Quest.Reward.ItemDefinitions.Clear();
                    for (int i = 0; i < questRewardCount; i++)
                    {
                        saveData.Quest.Reward.ItemDefinitions.Add(new ItemDefinition()
                            {
                                Id = br.ReadString(),
                                Type = br.ReadString(),
                                Amount = br.ReadInt32(),
                            });
                    }

                    saveData.Quest.IsProgression = br.ReadBoolean();
                    saveData.Quest.HasStatueContents = br.ReadBoolean();
                    saveData.Quest.CompletionDungeonXp = br.ReadBoolean();
                    saveData.Quest.CanRetreat = br.ReadBoolean();
                    saveData.Quest.AlwaysRetreatFromRaid = br.ReadBoolean();
                    saveData.Quest.RetreatKillCount = br.ReadInt32();
                    saveData.Quest.IsSurpriseEnabled = br.ReadBoolean();
                    saveData.Quest.IsScoutingEnabled = br.ReadBoolean();
                    saveData.Quest.IsStressClearedOnCompletion = br.ReadBoolean();
                    saveData.Quest.RosterBuffOnFailureMinimumPartyResolveLevel = br.ReadInt32();
                    int rostBuffsFailCount = br.ReadInt32();
                    saveData.Quest.RosterBuffsOnFailure.Clear();
                    for (int i = 0; i < rostBuffsFailCount; i++)
                        saveData.Quest.RosterBuffsOnFailure.Add(DarkestDungeonManager.Data.Buffs[br.ReadString()]);

                    int suggestedCount = br.ReadInt32();
                    saveData.Quest.SuggestedTrinkets.Clear();
                    for (int i = 0; i < suggestedCount; i++)
                        saveData.Quest.SuggestedTrinkets.Add(new ItemDefinition("trinket", br.ReadString(), br.ReadInt32()));

                    int tagCount = br.ReadInt32();
                    saveData.Quest.UpgradeTagsRemovedOnIgnore.Clear();
                    for (int i = 0; i < tagCount; i++)
                        saveData.Quest.UpgradeTagsRemovedOnIgnore.Add(new UpgradeTag(br.ReadString(), br.ReadInt32()));
                    #endregion

                    #region Dungeon
                    saveData.Dungeon = new Dungeon();
                    saveData.Dungeon.Name = br.ReadString();
                    saveData.Dungeon.GridSizeX = br.ReadInt32();
                    saveData.Dungeon.GridSizeY = br.ReadInt32();
                    saveData.Dungeon.StartingRoomId = br.ReadString();

                    #region Rooms
                    int roomCount = br.ReadInt32();
                    for (int i = 0; i < roomCount; i++)
                    {
                        var room = new Room(br.ReadString());
                        int doorCount = br.ReadInt32();
                        for (int j = 0; j < doorCount; j++)
                        {
                            room.Doors.Add(new Door("to", br.ReadString(), (Direction)br.ReadInt32()));
                        }
                        room.TextureId = br.ReadString();
                        room.Type = (AreaType)br.ReadInt32();
                        room.Knowledge = (Knowledge)br.ReadInt32();
                        bool hasProp = br.ReadBoolean();
                        if (hasProp)
                        {
                            AreaType propType = (AreaType)br.ReadInt32();

                            switch (propType)
                            {
                                case AreaType.Door:
                                    room.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                    break;
                                case AreaType.Curio:
                                    bool isQuestCurio = br.ReadBoolean();
                                    if (isQuestCurio)
                                    {
                                        if (saveData.Quest.Goal.Type == "activate")
                                        {
                                            if (saveData.Quest.Goal.QuestData is QuestActivateData)
                                            {
                                                var activateData = saveData.Quest.Goal.QuestData as QuestActivateData;
                                                var curio = new Curio(activateData.CurioName); br.ReadString();
                                                curio.IsQuestCurio = true;

                                                if (saveData.Quest.Goal.StartingItems.Count > 0)
                                                {
                                                    curio.ItemInteractions.Add(new ItemInteraction()
                                                    {
                                                        Chance = 1,
                                                        ItemId = saveData.Quest.Goal.StartingItems[0].Id,
                                                        ResultType = "loot",
                                                        Results = new List<CurioResult>(),
                                                    });
                                                }
                                                room.Prop = curio;
                                            }
                                        }
                                        else if (saveData.Quest.Goal.Type == "gather")
                                        {
                                            if (saveData.Quest.Goal.QuestData is QuestGatherData)
                                            {
                                                var gatherData = saveData.Quest.Goal.QuestData as QuestGatherData;
                                                var curio = new Curio(gatherData.CurioName); br.ReadString();
                                                curio.IsQuestCurio = true;

                                                var curioInteraction = new CurioInteraction();
                                                curioInteraction.Chance = 1;
                                                curioInteraction.ResultType = "loot";
                                                curioInteraction.Results = new List<CurioResult>();
                                                curioInteraction.Results.Add(new CurioResult()
                                                {
                                                    Chance = 1,
                                                    Draws = 1,
                                                    Item = gatherData.Item.Id,
                                                });
                                                curio.Results.Add(curioInteraction);
                                                room.Prop = curio;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                        newCurio.IsQuestCurio = isQuestCurio;
                                        room.Prop = newCurio;
                                    }
                                    break;
                                case AreaType.Obstacle:
                                    room.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                    break;
                                case AreaType.Trap:
                                    room.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                    break;
                            }
                        }
                        bool hasBattle = br.ReadBoolean();
                        if (hasBattle)
                        {
                            room.BattleEncounter = new BattleEncounter();
                            int monsterCount = br.ReadInt32();
                            for (int j = 0; j < monsterCount; j++)
                                room.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                            room.BattleEncounter.Cleared = br.ReadBoolean();
                        }
                        saveData.Dungeon.Rooms.Add(room.Id, room);
                    }
                    #endregion

                    #region Hallways
                    int hallwayCount = br.ReadInt32();
                    for (int i = 0; i < hallwayCount; i++)
                    {
                        var hallway = new Hallway(br.ReadString());
                        hallway.RoomA = saveData.Dungeon.Rooms[br.ReadString()];
                        hallway.RoomB = saveData.Dungeon.Rooms[br.ReadString()];
                        int hallsCount = br.ReadInt32();
                        for (int j = 0; j < hallsCount; j++)
                        {
                            var hallSector = new HallSector(br.ReadString(), hallway);
                            hallSector.TextureId = br.ReadString();
                            hallSector.Type = (AreaType)br.ReadInt32();
                            hallSector.Knowledge = (Knowledge)br.ReadInt32();
                            bool hasProp = br.ReadBoolean();
                            if (hasProp)
                            {
                                AreaType propType = (AreaType)br.ReadInt32();

                                switch (propType)
                                {
                                    case AreaType.Door:
                                        hallSector.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                        break;
                                    case AreaType.Curio:
                                        bool isQuestCurio = br.ReadBoolean();
                                        Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                        newCurio.IsQuestCurio = isQuestCurio;
                                        hallSector.Prop = newCurio;
                                        break;
                                    case AreaType.Obstacle:
                                        hallSector.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                        break;
                                    case AreaType.Trap:
                                        hallSector.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                        break;
                                }
                            }
                            bool hasBattle = br.ReadBoolean();
                            if (hasBattle)
                            {
                                hallSector.BattleEncounter = new BattleEncounter();
                                int monsterCount = br.ReadInt32();
                                for (int k = 0; k < monsterCount; k++)
                                    hallSector.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                                hallSector.BattleEncounter.Cleared = br.ReadBoolean();
                            }
                            hallway.Halls.Add(hallSector);
                        }
                        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
                    }
                    #endregion

                    var envData = DarkestDungeonManager.Data.DungeonEnviromentData[saveData.Quest.Dungeon];
                    saveData.Dungeon.SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].BattleMashes.Find(mash => mash.MashId == saveData.Quest.Difficulty);
                    saveData.Dungeon.DungeonMash = envData.BattleMashes.Find(mash => mash.MashId == saveData.Quest.Difficulty);

                    int sharedExecIdsCount = br.ReadInt32();
                    for (int i = 0; i < sharedExecIdsCount; i++)
                        saveData.Dungeon.SharedMashExecutionIds.Add(br.ReadInt32());
                    #endregion

                    #region Raid Party
                    saveData.RaidParty = new RaidPartySaveData();
                    saveData.RaidParty.IsMovingLeft = br.ReadBoolean();
                    int raidHeroCount = br.ReadInt32();

                    for (int i = 0; i < raidHeroCount; i++)
                    {
                        var heroInfo = new RaidPartyHeroInfoSaveData();
                        heroInfo.HeroRosterId = br.ReadInt32();
                        heroInfo.IsAlive = br.ReadBoolean();
                        if (heroInfo.IsAlive == false)
                        {
                            heroInfo.Factor = (DeathFactor)br.ReadInt32();
                            heroInfo.Killer = br.ReadString();
                        }
                        saveData.RaidParty.HeroInfo.Add(heroInfo);
                    }
                    #endregion

                    #region Raid Info
                    saveData.CampingPhase = (CampingPhase)br.ReadInt32();
                    saveData.CampingTimeLeft = br.ReadInt32();
                    saveData.NightAmbushReduced = br.ReadSingle();
                    saveData.HungerCooldown = br.ReadInt32();

                    saveData.ExploredRoomCount = br.ReadInt32();
                    saveData.CurrentLocation = br.ReadString();
                    saveData.LastRoom = br.ReadString();
                    saveData.PreviousLastSector = br.ReadString();
                    saveData.LastSector = br.ReadString();

                    int monsterKilledCount = br.ReadInt32();
                    for (int i = 0; i < monsterKilledCount; i++)
                        saveData.KilledMonsters.Add(br.ReadString());

                    int investigatedCount = br.ReadInt32();
                    for (int i = 0; i < investigatedCount; i++)
                        saveData.InvestigatedCurios.Add(br.ReadString());

                    saveData.TorchAmount = br.ReadInt32();
                    saveData.MaxTorchAmount = br.ReadInt32();
                    saveData.ModifiedMinTorch = br.ReadInt32();
                    saveData.ModifiedMaxTorch = br.ReadInt32();

                    #endregion

                    #region HeroFormationData
                    saveData.HeroFormationData = new BattleFormationSaveData();
                    saveData.HeroFormationData.ReadFormationData(br);
                    #endregion

                    #region Raid Party Inventory
                    int inventorySlotCount = br.ReadInt32();
                    saveData.InventoryItems = new List<InventorySlotData>();
                    for (int i = 0; i < inventorySlotCount; i++)
                    {
                        bool hasItem = br.ReadBoolean();
                        if (hasItem)
                        {
                            var newItem = new ItemDefinition()
                            {
                                Id = br.ReadString(),
                                Type = br.ReadString(),
                                Amount = br.ReadInt32(),
                            };
                            saveData.InventoryItems.Add(new InventorySlotData(newItem));
                        }
                        else
                            saveData.InventoryItems.Add(null);
                        
                    }
                    #endregion

                    #region Battle
                    saveData.inBattle = br.ReadBoolean();
                    if (saveData.inBattle == false)
                        return saveData;

                    saveData.battleGroundSaveData.ReadBattlegroundData(br);
                    #endregion
                    #endregion

                    return saveData;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            return null;
        }
    }

    public static void DeleteSave(int slotId)
    {
        if (File.Exists(Application.persistentDataPath + "\\Saves\\DarkestSave" + slotId + ".darkestsave"))
            File.Delete(Application.persistentDataPath + "\\Saves\\DarkestSave" + slotId + ".darkestsave");
    }

    public static SaveCampaignData WriteStartingSave(SaveCampaignData saveData)
    {
        saveData.isFirstStart = true;
        saveData.gameVersion = Application.version;
        saveData.locationName = "Raid";
        saveData.questsCompleted = 0;
        saveData.currentWeek = 1;

        saveData.goldAmount = 1000;
        saveData.bustsAmount = 10;
        saveData.deedsAmount = 10;
        saveData.portraitsAmount = 10;
        saveData.crestsAmount = 10;

        #region Initial Heroes
        saveData.saveHeroData = new SaveHeroData[2]
        {
            #region Hero 1
            new SaveHeroData()
            {
                rosterId = 1,
                name = "Reynald",
                heroClass = "crusader",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 10,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                rosterId = 2,
                name = "Dismas",
                heroClass = "highwayman",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 10,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.stageCoachData = new SaveHeroData[0];
        #endregion

        #region Initial Hero Purchases
        saveData.instancedPurchases = new Dictionary<int, Dictionary<string, UpgradePurchases>>();
        for (int i = 0; i < saveData.saveHeroData.Length; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.instancedPurchases.Add(saveData.saveHeroData[i].rosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.saveHeroData[i].heroClass + ".weapon", new UpgradePurchases(saveData.saveHeroData[i].heroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.saveHeroData[i].heroClass + ".armour", new UpgradePurchases(saveData.saveHeroData[i].heroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.saveHeroData[i].heroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.saveHeroData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.saveHeroData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.instancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.saveHeroData[0].selectedCombatSkillIndexes.Add(0);
        saveData.saveHeroData[0].selectedCombatSkillIndexes.Add(1);
        saveData.saveHeroData[0].selectedCombatSkillIndexes.Add(2);
        saveData.saveHeroData[0].selectedCombatSkillIndexes.Add(3);
        saveData.instancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.saveHeroData[0].selectedCampingSkillIndexes.Add(0);
        saveData.saveHeroData[0].selectedCampingSkillIndexes.Add(4);
        saveData.saveHeroData[0].selectedCampingSkillIndexes.Add(5);


        saveData.instancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.saveHeroData[1].selectedCombatSkillIndexes.Add(6);
        saveData.saveHeroData[1].selectedCombatSkillIndexes.Add(1);
        saveData.saveHeroData[1].selectedCombatSkillIndexes.Add(3);
        saveData.saveHeroData[1].selectedCombatSkillIndexes.Add(4);
        saveData.instancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.instancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.saveHeroData[0].selectedCampingSkillIndexes.Add(1);
        saveData.saveHeroData[0].selectedCampingSkillIndexes.Add(5);
        saveData.saveHeroData[0].selectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.activityLog = new List<WeekActivityLog>();
        saveData.completedPlot = new List<string>();
        saveData.generatedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.trinketData.Clear();

        saveData.wagonData.Clear();

        saveData.saveDungeonData.Clear();
        saveData.saveDungeonData.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.saveDungeonData.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.saveDungeonData.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.saveDungeonData.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.saveDungeonData.Add("darkest", new DungeonProgress("darkest", 0, 0, false, false));

        saveData.deathRecords = new List<DeathRecord>();

        saveData.buildingUpgrades = new Dictionary<string, UpgradePurchases>();

        saveData.buildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.buildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.buildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.buildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.buildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.buildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.buildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.buildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.buildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.buildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.buildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.buildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.buildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.buildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.buildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.buildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.buildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.buildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.buildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.buildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.abbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.tavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.sanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = new PlotQuest()
        {
            IsPlotQuest = true,
            Id = "tutorial",
            Difficulty = 1,
            Type = "tutorial_room",
            Dungeon = "weald",
            DungeonLevel = 1,
            Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals["tutorial_final_room"],
            Length = 1,
            PlotTrinket = new PlotTrinketReward() {  Amount = 0, Rarity = "very_common"},
            Reward = new CompletionReward()
            {
                 ResolveXP = 2,
                 ItemDefinitions = new List<ItemDefinition>()
                 {
                     new ItemDefinition("gold", "", 5000),
                 }
            },

            CanRetreat = false,
            CompletionDungeonXp = false,
        };
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = "weald";
        saveData.Dungeon.GridSizeX = 2;
        saveData.Dungeon.GridSizeY = 2;
        saveData.Dungeon.StartingRoomId = "room1_1";

        Room room = new Room("room1_1")
        {
            Knowledge = Knowledge.Completed,
            Type = AreaType.Entrance,
            MashId = 1,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "effigy_0",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);

        room = new Room("room2_1")
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            MashId = 1,
            Prop = DarkestDungeonManager.Data.Curios["bandits_trapped_chest"],
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["weald"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_2"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "effigy_1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);

        Hallway hallway = new Hallway("hallroom2_1_room1_1");
        hallway.RoomA = saveData.Dungeon.Rooms["room2_1"];
        hallway.RoomB = saveData.Dungeon.Rooms["room1_1"];
        hallway.RoomA.Doors.Add(new Door("room2_1", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room1_1", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "7",
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["travellers_tent_tutorial"],
            },
            new HallSector("2", hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "8",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["weald"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_1"][0].MonsterSet),
            },
            new HallSector("4", hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room1_1";
        saveData.LastRoom = "room1_1";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
            {
                IsHero = true,
                RosterId = 1,
                Rank = 1,
                CombatInfo = new FormationUnitInfo()
                {
                     CombatId = 1,
                }
            });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }
    public static SaveCampaignData WriteTestingSave(SaveCampaignData saveData)
    {
        saveData.isFirstStart = true;
        saveData.gameVersion = Application.version;
        saveData.locationName = "Hamlet";
        saveData.questsCompleted = 50;
        saveData.currentWeek = 2;

        saveData.goldAmount = 120000;
        saveData.bustsAmount = 123;
        saveData.deedsAmount = 128;
        saveData.portraitsAmount = 274;
        saveData.crestsAmount = 492;

        #region Initial Heroes
        saveData.saveHeroData = new SaveHeroData[20]
        {
            #region Hero 1
            new SaveHeroData()
            {
                rosterId = 1,
                name = "Reynald",
                heroClass = "plague_doctor",
                resolveLevel = 6,
                resolveXP = 94,
                stressLevel = 30,
                weaponLevel = 5,
                armorLevel = 4,
                leftTrinketId = "ancestors_coat",
                rightTrinketId = "ancestors_musket_ball",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                    new QuirkInfo("meditator"),
                    new QuirkInfo("tapeworm"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                rosterId = 2,
                name = "Dismas",
                heroClass = "occultist",
                resolveLevel = 5,
                resolveXP = 60,
                stressLevel = 15,
                weaponLevel = 3,
                armorLevel = 3,
                leftTrinketId = "ancestors_pistol",
                rightTrinketId = "ancestors_pen",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 3
            new SaveHeroData()
            {
                rosterId = 3,
                name = "Renold",
                heroClass = "man_at_arms",
                resolveLevel = 4,
                resolveXP = 10,
                stressLevel = 0,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "ancestors_handkerchief",
                rightTrinketId = "ancestors_lantern",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_skinned"),
                    new QuirkInfo("quick_reflexes"),
                    new QuirkInfo("quickdraw"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("quickdraw"),
                },
            },
            #endregion
            #region Hero 4
            new SaveHeroData()
            {
                rosterId = 4,
                name = "Maudit",
                heroClass = "abomination",
                resolveLevel = 5,
                resolveXP = 79,
                stressLevel = 60,
                weaponLevel = 5,
                armorLevel = 2,
                leftTrinketId = "ancestors_signet_ring",
                rightTrinketId = "ancestors_moustache_cream",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("tapeworm"),
                },
            },
            #endregion
            #region Hero 5
            new SaveHeroData()
            {
                rosterId = 5,
                name = "Fairfax",
                heroClass = "hellion",
                resolveLevel = 0,
                resolveXP = 1,
                stressLevel = 64,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "ancestors_map",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 6
            new SaveHeroData()
            {
                rosterId = 6,
                name = "Bernard",
                heroClass = "jester",
                resolveLevel = 1,
                resolveXP = 7,
                stressLevel = 99,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "ancestors_candle",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("warrior_of_light"),
                },
            },
            #endregion
            #region Hero 7
            new SaveHeroData()
            {
                rosterId = 7,
                name = "Trixy",
                heroClass = "grave_robber",
                resolveLevel = 4,
                resolveXP = 5,
                stressLevel = 15,
                weaponLevel = 2,
                armorLevel = 2,
                leftTrinketId = "ancestors_tentacle_idol",
                rightTrinketId = "ancestors_bottle",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("hard_skinned"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 8
            new SaveHeroData()
            {
                rosterId = 8,
                name = "Leonel",
                heroClass = "occultist",
                resolveLevel = 4,
                resolveXP = 21,
                stressLevel = 27,
                weaponLevel = 3,
                armorLevel = 3,
                leftTrinketId = "legendary_bracer",
                rightTrinketId = "focus_ring",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("meditator"),
                },
            },
            #endregion
            #region Hero 9
            new SaveHeroData()
            {
                rosterId = 9,
                name = "Daston",
                heroClass = "plague_doctor",
                resolveLevel = 6,
                resolveXP = 90,
                stressLevel = 87,
                weaponLevel = 4,
                armorLevel = 4,
                leftTrinketId = "tough_ring",
                rightTrinketId = "cleansing_crystal",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("slugger"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 10
            new SaveHeroData()
            {
                rosterId = 10,
                name = "Murax",
                heroClass = "leper",
                resolveLevel = 6,
                resolveXP = 15,
                stressLevel = 62,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 11
            new SaveHeroData()
            {
                rosterId = 11,
                name = "John",
                heroClass = "crusader",
                resolveLevel = 3,
                resolveXP = 10,
                stressLevel = 10,
                weaponLevel = 3,
                armorLevel = 3,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 12
            new SaveHeroData()
            {
                rosterId = 12,
                name = "Tulio",
                heroClass = "bounty_hunter",
                resolveLevel = 2,
                resolveXP = 12,
                stressLevel = 15,
                weaponLevel = 2,
                armorLevel = 2,
                leftTrinketId = "spiked_collar",
                rightTrinketId = "poisoning_buckle",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                },
            },
            #endregion
            #region Hero 13
            new SaveHeroData()
            {
                rosterId = 13,
                name = "Axe",
                heroClass = "leper",
                resolveLevel = 5,
                resolveXP = 20,
                stressLevel = 0,
                weaponLevel = 2,
                armorLevel = 3,
                leftTrinketId = "berserk_mask",
                rightTrinketId = "martyrs_seal",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                },
            },
            #endregion
            #region Hero 14
            new SaveHeroData()
            {
                rosterId = 14,
                name = "Jaraxus",
                heroClass = "highwayman",
                resolveLevel = 6,
                resolveXP = 78,
                stressLevel = 100,
                weaponLevel = 5,
                armorLevel = 2,
                leftTrinketId = "sun_cloak",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("robust"),
                },
            },
            #endregion
            #region Hero 15
            new SaveHeroData()
            {
                rosterId = 15,
                name = "Kirk",
                heroClass = "crusader",
                resolveLevel = 4,
                resolveXP = 31,
                stressLevel = 64,
                weaponLevel = 3,
                armorLevel = 4,
                leftTrinketId = "holy_orders",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                },
            },
            #endregion
            #region Hero 16
            new SaveHeroData()
            {
                rosterId = 16,
                name = "William",
                heroClass = "jester",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 99,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "berserk_charm",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 17
            new SaveHeroData()
            {
                rosterId = 17,
                name = "Jorgen",
                heroClass = "occultist",
                resolveLevel = 1,
                resolveXP = 2,
                stressLevel = 15,
                weaponLevel = 2,
                armorLevel = 2,
                leftTrinketId = "",
                rightTrinketId = "feather_crystal",
                quirks = new List<QuirkInfo>()
                {
                },
            },
            #endregion
            #region Hero 18
            new SaveHeroData()
            {
                rosterId = 18,
                name = "Robert",
                heroClass = "plague_doctor",
                resolveLevel = 5,
                resolveXP = 40,
                stressLevel = 27,
                weaponLevel = 3,
                armorLevel = 3,
                leftTrinketId = "brawlers_gloves",
                rightTrinketId = "life_crystal",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                },
            },
            #endregion
            #region Hero 19
            new SaveHeroData()
            {
                rosterId = 19,
                name = "Quasim",
                heroClass = "highwayman",
                resolveLevel = 5,
                resolveXP = 60,
                stressLevel = 87,
                weaponLevel = 4,
                armorLevel = 4,
                leftTrinketId = "snipers_ring",
                rightTrinketId = "quick_draw_charm",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                },
            },
            #endregion
            #region Hero 20
            new SaveHeroData()
            {
                rosterId = 20,
                name = "Lion",
                heroClass = "crusader",
                resolveLevel = 6,
                resolveXP = 94,
                stressLevel = 62,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "bulls_eye_bandana",
                rightTrinketId = "protective_collar",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.stageCoachData = new SaveHeroData[4]
        {
            #region Hero 1
            new SaveHeroData()
            {
                rosterId = 21,
                name = "Recald",
                heroClass = "crusader",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 0,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("meditator"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                rosterId = 22,
                name = "Resmas",
                heroClass = "highwayman",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 0,
                weaponLevel = 0,
                armorLevel = 0,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slugger"),
                    new QuirkInfo("warrior_of_light"),
                },
            },
            #endregion
            #region Hero 3
            new SaveHeroData()
            {
                rosterId = 23,
                name = "Recold",
                heroClass = "bounty_hunter",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 0,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_skinned"),
                    new QuirkInfo("slugger"),
                },
            },
            #endregion
            #region Hero 4
            new SaveHeroData()
            {
                rosterId = 24,
                name = "Reudit",
                heroClass = "vestal",
                resolveLevel = 0,
                resolveXP = 0,
                stressLevel = 0,
                weaponLevel = 1,
                armorLevel = 1,
                leftTrinketId = "",
                rightTrinketId = "",
                quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("slow_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region Initial Hero Purchases
        saveData.instancedPurchases = new Dictionary<int, Dictionary<string, UpgradePurchases>>();
        List<string> equipCodes = new List<string>() { "0", "1", "2", "3", "4" };
        for (int i = 0; i < saveData.saveHeroData.Length; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.instancedPurchases.Add(saveData.saveHeroData[i].rosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.saveHeroData[i].heroClass + ".weapon", new UpgradePurchases(saveData.saveHeroData[i].heroClass + ".weapon", equipCodes.GetRange(0,
                Mathf.Clamp(saveData.saveHeroData[i].weaponLevel - 1, 0, 4)).ToArray()));
            newHeroPurchases.Add(saveData.saveHeroData[i].heroClass + ".armour", new UpgradePurchases(saveData.saveHeroData[i].heroClass + ".armour", equipCodes.GetRange(0,
                Mathf.Clamp(saveData.saveHeroData[i].armorLevel - 1, 0, 4)).ToArray()));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.saveHeroData[i].heroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
            {
                    newHeroPurchases.Add(saveData.saveHeroData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                        saveData.saveHeroData[i].heroClass + "." + heroClass.CombatSkills[j].Id,equipCodes.GetRange(0,
                            Mathf.Clamp(saveData.saveHeroData[i].armorLevel, 0, 5)).ToArray()));
            }
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
            {
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(heroClass.CampingSkills[j].Id, new string[] { "0" }));
            }

            if (saveData.saveHeroData[i].heroClass != "abomination")
            {
                saveData.saveHeroData[i].selectedCombatSkillIndexes.AddRange(new int[] { 0, 1, 2, 3 });
                saveData.saveHeroData[i].selectedCampingSkillIndexes.AddRange(new int[] { 0, 3, 5, 6 });
            }
            else
            {
                saveData.saveHeroData[i].selectedCombatSkillIndexes.AddRange(new int[] { 0, 1, 2, 3, 4, 5, 6 });
                saveData.saveHeroData[i].selectedCampingSkillIndexes.AddRange(new int[] { 0, 3, 5, 6 });
            }

        }
        for (int i = 0; i < saveData.stageCoachData.Length; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.instancedPurchases.Add(saveData.stageCoachData[i].rosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.stageCoachData[i].heroClass + ".weapon", new UpgradePurchases(saveData.stageCoachData[i].heroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.stageCoachData[i].heroClass + ".armour", new UpgradePurchases(saveData.stageCoachData[i].heroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.stageCoachData[i].heroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
            {
                if (j == 4 || j == 5 || j == 6 )
                    newHeroPurchases.Add(saveData.stageCoachData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                        saveData.stageCoachData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
                else
                    newHeroPurchases.Add(saveData.stageCoachData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                        saveData.stageCoachData[i].heroClass + "." + heroClass.CombatSkills[j].Id, new string[1] { "0" }));

            }
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
            {
                if (j == 0 || j == 2 || j == 4 || j == 5)
                    newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(heroClass.CampingSkills[j].Id, new string[] { "0" }));
                else
                    newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(heroClass.CampingSkills[j].Id, new string[] { }));
            }
        }
        #endregion

        #region Activity Log
        saveData.activityLog = new List<WeekActivityLog>()
        {
            new WeekActivityLog(1)
            {
                 ReturnRecord = new PartyActivityRecord()
                 {
                      PartyActionType = PartyActionType.Tutorial,
                      QuestType = "explore",
                      QuestDifficulty = "1",
                      QuestLength = "1",
                      Dungeon = "weald",
                      Names = new List<string>(new string[2] { "Reynald", "Dismas", }),
                      Classes = new List<string>(new string[2] { "crusader", "highwayman"}),
                      Alive = new List<bool>(new bool[2] { true, true}),
                      IsSuccessfull = true,
                 },
                 EmbarkRecord = new PartyActivityRecord()
                 {
                      PartyActionType = PartyActionType.Embark,
                      QuestType = "gather",
                      QuestDifficulty = "1",
                      QuestLength = "2",
                      Dungeon = "crypts",
                      Names = new List<string>(new string[4] { "Reynald", "Dismas", "Renold", "Maudit" }),
                      Classes = new List<string>(new string[4] { "crusader", "highwayman", "bounty_hunter", "vestal" }),
                      Alive = new List<bool>(new bool[4] { true, true, true, true }),
                      IsSuccessfull = true,
                 },
            },
            new WeekActivityLog(2)
            {
                ReturnRecord = new PartyActivityRecord()
                 {
                      PartyActionType = PartyActionType.Result,
                      QuestType = "gather",
                      QuestDifficulty = "1",
                      QuestLength = "2",
                      Dungeon = "crypts",
                      Names = new List<string>(new string[4] { "Reynald", "Dismas", "Renold", "Maudit" }),
                      Classes = new List<string>(new string[4] { "crusader", "highwayman", "bounty_hunter", "vestal" }),
                      Alive = new List<bool>(new bool[4] { true, true, true, true }),
                      IsSuccessfull = true,
                 },
            },
        };
        #endregion

        #region Completed Plot
        saveData.completedPlot = new List<string>() { "leper", "crusader", "plot_tutorial_crypts" };
        #endregion

        saveData.trinketData = new List<string>()
        {
            "sun_ring","sun_cloak", "tough_ring", "heavy_boots","focus_ring","defenders_seal", "deteriorating_bracer",
            "demons_cauldron","sacred_scroll", "cleansing_crystal","ancestors_musket_ball", "ancestors_coat",
            "ancestors_bottle","ancestors_scroll", "poisoning_buckle", "spiked_collar","bloodied_fetish",
            "sacrificial_cauldron","witchs_vial", "sharpening_sheath","immunity_mask", "bright_tambourine", "agility_whistle",
        };

        saveData.wagonData = new List<string>()
        {
            "demons_cauldron", "sacred_scroll", "cleansing_crystal", "ancestors_musket_ball", "ancestors_coat", 
        };

        saveData.saveDungeonData.Clear();
        saveData.saveDungeonData.Add("crypts", new DungeonProgress("crypts", 7, 42, true, false));
        saveData.saveDungeonData.Add("warrens", new DungeonProgress("warrens", 6, 16, true, false));
        saveData.saveDungeonData.Add("weald", new DungeonProgress("weald", 5, 32, true, false));
        saveData.saveDungeonData.Add("cove", new DungeonProgress("cove", 3, 1, true, false));
        saveData.saveDungeonData.Add("darkest", new DungeonProgress("darkest", 1, 0, false, false));

        saveData.deathRecords = new List<DeathRecord>()
        {
            new DeathRecord() { HeroName = "Ronald", HeroClassIndex = 2, Factor = DeathFactor.Hunger, KillerName = "", ResolveLevel = 2, },
            new DeathRecord() { HeroName = "Losla", HeroClassIndex = 7, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 5, },
            new DeathRecord() { HeroName = "Qweoas", HeroClassIndex = 2, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 5, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Trexos", HeroClassIndex = 0, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 4, },
            new DeathRecord() { HeroName = "Oloks", HeroClassIndex = 9, Factor = DeathFactor.BleedMonster, KillerName = "necromancer_C", ResolveLevel = 1, },
            new DeathRecord() { HeroName = "Dismas", HeroClassIndex = 10, Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 5, },
        };

        saveData.buildingUpgrades = new Dictionary<string, UpgradePurchases>();

        saveData.buildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[6] { "a", "b", "c", "d", "e", "f" }));
        saveData.buildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[3] { "a", "b", "c" }));
        saveData.buildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.buildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[2] { "a", "b" }));
        saveData.buildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[5] { "a", "b", "c", "d", "e" }));
        saveData.buildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[1] { "a" }));
        saveData.buildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[3] { "a", "b", "c" }));
        saveData.buildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[1] { "a" }));
        saveData.buildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[2] { "a", "b" }));
        saveData.buildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[1] { "a" }));
        saveData.buildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[2] { "a", "b" }));
        saveData.buildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[4] { "a", "b", "c", "d" }));
        saveData.buildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[1] { "a" }));
        saveData.buildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[2] { "a", "b" }));
        saveData.buildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[2] { "a", "b" }));
        saveData.buildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[4] { "a", "b", "c", "d" }));
        saveData.buildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.buildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[4] { "a", "b", "c", "d" }));
        saveData.buildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[4] { "a", "b", "c", "d" }));
        saveData.buildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[1] { "a" }));

        #region ActivitySlots
        saveData.abbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.tavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.sanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion


        WriteSave(saveData);
        return saveData;
    }
}