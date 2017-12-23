using UnityEngine;
using UnityEngine.UI;

public class RaidQuestPanel : MonoBehaviour
{
    [SerializeField]
    private Text questTitle;
    [SerializeField]
    private Text questGoal;
    [SerializeField]
    private SkeletonAnimation completionSeal;
    [SerializeField]
    private Button retreatButton;

    public void UpdateQuest(Quest quest, bool completed = false)
    {
        if(completed)
        {
            CompleteQuest();
            return;
        }

        if (quest.IsPlotQuest)
        {
            PlotQuest plotQuest = (PlotQuest)quest;

            questTitle.text = LocalizationManager.GetString("town_quest_name_" + plotQuest.Id);
            questGoal.text = plotQuest.Goal.QuestData.GetDataString(plotQuest.Goal.Type);

            if (plotQuest.CanRetreat == false)
                retreatButton.gameObject.SetActive(false);
        }
        else
        {
            questTitle.text = LocalizationManager.GetString("town_quest_name_" +
                quest.Type + "+" + quest.Length.ToString() + "+" + quest.Dungeon + "+" + quest.Goal.Id);
            questGoal.text = quest.Goal.QuestData.GetDataString(quest.Goal.Type);
        }
    }

    public void UpdateQuest(Quest quest, PhotonPlayer player, bool isPanelOwner = false, bool completed = false)
    {
        if (completed)
        {
            CompleteQuest();
            return;
        }

        questTitle.text = player.NickName;
        questGoal.text = PhotonNetwork.room.PlayerCount > 1 ?
            isPanelOwner ? "Defeat the " + PhotonNetwork.otherPlayers[0].NickName :
            "Defeat the " + PhotonNetwork.player.NickName : "Defeat the opponent!";
    }

    public void CompleteQuest()
    {
        completionSeal.gameObject.SetActive(true);
        completionSeal.state.SetAnimation(0, "appear", false);
        completionSeal.state.AddAnimation(0, "idle_exploration", true, 0.33f);
        questGoal.text = "";
        questTitle.text = LocalizationManager.GetString("raid_quest_complete");
    }

    public void CompleteQuestClick()
    {
        RaidSceneManager.Instanse.AbandonButtonClicked();
    }

    public void SetCombatState()
    {
        if(RaidSceneManager.Raid.QuestCompleted)
            completionSeal.state.SetAnimation(0, "idle_combat", true);
    }

    public void SetPeacefulState()
    {
        if (RaidSceneManager.Raid.QuestCompleted)
            if(completionSeal.state != null)
                completionSeal.state.SetAnimation(0, "idle_exploration", true);
    }

    public void EnableRetreat()
    {
        if (RaidSceneManager.Raid.Quest.CanRetreat)
        {
            retreatButton.gameObject.SetActive(true);
            retreatButton.interactable = true;
        }
    }

    public void UpdateEncounterRetreat()
    {
        if(RaidSceneManager.SceneState == DungeonSceneState.Room)
        {
            if(RaidSceneManager.Raid.LastSector == null)
                retreatButton.gameObject.SetActive(false);
            else
                retreatButton.interactable = false;
        }
        else if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
        {
            if(RaidSceneManager.Raid.LastRoom == null)
                retreatButton.gameObject.SetActive(false);
            else
                retreatButton.interactable = false;
        }
    }

    public void UpdateCombatRetreat(bool setActive)
    {
        if (retreatButton.gameObject.activeSelf == false)
            return;

        if (RaidSceneManager.SceneState == DungeonSceneState.Room)
        {
            if (RaidSceneManager.Raid.LastSector == null)
                retreatButton.gameObject.SetActive(false);
            else
                retreatButton.interactable = setActive;
        }
        else if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
        {
            if (RaidSceneManager.Raid.LastRoom == null)
                retreatButton.gameObject.SetActive(false);
            else
                retreatButton.interactable = setActive;
        }
    }

    public void DisableRetreat(bool removeButton)
    {
        if (removeButton)
            retreatButton.gameObject.SetActive(false);
        else
            retreatButton.interactable = false;
    }
}
