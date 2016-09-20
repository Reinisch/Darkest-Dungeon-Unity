using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public delegate void WagonSlotEvent(WagonSlot slot);

public class WagonSlot : BaseSlot, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public Image rarityIcon;
    public Text costText;

    public Trinket Trinket { get; set; }
    public int Cost { get; set; }

    public event WagonSlotEvent onSlotPurchase;

    bool hasGeneratedTooltip = false;

    public void SetTrinket(Trinket newTrinket, float discount)
    {
        Trinket = newTrinket;
        itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_trinket+" + Trinket.Id];
        rarityIcon.sprite = rarityIcon.sprite = DarkestDungeonManager.Data.Sprites["rarity_" + newTrinket.Rarity];
        Cost = Mathf.RoundToInt(Trinket.PurchasePrice * (1 - discount));
        costText.text = Cost.ToString();
    }
    public void EmptySlot()
    {
        Trinket = null;
        OnPointerExit(null);
        gameObject.SetActive(false);
    }
    public void UpdatePrice(float discount)
    {
        if (Trinket != null)
        {
            Cost = Mathf.RoundToInt(Trinket.PurchasePrice * (1 - discount));
            costText.text = Cost.ToString();
        }
        else
        {
            Cost = 0;
            costText.text = "0";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rarityIcon.material = DarkestDungeonManager.HighlightMaterial;
        itemIcon.material = DarkestDungeonManager.HighlightMaterial;
        if (Trinket != null)
        {
            ToolTipManager.Instanse.Show(Trinket.ToolTip(), eventData, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
            hasGeneratedTooltip = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hasGeneratedTooltip)
        {
            rarityIcon.material = rarityIcon.defaultMaterial;
            itemIcon.material = itemIcon.defaultMaterial;
            ToolTipManager.Instanse.Hide();
            hasGeneratedTooltip = false;
        }
    }

    public void SlotClicked()
    {
        if (onSlotPurchase != null)
            onSlotPurchase(this);
    }
}
