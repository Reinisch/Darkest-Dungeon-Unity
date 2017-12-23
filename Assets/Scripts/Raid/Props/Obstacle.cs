using System.Collections.Generic;
using System.IO;

public class Obstacle : Prop
{
    public List<Effect> FailEffects;
    public float HealthPenalty;
    public float TorchlightPenalty;
    public bool AncestorTalk;

    public Obstacle()
    {
        Type = AreaType.Obstacle;
        FailEffects = new List<Effect>();
    }

    public Obstacle(string id) : this()
    {
        StringId = id;
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);

        bw.Write(StringId);
    }
}