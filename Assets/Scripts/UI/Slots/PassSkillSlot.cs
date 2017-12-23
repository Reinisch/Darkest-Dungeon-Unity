using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class PassSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image skillIcon;

    private RectTransform RectTransform { get; set; }
    private bool Available { get; set; }

    public event Action EventPassPressed;

    private void Awake()
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Available)
            if (EventPassPressed != null)
                EventPassPressed();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        skillIcon.material = !Available ? DarkestDungeonManager.GrayMaterial : DarkestDungeonManager.HighlightMaterial;

        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
        sb.Append(LocalizationManager.GetString("pass_ability_description"));
        sb.AppendFormat("</color>");
        
        ToolTipManager.Instanse.Show(sb.ToString(), RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillIcon.material = !Available ? DarkestDungeonManager.FullGrayDarkMaterial : skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
