using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TownEventWindow : MonoBehaviour
{
    [SerializeField]
    private Image eventTone;
    [SerializeField]
    private Image eventImage;
    [SerializeField]
    private Text eventName;
    [SerializeField]
    private Text eventDescription;
    [SerializeField]
    private Text eventEffects;
    [SerializeField]
    private RecruitPanel eventRecruits;
    [SerializeField]
    private List<Sprite> tones;
    [SerializeField]
    private List<Sprite> eventImages;

    public RecruitPanel EventRecruits { get { return eventRecruits; } }

    public void UpdateEvent(TownEvent townEvent)
    {
        switch(townEvent.Tone)
        {
            case TownEventTone.Good:
                eventTone.sprite = tones.Find(sprite => sprite.name.EndsWith("good"));
                break;
            case TownEventTone.Bad:
                eventTone.sprite = tones.Find(sprite => sprite.name.EndsWith("bad"));
                break;
            case TownEventTone.Neutral:
                eventTone.sprite = tones.Find(sprite => sprite.name.EndsWith("neutral"));
                break;
        }
        eventImage.sprite = eventImages.Find(sprite => sprite.name.EndsWith(townEvent.Id));
        eventName.text = LocalizationManager.GetString("town_event_title_" + townEvent.Id);
        eventDescription.text = LocalizationManager.GetString("town_event_description_" + townEvent.Id);

        bool isRecruitEvent = townEvent.Data.Find(data => data.Type == TownEventDataType.BonusRecruit ||
            data.Type == TownEventDataType.DeadRecruit) != null;

        if(isRecruitEvent)
        {
            EventRecruits.gameObject.SetActive(true);
            EventRecruits.UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.EventHeroes);
            eventEffects.gameObject.SetActive(false);
        }
        else
        {
            EventRecruits.gameObject.SetActive(false);
            eventEffects.gameObject.SetActive(true);
            eventEffects.text = townEvent.EffectTooltip;
        }
    }
}
