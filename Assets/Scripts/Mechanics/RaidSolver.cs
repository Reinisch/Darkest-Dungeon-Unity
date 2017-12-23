using System.Collections.Generic;
using System.Linq;

public static class RaidSolver
{
    private static LootDatabase LootDatabase { get { return DarkestDungeonManager.Data.LootDatabase; } }

    public static List<ItemDefinition> GenerateLoot(string code, int amount, RaidInfo raid)
    {
        List<ItemDefinition> lootItems = new List<ItemDefinition>();
        for (int j = 0; j < amount; j++)
        {
            LootEntry entry = GetLootEntry(code, raid);
            switch (entry.Type)
            {
                case LootType.Item:
                    LootEntryItem itemEntry = entry as LootEntryItem;
                    ItemDefinition data = new ItemDefinition();
                    data.Type = itemEntry.ItemType;
                    data.Id = itemEntry.ItemId;
                    data.Amount = itemEntry.ItemAmount;
                    lootItems.Add(data);
                    break;
                case LootType.Journal:
                    LootEntryJournal journalEntry = entry as LootEntryJournal;
                    ItemDefinition dataJournal = new ItemDefinition();
                    dataJournal.Type = "journal_page";
                    if (journalEntry.SpecificId.HasValue)
                        dataJournal.Id = journalEntry.SpecificId.Value.ToString();
                    else
                        dataJournal.Id = RandomSolver.Next(journalEntry.MinIndex, journalEntry.MaxIndex + 1).ToString();
                    dataJournal.Amount = 1;
                    lootItems.Add(dataJournal);
                    break;
                case LootType.Trinket:
                    LootEntryTrinket trinketEntry = entry as LootEntryTrinket;
                    var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.ToList().
                        FindAll(trinket => ((Trinket)trinket).RarityId == trinketEntry.Rarity);

                    Trinket trinketItem = (Trinket)trinketList[RandomSolver.Next(trinketList.Count)];
                    ItemDefinition trinketDef = new ItemDefinition();
                    trinketDef.Type = trinketItem.Type;
                    trinketDef.Id = trinketItem.Id;
                    trinketDef.Amount = 1;
                    lootItems.Add(trinketDef);
                    break;
                case LootType.Nothing:
                    break;
            }
        }
        return lootItems;
    }

    public static List<ItemDefinition> GenerateLoot(List<LootDefinition> battleLoot, RaidInfo raid)
    {
        List<ItemDefinition> lootItems = new List<ItemDefinition>();
        for (int i = 0; i < battleLoot.Count; i++)
        {
            for(int j = 0; j < battleLoot[i].Count; j++)
            {
                LootEntry entry = GetLootEntry(battleLoot[i].Code, raid);
                switch (entry.Type)
                {
                    case LootType.Item:
                        LootEntryItem itemEntry = entry as LootEntryItem;
                        ItemDefinition data = new ItemDefinition();
                        data.Type = itemEntry.ItemType;
                        data.Id = itemEntry.ItemId;
                        data.Amount = itemEntry.ItemAmount;
                        lootItems.Add(data);
                        break;
                    case LootType.Journal:
                        LootEntryJournal journalEntry = entry as LootEntryJournal;
                        ItemDefinition dataJournal = new ItemDefinition();
                        dataJournal.Type = "journal_page";
                        if (journalEntry.SpecificId.HasValue)
                            dataJournal.Id = journalEntry.SpecificId.Value.ToString();
                        else
                            dataJournal.Id = RandomSolver.Next(journalEntry.MinIndex, journalEntry.MaxIndex + 1).ToString();
                        dataJournal.Amount = 1;
                        lootItems.Add(dataJournal);
                        break;
                    case LootType.Trinket:
                        LootEntryTrinket trinketEntry = entry as LootEntryTrinket;
                        var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.ToList().
                            FindAll(trinket => ((Trinket)trinket).RarityId == trinketEntry.Rarity);

                        Trinket trinketItem = (Trinket)trinketList[RandomSolver.Next(trinketList.Count)];
                        ItemDefinition trinketDef = new ItemDefinition();
                        trinketDef.Type = trinketItem.Type;
                        trinketDef.Id = trinketItem.Id;
                        trinketDef.Amount = trinketDef.Amount;
                        lootItems.Add(trinketDef);
                        break;
                    case LootType.Nothing:
                        break;
                }
            }
        }
        return lootItems;
    }

    public static List<ItemDefinition> GenerateLoot(CurioResult curioResult, RaidInfo raid)
    {
        List<ItemDefinition> lootItems = new List<ItemDefinition>();
        for(int i = 0; i < curioResult.Draws; i++)
        {
            LootEntry entry = GetLootEntry(curioResult.Item, raid);
            switch(entry.Type)
            {
                case LootType.Item:
                    LootEntryItem itemEntry = entry as LootEntryItem;
                    ItemDefinition data = new ItemDefinition();
                    data.Type = itemEntry.ItemType;
                    data.Id = itemEntry.ItemId;
                    data.Amount = itemEntry.ItemAmount;
                    lootItems.Add(data);
                    break;
                case LootType.Journal:
                    LootEntryJournal journalEntry = entry as LootEntryJournal;
                    ItemDefinition dataJournal = new ItemDefinition();
                    dataJournal.Type = "journal_page";
                    if(journalEntry.SpecificId.HasValue)
                        dataJournal.Id = journalEntry.SpecificId.Value.ToString();
                    else
                        dataJournal.Id = RandomSolver.Next(journalEntry.MinIndex, journalEntry.MaxIndex + 1).ToString();
                    dataJournal.Amount = 1;
                    lootItems.Add(dataJournal);
                    break;
                case LootType.Trinket:
                    LootEntryTrinket trinketEntry = entry as LootEntryTrinket;
                    var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.ToList().
                        FindAll(trinket => ((Trinket)trinket).RarityId == trinketEntry.Rarity);

                    Trinket trinketItem = (Trinket)trinketList[RandomSolver.Next(trinketList.Count)];
                    ItemDefinition trinketDef = new ItemDefinition();
                    trinketDef.Type = trinketItem.Type;
                    trinketDef.Id = trinketItem.Id;
                    trinketDef.Amount = 1;
                    lootItems.Add(trinketDef);
                    break;
                case LootType.Nothing:
                    break;
            }
        }
        return lootItems;
    }

    private static LootEntry GetLootEntry(string tableId, RaidInfo raid)
    {
        LootTable lootTable = LootDatabase.LootTables[tableId.ToUpper()].
                Find(table => ((table.Difficulty == raid.Quest.Difficulty) || (table.Difficulty == 0))
                        && ((table.Dungeon == raid.Dungeon.Name) || (table.Dungeon == "")));

        LootEntry lootEntry = RandomSolver.ChooseBySingleRandom(lootTable.Entries);
        return lootEntry.Type != LootType.Table ? lootEntry : GetLootEntry(((LootEntryTable)lootEntry).TableId, raid);
    }
}
