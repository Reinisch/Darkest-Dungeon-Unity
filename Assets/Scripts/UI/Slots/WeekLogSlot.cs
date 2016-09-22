using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeekLogSlot: MonoBehaviour
{
    public Text weekTitle;
    public Image raidBanner;
    public RectTransform rectTransform;
    public RectTransform eventsTransform;

    public PartyActivityRecordSlot EmbarkSlot;
    public PartyActivityRecordSlot ReturnSlot;
    public List<ActivityRecordSlot> SimpleSlots;

    public float CachedHeight { get; set; }
    public ActivityLogWindow Window { get; set; }
    public WeekActivityLog WeekLog { get; set; }

    public void UpdateWeekLog(WeekActivityLog weekLog)
    {
        gameObject.SetActive(true);

        WeekLog = weekLog;
        weekTitle.text = string.Format(LocalizationManager.GetString("str_week"), WeekLog.WeekNumber);

        if (weekLog.ReturnRecord != null)
        {
            raidBanner.rectTransform.parent.gameObject.SetActive(true);
            if(weekLog.ReturnRecord.IsSuccessfull)
                raidBanner.sprite = DarkestDungeonManager.Data.Sprites["raid_success_banner"];
            else
                raidBanner.sprite = DarkestDungeonManager.Data.Sprites["raid_failure_banner"];

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
            var heroSlot = Instantiate(Window.heroActivitySlot).GetComponent<ActivityRecordSlot>();
            heroSlot.rectTransform.SetParent(eventsTransform, false);
            heroSlot.rectTransform.SetAsLastSibling();
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
