using System.Collections.Generic;
using System.Text;

public class Quirk
{
    public string Id { get; set; }
    public string Classification { get; set; }

    public bool ShowExplicitDescription { get; set; }
    public bool IsPositive { get; set; }
    public bool IsDisease { get; set; }
    public bool KeepLoot { get; set; }

    public string CurioTag { get; set; }
    public float CurioTagChance { get; set; }

    public List<string> IncompatibleQuirks { get; set; }
    public List<Buff> Buffs { get; set; }

    public Quirk()
    {
        IncompatibleQuirks = new List<string>();
        Buffs = new List<Buff>();
    }

    public string ToolTip()
    {
        StringBuilder sb = ToolTipManager.TipBody;
        if (ShowExplicitDescription)
        {
            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            string buffTooltip = "";
            for (int i = 0; i < Buffs.Count; i++)
            {
                string newBuffTip = Buffs[i].ToolTip;

                if(newBuffTip.Length > 0)
                {
                    if (buffTooltip.Length > 0)
                        buffTooltip += ToolTipManager.GetConcat("\n", newBuffTip);
                    else
                        buffTooltip += newBuffTip;
                }
            }
            sb.Append(buffTooltip);
            sb.Append("</color>");
        }
        else
        {
            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            sb.Append(LocalizationManager.GetString(ToolTipManager.GetConcat("str_quirk_description_", Id)));
            sb.Append("</color>");
        }      
        return sb.ToString();
    }
}