using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public enum LootResultType { Waiting, Discard, NotEnoughSpace }

public class ScrollEventLoot : MonoBehaviour
{
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text description;
    [SerializeField]
    private Button takeAllButton;
    [SerializeField]
    private Button passButton;
    [SerializeField]
    private PartyInventory partyInventory;

    public LootResultType ActionType { get; private set; }
    public bool KeepLoot { get; private set; }
    public bool HasSomething { get { return partyInventory.HasSomething(); } }

    public void Initialize()
    {
        partyInventory.Initialize();
        partyInventory.Configuration = InventoryConfiguration.LootInventory;
    }

    public void LoadSingleLoot(string code, int amount)
    {
        KeepLoot = false;
        takeAllButton.gameObject.SetActive(true);
        passButton.gameObject.SetActive(true);
        partyInventory.SetActivated();

        ActionType = LootResultType.Waiting;

        gameObject.SetActive(true);
        partyInventory.DiscardAll();

        foreach (var item in RaidSolver.GenerateLoot(code, amount, RaidSceneManager.Raid))
            partyInventory.DistributeItem(item);

        partyInventory.DeactivateEmptySlots();

        title.text = LocalizationManager.GetString("str_overlay_loot_chest_title");
        description.text = LocalizationManager.GetString("str_overlay_loot_chest_description");

        if (!partyInventory.HasSomething())
            Close();
    }

    public void LoadBattleLoot(List<LootDefinition> battleLoot)
    {
        KeepLoot = false;
        takeAllButton.gameObject.SetActive(true);
        passButton.gameObject.SetActive(true);
        partyInventory.SetActivated();

        ActionType = LootResultType.Waiting;

        gameObject.SetActive(true);
        partyInventory.DiscardAll();

        foreach (var item in RaidSolver.GenerateLoot(battleLoot, RaidSceneManager.Raid))
            partyInventory.DistributeItem(item);

        partyInventory.DeactivateEmptySlots();

        title.text = LocalizationManager.GetString("str_overlay_loot_battle_title");
        description.text = LocalizationManager.GetString("str_overlay_loot_battle_description");

        if (!partyInventory.HasSomething())
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

        ActionType = LootResultType.Waiting;

        gameObject.SetActive(true);
        partyInventory.DiscardAll();

        if(curio.IsQuestCurio)
        {
            if(curioResult != null)
                partyInventory.DistributeItem(new ItemDefinition("quest_item", curioResult.Item, 1));
        }
        else
        {
            if(curioResult.IsCombined)
            {
                foreach (var result in interaction.Results)
                    if (result.IsCombined && result.Item != "Nothing")
                        foreach (var item in RaidSolver.GenerateLoot(result, raid))
                            partyInventory.DistributeItem(item);
            }
            else if(curioResult.Item != "Nothing")
                foreach (var item in RaidSolver.GenerateLoot(curioResult, raid))
                    partyInventory.DistributeItem(item);

            if(RaidSceneManager.RaidPanel.SelectedHero != null)
            {
                var extraLoot = RaidSceneManager.RaidPanel.SelectedHero.HeroClass.ExtraCurioLoot;
                if (extraLoot != null)
                    foreach (var item in RaidSolver.GenerateLoot(extraLoot.Code, extraLoot.Count, raid))
                        partyInventory.DistributeItem(item);
            }
        }

        partyInventory.DeactivateEmptySlots();

        title.text = LocalizationManager.GetString("str_overlay_loot_chest_title");
        description.text = LocalizationManager.GetString("str_overlay_loot_chest_description");

        if (!partyInventory.HasSomething())
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

        if (!partyInventory.InventorySlots.Any(item => item.HasItem))
            Close();
        else
            ActionType = LootResultType.NotEnoughSpace;
    }

    public void Close()
    {
        if (partyInventory.ContaintItemType("quest_item"))
            return;

        ActionType = LootResultType.Discard;

        gameObject.SetActive(false);
        partyInventory.DiscardAll();
    }
}
