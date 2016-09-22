using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GlossaryRecord : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text termLabel;
    public Text termDefinitionLabel;

    public GlossaryWindow Window { get; set; }

    bool hovered;
    int pressed = 20;

    RectTransform tooltip;
    void Awake()
    {
        var objRect = gameObject.transform.FindChild("MouseOver");
        if(objRect != null)
            tooltip = objRect.GetComponent<RectTransform>();
    }
    void Update()
    {
        if (pressed > 0)
            pressed--;
        if(hovered)
        {
            if(Input.GetKey(KeyCode.F))
            {
                if (pressed > 0)
                    return;
                else
                    pressed = 20;

                Debug.Log("Mouse: " + Input.mousePosition.ToString());
                Vector2 localMouse = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), 
                    Input.mousePosition, Window.mainCamera, out localMouse);
                Debug.Log("Local mouse: " + localMouse.ToString());


                List<UILineInfo> lines = new List<UILineInfo>();
                termDefinitionLabel.cachedTextGenerator.GetLines(lines);
                bool insideTerm = false;
                for(int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
                {
                    int currentWidth = 0;
                    int currentRect = 0;
                    var line = lines[lineIndex];
                    var lastCharIndex = lineIndex + 1 == lines.Count ?
                        termDefinitionLabel.text.Length : lines[lineIndex + 1].startCharIdx;
                    for(int i = line.startCharIdx; i < lastCharIndex; i++)
                    {
                        CharacterInfo charInfo;
                        termDefinitionLabel.font.GetCharacterInfo(termDefinitionLabel.text[i],
                            out charInfo, termDefinitionLabel.font.fontSize);
                        if(insideTerm)
                        {
                            if (termDefinitionLabel.text[i] == '\"')
                            {
                                currentWidth += charInfo.advance;
                                currentRect += charInfo.advance;
                                tooltip.sizeDelta = new Vector2(currentRect, GetComponent<RectTransform>().sizeDelta.y/lines.Count);
                                currentRect = 0;
                                insideTerm = false;
                            }
                            else
                            {
                                currentWidth += charInfo.advance;
                                currentRect += charInfo.advance;
                            }
                        }
                        else
                        {
                            if (termDefinitionLabel.text[i] == '\"')
                            {
                                tooltip.localPosition = new Vector2(currentWidth,
                                    -GetComponent<RectTransform>().sizeDelta.y / lines.Count * lineIndex);
                                currentWidth += charInfo.advance;
                                currentRect = charInfo.advance;
                                insideTerm = true;
                            }
                            else
                            {
                                currentWidth += charInfo.advance;
                            }
                        }
                    }
                    if(insideTerm)
                    {
                        tooltip.sizeDelta = new Vector2(currentRect, termDefinitionLabel.font.lineHeight);
                    }
                }
            }
        }
    }

    public void UpdateRecord(int index)
    {
        termLabel.text = LocalizationManager.GetString("str_glossary_term_" + index.ToString()) + ":";
        termDefinitionLabel.text = LocalizationManager.GetString("str_glossary_term_definition_" + index.ToString());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
}
