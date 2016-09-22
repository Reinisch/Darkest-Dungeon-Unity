using System.Collections.Generic;

public class BattleEncounter
{
    public List<Monster> Monsters { get; set; }
    public bool Cleared { get; set; }

    public BattleEncounter()
    {
        Monsters = new List<Monster>();
    }
    public BattleEncounter(List<Monster> monsters)
    {
        Monsters = new List<Monster>(monsters);
        Cleared = false;
    }

    public BattleEncounter(List<string> monsterNames)
    {
        Monsters = new List<Monster>();
        Cleared = false;

        foreach (var monsterName in monsterNames)
            Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[monsterName]));
    }
}