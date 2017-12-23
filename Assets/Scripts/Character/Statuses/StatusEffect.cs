using System.IO;

public abstract class StatusEffect
{
    public abstract StatusType Type { get; }
    public abstract bool IsApplied { get; }

    public abstract void UpdateNextTurn();
    public abstract void ResetStatus();
    public abstract void WriteStatusData(BinaryWriter bw);
    public abstract void ReadStatusData(BinaryReader br);
}