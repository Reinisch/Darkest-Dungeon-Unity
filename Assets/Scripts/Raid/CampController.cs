using UnityEngine;
using System.Collections.Generic;

public class CampController : MonoBehaviour
{
    [SerializeField]
    private List<RectTransform> campingPositions;

    public void SwitchCamping(bool active)
    {
        gameObject.SetActive(active);
        if (active)
            SetHeroPositions();
        else
            ReturnHeroPositions();
    }

    private void SetHeroPositions()
    {
        RaidSceneManager.Raid.CampingPhase = CampingPhase.Meal;
        int positions = Mathf.Min(campingPositions.Count, RaidSceneManager.Formations.Heroes.Party.Units.Count);
        for(int i = 0; i < positions; i++)
        {
            RaidSceneManager.Rules.SetCamping(true);
            RaidSceneManager.HeroParty.Units[i].Character.ApplyAllBuffRules(
                RaidSceneManager.Rules.GetIdleUnitRules(RaidSceneManager.HeroParty.Units[i]));
            RaidSceneManager.HeroParty.Units[i].Character.RemoveCampingBuffs();
            RaidSceneManager.Formations.Heroes.Party.Units[i].SetRectTarget(campingPositions[i], 0, true);
            RaidSceneManager.Formations.Heroes.Party.Units[i].InstantRelocation();
            if (i < 2)
                RaidSceneManager.Formations.Heroes.Party.Units[i].InstantFlip();
            if (i == 0 || i == 3)
            {
                RaidSceneManager.Formations.Heroes.Party.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0.05f);
                RaidSceneManager.Formations.Heroes.Party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder);
            }
            else
            {
                RaidSceneManager.Formations.Heroes.Party.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0.12f);
                RaidSceneManager.Formations.Heroes.Party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder - 1);
            }
        }
        RaidSceneManager.Formations.HeroOverlay.UpdateOverlay();
    }

    private void ReturnHeroPositions()
    {
        RaidSceneManager.Raid.CampingPhase = CampingPhase.None;
        int positions = Mathf.Min(campingPositions.Count, RaidSceneManager.Formations.Heroes.Party.Units.Count);
        for (int i = 0; i < positions; i++)
        {
            RaidSceneManager.Rules.SetCamping(false);
            RaidSceneManager.HeroParty.Units[i].Character.ApplyAllBuffRules(
                RaidSceneManager.Rules.GetIdleUnitRules(RaidSceneManager.HeroParty.Units[i]));
            RaidSceneManager.HeroParty.Units[i].CombatInfo.SkillsUsedThisTurn.Clear();
            RaidSceneManager.Formations.Heroes.Party.Units[i].DeleteTarget(0, true);
            RaidSceneManager.Formations.Heroes.Party.Units[i].InstantRelocation();
            RaidSceneManager.Formations.Heroes.Party.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0);

            if (campingPositions.IndexOf(RaidSceneManager.HeroParty.Units[i].TargetRect) < 2)
                RaidSceneManager.Formations.Heroes.Party.Units[i].InstantFlip();
            if (RaidSceneManager.Formations.Heroes.Party.Units[i].Character.RenderRankOverride == 0)
                RaidSceneManager.Formations.Heroes.Party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder
                    - RaidSceneManager.Formations.Heroes.Party.Units[i].Rank);
            else
                RaidSceneManager.Formations.Heroes.Party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder
                    - RaidSceneManager.Formations.Heroes.Party.Units[i].Character.RenderRankOverride);
        }
    }
}
