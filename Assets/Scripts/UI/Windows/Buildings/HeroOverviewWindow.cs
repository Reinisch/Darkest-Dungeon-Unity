using UnityEngine;
using UnityEngine.UI;

public abstract class HeroOverviewWindow : MonoBehaviour
{
    [SerializeField]
    private Text heroNameLabel;
    [SerializeField]
    private Text heroClassLabel;
    [SerializeField]
    private Text classSkillLabel;
    [SerializeField]
    private Image heroGuildHeader;
    
    protected abstract BuildingType BuildingType { get; }
    protected Hero ViewedHero { get; private set; }
    private Building Building { get; set; }

    public virtual void Initialize()
    {
        Building = DarkestDungeonManager.Campaign.Estate.Buildings[BuildingType];
    }

    public virtual void LoadHeroOverview(Hero hero)
    {
        heroNameLabel.text = hero.HeroName;
        heroClassLabel.text = LocalizationManager.GetString("hero_class_name_" + hero.ClassStringId);
        classSkillLabel.text = LocalizationManager.GetString("action_verbose_body_" + Building.Name + "_" + hero.ClassStringId);
        heroGuildHeader.sprite = DarkestDungeonManager.HeroSprites[hero.ClassStringId].Header;
        ViewedHero = hero;
    }

    public virtual void UpdateHeroOverview()
    {
    }

    public virtual void ResetWindow()
    {
        ViewedHero = null;
    }
}