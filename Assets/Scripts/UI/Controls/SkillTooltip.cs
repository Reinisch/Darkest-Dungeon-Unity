using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillTooltip : ToolTip
{
    public GameObject targetRanksObject;

    public Image[] launchRanks;
    public Image[] targetRanks;

    public RectTransform connectorRect;

    Color targetColor;
    Color launchColor;
    Color neutralColor;

    int basicWidth = 14;
    int startConnectorOffset = 8;

    public override void Initialize()
    {
        base.Initialize();

        Color resultColor;
        if (ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["harmful"], out resultColor))
            targetColor = resultColor;
        else
            targetColor = Color.red;

        if (ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["notable"], out resultColor))
            launchColor = resultColor;
        else
            launchColor = Color.yellow;

        if (ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["neutral"], out resultColor))
            neutralColor = resultColor;
        else
            neutralColor = Color.gray;

        connectorRect.GetComponent<Image>().color = targetColor;
    }

    public void UpdateSkillRanks(CombatSkill skill)
    {
        for (int i = 0; i < launchRanks.Length; i++)
        {
            if (skill.LaunchRanks.Ranks.Contains(i + 1))
            {
                launchRanks[i].color = launchColor;
            }
            else
                launchRanks[i].color = neutralColor;
        }

        if(skill.TargetRanks.IsSelfFormation || skill.TargetRanks.IsSelfTarget)
            targetRanksObject.SetActive(false);
        else
        {
            targetRanksObject.SetActive(true);

            int firstTargetIndex = -1;
            int lastTargetIndex = -1;

            for(int i = 0; i < targetRanks.Length; i++)
            {
                if (skill.TargetRanks.Ranks.Contains(i + 1))
                {
                    if(firstTargetIndex == -1)
                        firstTargetIndex = i;
                    targetRanks[i].color = targetColor;
                    lastTargetIndex = i;
                }
                else
                    targetRanks[i].color = neutralColor;
            }

            if(skill.TargetRanks.IsMultitarget)
            {
                if(firstTargetIndex == -1 || firstTargetIndex == lastTargetIndex)
                    connectorRect.gameObject.SetActive(false);
                else
                {
                    connectorRect.gameObject.SetActive(true);
                    connectorRect.localPosition = new Vector3(startConnectorOffset + basicWidth * firstTargetIndex, 0, 0);
                    connectorRect.sizeDelta = new Vector2(basicWidth * (skill.TargetRanks.Ranks.Count - 1), connectorRect.sizeDelta.y);
                }
            }
            else
                connectorRect.gameObject.SetActive(false);
        }
    }
}
