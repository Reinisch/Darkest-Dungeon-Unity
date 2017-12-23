using System.IO;

public abstract class ActivityRecord : IBinarySaveData
{
    public bool IsMeetingSaveCriteria { get { return true; } }

    public virtual void Write(BinaryWriter bw)
    {
    }

    public virtual void Read(BinaryReader br)
    {
    }
}