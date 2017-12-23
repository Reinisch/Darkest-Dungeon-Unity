using UnityEngine;
using UnityEngine.UI;

public class RaidBannerPanel : MonoBehaviour
{
    [SerializeField]
    private Image portrait;
    [SerializeField]
    private Text heroName;
    [SerializeField]
    private Text heroClass;
    [SerializeField]
    private RaidCombatSkillsPanel skillPanel;

    public RaidCombatSkillsPanel SkillPanel { get { return skillPanel; } }

    public void UpdateHero()
    {
        portrait.sprite = DarkestDungeonManager.HeroSprites[RaidSceneManager.RaidPanel.SelectedHero.ClassStringId]["A"].Portrait;
        heroName.text = RaidSceneManager.RaidPanel.SelectedHero.HeroName;
        heroClass.text = LocalizationManager.GetString("hero_class_name_" + RaidSceneManager.RaidPanel.SelectedHero.ClassStringId);

        SkillPanel.UpdateSkillPanel();
        if (RaidSceneManager.Raid.CampingPhase == CampingPhase.Skill)
            SkillPanel.SetUsable();
    }

    public void SetCombatReady()
    {
        SkillPanel.SetUsable();
    }

    public void SetPeacefulState()
    {
        SkillPanel.SetPeaceful();
    }

    public void SetDisabledState()
    {
        SkillPanel.SetDisabled();
    }
}
