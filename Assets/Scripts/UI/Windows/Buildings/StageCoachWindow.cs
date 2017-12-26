using UnityEngine;

public class StageCoachWindow : UpgradableBuildingWindow
{
    [SerializeField]
    private RecruitPanel recruitPanel;
    [SerializeField]
    private HeroRosterPanel rosterPanel;

    public RecruitPanel Panel { get { return recruitPanel; } }

    protected override BuildingType BuildingType { get { return BuildingType.StageCoach; } }

    public override void Initialize()
    {
        base.Initialize();

        Panel.UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes);
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        base.UpdateUpgradeTrees(afterPurchase);

        rosterPanel.UpdateCapacity();
    }
}