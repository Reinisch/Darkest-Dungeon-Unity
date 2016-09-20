using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class RaidPreparationManager : MonoBehaviour
{
    public RectTransform dungeonPanels;
    public SelectedQuestPanel selectedQuestPanel;
    public RaidPartyPanel raidPartyPanel;

    bool areQuestsDistributed = false;

    public QuestSlot SelectedQuestSlot { get; set; }
    public List<DungeonPanel> DungeonPanels { get; private set; }

    void DistributeQuests()
    {
        if (!areQuestsDistributed)
        {
            List<Quest> quests = DarkestDungeonManager.Campaign.Quests;
            quests = quests.OrderBy(quest => quest.Difficulty).ToList();
            foreach (var panel in DungeonPanels)
                panel.DistributeQuests(quests);
            areQuestsDistributed = true;
        }
    }
    void SelectAnyQuest()
    {
        for (int i = 0; i < DungeonPanels.Count; i++)
        {
            for (int j = 0; j < DungeonPanels[i].QuestSlots.Count; j++)
            {
                if (DungeonPanels[i].QuestSlots[j].Quest != null)
                {
                    DungeonPanels[i].QuestSlots[j].QuestButtonClicked();
                    return;
                }
            }
        }
    }
    void RaidManager_onQuestSelected(QuestSlot questSlot)
    {
        for (int i = 0; i < DungeonPanels.Count; i++)
        {
            for (int j = 0; j < DungeonPanels[i].QuestSlots.Count; j++)
            {
                if (DungeonPanels[i].QuestSlots[j].isActiveAndEnabled)
                    DungeonPanels[i].QuestSlots[j].Selected = false;
            }
        }
        questSlot.Selected = true;
        SelectedQuestSlot = questSlot;
        selectedQuestPanel.SetSelectedQuest(questSlot.Quest);
    }

    public void Initialize()
    {
        DungeonPanels = new List<DungeonPanel>(dungeonPanels.GetComponentsInChildren<DungeonPanel>());
        for (int i = 0; i < DungeonPanels.Count; i++)
        {
            DungeonPanels[i].Initialize();
            DungeonPanels[i].onQuestSelected += RaidManager_onQuestSelected;
        }
    }

    public void UpdateUI()
    {
        DistributeQuests();

        if (SelectedQuestSlot == null)
            SelectAnyQuest();
        else
        {
            selectedQuestPanel.SetSelectedQuest(SelectedQuestSlot.Quest);
            SelectedQuestSlot.Selected = true;
        }
    }

}
