using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DungeonPanel : MonoBehaviour
{
    public Text dangeonName;
    public Slider dangeonProgress;
    public Text dangeonProgressLevel;

    public string dungeon;

    public List<QuestSlot> QuestSlots { get; private set; }

    public event QuestSelectionEvent onQuestSelected;

    private DungeonProgress dungeonProgressData;

    public void Initialize()
    {
        QuestSlots = new List<QuestSlot>(transform.Find("DungeonFrame")
            .Find("GeneratedQuests").GetComponentsInChildren<QuestSlot>());
        for (int i = 0; i < QuestSlots.Count; i++)
        {
            QuestSlots[i].Initialize();
            QuestSlots[i].onQuestSelected += DungeonPanel_onQuestSelected;
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

    void DungeonPanel_onQuestSelected(QuestSlot questSlot)
    {
        if (onQuestSelected != null)
            onQuestSelected(questSlot);
    }
}
