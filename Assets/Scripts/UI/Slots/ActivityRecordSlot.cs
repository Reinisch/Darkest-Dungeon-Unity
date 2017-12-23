using UnityEngine;
using UnityEngine.UI;

public class ActivityRecordSlot : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Image activityIcon;
    [SerializeField]
    private Text activityText;

    public RectTransform RectTransform { get { return rectTransform; } }

    public void UpdateActivityRecord(ActorActivityRecord record)
    {
        if(record.HeroClass != null)
            activityIcon.sprite = DarkestDungeonManager.HeroSprites[record.HeroClass]["A"].Portrait;
        activityText.text = record.Description;
        gameObject.SetActive(true);
    }

    public void ResetActivityRecord()
    {
        gameObject.SetActive(false);
    }
}
