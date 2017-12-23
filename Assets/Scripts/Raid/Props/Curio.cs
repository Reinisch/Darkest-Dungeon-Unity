using System.Collections.Generic;
using System.IO;

public class Curio : Prop
{
    public string OriginalId
    {
        get
        {
            if (StringId == "tutorial_shovel")
                return "unlocked_strongbox";
            else if (StringId == "tutorial_key")
                return "discarded_pack";
            else if (StringId == "tutorial_holy")
                return "sack";

            return StringId;
        }
    }
    public bool IsFullCurio { get; set; }
    public bool IsQuestCurio { get; set; }
    public string ResultTypes { get; set; }
    public string RegionFound { get; set; }

    public List<string> Tags { get; set; }

    public List<CurioInteraction> Results { get; set; }
    public List<ItemInteraction> ItemInteractions { get; set; }

    public Curio()
    {
        Type = AreaType.Curio;
        Tags = new List<string>();
        Results = new List<CurioInteraction>();
        ItemInteractions = new List<ItemInteraction>();
    }

    public Curio(string id) : this()
    {
        StringId = id;
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);

        bw.Write(IsQuestCurio);
        bw.Write(StringId);
    }
}