using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void EquipmentUpgradeSlotEvent(EquipmentUpgradeSlot slot);

public class EquipmentUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    RectTransform rectTransform;

    public BuildCostFrame costFrame;

    public Image background;
    public Image icon;

    public UpgradeTree Tree { get; set; }
    public HeroUpgrade Upgrade { get; set; }
    public Hero Hero { get; set; }
    public Equipment Equipment { get; set; }

    public event EquipmentUpgradeSlotEvent onClick;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Hero hero, UpgradeTree tree, HeroUpgrade upgrade, Equipment equipment)
    {
        Hero = hero;
        Tree = tree;
        Upgrade = upgrade;
        Equipment = equipment;
    }

    public void Reset()
    {
        Tree = null;
        Upgrade = null;
        Hero = null;

        Equipment = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Hero == null)
            return;

        icon.material = DarkestDungeonManager.HighlightMaterial;
        if(DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(Tree.Id, Hero, Upgrade) == UpgradeStatus.Locked)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over_2");
            ToolTipManager.Instanse.Show(Equipment.Tooltip + "\n" +
                Upgrade.PrerequisitesTooltip(Hero, DarkestDungeonManager.Campaign.Estate),
                eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        }
        else
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
            ToolTipManager.Instanse.Show(Equipment.Tooltip, eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        icon.material = icon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(this);
    }
}