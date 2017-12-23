using UnityEngine;

public class RaidPanel : MonoBehaviour
{
    [SerializeField]
    private RaidBannerPanel bannerPanel;
    [SerializeField]
    private RaidHeroPanel heroPanel;
    [SerializeField]
    private RaidMapPanel mapPanel;
    [SerializeField]
    private RaidInventoryPanel inventoryPanel;

    public RaidBannerPanel BannerPanel { get { return bannerPanel; } }
    public RaidHeroPanel HeroPanel { get { return heroPanel; } }
    public RaidMapPanel MapPanel { get { return mapPanel; } }
    public RaidInventoryPanel InventoryPanel { get { return inventoryPanel; } }
    public FormationUnit SelectedUnit { get; private set; }
    public Hero SelectedHero { get; private set; }
    public RaidCombatSkillsPanel SkillPanel { get { return BannerPanel.SkillPanel; } }
    public bool IsMapActive { get { return isMapActive; } }
    public bool SwitchBlocked { get; set; }

    private bool isMapActive = true;

    private void Start()
    {
        isMapActive = true;
        RightPanelSwitched(true);
    }

    public void UpdateSelection()
    {
        BannerPanel.UpdateHero();
        HeroPanel.UpdateHero();
    }

    public void SelectHeroUnit(FormationUnit heroUnit)
    {
        SelectedUnit = heroUnit;
        SelectedHero = heroUnit.Character as Hero;
        BannerPanel.UpdateHero();
        HeroPanel.UpdateHero();
    }

    public void SetCombatState()
    {
        BannerPanel.SetCombatReady();
    }

    public void SetDisabledState()
    {
        BannerPanel.SetDisabledState();
    }

    public void SetPeacefulState()
    {
        BannerPanel.SetPeacefulState();
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
            InventoryPanel.RectTransform.SetAsFirstSibling();
        }
        else
            InventoryPanel.RectTransform.SetAsLastSibling();
    }
}
