using UnityEngine;
using UnityEngine.UI;

public class ResultItemWindow : MonoBehaviour
{
    public Text treasureLabel;
    public Text heirloomLabel;
    public Text questRewardLabel;

    public QuestRewardPanel rewardPanel;

    public Text goldAmount;
    public Text crestAmount;
    public Text deedAmount;
    public Text portraitAmount;
    public Text bustAmount;

    public GameObject lootSlot;
    public RectTransform treasureSlots;
    public RectTransform heirloomSlots;
    public HorizontalLayoutGroup treasureLayout;
    public HorizontalLayoutGroup heirloomLayout;

    public void PrepareRewards()
    {
        rewardPanel.UpdateRewardSlots(RaidSceneManager.Raid.Quest);
        if(DarkestDungeonManager.RaidManager.Status == RaidStatus.Success)
        {
            if(RaidSceneManager.Raid.Quest.IsPlotQuest)
            {
                var plot = RaidSceneManager.Raid.Quest as PlotQuest;
                DarkestDungeonManager.Campaign.CompletedPlot.Add(plot.Id);
            }
            DarkestDungeonManager.Campaign.QuestsComleted++;

            if (RaidSceneManager.Raid.Quest.CompletionDungeonXp)
            {
                int dungeonExpAmount = DarkestDungeonManager.Data.CampaignGeneration.DungeonXpTable[RaidSceneManager.Raid.Quest.Length];
                dungeonExpAmount += (RaidSceneManager.Raid.Quest.Difficulty + 1) / 2;
                DarkestDungeonManager.Campaign.Dungeons[RaidSceneManager.Raid.Quest.Dungeon].AddExperience(dungeonExpAmount);
            }

            foreach(var rewardSlot in rewardPanel.RewardSlots)
            {
                if(rewardSlot.isActiveAndEnabled)
                {
                    if(rewardSlot.Item.Type == "gold")
                        DarkestDungeonManager.Campaign.Estate.AddGold(rewardSlot.Item.Amount);
                    else if (rewardSlot.Item.Type == "heirloom")
                    {
                        if (rewardSlot.Item.Id == "bust")
                            DarkestDungeonManager.Campaign.Estate.AddHeirlooms(0, 0, 0, rewardSlot.Item.Amount);
                        else if (rewardSlot.Item.Id == "portrait")
                            DarkestDungeonManager.Campaign.Estate.AddHeirlooms(0, 0, rewardSlot.Item.Amount, 0);
                        else if (rewardSlot.Item.Id == "deed")
                            DarkestDungeonManager.Campaign.Estate.AddHeirlooms(0, rewardSlot.Item.Amount, 0, 0);
                        else if (rewardSlot.Item.Id == "crest")
                            DarkestDungeonManager.Campaign.Estate.AddHeirlooms(rewardSlot.Item.Amount, 0, 0, 0);
                    }
                    else if (rewardSlot.Item.Type == "trinket")
                    {
                        for(int i = 0; i < rewardSlot.Item.Amount; i++)
                            DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Add(
                                DarkestDungeonManager.Data.Items[rewardSlot.Item.Type][rewardSlot.Item.Id] as Trinket);
                    }
                }
            }
        }
        int gold = 0;
        int crest = 0;
        int deed = 0;
        int portrait = 0;
        int bust = 0;

        int treasureCount = 0;
        int heirloomCount = 0;

        if (RaidSceneManager.Formations.heroes.party.Units.Count > 0)
        {
            foreach (var slot in RaidSceneManager.Inventory.InventorySlots)
            {
                if (slot.HasItem && slot.SlotItem.ItemType != "trinket")
                {
                    if (slot.SlotItem.Item.Type == "heirloom")
                    {
                        if (slot.SlotItem.Item.Id == "bust")
                            bust += slot.SlotItem.Item.Amount;
                        else if (slot.SlotItem.Item.Id == "portrait")
                            portrait += slot.SlotItem.Item.Amount;
                        else if (slot.SlotItem.Item.Id == "deed")
                            deed += slot.SlotItem.Item.Amount;
                        else if (slot.SlotItem.Item.Id == "crest")
                            crest += slot.SlotItem.Item.Amount;

                        for (int i = 0; i < slot.SlotItem.Item.Amount; i++)
                        {
                            GameObject newSlot = Instantiate(lootSlot);
                            RewardSlot rewSlot = newSlot.GetComponentInChildren<RewardSlot>();
                            rewSlot.SetSingle(slot.SlotItem.Item);
                            newSlot.transform.SetParent(heirloomSlots, false);
                            newSlot.transform.SetAsLastSibling();
                            heirloomCount++;
                        }

                    }
                    else if (slot.SlotItem.Item.Type == "gold")
                    {
                        gold += slot.SlotItem.Item.Amount;
                        GameObject newSlot = Instantiate(lootSlot);
                        RewardSlot rewSlot = newSlot.GetComponentInChildren<RewardSlot>();
                        rewSlot.SetItem(slot.SlotItem.Item);
                        newSlot.transform.SetParent(treasureSlots, false);
                        newSlot.transform.SetAsLastSibling();
                        treasureCount++;
                    }
                    else if (slot.SlotItem.ItemData.SellPrice != 0)
                    {
                        gold += slot.SlotItem.ItemData.SellPrice * slot.SlotItem.Item.Amount;

                        for (int i = 0; i < slot.SlotItem.Item.Amount; i++)
                        {
                            GameObject newSlot = Instantiate(lootSlot);
                            RewardSlot rewSlot = newSlot.GetComponentInChildren<RewardSlot>();
                            rewSlot.SetSingle(slot.SlotItem.Item);
                            newSlot.transform.SetParent(treasureSlots, false);
                            newSlot.transform.SetAsLastSibling();
                            treasureCount++;
                        }
                    }
                }
                else if (slot.HasItem && slot.SlotItem.ItemType == "trinket")
                {
                    DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Add(slot.SlotItem.ItemData as Trinket);
                }
            }
        }
        goldAmount.text = gold.ToString();
        crestAmount.text = crest.ToString();
        deedAmount.text = deed.ToString();
        portraitAmount.text = portrait.ToString();
        bustAmount.text = bust.ToString();

        DarkestDungeonManager.Campaign.Estate.AddGold(gold);
        DarkestDungeonManager.Campaign.Estate.AddHeirlooms(crest, deed, portrait, bust);
    }
}
