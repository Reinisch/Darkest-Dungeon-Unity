using UnityEngine;
using System.Text;

public enum ToolTipStyle
{
    FromBottom,
    FromRight,
    FromTop,
    FromLeft,
    FromTopLeft
}

public enum ToolTipSize
{
    Small,
    Normal
}

public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager Instanse { get; private set; }

    private const int BodyCapacity = 500;
    private const int SubCapacity = 200;
    private const int HelpCapacity = 200;

    private static StringBuilder tipBody;
    private static StringBuilder subTipBody;
    private static StringBuilder helpBody;

    public static string GetConcat(string body, params string[] appends)
    {
        helpBody.Length = 0;
        helpBody.Append(body);

        foreach (string line in appends)
            helpBody.Append(line);

        return helpBody.ToString();
    }

    public static StringBuilder TipBody
    {
        get
        {
            tipBody.Length = 0;
            return tipBody;
        }
    }

    public static StringBuilder SubTipBody
    {
        get
        {
            subTipBody.Length = 0;
            return subTipBody;
        }
    }

    [SerializeField]
    private ToolTip toolTip;
    [SerializeField]
    private SkillTooltip skillToolTip;

    public ToolTip ToolTip { get { return toolTip; } }
    public ToolTip CurrentTooltip { get; private set; }

    private void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;

            tipBody = new StringBuilder(BodyCapacity);
            subTipBody = new StringBuilder(SubCapacity);
            helpBody = new StringBuilder(HelpCapacity);
        }
    }

    private void Start()
    {
        if (Instanse.gameObject != gameObject)
            return;

        toolTip.TipDelay = 0.05f;
        toolTip.Initialize();
        skillToolTip.TipDelay = 0.1f;
        skillToolTip.Initialize();
    }

    public void ShowSkillTooltip(string tip, CombatSkill combatSkill, RectTransform senderRect, ToolTipStyle style, ToolTipSize size)
    {
        if (CurrentTooltip == null)
            CurrentTooltip = skillToolTip;
        else if (CurrentTooltip != skillToolTip)
        {
            CurrentTooltip.Hide();
            CurrentTooltip = skillToolTip;
        }

        UpdateToolTip(skillToolTip, senderRect, style);
        skillToolTip.UpdateSkillRanks(combatSkill);
        skillToolTip.CurrentTip = tip;
        skillToolTip.UpdateSize(size);
        skillToolTip.Show(senderRect);
    }

    public void ShowSkillTooltip(Hero hero, CombatSkill combatSkill, RectTransform senderRect, ToolTipStyle style, ToolTipSize size)
    {
        if (CurrentTooltip == null)
            CurrentTooltip = skillToolTip;
        else if(CurrentTooltip != skillToolTip)
        {
            CurrentTooltip.Hide();
            CurrentTooltip = skillToolTip;
        }

        skillToolTip.UpdateSize(size);
        UpdateToolTip(skillToolTip, senderRect, style);

        skillToolTip.UpdateSkillRanks(combatSkill);
        skillToolTip.CurrentTip = combatSkill.HeroSkillTooltip(hero);
        skillToolTip.Show(senderRect);
    }

    public void Show(string text, RectTransform senderRect, ToolTipStyle style, ToolTipSize size)
    {
        if (CurrentTooltip == null)
            CurrentTooltip = toolTip;
        else if (CurrentTooltip != toolTip)
        {
            CurrentTooltip.Hide();
            CurrentTooltip = toolTip;
        }

        toolTip.CurrentTip = text;
        UpdateToolTip(toolTip, senderRect, style);
        toolTip.UpdateSize(size);
        toolTip.Show(senderRect);
    }

    public void Hide()
    {
        if (CurrentTooltip != null)
            CurrentTooltip.Hide();
        CurrentTooltip = null;
    }

    private void UpdateToolTip(ToolTip currentTooltip, RectTransform senderRect, ToolTipStyle style)
    {
        if (!currentTooltip.enabled)
            return;

        switch (style)
        {
            case ToolTipStyle.FromRight:
                Vector3[] corners = new Vector3[4];
                senderRect.GetWorldCorners(corners);
                currentTooltip.RectTransform.pivot = new Vector2(0, 1);
                currentTooltip.LayoutGroup.childAlignment = TextAnchor.UpperLeft;

                currentTooltip.RectTransform.position = corners[2];
                currentTooltip.RectTransform.anchoredPosition += new Vector2(-5, 13);
                break;
            case ToolTipStyle.FromBottom:
                Vector3[] cornersBot = new Vector3[4];

                senderRect.GetWorldCorners(cornersBot);
                currentTooltip.RectTransform.pivot = new Vector2(0.5f, 1);
                currentTooltip.LayoutGroup.childAlignment = TextAnchor.UpperCenter;
                currentTooltip.RectTransform.position = (cornersBot[0] + cornersBot[3]) / 2;

                currentTooltip.RectTransform.anchoredPosition += new Vector2(0, 8);
                break;
            case ToolTipStyle.FromTop:
                Vector3[] cornersTop = new Vector3[4];
                senderRect.GetWorldCorners(cornersTop);
                currentTooltip.RectTransform.pivot = new Vector2(0.5f, 0);
                currentTooltip.LayoutGroup.childAlignment = TextAnchor.LowerCenter;
                currentTooltip.RectTransform.position = (cornersTop[1] + cornersTop[2]) / 2;
                currentTooltip.RectTransform.anchoredPosition += new Vector2(0, -8);
                break;
            case ToolTipStyle.FromLeft:
                Vector3[] cornersLeft = new Vector3[4];
                senderRect.GetWorldCorners(cornersLeft);
                currentTooltip.RectTransform.pivot = new Vector2(1, 1);
                currentTooltip.LayoutGroup.childAlignment = TextAnchor.UpperRight;

                currentTooltip.RectTransform.position = cornersLeft[1];
                currentTooltip.RectTransform.anchoredPosition += new Vector2(5, 13);
                break;
            case ToolTipStyle.FromTopLeft:
                Vector3[] cornersTopLeft = new Vector3[4];
                senderRect.GetWorldCorners(cornersTopLeft);
                currentTooltip.RectTransform.pivot = new Vector2(1, 0);
                currentTooltip.LayoutGroup.childAlignment = TextAnchor.LowerRight;

                currentTooltip.RectTransform.position = cornersTopLeft[1];
                currentTooltip.RectTransform.anchoredPosition += new Vector2(12, -10);
                break;
        }
    }
}