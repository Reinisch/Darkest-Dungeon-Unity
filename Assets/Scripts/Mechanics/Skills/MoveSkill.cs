using System.Text;

public class MoveSkill : Skill
{
    public int MoveForward { get; set; }
    public int MoveBackward { get; set; }

    public MoveSkill(string id, int backward, int forward)
    {
        Id = id;
        MoveBackward = backward;
        MoveForward = forward;
    }

    public string HeroSkillTooltip(Hero hero)
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString("combat_skill_name_" + hero.ClassStringId + "_move"));
        sb.AppendFormat("</color><color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);

        if (MoveForward != 0)
            sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_forward"), MoveForward);
        if (MoveBackward != 0)
            sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_back"), MoveBackward);

        sb.Append("</color>");
        return sb.ToString();
    }
}