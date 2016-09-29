using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum LootEventType { Curio, Battle, Quest, Camp }
public enum LootResultType { Waiting, Discard, TakeAll, NotEnoughSpace }

public class ScrollEventLoot : MonoBehaviour
{
    public Text title;
    public Text description;

    public Button takeAllButton;
    public Button passButton;

    public event ScrollEvent onScrollOpened;
    public event ScrollEvent onScrollClosed;

    public LootEventType LootType { get; private set; }
    public LootResultType ActionType { get; private set; }
    public bool KeepLoot { get; private set; }

    public PartyInventory partyInventory;

    public void Initialize()
    {
        partyInventory.Initialize();
    }

    public void ScrollOpened()
    {
        if (onScrollOpened != null)
            onScrollOpened();
    }
    public void ScrollClosed()
    {
        gameObject.SetActive(false);
        partyInventory.DiscardAll();

        if (onScrollClosed != null)
            onScrollClosed();
    }
    
    public void LoadSingleLoot(string code, int amount)
    {
        KeepLoot = false;
        takeAllButton.gameObject.SetActive(true);
        passButton.gameObject.SetActive(true);
        partyInventory.SetActivated();

        LootType = LootEventType.Camp;
        ActionType = LootResultType.Waiting;

        gameObject.SetActive(true);
        partyInventory.DiscardAll();

        foreach (var item in RaidSolver.GenerateLoot(code, amount, RaidSceneManager.Raid))
            partyInventory.DistributeItem(item);

        partyInventory.DeactivateEmptySlots();

        title.text = LocalizationManager.GetString("str_overlay_loot_chest_title");
        description.text = LocalizationManager.GetString("str_overlay_loot_chest_description");

        if (partyInventory.HasSomething())
            ScrollOpened();
        else
            Close();
    }
    public void LoadBattleLoot(List<LootDefinition> battleLoot)
    {
        KeepLoot = false;
        takeAllButton.gameObject.SetActive(true);
        passButton.gameObject.SetActive(true);
        partyInventory.SetActivated();

        LootType = LootEventType.Battle;
        ActionType = LootResultType.Waiting;

        gameObject.SetActive(true);
        partyInventory.DiscardAll();

        foreach (var item in RaidSolver.GenerateLoot(battleLoot, RaidSceneManager.Raid))
            partyInventory.DistributeItem(item);

        partyInventory.DeactivateEmptySlots();

        title.text = LocalizationManager.GetString("str_overlay_loot_battle_title");
        description.text = LocalizationManager.GetString("str_overlay_loot_battle_description");

        if (partyInventory.HasSomething())
            ScrollOpened();
        else
            Close();
    }
    public void LoadCurioLoot(Curio curio, CurioInteraction interaction, CurioResult curioResult, RaidInfo raid, bool keepLoot)
    {
        KeepLoot = keepLoot;
        if (keepLoot)
        {
            takeAllButton.gameObject.SetActive(false);
            passButton.gameObject.SetActive(false);
            partyInventory.SetDeactivated();
        }
        else
        {
            takeAllButton.gameObject.SetActive(true);
            passButton.gameObject.SetActive(true);
            partyInventory.SetActivated();
        }

        LootType = LootEventType.Curio;
        ActionType = LootResultType.Waiting;

        gameObject.SetActive(true);
        partyInventory.DiscardAll();

        if(curio.IsQuestCurio)
        {
            if(curioResult != null)
            {
                partyInventory.DistributeItem(new ItemDefinition("quest_item", curioResult.Item, 1));
            }
        }
        else
        {
            if(curioResult.IsCombined)
            {
                foreach (var result in interaction.Results)
                    if (result.IsCombined)
                        foreach (var item in RaidSolver.GenerateLoot(result, raid))
                            partyInventory.DistributeItem(item);
            }
            else
                foreach (var item in RaidSolver.GenerateLoot(curioResult, raid))
                    partyInventory.DistributeItem(item);
        }
            

        partyInventory.DeactivateEmptySlots();

        title.text = LocalizationManager.GetString("str_overlay_loot_chest_title");
        description.text = LocalizationManager.GetString("str_overlay_loot_chest_description");

        if (partyInventory.HasSomething())
            ScrollOpened();
        else
            Close();
    }
    public void ResetWaiting()
    {
        ActionType = LootResultType.Waiting;
    }

    public void TakeAll()
    {
        foreach(var slot in partyInventory.InventorySlots)
        {
            if(slot.HasItem)
            {
                if(RaidSceneManager.Inventory.CheckInventorySpace(slot.SlotItem.Item))
                {
                    RaidSceneManager.Inventory.DistributeItem(slot.SlotItem.Item);
                    slot.DeleteItem();
                }
            }
        }
        partyInventory.DeactivateEmptySlots();

        if (partyInventory.InventorySlots.Find(item => item.HasItem) == null)
        {
            ActionType = LootResultType.TakeAll;
            ScrollClosed();
        }
        else
        {
            ActionType = LootResultType.NotEnoughSpace;
        }
    }
    public void Close()
    {
        if (partyInventory.ContaintItemType("quest_item"))
            return;

        ActionType = LootResultType.Discard;

        ScrollClosed();
    }
}
