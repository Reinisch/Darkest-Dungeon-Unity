using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : BaseSlot, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image equipFrame;

    private Equipment Equipment { get; set; }

    public void UpdateEquipment(Equipment newEquipment , Hero hero)
    {
        Equipment = newEquipment;
        equipFrame.sprite = DarkestDungeonManager.HeroSprites[hero.ClassStringId].Equip[Equipment.Name];
    }

    public void ResetEquipment()
    {
        Equipment = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(Equipment != null)
            ToolTipManager.Instanse.Show(Equipment.Tooltip, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(Equipment != null)
            ToolTipManager.Instanse.Hide();
    }
}
