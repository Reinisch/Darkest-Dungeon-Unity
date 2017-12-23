using UnityEngine;
using UnityEngine.UI;

public enum HungerResultType { Wait, Eat, Starve }

public class ScrollHungerEvent : MonoBehaviour
{
    [SerializeField]
    private QuickParameterTip eatTip;
    [SerializeField]
    private QuickParameterTip starveTip;
    [SerializeField]
    private Button eatButton;

    public HungerResultType ActionType { get; private set; }
    public int MealAmount { get; private set; }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ToolTipManager.Instanse.Hide();
    }

    public void LoadHungerEventMeal()
    {
        ActionType = HungerResultType.Wait;

        int baseMeal = 0;
        for (int i = 0; i < RaidSceneManager.HeroParty.Units.Count; i++)
            baseMeal += Mathf.RoundToInt(1 + RaidSceneManager.HeroParty.Units[i].Character.FoodConsumption);

        MealAmount = Mathf.Clamp(baseMeal, 1, 12);
        if (RaidSceneManager.Inventory.ContainsEnoughItems("provision", MealAmount))
            eatButton.interactable = true;
        else
            eatButton.interactable = false;

        eatTip.SetParams(MealAmount, 0.05f);
        starveTip.SetParams(0.2f);
    }

    public void EatSelected()
    {
        if (ActionType == HungerResultType.Wait)
        {
            ActionType = HungerResultType.Eat;
        }
    }

    public void StarveSelected()
    {
        if (ActionType == HungerResultType.Wait)
        {
            ActionType = HungerResultType.Starve;
        }
    }
}
