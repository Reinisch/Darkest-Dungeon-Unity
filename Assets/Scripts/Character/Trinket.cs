using System.Collections.Generic;
using System.Text;

public class Trinket : ItemData
{
    public string OriginDungeon { get; set; }
    public int EquipLimit { get; set; }
    public List<string> ClassRequirements { get; set; }

    public Rarity Rarity { get; private set; }
    public List<Buff> Buffs { get; private set; }
    
    public string RarityId
    {
        get
        {
            return rarityId;
        }
        set
        {
            rarityId = value;
            Rarity = CharacterHelper.StringToRarity(value);
        }
    }

    private string rarityId;

    public Trinket():base("trinket", "", 1, 0)
    {
        Buffs = new List<Buff>();
        ClassRequirements = new List<string>();
    }

    public override string ToolTip(bool discard = false)
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_title"]);
        if(RarityId == "kickstarter")
            sb.Append(LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", Id)));
        else
            sb.Append(LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", Id)));
        sb.AppendFormat("</color>\n<color={0}>", DarkestDungeonManager.Data.HexColors[RarityId]);
        sb.Append(LocalizationManager.GetString(ToolTipManager.GetConcat("trinket_rarity_", RarityId)));
        sb.Append("</color>");
        #region Requirements
        if (ClassRequirements.Count > 0)
        {
            sb.AppendLine();
            string trinketRequirementFormat = LocalizationManager.GetString("trinket_hero_class_requirement_format");
            string requiredHeroes = "";

            for (int i = 0; i < ClassRequirements.Count; i++)
            {
                if (i == 0)
                    requiredHeroes += LocalizationManager.GetString("hero_class_name_" + ClassRequirements[i]);
                else
                    requiredHeroes += " " + LocalizationManager.GetString("trinket_hero_class_requirement_seperator")
                        + " " + LocalizationManager.GetString("hero_class_name_" + ClassRequirements[i]);
            }
            sb.AppendFormat(trinketRequirementFormat, requiredHeroes);
        }
        #endregion
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
        for (int i = 0; i < Buffs.Count; i++)
        {
            if (Buffs[i].AttributeType == AttributeType.DamageLow)
                continue;
            sb.Append(ToolTipManager.GetConcat("\n", Buffs[i].ToolTip));
        }
        sb.Append("</color>");
        if (PurchasePrice != 0)
        {
            sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["inventory_tooltip_gold_value"]);
            sb.AppendFormat(LocalizationManager.GetString("str_inventory_gold_value_format"), PurchasePrice);
            sb.Append("</color>");
        }
        if (discard)
        {
            sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
            sb.Append(LocalizationManager.GetString("str_discard_item_instructions"));
            sb.Append("</color>");
        }
        return sb.ToString();
    }

    public override string ToolTip(int amount, bool discard = false)
    {
        return ToolTip(discard);
    }
}