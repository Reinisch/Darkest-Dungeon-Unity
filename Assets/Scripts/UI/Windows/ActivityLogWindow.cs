using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActivityLogWindow : MonoBehaviour
{
    public GameObject heroActivitySlot;
    public LayoutElement topHolder;
    public LayoutElement botHolder;

    public Font generatorFont;
    public Scrollbar scrollbar;
    public RectTransform scrollRectTransform;
    public List<WeekLogSlot> VisibleSlots;

    public RectTransform questGoalBlock;
    public RectTransform rosterGoalBlock;
    public List<LogQuestGoal> PlotGoals { get; set; }
    public List<LogQuestGoal> RosterGoals { get; set; }

    public event WindowEvent onWindowClose;

    public WeekLogSlot GenerationSlot { get; private set; }
    public TextGenerator TextGenerator { get; private set; }
    public TextGenerationSettings PartySettings { get; private set; }
    public TextGenerationSettings RecordSettings { get; private set; }

    private Dictionary<int, float> SlotSizes = new Dictionary<int, float>();
    private float ViewHeight
    {
        get
        {
            return scrollRectTransform.sizeDelta.y;
        }
    }
    private float ContentHeight
    {
        get;
        set;
    }
    public bool Initialized { get; set; }

    private float holderTop;
    private float focusMax;
    private float focusMin;
    private float lastValue = -1;
    private float cachedSize = 0;

    void OnEnable()
    {
        CheckVisible();
    }
    void OnDisable()
    {
        lastValue = -1;
    }
    void Update()
    {
        if (lastValue != scrollbar.value)
            CheckVisible();
    }

    void DisableUnused(int currentIndex)
    {
        for (int i = currentIndex; i < VisibleSlots.Count; i++)
            VisibleSlots[i].gameObject.SetActive(false);
    }
    float CalculatePrefferedLogHeight(WeekActivityLog log)
    {
        float height = 100;

        if (log.ReturnRecord != null)
        {
            height += 190 + Mathf.Max(30, TextGenerator.GetPreferredHeight(log.ReturnRecord.Description, PartySettings));
        }

        if (log.HeroRecords.Count > 0)
        {
            for (int i = 0; i < log.HeroRecords.Count; i++)
                height += Mathf.Max(130, TextGenerator.GetPreferredHeight(log.HeroRecords[i].Description, RecordSettings)) * 1.0056074766355140186915887850467f;

            if (log.HeroRecords.Count > 1)
                height += 20 * (log.HeroRecords.Count - 1);
        }

        if (log.EmbarkRecord != null)
        {
            height += 160 + Mathf.Max(30, TextGenerator.GetPreferredHeight(log.EmbarkRecord.Description, PartySettings));
        }
        return height;
    }

    public void Initialize()
    {
        Initialized = true;
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

        int initVisible = Mathf.Min(VisibleSlots.Count, DarkestDungeonManager.Campaign.Logs.Count);
        int lastIndex = DarkestDungeonManager.Campaign.Logs.Count - initVisible;

        for (int i = 0; i < VisibleSlots.Count; i++)
            VisibleSlots[i].Window = this;

        for (int i = 0; i < initVisible; i++)
        {
            var log = DarkestDungeonManager.Campaign.Logs[lastIndex + i];
            VisibleSlots[i].UpdateWeekLog(log);
        }

        for (int i = DarkestDungeonManager.Campaign.Logs.Count - 1; i >= 0; i--)
        {
            var log = DarkestDungeonManager.Campaign.Logs[i];
            float newSize = CalculatePrefferedLogHeight(log);
            SlotSizes.Add(log.WeekNumber, newSize);
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
        SlotSizes.Add(log.WeekNumber, newSize);
        cachedSize += newSize;
    }
    public void CheckVisible()
    {
        lastValue = scrollbar.value;
        holderTop = 0;

        focusMin = ViewHeight > ContentHeight ? -20 :
                (ContentHeight - ViewHeight) * (1 - scrollbar.value) - 20;
        focusMax = focusMin + ViewHeight + 20; 

        int currentIndex = 0;
        if(DarkestDungeonManager.Campaign.Logs.Count < VisibleSlots.Count)
        {
            for (int i = DarkestDungeonManager.Campaign.Logs.Count - 1; i >= 0; i--)
            {
                VisibleSlots[currentIndex++].UpdateWeekLog(DarkestDungeonManager.Campaign.Logs[i]);
            }
            topHolder.preferredHeight = 0;
            botHolder.preferredHeight = 0;
            DisableUnused(currentIndex);
            return;
        }

        for (int i = DarkestDungeonManager.Campaign.Logs.Count; i >= 1; i--)
        {
            holderTop += SlotSizes[i];
            if (holderTop >= focusMin)
            {
                holderTop -= SlotSizes[i];
                topHolder.preferredHeight = holderTop;
                botHolder.preferredHeight = ContentHeight - holderTop;

                int lastWeek = i > VisibleSlots.Count ? i - VisibleSlots.Count + 1 : 1;
                for (int j = i; j >= lastWeek; j--)
                {
                    VisibleSlots[currentIndex++].UpdateWeekLog(DarkestDungeonManager.Campaign.Logs[j - 1]);
                    if (holderTop > focusMax) break;
                    holderTop += SlotSizes[j];
                }
                break;
            }
        }

        for(int i = 0; i < currentIndex; i++)
            botHolder.preferredHeight -= SlotSizes[VisibleSlots[i].WeekLog.WeekNumber];

        DisableUnused(currentIndex);
    }
    public void RecalculateHeight()
    {
        cachedSize -= SlotSizes[DarkestDungeonManager.Campaign.CurrentWeek];
        SlotSizes[DarkestDungeonManager.Campaign.CurrentWeek] = CalculatePrefferedLogHeight(DarkestDungeonManager.Campaign.CurrentLog());
        cachedSize += SlotSizes[DarkestDungeonManager.Campaign.CurrentWeek];
        ContentHeight = cachedSize;
    }

    public void WindowClosed()
    {
        if (onWindowClose != null)
            onWindowClose();
    }
}