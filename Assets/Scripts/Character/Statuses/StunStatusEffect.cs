using System.IO;

public class StunStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.Stun; } }
    public override bool IsApplied { get { return StunApplied; } }

    public bool StunApplied { private get; set; }

    public override void UpdateNextTurn()
    {
    }

    public override void ResetStatus()
    {
        StunApplied = false;
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(StunApplied);
    }

    public override void ReadStatusData(BinaryReader br)
    {
        StunApplied = br.ReadBoolean();
    }
}