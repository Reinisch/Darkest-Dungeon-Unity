using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;

public class BuildingUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
    public UpgradeTree Tree { get; set; }
    public TownUpgrade UpgradeInfo { get; set; }
    public List<ITownUpgrade> TownUpgrades { private get; set; }

    private RectTransform rectTransform;

    public event Action<BuildingUpgradeSlot> EventClicked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Icon.material = DarkestDungeonManager.HighlightMaterial;
        StringBuilder sb = ToolTipManager.TipBody;

        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString("upgrade_tree_name_" + Tree.Id));
        sb.Append("</color>");

        string toolTip;
        switch(Tree.Id)
        {
            case "sanitarium.cost":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_tree_tooltip_description_reduces_positive_quirk_treatment_cost_format"), 10);
                sb.AppendLine();
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_tree_tooltip_description_reduces_negative_quirk_treatment_cost_format"), 10);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            case "blacksmith.weapon":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_tree_tooltip_description_allows_weapon_upgrades_of_rank_format"),
                    UpgradeInfo.Code == "a"? 2 : UpgradeInfo.Code == "b"? 3 : UpgradeInfo.Code == "c"? 4 : 5);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            case "blacksmith.armour":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_tree_tooltip_description_allows_armour_upgrades_of_rank_format"),
                    UpgradeInfo.Code == "a"? 2 : UpgradeInfo.Code == "b"? 3 : UpgradeInfo.Code == "c"? 4 : 5);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            case "guild.skill_levels":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_tree_tooltip_description_allows_combat_skill_upgrades_of_rank_format"),
                    UpgradeInfo.Code == "a" ? 2 : UpgradeInfo.Code == "b" ? 3 : UpgradeInfo.Code == "c" ? 4 : 5);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            default:       
                toolTip = sb.ToString();
                for (int i = 0; i < TownUpgrades.Count; i++ )
                    toolTip += "\n" + TownUpgrades[i].ToolTip;
                break;
        }

        if (DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(Tree.Id, UpgradeInfo) == UpgradeStatus.Locked)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over_2");
            ToolTipManager.Instanse.Show(toolTip + "\n" + UpgradeInfo.PrerequisitesTooltip(), rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        }
        else
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
            ToolTipManager.Instanse.Show(toolTip, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
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