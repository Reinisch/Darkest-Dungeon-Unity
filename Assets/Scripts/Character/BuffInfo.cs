using System.IO;
using UnityEngine;

public class BuffInfo : IBinarySaveData
{
    public Buff Buff { get; private set; }
    public BuffDurationType DurationType { get; private set; }
    public BuffSourceType SourceType { get; private set; }
    public int Duration { get; set; }
    public bool IsApplied { get; set; }

    public float ModifierValue { get { return Mathf.Approximately(OverridenValue, 0.0f) ? Buff.ModifierValue : OverridenValue; } }
    public bool IsMeetingSaveCriteria { get { return SourceType.IsSaveData(); } }

    private float OverridenValue { get; set; }

    public BuffInfo()
    {
    }

    public BuffInfo(Buff buff, BuffDurationType durationType, BuffSourceType sourceType, int duration = 1)
    {
        Buff = buff;
        DurationType = durationType;
        SourceType = sourceType;
        Duration = duration;
    }

    public BuffInfo(Buff buff, BuffSourceType sourceType)
    {
        Buff = buff;
        DurationType = buff.DurationType;
        SourceType = sourceType;
        Duration = buff.DurationAmount;
    }

    public BuffInfo(Buff buff, float overridenValue, BuffSourceType sourceType)
    {
        Buff = buff;
        OverridenValue = overridenValue;
        DurationType = buff.DurationType;
        SourceType = sourceType;
        Duration = buff.DurationAmount;
    }

    public void Write(BinaryWriter bw)
    {
        if (!SourceType.IsSaveData())
            return;

        bw.Write((int)SourceType);
        bw.Write((int)DurationType);
        bw.Write(OverridenValue);
        bw.Write(Duration);

        // save info only for custom buffs without id, otherwise load other info from database
        bw.Write(Buff.Id ?? "");
        if (string.IsNullOrEmpty(Buff.Id))
        {
            bw.Write(Buff.ModifierValue);
            bw.Write((int)Buff.Type);
            bw.Write((int)Buff.AttributeType);
            bw.Write((int)Buff.RuleType);
        }
    }

    public void Read(BinaryReader br)
    {
        SourceType = (BuffSourceType)br.ReadInt32();
        DurationType = (BuffDurationType)br.ReadInt32();
        OverridenValue = br.ReadSingle();
        Duration = br.ReadInt32();

        string buffId = br.ReadString();
        if (buffId == "")
        {
            Buff = new Buff()
            {
                Id = "",
                ModifierValue = br.ReadSingle(),
                Type = (BuffType)br.ReadInt32(),
                AttributeType = (AttributeType)br.ReadInt32(),
                RuleType = (BuffRule)br.ReadInt32(),
            };
        }
        else
            Buff = DarkestDungeonManager.Data.Buffs[buffId];
    }
}