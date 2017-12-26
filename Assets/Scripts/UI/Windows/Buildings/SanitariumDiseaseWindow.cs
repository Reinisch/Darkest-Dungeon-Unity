using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SanitariumDiseaseWindow : MonoBehaviour
{
    [SerializeField]
    private Text costLabel;
    [SerializeField]
    private Button treatmentButton;
    [SerializeField]
    private List<QuirkTreatmentSlot> diseaseSlots;

    private TreatmentHeroSlot SelectedSlot { get; set; }

    public void Initialize()
    {
        for (int i = 0; i < diseaseSlots.Count; i++)
        {
            diseaseSlots[i].EventSelected += QuirkTreatmentSlotSelected;
            diseaseSlots[i].EventDeselected += QuirkTreatmentSlotDeselected;
        }
    }

    public void RecalculateCost()
    {
        if (SelectedSlot != null)
        {
            int cost = 0;
            if (SelectedSlot.TreatmentSlot.TargetDiseaseQuirk != null)
                cost += SelectedSlot.TreatmentSlot.BaseDiseaseCost;

            if (cost != 0)
            {
                treatmentButton.gameObject.SetActive(true);
                costLabel.gameObject.SetActive(true);
                costLabel.text = cost.ToString();
            }
            else
            {
                if (SelectedSlot.TreatmentSlot.TargetDiseaseQuirk != null)
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
        if (SelectedSlot.TreatmentSlot.TargetDiseaseQuirk != null)
            cost += SelectedSlot.TreatmentSlot.BaseDiseaseCost;

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
                    DarkestSoundManager.PlayOneShot("event:/town/sanitarium_disease_treatment");
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
        int diseasesCount = 0;
        foreach (var disease in slot.TreatmentSlot.Hero.Diseases)
            diseaseSlots[diseasesCount++].UpdateQuirk(disease);

        for (int i = diseasesCount; i < diseaseSlots.Count; i++)
            diseaseSlots[i].ResetSlot();

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
        for (int i = 0; i < diseaseSlots.Count; i++)
            diseaseSlots[i].ResetSlot();
        gameObject.SetActive(false);
    }

    private void QuirkTreatmentSlotDeselected(QuirkTreatmentSlot slot)
    {
        SelectedSlot.TreatmentSlot.TargetDiseaseQuirk = null;
        RecalculateCost();
    }

    private void QuirkTreatmentSlotSelected(QuirkTreatmentSlot slot)
    {
        for (int i = 0; i < diseaseSlots.Count; i++)
        {
            if (diseaseSlots[i] != slot && diseaseSlots[i].Selected)
                diseaseSlots[i].Deselect();
        }
        SelectedSlot.TreatmentSlot.TargetDiseaseQuirk = slot.QuirkInfo.Quirk.Id;
        RecalculateCost();
    }
}