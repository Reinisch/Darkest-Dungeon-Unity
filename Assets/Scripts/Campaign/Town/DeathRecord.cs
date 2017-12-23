using System.IO;

public enum DeathFactor
{
    Hunger,
    Trap,
    Obstacle,
    AttackMonster,
    BleedMonster,
    PoisonMonster,
    AttackFriend,
    BleedFriend,
    PoisonFriend,
    PoisonUnknown,
    BleedUnknown,
    Unknown,
    CaptorMonster,
    HeartAttack
}

public class DeathRecord : IBinarySaveData
{
    public string HeroName { get; set; }
    public int HeroClassIndex { get; set; }
    public string KillerName { get; set; }
    public int ResolveLevel { get; set; }
    public DeathFactor Factor { get; set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public void Write(BinaryWriter bw)
    {
        bw.Write(HeroName);
        bw.Write(HeroClassIndex);
        bw.Write(ResolveLevel);
        bw.Write((int)Factor);
        bw.Write(KillerName ?? "");
    }

    public void Read(BinaryReader br)
    {
        HeroName = br.ReadString();
        HeroClassIndex = br.ReadInt32();
        ResolveLevel = br.ReadInt32();
        Factor = (DeathFactor)br.ReadInt32();
        KillerName = br.ReadString();
    }
}
