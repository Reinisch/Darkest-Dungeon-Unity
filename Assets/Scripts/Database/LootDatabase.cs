using System.Collections.Generic;

public enum LootType { Nothing, Table, Item, Trinket, Journal }

public class LootDatabase
{
    public Dictionary<string, List<DarknessBonus>> DarknessLoot { get; set; }
    public Dictionary<string, List<LootTable>> LootTables { get; set; }

    public LootDatabase()
    {
        DarknessLoot = new Dictionary<string, List<DarknessBonus>>();
        LootTables = new Dictionary<string, List<LootTable>>();
    }
}

public class DarknessBonus
{
    public int DarknessLevel { get; set; }
    public float Chance { get; set; }
    public List<string> Codes { get; set; }
}

public class LootTable
{
    public string Id { get; set; }
    public int Difficulty { get; set; }
    public string Dungeon { get; set; }
    public List<LootEntry> Entries { get; set; }

    public LootTable()
    {
        Entries = new List<LootEntry>();
    }
}

public class LootEntry : ISingleProportion
{
    public LootType Type { get; private set; }
    public float Chance { get; set; }

    public LootEntry(LootType type = LootType.Nothing)
    {
        Type = type;
    }
}

public class LootEntryTable : LootEntry
{
    public string TableId { get; set; }

    public LootEntryTable():base(LootType.Table)
    {
    }
}

public class LootEntryItem : LootEntry
{
    public string ItemType { get; set; }
    public string ItemId { get; set; }
    public int ItemAmount { get; set; }

    public LootEntryItem():base(LootType.Item)
    {
    }
}

public class LootEntryJournal : LootEntry
{
    public int MinIndex { get; set; }
    public int MaxIndex { get; set; }
    public int? SpecificId { get; set; }

    public LootEntryJournal()
        : base(LootType.Journal)
    {
    }
}

public class LootEntryTrinket : LootEntry
{
    public string Rarity { get; set; }

    public LootEntryTrinket():base(LootType.Trinket)
    {
    }
}