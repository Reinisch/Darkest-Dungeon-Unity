using System;
using UnityEngine;
using UnityEngine.UI;

public enum InteractionEventType { Obstacle, Curio }
public enum InteractionResultType { Waiting, Cancel, ManualInteraction, ItemInteraction }

public class ScrollEventInteraction : MonoBehaviour
{
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text description;
    [SerializeField]
    private Button handButton;
    [SerializeField]
    private InteractionSlot interactionSlot;

    public event Action OnScrollOpened;
    public event Action OnScrollClosed;

    public ItemData SelectedItem { get; private set; }
    public IRaidArea AreaView { get; private set; }
    public InteractionEventType EventType { get; private set; }
    public InteractionResultType ActionType { get; private set; }

    private void InteractionSlot_onDropIn(InventoryItem inventoryItem)
    {
        var itemData = inventoryItem.ItemData;
        inventoryItem.RemoveItems(1);
        ItemActionSelected(itemData);
    }

    private void InteractionSlot_onActivate(ItemData item)
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

    private void ScrollOpened()
    {
        gameObject.SetActive(true);

        if (OnScrollOpened != null)
            OnScrollOpened();
    }

    private void ScrollClosed()
    {
        gameObject.SetActive(false);
        ToolTipManager.Instanse.Hide();
        if (OnScrollClosed != null)
            OnScrollClosed();
    }

    private void ItemActionSelected(ItemData item)
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

    public void ManualActionSelected()
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

    public void CancelActionSelected()
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
        interactionSlot.EventDropIn += InteractionSlot_onDropIn;
        interactionSlot.EventActivate += InteractionSlot_onActivate;
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
        interactionSlot.ItemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_supply+shovel"];
        interactionSlot.ItemIcon.enabled = true;
        interactionSlot.IsItemAllowed = false;

        if (RaidSceneManager.Inventory.ContainsItem(interactionSlot.Item))
        {
            interactionSlot.IsItemFixed = true;
            interactionSlot.ItemIcon.material = interactionSlot.ItemIcon.defaultMaterial;
        }
        else
        {
            interactionSlot.IsItemFixed = false;
            interactionSlot.ItemIcon.material = DarkestDungeonManager.GrayDarkMaterial;
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
                    interactionSlot.ItemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_quest_item+" +
                        curio.ItemInteractions[0].ItemId];
                    interactionSlot.ItemIcon.enabled = true;
                    interactionSlot.IsItemAllowed = false;
                    interactionSlot.IsItemFixed = true;
                    interactionSlot.ItemIcon.material = interactionSlot.ItemIcon.defaultMaterial;
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

        title.text = LocalizationManager.GetString("str_curio_title_" + curio.OriginalId);
        description.text = LocalizationManager.GetString("str_curio_content_" + curio.OriginalId);

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