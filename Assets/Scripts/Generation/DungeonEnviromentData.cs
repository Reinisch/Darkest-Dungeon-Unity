using System.Collections.Generic;

public class DungeonEnviromentData
{
    public int HallVariations { get; set; }
    public List<string> RoomVariations { get; set; }
    public List<DungeonBattleMash> BattleMashes { get; set; }

    public List<DungeonPropsEncounter> HallCurios { get; set; }
    public List<DungeonPropsEncounter> RoomCurios { get; set; }
    public List<DungeonPropsEncounter> RoomTresures { get; set; }
    public List<DungeonPropsEncounter> SecretTresures { get; set; }
    public List<DungeonPropsEncounter> Traps { get; set; }
    public List<DungeonPropsEncounter> Obstacles { get; set; }

    public DungeonEnviromentData()
    {
        RoomVariations = new List<string>();
        BattleMashes = new List<DungeonBattleMash>();

        HallCurios = new List<DungeonPropsEncounter>();
        RoomCurios = new List<DungeonPropsEncounter>();
        RoomTresures = new List<DungeonPropsEncounter>();
        SecretTresures = new List<DungeonPropsEncounter>();
        Traps = new List<DungeonPropsEncounter>();
        Obstacles = new List<DungeonPropsEncounter>();
    }
}

public class DungeonBattleMash
{
    public int MashId { get; set; }
    public List<DungeonBattleEncounter> HallEncounters { get; set; }
    public List<DungeonBattleEncounter> RoomEncounters { get; set; }
    public List<DungeonBattleEncounter> BossEncounters { get; set; }
    public List<DungeonBattleEncounter> StallEncounters { get; set; }
    public Dictionary<string, List<DungeonBattleEncounter>> NamedEncounters { get; set; }

    public DungeonBattleMash()
    {
        HallEncounters = new List<DungeonBattleEncounter>();
        RoomEncounters = new List<DungeonBattleEncounter>();
        BossEncounters = new List<DungeonBattleEncounter>();
        StallEncounters = new List<DungeonBattleEncounter>();
        NamedEncounters = new Dictionary<string, List<DungeonBattleEncounter>>();
    }
}

public class DungeonPropsEncounter : IProportionValue
{
    public int Chance { get; set; }
    public string PropName { get; private set; }

    public DungeonPropsEncounter(int chance, string prop)
    {
        Chance = chance;
        PropName = prop;
    }
}

public class DungeonBattleEncounter : IProportionValue
{
    public int Chance { get; set; }
    public List<string> MonsterSet { get; private set; }

    public DungeonBattleEncounter()
    {
        MonsterSet = new List<string>();
    }

    public DungeonBattleEncounter(int chance, List<string> set)
    {
        Chance = chance;
        MonsterSet = set;
    }
}