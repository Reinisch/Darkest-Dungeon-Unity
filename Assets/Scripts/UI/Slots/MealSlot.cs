using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public delegate void MealSlotEvent(MealSlot slot);

public class MealSlot : BaseSlot, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image itemIcon;
    public Text amountText;

    public int FoodRank { get; set; }
    public int Amount { get; set; }
    public bool Deactivated { get; set; }
    public bool Highlighted { get; set; }

    public event MealSlotEvent onMealSelect;

    public void SetFoodAmount(int amount)
    {
        Amount = amount;
        amountText.text = amount.ToString();
    }
    public void SetAvailability()
    {
        Highlighted = false;
        if (RaidSceneManager.Inventory.ContainsEnoughItems("provision", Amount))
            SetActive(true);
        else
            SetActive(false);
    }
    public void SetActive(bool active)
    {
        Deactivated = !active;

        if (active)
        {
            if (Highlighted)
            {
                itemIcon.material = DarkestDungeonManager.HighlightMaterial;
                amountText.material = DarkestDungeonManager.HighlightMaterial;
            }
            else
            {
                itemIcon.material = itemIcon.defaultMaterial;
                amountText.material = amountText.defaultMaterial;
            }
        }
        else
        {
            if (Highlighted)
            {
                itemIcon.material = DarkestDungeonManager.DeactivatedHighlightedMaterial;
                amountText.material = DarkestDungeonManager.DeactivatedHighlightedMaterial;
            }
            else
            {
                itemIcon.material = DarkestDungeonManager.DeactivatedMaterial;
                amountText.material = DarkestDungeonManager.DeactivatedMaterial;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        SetActive(!Deactivated);

        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_title"]);
        sb.Append(LocalizationManager.GetString("str_meal_title_" + FoodRank));
        sb.AppendFormat("</color>");
        if (FoodRank == 2)
        {
            sb.AppendLine();
            sb.AppendFormat(LocalizationManager.GetString("str_meal_heal_format"), 0.1f);
        }
        else if (FoodRank == 3)
        {
            sb.AppendLine();
            sb.AppendFormat(LocalizationManager.GetString("str_meal_heal_format"), 0.25f);
        }
        else if (FoodRank == 0)
        {
            sb.AppendLine();
            sb.AppendFormat(LocalizationManager.GetString("str_meal_heal_format"), -0.2f);
        }

        if (FoodRank == 3)
        {
            sb.AppendLine();
            sb.AppendFormat(LocalizationManager.GetString("str_meal_stress_format"), -10);
        }
        else if (FoodRank == 0)
        {
            sb.AppendLine();
            sb.AppendFormat(LocalizationManager.GetString("str_meal_stress_format"), 15);
        }

        ToolTipManager.Instanse.Show(sb.ToString(), eventData, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        SetActive(!Deactivated);
        ToolTipManager.Instanse.Hide();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Deactivated && onMealSelect != null)
        {
            onMealSelect(this);
            if (Highlighted)
                OnPointerExit(eventData);
        }
    }
}