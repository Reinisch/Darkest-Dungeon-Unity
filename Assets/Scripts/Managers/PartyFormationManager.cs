using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PartyFormationManager : MonoBehaviour
{
    public PositionSet partyBuffPositions;
    public PositionSet heroesDefencePositions;
    public PositionSet heroesAttackMeleePosition;
    public PositionSet heroesAttackRangePosition;
    public PositionSet monstersDefencePositions;
    public PositionSet monstersAttackMeleePosition;
    public PositionSet monstersAttackRangePosition;

    public BattleFormation heroes;
    public BattleFormation monsters;

    public FormationOverlay HeroOverlay
    {
        get
        {
            return heroes.overlay;
        }
    }

    private float enterDoorYOffset = 5;
    private float enterDoorZOffset = 30;

    public static int ShowoffOrder = 10;
    public static int BackgroundOrder = 4;

    public void Initialize()
    {
        heroes.LoadParty(RaidSceneManager.Raid.RaidParty);
    }
    public void Initialize(BattleFormationSaveData heroFormationData)
    {
        heroes.LoadParty(heroFormationData, true);
    }

    public void TransferToHallway(RaidHallwayView hallwayView)
    {
        RaidSceneManager.DungeonCamera.mode = CameraMode.Follow;
        RaidSceneManager.DungeonCamera.Transform.position = new Vector3(hallwayView.startingPosition.position.x, -2, -290);

        heroes.RectTransform.SetParent(hallwayView.hallwayPassage.RectTransform, false);
        heroes.RectTransform.position = hallwayView.startingPosition.position;
        heroes.ranks.InstantRelocation();
    }
    public void TransferToHallwaySector(RaidHallwayView hallwayView, RaidHallSector raidHallSector)
    {
        RaidSceneManager.DungeonCamera.mode = CameraMode.Follow;
        RaidSceneManager.DungeonCamera.Transform.position = new Vector3(raidHallSector.RectTransform.position.x, -2, -290);

        heroes.RectTransform.SetParent(hallwayView.hallwayPassage.RectTransform, false);
        heroes.RectTransform.position = hallwayView.startingPosition.position;
        Vector3[] corners = new Vector3[4];
        raidHallSector.RectTransform.GetWorldCorners(corners);
        if(raidHallSector.Area.HasActiveBattle)
            heroes.ranks.RectTransform.position = new Vector3(corners[0].x + (corners[3].x - corners[0].x) * 1.05f/6,
                hallwayView.startingPosition.position.y, hallwayView.startingPosition.position.z);
        else
            heroes.ranks.RectTransform.position = new Vector3(corners[3].x, 
                hallwayView.startingPosition.position.y, hallwayView.startingPosition.position.z);

        heroes.ranks.InstantRelocation();
    }
    public void TransferToRoom(RaidRoomView roomView)
    {
        RaidSceneManager.DungeonCamera.mode = CameraMode.Static;
        RaidSceneManager.DungeonCamera.Transform.position = new Vector3(roomView.transform.position.x, 0, -300);

        heroes.RectTransform.SetParent(roomView.hallwayPassage.RectTransform, false);
        heroes.RectTransform.position = roomView.startingPosition.position;
        heroes.ranks.InstantRelocation();
    }

    public void HideUnitOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Hide();
        HideHeroOverlay();
        HideMonsterOverlay();
    }
    public void HideHeroOverlay()
    {
        heroes.overlay.Hide();
    }
    public void HideMonsterOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Hide();
        monsters.overlay.Hide();
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
        heroes.overlay.Show();
    }
    public void ShowMonsterOverlay()
    {
        if (RaidSceneManager.BattleGround.SharedHealth.IsActive)
            RaidSceneManager.BattleGround.SharedHealth.Show();
        monsters.overlay.Show();
    }
    public void ResetSelections()
    {
        monsters.overlay.ResetSelections();
        heroes.overlay.ResetSelections();
    }
    public void LockSelections()
    {
        monsters.overlay.LockSelection();
        heroes.overlay.LockSelection();
    }
    public void UnlockSelections()
    {
        monsters.overlay.UnlockSelection();
        heroes.overlay.UnlockSelection();
    }

    public void InvestigateCurioIntro(RaidProp raidProp)
    {
        HideHeroOverlay();
        if (RaidSceneManager.DungeonCamera.mode == CameraMode.Follow)
        {
            raidProp.Relocate(new Vector3(RaidSceneManager.DungeonCamera.target.position.x,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 80), 0.2f);
            raidProp.SetSortingOrder(PartyFormationManager.ShowoffOrder + 2);

            RaidSceneManager.RaidPanel.SelectedUnit.SetTarget(new Vector3(RaidSceneManager.DungeonCamera.target.position.x - 12,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 70), 0.2f);
            RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 3);
            RaidSceneManager.RaidPanel.SelectedUnit.SetInvestigateAnimation(true);
        }
        else if (RaidSceneManager.DungeonCamera.mode == CameraMode.Static)
        {
            raidProp.Relocate(new Vector3(RaidSceneManager.DungeonCamera.Transform.position.x,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 80), 0.2f);
            raidProp.SetSortingOrder(PartyFormationManager.ShowoffOrder + 2);

            RaidSceneManager.RaidPanel.SelectedUnit.SetTarget(new Vector3(RaidSceneManager.DungeonCamera.Transform.position.x - 12,
                RaidSceneManager.DungeonCamera.Transform.position.y - 15,
                RaidSceneManager.DungeonCamera.Transform.position.z + 70), 0.2f);
            RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 3);
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
        RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(PartyFormationManager.ShowoffOrder 
            - RaidSceneManager.RaidPanel.SelectedUnit.Rank);
    }
    public void InvestigateTrapOutro(IRaidArea areaView, bool isDisarmed)
    {
        RaidSceneManager.RaidPanel.SelectedUnit.SetSortingOrder(PartyFormationManager.ShowoffOrder
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
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(PartyFormationManager.ShowoffOrder);
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
        unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - RaidSceneManager.RaidPanel.SelectedUnit.Rank);
        unit.DeleteTarget(0.1f);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (isHeroic)
            unit.SetHeroic(false);
        else
            unit.SetAfflicted(false);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = true;
    }

    public void UnitSkillIntro(FormationUnit unit, SkillArtInfo skillArtInfo)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 6);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        unit.SetPerformerSkillAnimation(skillArtInfo, true);
    }
    public void UnitSkillIntro(FormationUnit unit, string skillArtId)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 6);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        unit.SetPerformerSkillAnimation(skillArtId, true);
    }
    public void UnitSkillIntroOverriden(FormationUnit unit, SkillArtInfo skillArtInfo, string mode)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 6);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);
        unit.SetPerformerSkillAnimationOverriden(skillArtInfo, mode, true);
    }
    public void UnitDefendIntro(FormationUnit unit)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        if(unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 4 - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 4 - unit.Character.RenderRankOverride);

        unit.SetDefendAnimation(true);
    }
    public void UnitBuffedIntro(FormationUnit unit)
    {
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = false;

        unit.SetLayer(9);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 4 - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder + 4 - unit.Character.RenderRankOverride);
    }

    public void UnitSkillOutro(FormationUnit unit, SkillArtInfo skillArtInfo)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);
        unit.SetPerformerSkillAnimation(skillArtInfo, false);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = true;
    }
    public void UnitSkillOutro(FormationUnit unit, string skillArtId)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);
        unit.SetPerformerSkillAnimation(skillArtId, false);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = true;
    }
    public void UnitSkillOutroOverriden(FormationUnit unit, SkillArtInfo skillArtInfo, string mode)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);
        unit.SetPerformerSkillAnimationOverriden(skillArtInfo, mode, false);
        if (unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(true);
        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = true;
        
    }

    public void UnitDefendOutro(FormationUnit unit)
    {
        unit.SetLayer(0);
        if(unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Character.RenderRankOverride);

        unit.DeleteTarget(0.1f);
        if (!unit.CombatInfo.IsImmobilized)
            unit.SetDefendAnimation(false);

        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = true;
    }
    public void UnitBuffedOutro(FormationUnit unit)
    {
        unit.SetLayer(0);
        if (unit.Character.RenderRankOverride == 0)
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Rank);
        else
            unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Character.RenderRankOverride);
        unit.DeleteTarget(0.1f);

        if (unit.CurrentHalo != null)
            unit.CurrentHalo.skeletonAnimation.MeshRenderer.enabled = true;
    }

    public void SendUnitIntoDoor(RaidHallSector sector, int unitIndex)
    {
        heroes.party.Units[unitIndex].SetTarget(new Vector3(sector.Prop.RectTransform.position.x,
        heroes.party.Units[unitIndex].RectTransform.position.y + enterDoorYOffset,
        sector.Prop.RectTransform.position.z + enterDoorZOffset), 0.2f);
    }
    public void GetUnitOutOfDoor(RaidHallSector sector, int unitIndex)
    {
        heroes.party.Units[unitIndex].DeleteTarget(0.2f);
    }
}