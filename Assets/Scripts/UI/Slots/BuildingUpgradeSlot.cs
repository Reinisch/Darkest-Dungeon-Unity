using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public delegate void UpgradeSlotEvent(BuildingUpgradeSlot slot);

public class BuildingUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    RectTransform rectTransform;

    public BuildCostFrame costFrame;

    public Image background;
    public Image icon;

    public UpgradeTree Tree { get; set; }
    public TownUpgrade UpgradeInfo { get; set; }
    public List<ITownUpgrade> TownUpgrades { get; set; }

    public event UpgradeSlotEvent onClick;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        icon.material = DarkestDungeonManager.HighlightMaterial;
        StringBuilder sb = ToolTipManager.TipBody;

        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString("upgrade_tree_name_" + Tree.Id));
        sb.Append("</color>");

        string toolTip = "";
        switch(Tree.Id)
        {
            case "sanitarium.cost":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString("upgrade_tree_tooltip_description_reduces_positive_quirk_treatment_cost_format"), 10);
                sb.AppendLine();
                sb.AppendFormat(LocalizationManager.GetString("upgrade_tree_tooltip_description_reduces_negative_quirk_treatment_cost_format"), 10);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            case "blacksmith.weapon":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString("upgrade_tree_tooltip_description_allows_weapon_upgrades_of_rank_format"),
                    UpgradeInfo.Code == "a"? 2 : UpgradeInfo.Code == "b"? 3 : UpgradeInfo.Code == "c"? 4 : 5);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            case "blacksmith.armour":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString("upgrade_tree_tooltip_description_allows_armour_upgrades_of_rank_format"),
                    UpgradeInfo.Code == "a"? 2 : UpgradeInfo.Code == "b"? 3 : UpgradeInfo.Code == "c"? 4 : 5);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            case "guild.skill_levels":
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
                sb.AppendFormat(LocalizationManager.GetString("upgrade_tree_tooltip_description_allows_combat_skill_upgrades_of_rank_format"),
                    UpgradeInfo.Code == "a" ? 2 : UpgradeInfo.Code == "b" ? 3 : UpgradeInfo.Code == "c" ? 4 : 5);
                sb.Append("</color>");
                toolTip = sb.ToString();
                break;
            default:       
                toolTip = sb.ToString();
                for (int i = 0; i < TownUpgrades.Count; i++ )
                {
                        toolTip += "\n" + TownUpgrades[i].ToolTip;
                }
                break;
        }

        if (DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(Tree.Id, UpgradeInfo) == UpgradeStatus.Locked)
            ToolTipManager.Instanse.Show(toolTip + "\n" + UpgradeInfo.PrerequisitesTooltip(), eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        else
            ToolTipManager.Instanse.Show(toolTip, eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        
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