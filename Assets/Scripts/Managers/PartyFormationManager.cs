using UnityEngine;

public class PartyFormationManager : MonoBehaviour
{
    [SerializeField]
    private PositionSet partyBuffPositions;
    [SerializeField]
    private PositionSet heroesDefencePositions;
    [SerializeField]
    private PositionSet heroesAttackMeleePosition;
    [SerializeField]
    private PositionSet heroesAttackRangePosition;
    [SerializeField]
    private PositionSet monstersDefencePositions;
    [SerializeField]
    private PositionSet monstersAttackMeleePosition;
    [SerializeField]
    private PositionSet monstersAttackRangePosition;

    [SerializeField]
    private BattleFormation heroes;
    [SerializeField]
    private BattleFormation monsters;

    public FormationOverlay HeroOverlay { get { return Heroes.Overlay; } }
    public PositionSet PartyBuffPositions { get { return partyBuffPositions; } }
    public PositionSet HeroesDefencePositions { get { return heroesDefencePositions; } }
    public PositionSet HeroesAttackMeleePosition { get { return heroesAttackMeleePosition; } }
    public PositionSet HeroesAttackRangePosition { get { return heroesAttackRangePosition; } }
    public PositionSet MonstersDefencePositions { get { return monstersDefencePositions; } }
    public PositionSet MonstersAttackMeleePosition { get { return monstersAttackMeleePosition; } }
    public PositionSet MonstersAttackRangePosition { get { return monstersAttackRangePosition; } }
    public BattleFormation Heroes { get { return heroes; } }
    public BattleFormation Monsters { get { return monsters; } }

    private float enterDoorYOffset = 5;
    private float enterDoorZOffset = 30;

    public static int ShowoffOrder = 10;
    public static int BackgroundOrder = 4;

    public void Initialize()
    {
        Heroes.LoadParty(RaidSceneManager.Raid.RaidParty);
    }

    public void Initialize(BattleFormationSaveData heroFormationData)
    {
        Heroes.LoadParty(heroFormationData, true);
    }

    public void TransferToHallway(RaidHallwayView hallwayView)
    {
        RaidSceneManager.DungeonCamera.Mode = CameraMode.Follow;
        RaidSceneManager.DungeonCamera.Transform.position = new Vector3(hallwayView.StartingPosition.position.x, -2, -290);

        Heroes.RectTransform.SetParent(hallwayView.HallwayPassage.RectTransform, false);
        Heroes.RectTransform.position = hallwayView.StartingPosition.position;
        Heroes.Ranks.InstantRelocation();
    }

    public void TransferToHallwaySector(RaidHallwayView hallwayView, RaidHallSector raidHallSector)
    {
        RaidSceneManager.DungeonCamera.Mode = CameraMode.Follow;
        RaidSceneManager.DungeonCamera.Transform.position = new Vector3(raidHallSector.RectTransform.position.x, -2, -290);

        Heroes.RectTransform.SetParent(hallwayView.HallwayPassage.RectTransform, false);
        Heroes.RectTransform.position = hallwayView.StartingPosition.position;
        Vector3[] corners = new Vector3[4];
        raidHallSector.RectTransform.GetWorldCorners(corners);
        if(raidHallSector.Area.HasActiveBattle)
            Heroes.Ranks.RectTransform.position = new Vector3(corners[0].x + (corners[3].x - corners[0].x) * 1.05f/6,
                hallwayView.StartingPosition.position.y, hallwayView.StartingPosition.position.z);
        else
            Heroes.Ranks.RectTransform.position = new Vector3(corners[3].x, 
                hallwayView.StartingPosition.position.y, hallwayView.StartingPosition.position.z);

        Heroes.Ranks.InstantRelocation();
    }

    public void TransferToRoom(RaidRoomView roomView)
    {
        RaidSceneManager.DungeonCamera.Mode = CameraMode.Static;
        RaidSceneManager.DungeonCamera.Transform.position = new Vector3(roomView.transform.position.x, 0, -300);

        Heroes.RectTransform.SetParent(roomView.HallwayPassage.RectTransform, false);
        Heroes.RectTransform.position = roomView.StartingPosition.position;
        Heroes.Ranks.InstantRelocation();
    }

    #region Unit Overlay

    public void HideUnitOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Hide();
        HideHeroOverlay();
        HideMonsterOverlay();
    }

    public void HideHeroOverlay()
    {
        Heroes.Overlay.Hide();
    }

    public void HideMonsterOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Hide();
        Monsters.Overlay.Hide();
    }

    public void ShowUnitOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Show();
        ShowHeroOverlay();
        ShowMonsterOverlay();
    }

    public void ShowHeroOverlay()
    {
        Heroes.Overlay.Show();
    }

    public void ResetSelections()
    {
        Monsters.Overlay.ResetSelections();
        Heroes.Overlay.ResetSelections();
    }

    public void LockSelections()
    {
        Monsters.Overlay.LockSelection();
        Heroes.Overlay.LockSelection();
    }

    public void UnlockSelections()
    {
        Monsters.Overlay.UnlockSelection();
        Heroes.Overlay.UnlockSelection();
    }

    private void ShowMonsterOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Show();
        Monsters.Overlay.Show();
    }

    #endregion

    #region Animation Helpers

    public void InvestigateCurioIntro(RaidProp raidProp)
    {
        HideHeroOverlay();
        if (RaidSceneManager.DungeonCamera.Mode == CameraMode.Follow)
        {
            raidProp.Relocate(new Vector3(RaidSceneManager.DungeonCamera.Target.position.x,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 80), 0.2f);
            raidProp.SetSortingOrder(ShowoffOrder + 2);

            RaidSceneManager.RaidPanel.SelectedUnit.SetTarget(new Vector3(RaidSceneManager.DungeonCamera.Target.position.x - 12,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 70), 0.2f);
            RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(ShowoffOrder + 3);
            RaidSceneManager.RaidPanel.SelectedUnit.SetInvestigateAnimation(true);
        }
        else if (RaidSceneManager.DungeonCamera.Mode == CameraMode.Static)
        {
            raidProp.Relocate(new Vector3(RaidSceneManager.DungeonCamera.Transform.position.x,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 80), 0.2f);
            raidProp.SetSortingOrder(ShowoffOrder + 2);

            RaidSceneManager.RaidPanel.SelectedUnit.SetTarget(new Vector3(RaidSceneManager.DungeonCamera.Transform.position.x - 12,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 70), 0.2f);
            RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(ShowoffOrder + 3);
            RaidSceneManager.RaidPanel.SelectedUnit.SetInvestigateAnimation(true);
        }
    }

    public void InvestigateTrapIntro(RaidTrap raidTrap, bool isDisarmed)
    {
        raidTrap.Relocate(Vector3.zero, 0.1f);
        if (isDisarmed)
        {
            raidTrap.SkeletonAnimation.state.ClearTracks();
            raidTrap.SkeletonAnimation.state.SetAnimation(0, "idle", false);
            RaidSceneManager.RaidPanel.SelectedUnit.SetInvestigateAnimation(true);
        }
        else
        {
            raidTrap.SkeletonAnimation.state.ClearTracks();
            raidTrap.SkeletonAnimation.state.SetAnimation(0, "sprung", false);
            RaidSceneManager.RaidPanel.SelectedUnit.SetDefendAnimation(true);
        }
        raidTrap.SetSortingOrder(ShowoffOrder + 3);
        RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(ShowoffOrder + 1);
    }

    public void InvestigateCurioOutro(IRaidArea areaView)
    {
        areaView.Prop.Return(0.1f);

        RaidSceneManager.RaidPanel.SelectedUnit.DeleteTarget(0.2f);
        RaidSceneManager.RaidPanel.SelectedUnit.SetInvestigateAnimation(false);
        RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(ShowoffOrder - RaidSceneManager.RaidPanel.SelectedUnit.Rank);
    }

    public void InvestigateTrapOutro(IRaidArea areaView, bool isDisarmed)
    {
        RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(ShowoffOrder
            - RaidSceneManager.RaidPanel.SelectedUnit.Rank);

        if (isDisarmed)
            RaidSceneManager.RaidPanel.SelectedUnit.SetInvestigateAnimation(false);
        else
        {
            RaidSceneManager.RaidPanel.SelectedUnit.SetDefendAnimation(false);
        }
        RaidSceneManager.RaidPanel.SelectedUnit.DeleteTarget(0.1f);
        areaView.Prop.Return(0.1f);
    }

    public void HeroResolveCheckIntro(FormationUnit unit, bool isHeroic)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(ShowoffOrder);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        if (isHeroic)
            unit.SetHeroic(true);
        else
            unit.SetAfflicted(true);
    }

    public void HeroResolveCheckOutro(FormationUnit unit, bool isHeroic)
    {
        unit.SetLayer(0);
        unit.SetSortingOrder(ShowoffOrder - RaidSceneManager.RaidPanel.SelectedUnit.Rank);
        unit.DeleteTarget(0.1f);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (isHeroic)
            unit.SetHeroic(false);
        else
            unit.SetAfflicted(false);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = true;
    }

    public void UnitSkillIntro(FormationUnit unit, SkillArtInfo skillArtInfo)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(ShowoffOrder + 6);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        unit.SetPerformerSkillAnimation(skillArtInfo, true);
    }

    public void UnitSkillIntro(FormationUnit unit, string skillArtId)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(ShowoffOrder + 6);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        unit.SetPerformerSkillAnimation(skillArtId, true);
    }

    public void UnitSkillIntroOverriden(FormationUnit unit, SkillArtInfo skillArtInfo, string mode)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(ShowoffOrder + 6);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        unit.SetPerformerSkillAnimationOverriden(skillArtInfo, mode, true);
    }

    public void UnitDefendIntro(FormationUnit unit)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        if(unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder + 4 - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder + 4 - unit.Character.RenderRankOverride);

        unit.SetDefendAnimation(true);
    }

    public void UnitBuffedIntro(FormationUnit unit)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder + 4 - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder + 4 - unit.Character.RenderRankOverride);
    }

    public void UnitSkillOutro(FormationUnit unit, SkillArtInfo skillArtInfo)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);
        unit.SetPerformerSkillAnimation(skillArtInfo, false);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = true;
    }

    public void UnitSkillOutro(FormationUnit unit, string skillArtId)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);
        unit.SetPerformerSkillAnimation(skillArtId, false);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = true;
    }

    public void UnitSkillOutroOverriden(FormationUnit unit, SkillArtInfo skillArtInfo, string mode)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);
        unit.SetPerformerSkillAnimationOverriden(skillArtInfo, mode, false);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = true;
    }

    public void UnitDefendOutro(FormationUnit unit)
    {
        unit.SetLayer(0);
        if(unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder - unit.Character.RenderRankOverride);

        unit.DeleteTarget(0.1f);
        if (!unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);

        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = true;
    }

    public void UnitBuffedOutro(FormationUnit unit)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);

        if (unit.CurrentHalo != null)
            unit.CurrentHalo.SkeletonAnimation.MeshRenderer.enabled = true;
    }

    #endregion

    public void SendUnitIntoDoor(RaidHallSector sector, int unitIndex)
    {
        Heroes.Party.Units[unitIndex].SetTarget(new Vector3(sector.Prop.RectTransform.position.x,
        Heroes.Party.Units[unitIndex].RectTransform.position.y + enterDoorYOffset,
        sector.Prop.RectTransform.position.z + enterDoorZOffset), 0.2f);
    }

    public void GetUnitOutOfDoor(RaidHallSector sector, int unitIndex)
    {
        Heroes.Party.Units[unitIndex].DeleteTarget(0.2f);
    }
}