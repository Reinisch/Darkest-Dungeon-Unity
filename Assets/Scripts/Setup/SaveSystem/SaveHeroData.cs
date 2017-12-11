using System.Collections.Generic;
using System.IO;

public class SaveHeroData : IBinarySaveData
{
    public HeroStatus Status = HeroStatus.Available;
    public string InActivity;
    public int MissingDuration;

    public int RosterId;
    public string Name;
    public string HeroClass;
    public string Trait;

    public int ResolveLevel;
    public int ResolveXP;

    public float CurrentHp = 1000;
    public float StressLevel;

    public int WeaponLevel;
    public int ArmorLevel;

    public string LeftTrinketId;
    public string RightTrinketId;

    public List<QuirkInfo> Quirks = new List<QuirkInfo>();
    public List<BuffInfo> Buffs = new List<BuffInfo>();
    public readonly List<int> SelectedCombatSkillIndexes = new List<int>();
    public readonly List<int> SelectedCampingSkillIndexes = new List<int>();

    public bool IsMeetingSaveCriteria { get { return true; } }


    public SaveHeroData()
    {
    }

    public SaveHeroData(int id, string name, string heroClass, int resolveLevel, int resolveXp, int stressLevel,
        int weaponLevel, int armorLevel, string leftTrinket, string rightTrinket, params string[] quirks)
    {
        RosterId = id;
        Name = name;
        HeroClass = heroClass;
        ResolveLevel = resolveLevel;
        ResolveXP = resolveXp;
        StressLevel = stressLevel;
        WeaponLevel = weaponLevel;
        ArmorLevel = armorLevel;
        LeftTrinketId = leftTrinket;
        RightTrinketId = rightTrinket;

        foreach (string quirk in quirks)
            Quirks.Add(new QuirkInfo(quirk));
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write((int)Status);
        bw.Write(MissingDuration);
        bw.Write(InActivity ?? "");
        bw.Write(Trait ?? "");
        bw.Write(RosterId);
        bw.Write(Name ?? "");
        bw.Write(HeroClass ?? "");

        bw.Write(ResolveLevel);
        bw.Write(ResolveXP);
        bw.Write(CurrentHp);
        bw.Write(StressLevel);

        bw.Write(WeaponLevel);
        bw.Write(ArmorLevel);
        bw.Write(LeftTrinketId ?? "");
        bw.Write(RightTrinketId ?? "");

        Quirks.Write(bw);
        Buffs.Write(bw);
        SelectedCombatSkillIndexes.Write(bw);
        SelectedCampingSkillIndexes.Write(bw);
    }

    public void Read(BinaryReader br)
    {
        Status = (HeroStatus)br.ReadInt32();
        MissingDuration = br.ReadInt32();
        InActivity = br.ReadString();
        Trait = br.ReadString();
        RosterId = br.ReadInt32();
        Name = br.ReadString();
        HeroClass = br.ReadString();

        ResolveLevel = br.ReadInt32();
        ResolveXP = br.ReadInt32();
        CurrentHp = br.ReadSingle();
        StressLevel = br.ReadSingle();

        WeaponLevel = br.ReadInt32();
        ArmorLevel = br.ReadInt32();
        LeftTrinketId = br.ReadString();
        RightTrinketId = br.ReadString();

        Quirks.Read(br);
        Buffs.Read(br);
        SelectedCombatSkillIndexes.Read(br);
        SelectedCampingSkillIndexes.Read(br);
    }
}