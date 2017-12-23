using System.Collections.Generic;
using System.IO;

public class BattleEncounter : IBinarySaveData
{
    public List<Monster> Monsters { get; set; }
    public bool Cleared { get; set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public BattleEncounter()
    {
        Monsters = new List<Monster>();
    }

    public BattleEncounter(List<Monster> monsters) : this()
    {
        Monsters.AddRange(monsters);
    }

    public BattleEncounter(List<string> monsterNames) : this()
    {
        foreach (var monsterName in monsterNames)
            Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[monsterName]));
    }

    public BattleEncounter(params string[] monsterNames) : this()
    {
        foreach (var monsterName in monsterNames)
            Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[monsterName]));
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Monsters.Count);
        Monsters.ForEach(monster => bw.Write(monster.Data.StringId));
        bw.Write(Cleared);
    }

    public void Read(BinaryReader br)
    {
        Monsters.Clear();
        int monsterCount = br.ReadInt32();
        for (int j = 0; j < monsterCount; j++)
            Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
        Cleared = br.ReadBoolean();
    }
}