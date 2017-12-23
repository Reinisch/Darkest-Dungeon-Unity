using System.IO;

public class RiposteStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.Riposte; } }
    public override bool IsApplied { get { return RiposteDuration > 0; } }

    public DurationType DurationType { private get; set; }
    public int RiposteDuration { get; set; }

    public override void UpdateNextTurn()
    {
        if (DurationType == DurationType.Combat)
            return;

        if (RiposteDuration > 0)
            RiposteDuration--;
    }

    public override void ResetStatus()
    {
        RiposteDuration = 0;
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write((int) DurationType);
        bw.Write(RiposteDuration);
    }

    public override void ReadStatusData(BinaryReader br)
    {
        DurationType = (DurationType) br.ReadInt32();
        RiposteDuration = br.ReadInt32();
    }
}