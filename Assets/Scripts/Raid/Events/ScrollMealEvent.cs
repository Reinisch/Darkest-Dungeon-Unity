using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum CampMealResultType { Wait, Eat }

public class ScrollMealEvent : MonoBehaviour
{
    public Text title;
    public Animator campAnimator;
    public Button eatButton;
    public Button starveButton;

    public List<MealSlot> mealSlots;

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

    public void Initialize()
    {
        for (int i = 0; i < mealSlots.Count; i++)
        {
            mealSlots[i].FoodRank = i;
            mealSlots[i].SetFoodAmount(2*i);
            mealSlots[i].onMealSelect += MealSelected;
        }
    }
    public void MealSelected(MealSlot selectedSlot)
    {
        if(MealResult == CampMealResultType.Wait)
        {
            MealResult = CampMealResultType.Eat;
            SelectedMealSlot = selectedSlot;
        }
    }
    public void LoadCampingMeal()
    {
        MealResult = CampMealResultType.Wait;

        int baseMeal = 0;
        for(int i = 0; i < RaidSceneManager.HeroParty.Units.Count; i++)
            baseMeal += Mathf.RoundToInt(1 + RaidSceneManager.HeroParty.Units[i].Character.FoodConsumption);

        mealSlots[1].SetFoodAmount(Mathf.Clamp(baseMeal / 2, 1, 12));
        mealSlots[1].SetAvailability();
        mealSlots[2].SetFoodAmount(Mathf.Clamp(baseMeal, 2, 12));
        mealSlots[2].SetAvailability();
        mealSlots[3].SetFoodAmount(Mathf.Clamp(baseMeal * 2, 3, 12));
        mealSlots[3].SetAvailability();

        ScrollOpened();
    }
    public void ScrollOpened()
    {
        gameObject.SetActive(true);
    }
    public void ScrollClosed()
    {
        gameObject.SetActive(false);
    }
}
