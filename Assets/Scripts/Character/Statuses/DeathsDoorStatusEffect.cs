using System.IO;

public class DeathsDoorStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.DeathsDoor; } }
    public override bool IsApplied { get { return AtDeathsDoor; } }

    public bool AtDeathsDoor { private get; set; }

    public override void UpdateNextTurn()
    {
    }

    public override void ResetStatus()
    {
        AtDeathsDoor = false;
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(AtDeathsDoor);
    }

    public override void ReadStatusData(BinaryReader br)
    {
        AtDeathsDoor = br.ReadBoolean();
    }
}