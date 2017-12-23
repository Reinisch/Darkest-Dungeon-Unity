using System.Collections.Generic;
using System.Text;

public class Equipment
{
    public string Name { get; private set; }
    public int UpgradeLevel { get; private set; }
    public List<FlatModifier> EquipmentModifiers { get; private set; }

    private HeroEquipmentSlot Slot { get; set; }

    public Equipment(string name, int level, HeroEquipmentSlot slot)
    {
        Name = name;
        UpgradeLevel = level;
        Slot = slot;
        EquipmentModifiers = new List<FlatModifier>();
    }

    public void ApplyModifiers(Character character)
    {
        foreach (FlatModifier modifier in EquipmentModifiers)
            modifier.ApplyModifier(character);
    }

    public void RevertModifiers(Character character)
    {
        foreach (FlatModifier modifier in EquipmentModifiers)
            modifier.RevertModifier(character);
    }

    public string Tooltip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;
            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_level_" + (UpgradeLevel - 1).ToString()]);
            sb.Append(LocalizationManager.GetString(Name));
            sb.AppendFormat("</color>\n<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            for (int i = 0; i < EquipmentModifiers.Count; i++)
            {
                if(i == 0)
                {
                    if(Slot == HeroEquipmentSlot.Weapon)
                    {
                        sb.Append(LocalizationManager.GetString("str_stat_base_damage") + ": " +
                            EquipmentModifiers[0].ModifierValue + "-" + EquipmentModifiers[1].ModifierValue);
                        i++;
                    }
                    else
                        sb.Append(EquipmentModifiers[i].Tooltip);
                }
                else
                    sb.Append("\n" + EquipmentModifiers[i].Tooltip);
            }
            sb.Append("</color>");
            return sb.ToString();
        }
    }
}