using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CampController : MonoBehaviour
{
    public List<RectTransform> campingPositions;

    public void SwitchCamping(bool active)
    {
        gameObject.SetActive(active);
        if (active)
            SetHeroPositions();
        else
            ReturnHeroPositions();
    }

    void SetHeroPositions()
    {
        RaidSceneManager.Raid.CampingPhase = CampingPhase.Meal;
        int positions = Mathf.Min(campingPositions.Count, RaidSceneManager.Formations.heroes.party.Units.Count);
        for(int i = 0; i < positions; i++)
        {
            RaidSceneManager.Rules.SetCamping(true);
            RaidSceneManager.HeroParty.Units[i].Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(RaidSceneManager.HeroParty.Units[i]));
            RaidSceneManager.HeroParty.Units[i].Character.RemoveCampingBuffs();
            RaidSceneManager.Formations.heroes.party.Units[i].SetRectTarget(campingPositions[i], 0, true);
            RaidSceneManager.Formations.heroes.party.Units[i].InstantRelocation();
            if (i < 2)
                RaidSceneManager.Formations.heroes.party.Units[i].InstantFlip();
            if (i == 0 || i == 3)
            {
                RaidSceneManager.Formations.heroes.party.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0.05f);
                RaidSceneManager.Formations.heroes.party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder);
            }
            else
            {
                RaidSceneManager.Formations.heroes.party.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0.12f);
                RaidSceneManager.Formations.heroes.party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder - 1);
            }
        }
        RaidSceneManager.Formations.HeroOverlay.UpdateOverlay();
    }
    void ReturnHeroPositions()
    {
        RaidSceneManager.Raid.CampingPhase = CampingPhase.None;
        int positions = Mathf.Min(campingPositions.Count, RaidSceneManager.Formations.heroes.party.Units.Count);
        for (int i = 0; i < positions; i++)
        {
            RaidSceneManager.Rules.SetCamping(false);
            RaidSceneManager.HeroParty.Units[i].Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(RaidSceneManager.HeroParty.Units[i]));
            RaidSceneManager.HeroParty.Units[i].CombatInfo.SkillsUsedThisTurn.Clear();
            RaidSceneManager.Formations.heroes.party.Units[i].DeleteTarget(0, true);
            RaidSceneManager.Formations.heroes.party.Units[i].InstantRelocation();
            RaidSceneManager.Formations.heroes.party.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0);

            if (campingPositions.IndexOf(RaidSceneManager.HeroParty.Units[i].TargetRect) < 2)
                RaidSceneManager.Formations.heroes.party.Units[i].InstantFlip();
            if (RaidSceneManager.Formations.heroes.party.Units[i].Character.RenderRankOverride == 0)
                RaidSceneManager.Formations.heroes.party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder
                    - RaidSceneManager.Formations.heroes.party.Units[i].Rank);
            else
                RaidSceneManager.Formations.heroes.party.Units[i].SetSortingOrder(PartyFormationManager.ShowoffOrder
                    - RaidSceneManager.Formations.heroes.party.Units[i].Character.RenderRankOverride);
        }
    }
}
