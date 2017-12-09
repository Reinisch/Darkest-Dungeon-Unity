using UnityEngine;
using System.Collections;
using System.IO;

public class SaveEventData : IBinarySaveData<SaveEventData>
{
    public string EventId { get; set; }
    public int ActiveCooldown { get; set; }
    public int NotRolledAmount { get; set; }

    public bool IsMeetingSaveCriteria { get { return true; } }


    public void Write(BinaryWriter bw)
    {
        bw.Write(EventId);
        bw.Write(ActiveCooldown);
        bw.Write(NotRolledAmount);
    }

    public SaveEventData Read(BinaryReader br)
    {
        EventId = br.ReadString();
        ActiveCooldown = br.ReadInt32();
        NotRolledAmount = br.ReadInt32();

        return this;
    }
}
