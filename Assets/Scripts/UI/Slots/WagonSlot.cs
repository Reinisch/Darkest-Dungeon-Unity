using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WagonSlot : BaseSlot, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image itemIcon;
    [SerializeField]
    private Image rarityIcon;
    [SerializeField]
    private Text costText;

    public Trinket Trinket { get; private set; }
    public int Cost { get; private set; }

    private bool hasGeneratedTooltip;

    public event Action<WagonSlot> EventSlotPurchase;

    public void SetTrinket(Trinket newTrinket, float discount)
    {
        Trinket = newTrinket;
        itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_trinket+" + Trinket.Id];
        rarityIcon.sprite = rarityIcon.sprite = DarkestDungeonManager.Data.Sprites["rarity_" + newTrinket.RarityId];
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
            ToolTipManager.Instanse.Show(Trinket.ToolTip(), RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
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
        if (EventSlotPurchase != null)
            EventSlotPurchase(this);
    }
}
