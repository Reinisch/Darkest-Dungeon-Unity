using System.Text;

public class ItemData
{
    private int stackLimit;

    public string Type { get; set; }
    public string Id { get; set; }
    public int PurchasePrice { get; set; }
    public int SellPrice { get; set; }
    public int ExtraStackLimit { get; set; }
    public int StackLimit
    {
        get
        {
            return stackLimit + ExtraStackLimit;
        }
        set
        {
            stackLimit = value;
        }
    }

    public ItemData()
    {
    }

    public ItemData(string type, string id, int limit, int goldValue)
    {
        Type = type;
        Id = id;
        StackLimit = limit;
        PurchasePrice = goldValue;
    }

    public virtual string ToolTip(bool discard = false)
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_title"]);
        sb.Append(LocalizationManager.GetString(LocalizationManager.GetString("str_inventory_title_" +
            Type + (Type == "journal_page" ? "" : Id))));
        sb.AppendFormat("</color><color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
        sb.Append("\n" + LocalizationManager.GetString("str_inventory_description_" + Type + (Type == "journal_page" ? "" : Id)));
        sb.Append("</color>");
        if (SellPrice != 0)
        {
            sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["inventory_tooltip_gold_value"]);
            sb.AppendFormat(LocalizationManager.GetString("str_inventory_gold_value_format"), SellPrice);
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

    public virtual string ToolTip(int amount, bool discard = false)
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_title"]);
        sb.Append(LocalizationManager.GetString(LocalizationManager.GetString("str_inventory_title_" +
            Type + (Type == "journal_page" ? "" : Id))));
        sb.AppendFormat("</color><color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
        sb.Append("\n" + LocalizationManager.GetString("str_inventory_description_" + Type + (Type == "journal_page" ? "" : Id)));
        sb.Append("</color>");
        if (SellPrice != 0)
        {
            sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["inventory_tooltip_gold_value"]);
            sb.AppendFormat(LocalizationManager.GetString("str_inventory_gold_value_format"), SellPrice);
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
}