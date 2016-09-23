using UnityEngine;
using System.Collections;

public class RaidPanel : MonoBehaviour
{
    public RaidBannerPanel bannerPanel;
    public RaidHeroPanel heroPanel;

    bool isMapActive = true;
    public RaidMapPanel mapPanel;
    public RaidInventoryPanel inventoryPanel;

    public FormationUnit SelectedUnit { get; private set; }
    public Hero SelectedHero { get; private set; }

    public RaidCombatSkillsPanel SkillPanel
    {
        get
        {
            return bannerPanel.skillPanel;
        }
    }

    public bool IsMapActive
    {
        get
        {
            return isMapActive;
        }
    }
    public bool SwitchBlocked { get; set; }

    void Start()
    {
        isMapActive = true;
        RightPanelSwitched(true);
    }

    public void UpdateSelection()
    {
        bannerPanel.UpdateHero();
        heroPanel.UpdateHero();
    }
    public void SelectHeroUnit(FormationUnit heroUnit)
    {
        SelectedUnit = heroUnit;
        SelectedHero = heroUnit.Character as Hero;
        bannerPanel.UpdateHero();
        heroPanel.UpdateHero();
    }

    public void SetCombatState()
    {
        bannerPanel.SetCombatReady();
    }
    public void SetDisabledState()
    {
        bannerPanel.SetDisabledState();
    }
    public void SetPeacefulState()
    {
        bannerPanel.SetPeacefulState();
    }
    public void LockOnMap()
    {
        SwitchBlocked = true;
        if (!IsMapActive)
            RightPanelSwitched(true);
    }
    public void RightPanelSwitched(bool forced = false)
    {
        if (SwitchBlocked && forced == false)
            return;

        isMapActive = !isMapActive;

        if(isMapActive)
        {
            inventoryPanel.RectTransform.SetAsFirstSibling();
        }
        else
            inventoryPanel.RectTransform.SetAsLastSibling();
    }
}
