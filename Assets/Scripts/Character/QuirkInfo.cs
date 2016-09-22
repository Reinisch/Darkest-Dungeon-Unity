public class QuirkInfo
{
    public Quirk Quirk { get; set; }
    public bool IsLocked { get; set; }
    public bool IsReplaced { get; set; }
    public bool IsNew { get; set; }
    public int Longetivity { get; set; }
    public string ReplacedQuirk { get; set; }

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