using UnityEngine;
using System.Collections;

public class SaveActivitySlot
{
    public string TargetPositiveQuirk { get; set; }
    public string TargetNegativeQuirk { get; set; }
    public string TargetDiseaseQuirk { get; set; }

    public int HeroRosterId { get; set; }
    public ActivitySlotStatus Status { get; set; }

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
        TargetPositiveQuirk = slot.TargetPositiveQuirk == null ? "" : slot.TargetPositiveQuirk;
        TargetNegativeQuirk = slot.TargetNegativeQuirk == null ? "" : slot.TargetNegativeQuirk;
        TargetDiseaseQuirk = slot.TargetDiseaseQuirk == null ? "" : TargetDiseaseQuirk;

        HeroRosterId = slot.Hero == null ? -1 : slot.Hero.RosterId;
        Status = slot.Status == ActivitySlotStatus.Paid && HeroRosterId == -1 ? ActivitySlotStatus.Available : slot.Status;
    }
}
