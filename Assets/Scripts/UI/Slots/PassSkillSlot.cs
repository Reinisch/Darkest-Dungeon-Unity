using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text;

public delegate void PassSkillSlotEvent();

public class PassSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon;

    public RectTransform RectTransform { get; private set; }

    public bool Available { get; set; }
    public bool Highlighted { get; set; }

    public event PassSkillSlotEvent onPassPressed;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void SetCombatState()
    {
        skillIcon.enabled = true;
        skillIcon.material = skillIcon.defaultMaterial;

        Available = true;
    }
    public void SetDisabledState()
    {
        skillIcon.enabled = true;
        skillIcon.material = DarkestDungeonManager.FullGrayDarkMaterial;

        Available = false;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (Available)
            if (onPassPressed != null)
                onPassPressed();
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        if (!Available)
            skillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            skillIcon.material = DarkestDungeonManager.HighlightMaterial;

        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
        sb.Append(LocalizationManager.GetString("pass_ability_description"));
        sb.AppendFormat("</color>");
        
        ToolTipManager.Instanse.Show(sb.ToString(), eventData, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (!Available)
            skillIcon.material = DarkestDungeonManager.FullGrayDarkMaterial;
        else
            skillIcon.material = skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
