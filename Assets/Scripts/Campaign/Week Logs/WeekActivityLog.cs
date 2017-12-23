using System.Collections.Generic;
using System.IO;

public class WeekActivityLog : IBinarySaveData
{
    public int WeekNumber { get; private set; }
    public PartyActivityRecord EmbarkRecord { get; set; }
    public PartyActivityRecord ReturnRecord { get; set; }
    public List<ActorActivityRecord> HeroRecords { get; private set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public WeekActivityLog()
    {
        HeroRecords = new List<ActorActivityRecord>();
    }

    public WeekActivityLog(int weekNumber) : this()
    {
        WeekNumber = weekNumber;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(WeekNumber);
        bw.Write(ReturnRecord != null);
        bw.Write(EmbarkRecord != null);

        if (ReturnRecord != null)
            ReturnRecord.Write(bw);
        if (EmbarkRecord != null)
            EmbarkRecord.Write(bw);
    }

    public void Read(BinaryReader br)
    {
        WeekNumber = br.ReadInt32();

        bool hasReturn = br.ReadBoolean();
        bool hasEmbark = br.ReadBoolean();
        if (hasReturn)
        {
            ReturnRecord = new PartyActivityRecord();
            ReturnRecord.Read(br);
        }
        if (hasEmbark)
        {
            EmbarkRecord = new PartyActivityRecord();
            EmbarkRecord.Read(br);
        }
    }
}