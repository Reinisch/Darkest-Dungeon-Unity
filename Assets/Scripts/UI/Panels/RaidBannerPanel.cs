using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RaidBannerPanel : MonoBehaviour
{
    public Image portrait;
    public Text heroName;
    public Text heroClass;

    public RaidCombatSkillsPanel skillPanel;

    public void UpdateHero()
    {
        portrait.sprite = DarkestDungeonManager.HeroSprites[RaidSceneManager.RaidPanel.SelectedHero.ClassStringId]["A"].Portrait;
        heroName.text = RaidSceneManager.RaidPanel.SelectedHero.HeroName;
        heroClass.text = LocalizationManager.GetString("hero_class_name_" + RaidSceneManager.RaidPanel.SelectedHero.ClassStringId);

        skillPanel.UpdateSkillPanel();
        if (RaidSceneManager.Raid.CampingPhase == CampingPhase.Skill)
            skillPanel.SetUsable();
    }

    public void SetCombatReady()
    {
        skillPanel.SetUsable();
    }
    public void SetPeacefulState()
    {
        skillPanel.SetPeaceful();
    }
    public void SetDisabledState()
    {
        skillPanel.SetDisabled();
    }
}
