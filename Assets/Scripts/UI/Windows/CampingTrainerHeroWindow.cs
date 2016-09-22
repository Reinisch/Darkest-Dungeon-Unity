using UnityEngine;
using UnityEngine.UI;

public class CampingTrainerHeroWindow : MonoBehaviour
{
    public Text heroNameLabel;
    public Text heroClassLabel;
    public Text classSkillLabel;
    public Image heroGuildHeader;

    public CampingSkillPurchaseSlot[] campingSkills;

    public TownManager TownManager { get; set; }
    public Hero ViewedHero { get; set; }

    void CampingTrainerHeroWindow_onSkillClick(CampingSkillPurchaseSlot slot)
    {
        if (slot.Unlocked)
            return;
        else
        {
            float discount = 1 - DarkestDungeonManager.Campaign.Estate.CampingTrainer.Discount;

            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Skill, slot.Hero, discount))
            {
                TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReskillCampingHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
    }

    public void Initialize(TownManager townManager)
    {
        TownManager = townManager;
        for (int i = 0; i < campingSkills.Length; i++)
            campingSkills[i].onClick += CampingTrainerHeroWindow_onSkillClick;
    }

    public void LoadHeroOverview(Hero hero)
    {
        heroNameLabel.text = hero.HeroName;
        heroClassLabel.text = LocalizationManager.GetString("hero_class_name_" + hero.ClassStringId);
        classSkillLabel.text = LocalizationManager.GetString("action_verbose_body_camping_trainer_" + hero.ClassStringId);
        heroGuildHeader.sprite = DarkestDungeonManager.HeroSprites[hero.ClassStringId].Header;
        ViewedHero = hero;

        float discountCamping = 1 - DarkestDungeonManager.Campaign.Estate.CampingTrainer.Discount;
        int availableCampingSkills = Mathf.Min(campingSkills.Length, hero.HeroClass.CampingSkills.Count);
        for (int i = 0; i < availableCampingSkills; i++)
            campingSkills[i].Initialize(hero, i, discountCamping);
    }

    public void UpdateHeroOverview()
    {
        if (ViewedHero != null)
        {
            float discountCamping = 1 - DarkestDungeonManager.Campaign.Estate.CampingTrainer.Discount;
            int availableCampingSkills = Mathf.Min(campingSkills.Length, ViewedHero.HeroClass.CampingSkills.Count);
            for (int i = 0; i < availableCampingSkills; i++)
                campingSkills[i].UpdateSkill(discountCamping);
        }
    }

    public void ResetWindow()
    {
        ViewedHero = null;
        for (int i = 0; i < campingSkills.Length; i++)
            campingSkills[i].Reset();
    }
}