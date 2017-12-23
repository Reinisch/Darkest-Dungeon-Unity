using System.IO;

public class DeathRecoveryStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.DeathRecovery; } }
    public override bool IsApplied { get { return AtDeathRecovery; } }

    public bool AtDeathRecovery { private get; set; }

    public override void UpdateNextTurn()
    {
    }

    public override void ResetStatus()
    {
        AtDeathRecovery = false;
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(AtDeathRecovery);
    }

    public override void ReadStatusData(BinaryReader br)
    {
        AtDeathRecovery = br.ReadBoolean();
    }
}