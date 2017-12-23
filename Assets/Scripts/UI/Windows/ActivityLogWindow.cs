using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ActivityLogWindow : MonoBehaviour
{
    [SerializeField]
    private GameObject heroActivitySlot;
    [SerializeField]
    private LayoutElement topHolder;
    [SerializeField]
    private LayoutElement botHolder;

    [SerializeField]
    private Font generatorFont;
    [SerializeField]
    private Scrollbar scrollbar;
    [SerializeField]
    private RectTransform scrollRectTransform;
    [SerializeField]
    private List<WeekLogSlot> visibleSlots;

    [SerializeField]
    private RectTransform questGoalBlock;
    [SerializeField]
    private RectTransform rosterGoalBlock;

    private List<LogQuestGoal> PlotGoals { get; set; }
    private List<LogQuestGoal> RosterGoals { get; set; }
    private TextGenerator TextGenerator { get; set; }
    private TextGenerationSettings PartySettings { get; set; }
    private TextGenerationSettings RecordSettings { get; set; }

    private float ViewHeight { get { return scrollRectTransform.sizeDelta.y; } }
    private float ContentHeight { get; set; }

    public GameObject HeroActivitySlot { get { return heroActivitySlot; } }

    private readonly Dictionary<int, float> slotSizes = new Dictionary<int, float>();
    private float holderTop;
    private float focusMax;
    private float focusMin;
    private float cachedSize;
    private float lastValue = -1;

    public event Action EventWindowClosed;

    private void OnEnable()
    {
        CheckVisible();
    }

    private void OnDisable()
    {
        lastValue = -1;
    }

    private void Update()
    {
        if (!Mathf.Approximately(lastValue, scrollbar.value))
            CheckVisible();
    }

    public void Initialize()
    {
        TextGenerator = new TextGenerator();
        PartySettings = new TextGenerationSettings()
        {
            font = generatorFont,
            fontSize = 0,
            fontStyle = FontStyle.Normal,
            lineSpacing = 1,
            color = Color.white,
            generateOutOfBounds = false,
            horizontalOverflow = HorizontalWrapMode.Wrap,
            generationExtents = new Vector2(520, 400),
            pivot = new Vector2(0.5f, 0.5f),
            resizeTextForBestFit = false,
            resizeTextMaxSize = 0,
            resizeTextMinSize = 0,
            richText = true,
            scaleFactor = 1,
            textAnchor = TextAnchor.UpperLeft,
            updateBounds = false,
            verticalOverflow = VerticalWrapMode.Overflow,
        };
        RecordSettings = new TextGenerationSettings()
        {
            font = generatorFont,
            fontSize = 0,
            fontStyle = FontStyle.Normal,
            lineSpacing = 1,
            color = Color.white,
            generateOutOfBounds = false,
            horizontalOverflow = HorizontalWrapMode.Wrap,
            generationExtents = new Vector2(470, 400),
            pivot = new Vector2(0.5f, 0.5f),
            resizeTextForBestFit = false,
            resizeTextMaxSize = 0,
            resizeTextMinSize = 0,
            richText = true,
            scaleFactor = 1,
            textAnchor = TextAnchor.UpperLeft,
            updateBounds = false,
            verticalOverflow = VerticalWrapMode.Overflow,
        };

        int initVisible = Mathf.Min(visibleSlots.Count, DarkestDungeonManager.Campaign.Logs.Count);
        int lastIndex = DarkestDungeonManager.Campaign.Logs.Count - initVisible;
        visibleSlots.ForEach(slot => slot.Window = this);

        for (int i = 0; i < initVisible; i++)
        {
            var log = DarkestDungeonManager.Campaign.Logs[lastIndex + i];
            visibleSlots[i].UpdateWeekLog(log);
        }

        for (int i = DarkestDungeonManager.Campaign.Logs.Count - 1; i >= 0; i--)
        {
            var log = DarkestDungeonManager.Campaign.Logs[i];
            float newSize = CalculatePrefferedLogHeight(log);
            slotSizes.Add(log.WeekNumber, newSize);
            cachedSize += newSize;
        }
        RecalculateHeight();
        CheckVisible();

        PlotGoals = new List<LogQuestGoal>(questGoalBlock.GetComponentsInChildren<LogQuestGoal>());
        RosterGoals = new List<LogQuestGoal>(rosterGoalBlock.GetComponentsInChildren<LogQuestGoal>());
         
        var plotQuests = DarkestDungeonManager.Data.QuestDatabase.PlotQuests;
        int plotGoalsCount = Mathf.Min(PlotGoals.Count, plotQuests.Count);
        for(int i = 0; i < plotGoalsCount; i++)
            PlotGoals[i].UpdateInfo(plotQuests[i]);
        if(plotGoalsCount < PlotGoals.Count)
            for (int i = PlotGoals.Count - 1; i >= plotGoalsCount; i--)
            {
                Destroy(PlotGoals[i].gameObject);
                PlotGoals.RemoveAt(i);
            }
        else if (plotGoalsCount > PlotGoals.Count)
        {
            for(int i = plotGoalsCount; i < plotQuests.Count; i++)
            {
                var newPlotGoal = Instantiate(PlotGoals[0].gameObject).GetComponent<LogQuestGoal>();
                newPlotGoal.UpdateInfo(plotQuests[i]);
                PlotGoals.Add(newPlotGoal);
            }
        }

        var heroClasses = DarkestDungeonManager.Data.HeroClasses.Values.ToList();
        int rosterGoalsCount = Mathf.Min(RosterGoals.Count, heroClasses.Count);
        for (int i = 0; i < rosterGoalsCount; i++)
            RosterGoals[i].UpdateInfo(heroClasses[i]);
        if (rosterGoalsCount < RosterGoals.Count)
            for (int i = RosterGoals.Count - 1; i >= rosterGoalsCount; i--)
            {
                Destroy(RosterGoals[i].gameObject);
                RosterGoals.RemoveAt(i);
            }
        else if (rosterGoalsCount > RosterGoals.Count)
        {
            for (int i = rosterGoalsCount; i < heroClasses.Count; i++)
            {
                var newRosterGoal = Instantiate(RosterGoals[0].gameObject).GetComponent<LogQuestGoal>();
                newRosterGoal.UpdateInfo(heroClasses[i]);
                RosterGoals.Add(newRosterGoal);
            }
        }
    }

    public void ProgressWeek()
    {
        var log = DarkestDungeonManager.Campaign.Logs[DarkestDungeonManager.Campaign.Logs.Count - 1];
        float newSize = CalculatePrefferedLogHeight(log);
        slotSizes.Add(log.WeekNumber, newSize);
        cachedSize += newSize;
    }

    public void RecalculateHeight()
    {
        cachedSize -= slotSizes[DarkestDungeonManager.Campaign.CurrentWeek];
        slotSizes[DarkestDungeonManager.Campaign.CurrentWeek] = CalculatePrefferedLogHeight(DarkestDungeonManager.Campaign.CurrentLog());
        cachedSize += slotSizes[DarkestDungeonManager.Campaign.CurrentWeek];
        ContentHeight = cachedSize;
    }

    public void WindowClosed()
    {
        if (EventWindowClosed != null)
            EventWindowClosed();
    }

    private void DisableUnused(int currentIndex)
    {
        for (int i = currentIndex; i < visibleSlots.Count; i++)
            visibleSlots[i].gameObject.SetActive(false);
    }

    private void CheckVisible()
    {
        lastValue = scrollbar.value;
        holderTop = 0;

        focusMin = ViewHeight > ContentHeight ? -20 :
                (ContentHeight - ViewHeight) * (1 - scrollbar.value) - 20;
        focusMax = focusMin + ViewHeight + 20;

        int currentIndex = 0;
        if (DarkestDungeonManager.Campaign.Logs.Count < visibleSlots.Count)
        {
            for (int i = DarkestDungeonManager.Campaign.Logs.Count - 1; i >= 0; i--)
            {
                visibleSlots[currentIndex++].UpdateWeekLog(DarkestDungeonManager.Campaign.Logs[i]);
            }
            topHolder.preferredHeight = 0;
            botHolder.preferredHeight = 0;
            DisableUnused(currentIndex);
            return;
        }

        for (int i = DarkestDungeonManager.Campaign.Logs.Count; i >= 1; i--)
        {
            holderTop += slotSizes[i];
            if (holderTop >= focusMin)
            {
                holderTop -= slotSizes[i];
                topHolder.preferredHeight = holderTop;
                botHolder.preferredHeight = ContentHeight - holderTop;

                int lastWeek = i > visibleSlots.Count ? i - visibleSlots.Count + 1 : 1;
                for (int j = i; j >= lastWeek; j--)
                {
                    visibleSlots[currentIndex++].UpdateWeekLog(DarkestDungeonManager.Campaign.Logs[j - 1]);
                    if (holderTop > focusMax) break;
                    holderTop += slotSizes[j];
                }
                break;
            }
        }

        for (int i = 0; i < currentIndex; i++)
            botHolder.preferredHeight -= slotSizes[visibleSlots[i].WeekLog.WeekNumber];

        DisableUnused(currentIndex);
    }

    private float CalculatePrefferedLogHeight(WeekActivityLog log)
    {
        float height = 100;

        if (log.ReturnRecord != null)
            height += 190 + Mathf.Max(30, TextGenerator.GetPreferredHeight(log.ReturnRecord.Description, PartySettings));

        if (log.HeroRecords.Count > 0)
        {
            height += log.HeroRecords.Sum(t => Mathf.Max(130, TextGenerator.GetPreferredHeight(t.Description, RecordSettings)) * 1.0056f);

            if (log.HeroRecords.Count > 1)
                height += 20 * (log.HeroRecords.Count - 1);
        }

        if (log.EmbarkRecord != null)
            height += 160 + Mathf.Max(30, TextGenerator.GetPreferredHeight(log.EmbarkRecord.Description, PartySettings));
        return height;
    }
}