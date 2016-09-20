using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ActivityRecordSlot : MonoBehaviour
{
    public RectTransform rectTransform;

    public Image activityIcon;
    public Text activityText;

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
