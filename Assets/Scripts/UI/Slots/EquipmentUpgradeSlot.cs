using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private BuildCostFrame costFrame;
    [SerializeField]
    private Image background;
    [SerializeField]
    private Image icon;

    public BuildCostFrame CostFrame { get { return costFrame; } }
    public Image Background { get { return background; } }
    public Image Icon { get { return icon; } }
    public UpgradeTree Tree { get; private set; }
    public HeroUpgrade Upgrade { get; private set; }
    public Hero Hero { get; private set; }

    private Equipment Equipment { get; set; }

    private RectTransform rectTransform;

    public event Action<EquipmentUpgradeSlot> EventClicked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Reset()
    {
        Tree = null;
        Upgrade = null;
        Hero = null;

        Equipment = null;
    }

    public void Initialize(Hero hero, UpgradeTree tree, HeroUpgrade upgrade, Equipment equipment)
    {
        Hero = hero;
        Tree = tree;
        Upgrade = upgrade;
        Equipment = equipment;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Hero == null)
            return;

        Icon.material = DarkestDungeonManager.HighlightMaterial;
        if(DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(Tree.Id, Hero, Upgrade) == UpgradeStatus.Locked)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over_2");
            ToolTipManager.Instanse.Show(Equipment.Tooltip + "\n" +
                Upgrade.PrerequisitesTooltip(Hero, DarkestDungeonManager.Campaign.Estate), rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        }
        else
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
            ToolTipManager.Instanse.Show(Equipment.Tooltip, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Icon.material = Icon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventClicked != null)
            EventClicked(this);
    }
}