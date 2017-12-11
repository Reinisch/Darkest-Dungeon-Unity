using System.IO;

public class SaveActivitySlot : IBinarySaveData
{
    public string TargetPositiveQuirk { get; set; }
    public string TargetNegativeQuirk { get; set; }
    public string TargetDiseaseQuirk { get; set; }

    public int HeroRosterId { get; set; }
    public ActivitySlotStatus Status { get; set; }
    public bool IsMeetingSaveCriteria { get { return true; } }


    public SaveActivitySlot()
    {
        TargetPositiveQuirk = "";
        TargetNegativeQuirk = "";
        TargetDiseaseQuirk = "";

        HeroRosterId = -1;
    }


    public void UpdateFromActivity(ActivitySlot slot)
    {
        TargetPositiveQuirk = "";
        TargetNegativeQuirk = "";
        TargetDiseaseQuirk = "";

        HeroRosterId = slot.Hero == null ? -1 : slot.Hero.RosterId;
        Status = slot.Status == ActivitySlotStatus.Paid && HeroRosterId == -1? ActivitySlotStatus.Available : slot.Status;
    }

    public void UpdateFromTreatment(TreatmentSlot slot)
    {
        TargetPositiveQuirk = slot.TargetPositiveQuirk ?? "";
        TargetNegativeQuirk = slot.TargetNegativeQuirk ?? "";
        TargetDiseaseQuirk = slot.TargetDiseaseQuirk ?? "";

        HeroRosterId = slot.Hero == null ? -1 : slot.Hero.RosterId;
        Status = slot.Status == ActivitySlotStatus.Paid && HeroRosterId == -1 ? ActivitySlotStatus.Available : slot.Status;
    }


    public void Write(BinaryWriter bw)
    {
        bw.Write(HeroRosterId);
        bw.Write((int)Status);

        bw.Write(TargetDiseaseQuirk ?? "");
        bw.Write(TargetNegativeQuirk ?? "");
        bw.Write(TargetPositiveQuirk ?? "");
    }

    public void Read(BinaryReader br)
    {
        HeroRosterId = br.ReadInt32();
        Status = (ActivitySlotStatus)br.ReadInt32();

        TargetDiseaseQuirk = br.ReadString();
        TargetNegativeQuirk = br.ReadString();
        TargetPositiveQuirk = br.ReadString();
    }
}