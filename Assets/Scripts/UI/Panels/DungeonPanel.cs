using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DungeonPanel : MonoBehaviour
{
    [SerializeField]
    private Slider dangeonProgress;
    [SerializeField]
    private Text dangeonProgressLevel;
    [SerializeField]
    private string dungeon;

    public List<QuestSlot> QuestSlots { get; private set; }

    private DungeonProgress dungeonProgressData;

    public event Action<QuestSlot> EventQuestSelected;

    public void Initialize()
    {
        QuestSlots = new List<QuestSlot>(transform.Find("DungeonFrame")
            .Find("GeneratedQuests").GetComponentsInChildren<QuestSlot>());
        for (int i = 0; i < QuestSlots.Count; i++)
        {
            QuestSlots[i].Initialize();
            QuestSlots[i].EventQuestSelected += DungeonPanelQuestSelected;
        }

        dungeonProgressData = DarkestDungeonManager.Campaign.Dungeons[dungeon];
        dangeonProgress.value = dungeonProgressData.XpRatio;
        dangeonProgressLevel.text = dungeonProgressData.MasteryLevel.ToString();
    }

    public void DistributeQuests(List<Quest> quests)
    {
        int currentSlot = 0;

        for (int i = 0; i < quests.Count; i++)
        {
            if (currentSlot >= QuestSlots.Count)
                return;
            if (quests[i].Dungeon == dungeon)
            {
                QuestSlots[currentSlot].UpdateQuest(quests[i]);
                QuestSlots[currentSlot].gameObject.SetActive(true);
                currentSlot++;
            }
        }

        if (currentSlot == 0 && dungeonProgressData.IsEvent)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);

        for (int i = currentSlot; i < QuestSlots.Count; i++)
            QuestSlots[i].gameObject.SetActive(false);
    }

    private void DungeonPanelQuestSelected(QuestSlot questSlot)
    {
        if (EventQuestSelected != null)
            EventQuestSelected(questSlot);
    }
}
