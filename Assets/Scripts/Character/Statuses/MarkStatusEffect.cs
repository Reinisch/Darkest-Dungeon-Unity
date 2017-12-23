using System.IO;

public class MarkStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.Marked; } }
    public override bool IsApplied { get { return MarkDuration > 0; } } 

    public DurationType DurationType { get; set; }
    public int MarkDuration { get; set; }

    public override void UpdateNextTurn()
    {
        if (DurationType == DurationType.Combat)
            return;

        if (MarkDuration > 0)
            MarkDuration--;
    }

    public override void ResetStatus()
    {
        MarkDuration = 0;
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write((int) DurationType);
        bw.Write(MarkDuration);
    }

    public override void ReadStatusData(BinaryReader br)
    {
        DurationType = (DurationType) br.ReadInt32();
        MarkDuration = br.ReadInt32();
    }
}