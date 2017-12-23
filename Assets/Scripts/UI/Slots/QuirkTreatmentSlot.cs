using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuirkTreatmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Text quirkText;
    [SerializeField]
    private Image highlightBackground;
    [SerializeField]
    private Image statusIcon;

    public event Action<QuirkTreatmentSlot> EventSelected;
    public event Action<QuirkTreatmentSlot> EventDeselected;

    public bool Selected { get; private set; }
    public QuirkInfo QuirkInfo { get; private set; }

    private RectTransform RectTransform { get; set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Select()
    {
        highlightBackground.enabled = true;
        statusIcon.gameObject.SetActive(true);

        if (!QuirkInfo.Quirk.IsPositive)
            statusIcon.sprite = DarkestDungeonManager.Data.Sprites["removequirk"];
        else
            statusIcon.sprite = DarkestDungeonManager.Data.Sprites["lockquirk"];
        Selected = true;

        if (EventSelected != null)
            EventSelected(this);
    }

    public void Deselect()
    {
        highlightBackground.enabled = false;
        if (QuirkInfo.IsLocked)
        {
            if (!QuirkInfo.Quirk.IsPositive)
            {
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["seriousquirk"];
            }
        }
        else
            statusIcon.gameObject.SetActive(false);
        Selected = false;

        if (EventDeselected != null)
            EventDeselected(this);
    }

    public void UpdateQuirk(QuirkInfo quirkInfo)
    {
        QuirkInfo = quirkInfo;

        if (QuirkInfo != null)
        {
            gameObject.SetActive(true);
            quirkText.text = LocalizationManager.GetString("str_quirk_name_" + QuirkInfo.Quirk.Id);
            Selected = false;
            if (quirkInfo.IsLocked)
            {
                if (QuirkInfo.Quirk.IsPositive)
                {
                    statusIcon.gameObject.SetActive(true);
                    statusIcon.sprite = DarkestDungeonManager.Data.Sprites["lockquirk"];
                    statusIcon.material = DarkestDungeonManager.GrayMaterial;
                    highlightBackground.enabled = true;
                    highlightBackground.material = DarkestDungeonManager.GrayMaterial;
                }
                else
                {
                    statusIcon.gameObject.SetActive(true);
                    statusIcon.sprite = DarkestDungeonManager.Data.Sprites["seriousquirk"];
                    statusIcon.material = statusIcon.defaultMaterial;
                    highlightBackground.enabled = false;
                    highlightBackground.material = highlightBackground.defaultMaterial;
                }
            }
            else
            {
                highlightBackground.enabled = false;
                statusIcon.gameObject.SetActive(false);
            }
        }
        else
            ResetSlot();
    }

    public void ResetSlot()
    {
        Selected = false;
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (QuirkInfo.IsLocked && QuirkInfo.Quirk.IsPositive)
            return;

        if (Selected)
            Deselect();
        else
            Select();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (QuirkInfo != null)
            ToolTipManager.Instanse.Show(QuirkInfo.Quirk.ToolTip(), RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
