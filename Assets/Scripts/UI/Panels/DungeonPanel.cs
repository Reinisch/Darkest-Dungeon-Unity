using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class DungeonPanel : MonoBehaviour
{
    public Text dangeonName;
    public Slider dangeonProgress;
    public Text dangeonProgressLevel;

    public string dungeon;

    public List<QuestSlot> QuestSlots { get; private set; }

    public event QuestSelectionEvent onQuestSelected;

    public void Initialize()
    {
        QuestSlots = new List<QuestSlot>(transform.FindChild("DungeonFrame")
            .FindChild("GeneratedQuests").GetComponentsInChildren<QuestSlot>());
        for (int i = 0; i < QuestSlots.Count; i++)
        {
            QuestSlots[i].Initialize();
            QuestSlots[i].onQuestSelected += DungeonPanel_onQuestSelected;
        }

        DungeonProgress progress = DarkestDungeonManager.Campaign.Dungeons[dungeon];
        dangeonProgress.value = progress.XpRatio;
        dangeonProgressLevel.text = progress.MasteryLevel.ToString();
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

        for (int i = currentSlot; i < QuestSlots.Count; i++)
            QuestSlots[i].gameObject.SetActive(false);
    }

    void DungeonPanel_onQuestSelected(QuestSlot questSlot)
    {
        if (onQuestSelected != null)
            onQuestSelected(questSlot);
    }
}
