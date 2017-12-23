using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : ToolTip
{
    [SerializeField]
    private GameObject targetRanksObject;
    [SerializeField]
    private Image[] launchRanks;
    [SerializeField]
    private Image[] targetRanks;
    [SerializeField]
    private RectTransform connectorRect;

    private Color targetColor;
    private Color launchColor;
    private Color neutralColor;

    private int basicWidth = 14;
    private int startConnectorOffset = 8;

    public override void Initialize()
    {
        base.Initialize();

        Color resultColor;
        targetColor = ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["harmful"], out resultColor) ? resultColor : Color.red;
        launchColor = ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["notable"], out resultColor) ? resultColor : Color.yellow;
        neutralColor = ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["neutral"], out resultColor) ? resultColor : Color.gray;

        connectorRect.GetComponent<Image>().color = targetColor;
    }

    public void UpdateSkillRanks(CombatSkill skill)
    {
        for (int i = 0; i < launchRanks.Length; i++)
            launchRanks[i].color = skill.LaunchRanks.Ranks.Contains(i + 1) ? launchColor : neutralColor;

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
