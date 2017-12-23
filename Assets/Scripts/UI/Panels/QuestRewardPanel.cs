using UnityEngine;
using System.Collections.Generic;

public class QuestRewardPanel : MonoBehaviour
{
    public List<RewardSlot> RewardSlots { get; private set; }

    private void Awake()
    {
        RewardSlots = new List<RewardSlot>(transform.Find("RewardItems").GetComponentsInChildren<RewardSlot>());
    }

    public void UpdateRewardSlots(Quest quest)
    {
        int rewardsDistributed = 0;
        for(int i = 0; i < quest.Reward.ItemDefinitions.Count; i++)
        {
            if (i >= RewardSlots.Count)
                break;

            RewardSlots[i].gameObject.SetActive(true);
            RewardSlots[i].SetItem(quest.Reward.ItemDefinitions[i]);
            rewardsDistributed++;
        }

        for (int i = rewardsDistributed; i < RewardSlots.Count; i++)
            RewardSlots[i].gameObject.SetActive(false);
    }
}
