using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SanitariumQuirkWindow : MonoBehaviour
{
    [SerializeField]
    private Text costLabel;
    [SerializeField]
    private Button treatmentButton;
    [SerializeField]
    private List<QuirkTreatmentSlot> positiveSlots;
    [SerializeField]
    private List<QuirkTreatmentSlot> negativeSlots;

    private TreatmentHeroSlot SelectedSlot { get; set; }

    public void Initialize()
    {
        for (int i = 0; i < positiveSlots.Count; i++)
        {
            positiveSlots[i].EventSelected += QuirkTreatmentSlotPositiveSelected;
            positiveSlots[i].EventDeselected += QuirkTreatmentSlotPositiveDeselected;
        }
        for (int i = 0; i < negativeSlots.Count; i++)
        {
            negativeSlots[i].EventSelected += QuirkTreatmentSlotNegativeSelected;
            negativeSlots[i].EventDeselected += QuirkTreatmentSlotNegativeDeselected;
        }
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
                if (SelectedSlot.TreatmentSlot.TargetPositiveQuirk != null || SelectedSlot.TreatmentSlot.TargetNegativeQuirk != null)
                {
                    treatmentButton.gameObject.SetActive(true);
                    costLabel.gameObject.SetActive(true);
                    costLabel.text = LocalizationManager.GetString("town_free");
                }
                else
                {
                    treatmentButton.gameObject.SetActive(false);
                    costLabel.gameObject.SetActive(false);
                }
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
        if (cost != 0 || DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(SelectedSlot.ActivityName))
        {
            if (SelectedSlot.TreatmentSlot.Status == ActivitySlotStatus.Checkout)
            {
                if (DarkestDungeonManager.Campaign.Estate.CanPayGold(cost))
                {
                    DarkestDungeonManager.Campaign.Estate.RemoveGold(cost);
                    EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
                    EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("gold");
                    EstateSceneManager.Instanse.TownManager.GetHeroSlot(SelectedSlot.TreatmentSlot.Hero).SetStatus(HeroStatus.Sanitarium);
                    SelectedSlot.PayoutSlot();
                    DarkestSoundManager.PlayOneShot("event:/town/sanitarium_treatment");
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

    private void QuirkTreatmentSlotPositiveDeselected(QuirkTreatmentSlot slot)
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");

        SelectedSlot.TreatmentSlot.TargetPositiveQuirk = null;
        RecalculateCost();
    }

    private void QuirkTreatmentSlotNegativeDeselected(QuirkTreatmentSlot slot)
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");

        SelectedSlot.TreatmentSlot.TargetNegativeQuirk = null;
        RecalculateCost();
    }

    private void QuirkTreatmentSlotPositiveSelected(QuirkTreatmentSlot slot)
    {
        if (SelectedSlot != null && SelectedSlot.TreatmentSlot.Hero.LockedPositiveQuirks.Count > 2)
        {
            slot.Deselect();
            return;
        }
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");

        for (int i = 0; i < positiveSlots.Count; i++)
        {
            if (positiveSlots[i] != slot && positiveSlots[i].Selected)
                positiveSlots[i].Deselect();
        }
        SelectedSlot.TreatmentSlot.TargetPositiveQuirk = slot.QuirkInfo.Quirk.Id;
        RecalculateCost();
    }

    private void QuirkTreatmentSlotNegativeSelected(QuirkTreatmentSlot slot)
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");

        for (int i = 0; i < negativeSlots.Count; i++)
        {
            if (negativeSlots[i] != slot && negativeSlots[i].Selected)
                negativeSlots[i].Deselect();
        }
        SelectedSlot.TreatmentSlot.TargetNegativeQuirk = slot.QuirkInfo.Quirk.Id;
        RecalculateCost();
    }
}