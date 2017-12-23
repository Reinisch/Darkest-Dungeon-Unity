public class TreatmentSlot : ActivitySlot
{
    public int BasePositiveCost { get; set; }
    public int BasePermanentCost { get; set; }
    public int BaseDiseaseCost { get; set; }

    public string TargetPositiveQuirk { get; set; }
    public string TargetNegativeQuirk { get; set; }
    public string TargetDiseaseQuirk { get; set; }

    public TreatmentSlot(bool isUnlocked, int baseNegativeCost, int basePositiveCost, int basePermanentCost, int baseDiseaseCost)
        :base(isUnlocked, baseNegativeCost)
    {
        BasePositiveCost = basePositiveCost;
        BasePermanentCost = basePermanentCost;
        BaseDiseaseCost = baseDiseaseCost;
    }

    public void UpdateTreatmentSlot(bool isUnlocked, int baseNegativeCost, int basePositiveCost, int basePermanentCost, int baseDiseaseCost)
    {
        IsUnlocked = isUnlocked;
        BaseCost = baseNegativeCost;
        BasePositiveCost = basePositiveCost;
        BasePermanentCost = basePermanentCost;
        BaseDiseaseCost = baseDiseaseCost;
    }
}
