using System.IO;

public class QuirkInfo : IBinarySaveData
{
    public Quirk Quirk { get; set; }
    public bool IsLocked { get; set; }
    public bool IsReplaced { get; set; }
    public bool IsNew { get; set; }
    public int Longetivity { get; set; }
    public string ReplacedQuirk { get; set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public QuirkInfo()
    {
    }

    public QuirkInfo(string quirkName)
    {
        Quirk = DarkestDungeonManager.Data.Quirks[quirkName];
        ReplacedQuirk = "";
    }

    public QuirkInfo(Quirk quirk, bool isLocked, int longetivity, bool isNew)
    {
        Quirk = quirk;
        IsLocked = isLocked;
        IsNew = isNew;
        Longetivity = longetivity;
        ReplacedQuirk = "";
    }

    public QuirkInfo(Quirk quirk, int longetivity, string replacedQuirk)
    {
        Quirk = quirk;
        Longetivity = longetivity;
        ReplacedQuirk = replacedQuirk;
        IsReplaced = true;
        IsNew = true;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Quirk.Id);
        bw.Write(IsLocked);
        bw.Write(IsNew);
        bw.Write(IsReplaced);
        bw.Write(Longetivity);
        bw.Write(ReplacedQuirk);
    }

    public void Read(BinaryReader br)
    {
        Quirk = DarkestDungeonManager.Data.Quirks[br.ReadString()];
        IsLocked = br.ReadBoolean();
        IsNew = br.ReadBoolean();
        IsReplaced = br.ReadBoolean();
        Longetivity = br.ReadInt32();
        ReplacedQuirk = br.ReadString();
    }

    public void ReplaceBy(Quirk newQuirk)
    {
        IsLocked = false;
        IsReplaced = true;
        IsNew = true;
        Longetivity = 1;
        ReplacedQuirk = Quirk.Id;
        Quirk = newQuirk;
    }
}