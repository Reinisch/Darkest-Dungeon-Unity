using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeekLogSlot: MonoBehaviour
{
    [SerializeField]
    private Text weekTitle;
    [SerializeField]
    private Image raidBanner;
    [SerializeField]
    private RectTransform eventsTransform;

    public PartyActivityRecordSlot EmbarkSlot;
    public PartyActivityRecordSlot ReturnSlot;
    public List<ActivityRecordSlot> SimpleSlots;

    public ActivityLogWindow Window { private get; set; }
    public WeekActivityLog WeekLog { get; private set; }

    public void UpdateWeekLog(WeekActivityLog weekLog)
    {
        gameObject.SetActive(true);

        WeekLog = weekLog;
        weekTitle.text = string.Format(LocalizationManager.GetString("str_week"), WeekLog.WeekNumber);

        if (weekLog.ReturnRecord != null)
        {
            raidBanner.rectTransform.parent.gameObject.SetActive(true);
            raidBanner.sprite = weekLog.ReturnRecord.IsSuccessfull ?
                DarkestDungeonManager.Data.Sprites["raid_success_banner"] :
                DarkestDungeonManager.Data.Sprites["raid_failure_banner"];

            ReturnSlot.UpdatePartyActivity(weekLog.ReturnRecord);
        }
        else
        {
            raidBanner.gameObject.SetActive(false);
            ReturnSlot.ResetPartyActivity();
        }

        int lastRecordIndex = Mathf.Min(SimpleSlots.Count, weekLog.HeroRecords.Count);

        for (int i = 0; i < lastRecordIndex; i++)
            SimpleSlots[i].UpdateActivityRecord(weekLog.HeroRecords[i]);

        for (int i = lastRecordIndex; i < weekLog.HeroRecords.Count; i++)
        {
            var heroSlot = Instantiate(Window.HeroActivitySlot).GetComponent<ActivityRecordSlot>();
            heroSlot.RectTransform.SetParent(eventsTransform, false);
            heroSlot.RectTransform.SetAsLastSibling();
            heroSlot.UpdateActivityRecord(weekLog.HeroRecords[i]);
            SimpleSlots.Add(heroSlot);
        }
        for (int i = weekLog.HeroRecords.Count; i < SimpleSlots.Count; i++)
            SimpleSlots[i].ResetActivityRecord();

        if (weekLog.EmbarkRecord != null)
            EmbarkSlot.UpdatePartyActivity(weekLog.EmbarkRecord);
        else
            EmbarkSlot.ResetPartyActivity();
    }

    public void UpdateEmbarkPartyLog(PartyActivityRecord partyRecord)
    {
        WeekLog.EmbarkRecord = partyRecord;
        EmbarkSlot.UpdatePartyActivity(partyRecord);
    }

    public void UpdateReturnPartyLog(PartyActivityRecord partyRecord)
    {
        WeekLog.ReturnRecord = partyRecord;
        ReturnSlot.UpdatePartyActivity(partyRecord);
    }
}
