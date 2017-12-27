using UnityEngine;

public class CampingTrainerHeroWindow : HeroOverviewWindow
{
    [SerializeField]
    private CampingSkillPurchaseSlot[] campingSkills;

    protected override BuildingType BuildingType { get {return BuildingType.CampingTrainer; } }

    public override void Initialize()
    {
        base.Initialize();

        foreach (CampingSkillPurchaseSlot campingSkillSlot in campingSkills)
            campingSkillSlot.EventClicked += CampingSkillPurchaseSlotClicked;
    }

    public override void LoadHeroOverview(Hero hero)
    {
        base.LoadHeroOverview(hero);

        float discountCamping = 1 - DarkestDungeonManager.Campaign.Estate.CampingTrainer.Discount;
        int availableCampingSkills = Mathf.Min(campingSkills.Length, hero.HeroClass.CampingSkills.Count);
        for (int i = 0; i < availableCampingSkills; i++)
            campingSkills[i].Initialize(hero, i, discountCamping);
    }

    public override void UpdateHeroOverview()
    {
        base.UpdateHeroOverview();

        if (ViewedHero != null)
        {
            float discountCamping = 1 - DarkestDungeonManager.Campaign.Estate.CampingTrainer.Discount;
            int availableCampingSkills = Mathf.Min(campingSkills.Length, ViewedHero.HeroClass.CampingSkills.Count);
            for (int i = 0; i < availableCampingSkills; i++)
                campingSkills[i].UpdateSkill(discountCamping);
        }
    }

    public override void ResetWindow()
    {
        base.ResetWindow();
        
        foreach (CampingSkillPurchaseSlot campingSkillSlot in campingSkills)
            campingSkillSlot.Reset();
    }

    private void CampingSkillPurchaseSlotClicked(CampingSkillPurchaseSlot slot)
    {
        float discount = 1 - DarkestDungeonManager.Campaign.Estate.CampingTrainer.Discount;

        if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Skill, slot.Hero, discount))
        {
            EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
            DarkestDungeonManager.Campaign.Estate.ReskillCampingHero(slot.Hero);
            UpdateHeroOverview();
            DarkestSoundManager.PlayOneShot("event:/town/trainer_purchase_skill");
        }
    }
}