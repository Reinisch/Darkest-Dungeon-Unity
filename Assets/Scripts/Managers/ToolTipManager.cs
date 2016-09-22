using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public enum ToolTipStyle { FromBottom, FromRight, FromTop }
public enum ToolTipSize { Small, Normal }

public class ToolTipManager : MonoBehaviour
{
    const float rightOffset = -5f;
    const int bodyCapacity = 500;
    const int subCapacity = 200;
    const int helpCapacity = 200;

    static StringBuilder tipBody;
    static StringBuilder subTipBody;
    static StringBuilder helpBody;

    public static ToolTipManager Instanse { get; private set; }

    public static string GetConcat(string body, params string[] appends)
    {
        helpBody.Length = 0;
        helpBody.Append(body);

        for (int i = 0; i < appends.Length; i++)
            helpBody.Append(appends[i]);

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

    public ToolTip toolTip;
    public SkillTooltip skillToolTip;

    public RectTransform OverlayRect { get; set; }
    public Camera OverlayCamera { get; set; }
    public ToolTip CurrentTooltip { get; set; }

    void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;

            tipBody = new StringBuilder(bodyCapacity);
            subTipBody = new StringBuilder(subCapacity);
            helpBody = new StringBuilder(helpCapacity);
        }
    }
    void Start()
    {
        if (Instanse.gameObject != gameObject)
            return;

        toolTip.TipDelay = 0.05f;
        toolTip.Initialize();
        skillToolTip.TipDelay = 0.1f;
        skillToolTip.Initialize();
    }
    void UpdateToolTip(ToolTip currentTooltip, PointerEventData eventData, RectTransform senderRect, ToolTipStyle style)
    {
        if (currentTooltip.enabled)
        {
            switch (style)
            {
                case ToolTipStyle.FromRight:
                    Vector3[] corners = new Vector3[4];
                    senderRect.GetWorldCorners(corners);
                    currentTooltip.RectTransform.pivot = new Vector2(0, 1);
                    currentTooltip.layoutGroup.childAlignment = TextAnchor.UpperLeft;

                    currentTooltip.RectTransform.position = corners[2];
                    currentTooltip.RectTransform.anchoredPosition += new Vector2(-5, 13);
                    break;
                case ToolTipStyle.FromBottom:
                    Vector3[] cornersBot = new Vector3[4];

                    senderRect.GetWorldCorners(cornersBot);
                    currentTooltip.RectTransform.pivot = new Vector2(0.5f, 1);
                    currentTooltip.layoutGroup.childAlignment = TextAnchor.UpperCenter;
                    currentTooltip.RectTransform.position = (cornersBot[0] + cornersBot[3]) / 2;

                    currentTooltip.RectTransform.anchoredPosition += new Vector2(0, 8);
                    break;
                case ToolTipStyle.FromTop:
                    Vector3[] cornersTop = new Vector3[4];
                    senderRect.GetWorldCorners(cornersTop);
                    currentTooltip.RectTransform.pivot = new Vector2(0.5f, 0);
                    currentTooltip.layoutGroup.childAlignment = TextAnchor.LowerCenter;
                    currentTooltip.RectTransform.position = (cornersTop[1] + cornersTop[2]) / 2;
                    currentTooltip.RectTransform.anchoredPosition += new Vector2(0, -8);
                    break;
            }

        }
    }

    public void ShowSkillTooltip(string tip, CombatSkill combatSkill,
        PointerEventData eventData, RectTransform senderRect, ToolTipStyle style, ToolTipSize size)
    {
        if (CurrentTooltip == null)
            CurrentTooltip = skillToolTip;
        else if (CurrentTooltip != skillToolTip)
        {
            CurrentTooltip.Hide();
            CurrentTooltip = skillToolTip;
        }

        UpdateToolTip(skillToolTip, eventData, senderRect, style);
        skillToolTip.UpdateSkillRanks(combatSkill);
        skillToolTip.CurrentTip = tip;
        skillToolTip.UpdateSize(size);
        skillToolTip.Show(senderRect);
    }
    public void ShowSkillTooltip(Hero hero, CombatSkill combatSkill,
        PointerEventData eventData, RectTransform senderRect, ToolTipStyle style, ToolTipSize size)
    {
        if (CurrentTooltip == null)
            CurrentTooltip = skillToolTip;
        else if(CurrentTooltip != skillToolTip)
        {
            CurrentTooltip.Hide();
            CurrentTooltip = skillToolTip;
        }

        skillToolTip.UpdateSize(size);
        UpdateToolTip(skillToolTip, eventData, senderRect, style);

        skillToolTip.UpdateSkillRanks(combatSkill);
        skillToolTip.CurrentTip = combatSkill.HeroSkillTooltip(hero);
        skillToolTip.Show(senderRect);
    }
    public void Show(string text, PointerEventData eventData, RectTransform senderRect, ToolTipStyle style, ToolTipSize size)
    {
        if (CurrentTooltip == null)
            CurrentTooltip = toolTip;
        else if (CurrentTooltip != toolTip)
        {
            CurrentTooltip.Hide();
            CurrentTooltip = toolTip;
        }

        toolTip.CurrentTip = text;
        UpdateToolTip(toolTip, eventData, senderRect, style);
        toolTip.UpdateSize(size);
        toolTip.Show(senderRect);
    }
    public void Hide()
    {
        if (CurrentTooltip != null)
            CurrentTooltip.Hide();
        CurrentTooltip = null;
    }
}