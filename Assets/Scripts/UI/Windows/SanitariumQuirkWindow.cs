using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SanitariumQuirkWindow : MonoBehaviour
{
    public Text headerLabel;
    public Text costLabel;
    public Button treatmentButton;

    public List<QuirkTreatmentSlot> positiveSlots;
    public List<QuirkTreatmentSlot> negativeSlots;

    public TreatmentHeroSlot SelectedSlot { get; set; }
    public TownManager TownManager { get; set; }

    public void Initialize(TownManager townManager)
    {
        TownManager = townManager;
        for (int i = 0; i < positiveSlots.Count; i++)
        {
            positiveSlots[i].onSelect += SanitariumQuirkWindow_onPositiveSelect;
            positiveSlots[i].onDeselect += SanitariumQuirkWindow_onPositiveDeselect;
        }
        for (int i = 0; i < negativeSlots.Count; i++)
        {
            negativeSlots[i].onSelect += SanitariumQuirkWindow_onNegativeSelect;
            negativeSlots[i].onDeselect += SanitariumQuirkWindow_onNegativeDeselect;
        }
    }

    void SanitariumQuirkWindow_onPositiveDeselect(QuirkTreatmentSlot slot)
    {
        SelectedSlot.TreatmentSlot.TargetPositiveQuirk = null;
        RecalculateCost();
    }
    void SanitariumQuirkWindow_onNegativeDeselect(QuirkTreatmentSlot slot)
    {
        SelectedSlot.TreatmentSlot.TargetNegativeQuirk = null;
        RecalculateCost();
    }
    void SanitariumQuirkWindow_onPositiveSelect(QuirkTreatmentSlot slot)
    {
        if (SelectedSlot != null && SelectedSlot.TreatmentSlot.Hero.LockedPositiveQuirks.Count > 2)
        {
            slot.Deselect();
            return;
        }

        for (int i = 0; i < positiveSlots.Count; i++)
        {
            if (positiveSlots[i] != slot && positiveSlots[i].Selected)
                positiveSlots[i].Deselect();
        }
        SelectedSlot.TreatmentSlot.TargetPositiveQuirk = slot.QuirkInfo.Quirk.Id;
        RecalculateCost();
    }
    void SanitariumQuirkWindow_onNegativeSelect(QuirkTreatmentSlot slot)
    {
        for (int i = 0; i < negativeSlots.Count; i++)
        {
            if (negativeSlots[i] != slot && negativeSlots[i].Selected)
                negativeSlots[i].Deselect();
        }
        SelectedSlot.TreatmentSlot.TargetNegativeQuirk = slot.QuirkInfo.Quirk.Id;
        RecalculateCost();
    }

    public void RecalculateCost()
    {
        if(SelectedSlot != null)
        {
            int cost = 0;
            if (SelectedSlot.TreatmentSlot.TargetPositiveQuirk != null)
                cost += SelectedSlot.TreatmentSlot.BasePositiveCost;
            if (SelectedSlot.TreatmentSlot.TargetNegativeQuirk != null)
            {
                var quirk = SelectedSlot.TreatmentSlot.Hero.GetQuirkInfo(SelectedSlot.TreatmentSlot.TargetNegativeQuirk);
                if(quirk != null)
                {
                    if (quirk.IsLocked)
                        cost += SelectedSlot.TreatmentSlot.BasePermanentCost;
                    else
                        cost += SelectedSlot.TreatmentSlot.BaseCost;
                }
            }
            if(cost != 0)
            {
                treatmentButton.gameObject.SetActive(true);
                costLabel.gameObject.SetActive(true);
                costLabel.text = cost.ToString();
            }
            else
            {
                treatmentButton.gameObject.SetActive(false);
                costLabel.gameObject.SetActive(false);
            }
        }
    }
    public void TreatmentButtonClicked()
    {
        int cost = 0;
        if (SelectedSlot.TreatmentSlot.TargetPositiveQuirk != null)
            cost += SelectedSlot.TreatmentSlot.BasePositiveCost;
        if (SelectedSlot.TreatmentSlot.TargetNegativeQuirk != null)
        {
            var quirk = SelectedSlot.TreatmentSlot.Hero.GetQuirkInfo(SelectedSlot.TreatmentSlot.TargetNegativeQuirk);
            if (quirk != null)
            {
                if (quirk.IsLocked)
                    cost += SelectedSlot.TreatmentSlot.BasePermanentCost;
                else
                    cost += SelectedSlot.TreatmentSlot.BaseCost;
            }
        }
        if (cost != 0)
        {
            if (SelectedSlot.TreatmentSlot.Status == ActivitySlotStatus.Checkout)
            {
                if (DarkestDungeonManager.Campaign.Estate.CanPayGold(cost))
                {
                    DarkestDungeonManager.Campaign.Estate.RemoveGold(cost);
                    TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                    TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                    TownManager.GetHeroSlot(SelectedSlot.TreatmentSlot.Hero).SetStatus(HeroStatus.Sanitarium);
                    SelectedSlot.PayoutSlot();
                    ResetWindow();
                }
            }
        }
        else
        {
            treatmentButton.gameObject.SetActive(false);
            costLabel.gameObject.SetActive(false);
        }
    }

    public void LoadHeroOverview(TreatmentHeroSlot slot)
    {
        SelectedSlot = slot;
        SelectedSlot.ResetTreatment();
        int positiveQuirks = 0;
        foreach (var posQuirk in slot.TreatmentSlot.Hero.PositiveQuirks)
            positiveSlots[positiveQuirks++].UpdateQuirk(posQuirk);

        for (int i = positiveQuirks; i < positiveSlots.Count; i++)
            positiveSlots[i].ResetSlot();

        int negQuirks = 0;
        foreach (var negQuirk in slot.TreatmentSlot.Hero.NegativeQuirks)
            negativeSlots[negQuirks++].UpdateQuirk(negQuirk);

        for (int i = negQuirks; i < negativeSlots.Count; i++)
            negativeSlots[i].ResetSlot();

        RecalculateCost();
        gameObject.SetActive(true);
    }
    public void UpdateHeroOverview()
    {
        RecalculateCost();
    }

    public void ResetWindow()
    {
        SelectedSlot = null;
        for (int i = 0; i < positiveSlots.Count; i++)
            positiveSlots[i].ResetSlot();
        for (int i = 0; i < negativeSlots.Count; i++)
            negativeSlots[i].ResetSlot();
        gameObject.SetActive(false);
    }
}