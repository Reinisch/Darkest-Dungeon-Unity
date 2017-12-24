using System.IO;

public enum AreaType
{
    Empty, 
    Entrance,
    Tresure,
    Curio,
    Boss,
    Battle,
    Trap,
    Hunger,
    Obstacle,
    Door,
    BattleCurio,
    BattleTresure,
}

public enum Knowledge
{
    Hidden = 0,
    Scouted,
    Visited,
    Completed,
}

public enum Direction
{
    Top,
    Bot,
    Left,
    Right,
}

public abstract class Prop : IBinarySaveData
{
    public string StringId { get; set; }
    public AreaType Type { get; protected set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public virtual void Write(BinaryWriter bw)
    {
        bw.Write((int)Type);
    }

    public virtual void Read(BinaryReader br)
    {
        // Type = (AreaType)br.ReadInt32(); called in BinarySaveDataHelper.Create<Prop>()
    }
}

public abstract class Area : IBinarySaveData
{
    public string Id { get; set; }
    public string TextureId { get; set; }
    public int MashId { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }

    public AreaType Type { get; set; }
    public Knowledge Knowledge { get; set; }

    public Prop Prop { get; set; }
    public BattleEncounter BattleEncounter { get; set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public bool HasActiveBattle
    {
        get
        {
            return BattleEncounter != null && BattleEncounter.Cleared == false;
        }
    }

    public void SetNamedEncounter(string dungeon, string encounter, int index, int mashId)
    {
        BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData[dungeon].BattleMashes.
            Find(mash => mash.MashId == mashId).NamedEncounters[encounter][index].MonsterSet);
    }

    public void SetBossEncounter(string dungeon, string boss, int mashId)
    {
        BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData[dungeon].BattleMashes.
            Find(mash => mash.MashId == mashId).BossEncounters.Find(enc => enc.MonsterSet.Contains(boss)).MonsterSet);
    }

    public void SetCurio(string propName)
    {
        Prop = DarkestDungeonManager.Data.Curios[propName];
    }

    public abstract void Scout();

    public virtual void Write(BinaryWriter bw)
    {
        bw.Write(Id);
        bw.Write(GridX);
        bw.Write(GridY);

        bw.Write(TextureId);
        bw.Write((int)Type);
        bw.Write((int)Knowledge);

        bw.Write(Prop != null);
        if (Prop != null)
            Prop.Write(bw);

        bw.Write(BattleEncounter != null);
        if (BattleEncounter != null)
            BattleEncounter.Write(bw);
    }

    public virtual void Read(BinaryReader br)
    {
        Id = br.ReadString();
        GridX = br.ReadInt32();
        GridY = br.ReadInt32();

        TextureId = br.ReadString();
        Type = (AreaType)br.ReadInt32();
        Knowledge = (Knowledge)br.ReadInt32();

        Prop = br.ReadBoolean() ? BinarySaveDataHelper.Create<Prop>(br) : null;
        BattleEncounter = br.ReadBoolean() ? BinarySaveDataHelper.Create<BattleEncounter>(br) : null;
    }
}