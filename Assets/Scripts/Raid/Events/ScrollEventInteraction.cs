using UnityEngine;
using UnityEngine.UI;

public enum InteractionEventType { Obstacle, Curio }
public enum InteractionResultType { Waiting, Cancel, ManualInteraction, ItemInteraction }

public class ScrollEventInteraction : MonoBehaviour
{
    public Text title;
    public Text description;
    public Button handButton;
    public Button passButton;

    public event ScrollEvent onScrollOpened;
    public event ScrollEvent onScrollClosed;

    public InteractionSlot interactionSlot;

    public ItemData SelectedItem { get; set; }

    public IRaidArea AreaView { get; private set; }
    public InteractionEventType EventType { get; private set; }
    public InteractionResultType ActionType { get; private set; }

    void InteractionSlot_onDropIn(InventoryItem inventoryItem)
    {
        var itemData = inventoryItem.ItemData;
        inventoryItem.RemoveItems(1);
        ItemActionSelected(itemData);
    }
    void InteractionSlot_onActivate(ItemData item)
    {
        switch (EventType)
        {
            case InteractionEventType.Obstacle:
                if(RaidSceneManager.Inventory.UseItem(item))
                    RaidSceneManager.Instanse.ActivateObstacle(AreaView as RaidHallSector, false);
                else
                    RaidSceneManager.Instanse.ActivateObstacle(AreaView as RaidHallSector, true);
                ScrollClosed();
                break;
            case InteractionEventType.Curio:
                RaidSceneManager.Inventory.UseItem(item);
                SelectedItem = item;
                ActionType = InteractionResultType.ItemInteraction;
                ScrollClosed();
                break;
        }
    }

    void ScrollOpened()
    {
        gameObject.SetActive(true);

        if (onScrollOpened != null)
            onScrollOpened();
    }
    void ScrollClosed()
    {
        gameObject.SetActive(false);
        ToolTipManager.Instanse.Hide();
        if (onScrollClosed != null)
            onScrollClosed();
    }

    void ItemActionSelected(ItemData item)
    {
        switch (EventType)
        {
            case InteractionEventType.Obstacle:
                RaidSceneManager.Instanse.ActivateObstacle(AreaView as RaidHallSector, false);
                ScrollClosed();
                break;
            case InteractionEventType.Curio:
                SelectedItem = item;
                ActionType = InteractionResultType.ItemInteraction;
                ScrollClosed();
                break;
        }
    }
    void ManualActionSelected()
    {
        switch (EventType)
        {
            case InteractionEventType.Obstacle:
                RaidSceneManager.Instanse.ActivateObstacle(AreaView as RaidHallSector, true);
                ScrollClosed();
                break;
            case InteractionEventType.Curio:
                ActionType = InteractionResultType.ManualInteraction;
                ScrollClosed();
                break;
        }
    }
    void CancelActionSelected()
    {
        switch (EventType)
        {
            case InteractionEventType.Obstacle:
                ScrollClosed();
                RaidSceneManager.Inventory.SetPeacefulState(false);
                RaidSceneManager.Instanse.EnablePartyMovement();
                break;
            case InteractionEventType.Curio:
                ActionType = InteractionResultType.Cancel;
                ScrollClosed();
                break;
        }
    }

    public void Initialize()
    {
        interactionSlot.onDropIn += InteractionSlot_onDropIn;
        interactionSlot.onActivate += InteractionSlot_onActivate;
    }
    public void LoadInteraction(Obstacle obstacle, RaidHallSector sector)
    {
        RaidSceneManager.Inventory.SetObstacleState();

        EventType = InteractionEventType.Obstacle;
        ActionType = InteractionResultType.Waiting;
        AreaView = sector;

        interactionSlot.Reset();
        handButton.interactable = true;

        interactionSlot.Item = DarkestDungeonManager.Data.Items["supply"]["shovel"];
        interactionSlot.itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_supply+shovel"];
        interactionSlot.itemIcon.enabled = true;
        interactionSlot.IsItemAllowed = false;

        if (RaidSceneManager.Inventory.ContainsItem(interactionSlot.Item))
        {
            interactionSlot.IsItemFixed = true;
            interactionSlot.itemIcon.material = interactionSlot.itemIcon.defaultMaterial;
        }
        else
        {
            interactionSlot.IsItemFixed = false;
            interactionSlot.itemIcon.material = DarkestDungeonManager.GrayDarkMaterial;
        }

        title.text = LocalizationManager.GetString("str_obstacle_" + obstacle.StringId + "_title");
        description.text = LocalizationManager.GetString("str_obstacle_" + obstacle.StringId + "_description");

        ScrollOpened();
    }
    public void LoadInteraction(Curio curio, IRaidArea area)
    {
        EventType = InteractionEventType.Curio;
        ActionType = InteractionResultType.Waiting;
        AreaView = area;

        interactionSlot.Reset();
        if(curio.IsQuestCurio)
        {
            if (curio.Results.Count == 0)
            {
                if (curio.ItemInteractions.Count != 0)
                {
                    handButton.interactable = false;

                    interactionSlot.Item = DarkestDungeonManager.Data.Items["quest_item"][curio.ItemInteractions[0].ItemId];
                    interactionSlot.itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_quest_item+" +
                        curio.ItemInteractions[0].ItemId];
                    interactionSlot.itemIcon.enabled = true;
                    interactionSlot.IsItemAllowed = false;
                    interactionSlot.IsItemFixed = true;
                    interactionSlot.itemIcon.material = interactionSlot.itemIcon.defaultMaterial;
                }
                else
                {
                    handButton.interactable = true;
                    interactionSlot.IsItemAllowed = true;
                    interactionSlot.IsItemFixed = false;
                }
            }
            else if (curio.Results.Count != 0)
            {
                handButton.interactable = true;
                interactionSlot.IsItemAllowed = true;
                interactionSlot.IsItemFixed = false;
            }
            
        }
        else
        {
            handButton.interactable = true;
            interactionSlot.IsItemAllowed = true;
            interactionSlot.IsItemFixed = false;
        }

        title.text = LocalizationManager.GetString("str_curio_title_" + curio.StringId);
        description.text = LocalizationManager.GetString("str_curio_content_" + curio.StringId);

        ScrollOpened();
    }
    public void ResetInteraction(Curio curio)
    {
        EventType = InteractionEventType.Curio;
        ActionType = InteractionResultType.Waiting;

        interactionSlot.Reset();
        interactionSlot.IsItemAllowed = true;
        interactionSlot.IsItemFixed = false;

        ScrollOpened();
    }
}
