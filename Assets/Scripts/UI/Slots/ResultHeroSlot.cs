using UnityEngine;
using UnityEngine.UI;

public class ResultHeroSlot : MonoBehaviour
{
    [SerializeField]
    private Text heroLabel;
    [SerializeField]
    private Text expLabel;
    [SerializeField]
    private Image portrait;
    [SerializeField]
    private StressOverlayPanel stressPanel;
    [SerializeField]
    private ResolveBar resolveBar;

    public void UpdateResult(RaidHeroInfo heroInfo)
    {
        heroLabel.text = heroInfo.Hero.HeroName;
        stressPanel.UpdateStress(heroInfo.Hero.GetPairedAttribute(AttributeType.Stress).ValueRatio);

        if(heroInfo.IsAlive)
        {
            portrait.sprite = DarkestDungeonManager.HeroSprites[heroInfo.Hero.ClassStringId]["A"].Portrait;

            if (DarkestDungeonManager.RaidManager.Status == RaidStatus.Success)
            {
                int expAmount = RaidSceneManager.Raid.Quest.Reward.ResolveXP;

                expLabel.text = string.Format(LocalizationManager.GetString("raid_results_hero_resolve_from_quest"), expAmount);
                if (heroInfo.Hero.Resolve.AddExperience(expAmount) != 0)
                    heroInfo.Hero.UpdateResolve();
            }
            else
                expLabel.text = "";

            resolveBar.UpdateResolve(heroInfo.Hero);
        }
        else
        {
            portrait.sprite = DarkestDungeonManager.Data.Sprites["deadhero_portrait"];
            expLabel.text = "";
            resolveBar.UpdateResolve(heroInfo.Hero);
        }
    }

    public void SetEmpty()
    {
        heroLabel.text = "";
        stressPanel.UpdateStress(0);
        resolveBar.UpdateEmpty();
        portrait.enabled = false;
        expLabel.text = "";
    }
}
