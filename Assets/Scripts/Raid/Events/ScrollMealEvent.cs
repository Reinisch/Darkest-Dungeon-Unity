using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum CampMealResultType { Wait, Eat }

public class ScrollMealEvent : MonoBehaviour
{
    [SerializeField]
    private Animator campAnimator;
    [SerializeField]
    private List<MealSlot> mealSlots;

    public CampMealResultType MealResult { get; private set; }
    public MealSlot SelectedMealSlot { get; private set; }

    public void Show()
    {
        campAnimator.SetBool("IsShown", true);
    }

    public void Hide()
    {
        campAnimator.SetBool("IsShown", false);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void Initialize()
    {
        for (int i = 0; i < mealSlots.Count; i++)
        {
            mealSlots[i].FoodRank = i;
            mealSlots[i].SetFoodAmount(2*i);
            mealSlots[i].EventMealSelected += MealSelected;
        }
    }

    public void LoadCampingMeal()
    {
        MealResult = CampMealResultType.Wait;

        int baseMeal = RaidSceneManager.HeroParty.Units.Sum(unit => Mathf.RoundToInt(1 + unit.Character.FoodConsumption));
        mealSlots[1].SetFoodAmount(Mathf.Clamp(baseMeal / 2, 1, 12));
        mealSlots[1].SetAvailability();
        mealSlots[2].SetFoodAmount(Mathf.Clamp(baseMeal, 2, 12));
        mealSlots[2].SetAvailability();
        mealSlots[3].SetFoodAmount(Mathf.Clamp(baseMeal * 2, 3, 12));
        mealSlots[3].SetAvailability();

        SetActive(true);
    }

    public void MealSelected(MealSlot selectedSlot)
    {
        if(MealResult == CampMealResultType.Wait)
        {
            MealResult = CampMealResultType.Eat;
            SelectedMealSlot = selectedSlot;
        }
    }
}
