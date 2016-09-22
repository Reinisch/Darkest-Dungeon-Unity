using UnityEngine;
using System.Collections.Generic;

public class TownManager : MonoBehaviour
{
    public List<BuildingSlot> buildingSlots;
    public List<BuildingWindow> buildingWindows;

    public Sprite purchasedUpgradeIcon; 
    public Sprite lockedUpgradeIcon;
    public Sprite availableUpgradeIcon;

    public Sprite caretakenSlot;
    public Sprite emptySlot;
    public Sprite heroLockedSlot;
    public Sprite lockedSlot;
    public Sprite highlightedSlot;

    public bool AnyWindowsOpened
    {
        get
        {
            return EstateSceneManager.AnyWindowsOpened || BuildingWindowActive;
        }
    }

    public EstateSceneManager EstateSceneManager { get; set; }
    public bool BuildingWindowActive { get; set; }

    void Awake()
    {
        EstateSceneManager = GetComponent<EstateSceneManager>();
        for (int i = 0; i < buildingSlots.Count; i++ )
        {
            buildingSlots[i].TownManager = this;
            buildingWindows[i].TownManager = this;
        }
    }

    public void CloseBuildingWindow()
    {
        for (int i = 0; i < buildingWindows.Count; i++)
            if (buildingWindows[i].isActiveAndEnabled)
                buildingWindows[i].WindowClosed();
    }

    public void InitializeBuildings()
    {
        for (int i = 0; i < buildingSlots.Count; i++)
        {
            buildingWindows[i].Initialize();
        }
    }
    public HeroSlot GetHeroSlot(Hero hero)
    {
        return EstateSceneManager.rosterPanel.HeroSlots.Find(heroSlot => heroSlot.Hero == hero);
    }

    public void UpdateUpgradeSlot(UpgradeStatus status, SkillUpgradeSlot slot, float discount = 1)
    {
        switch (status)
        {
            case UpgradeStatus.Available:
                SetUpgradeSlotAvailable(slot, discount);
                break;
            case UpgradeStatus.Locked:
                SetUpgradeSlotLocked(slot);
                break;
            case UpgradeStatus.Purchased:
                SetUpgradeSlotPurchased(slot);
                break;
        }
    }
    public void UpdateUpgradeSlot(UpgradeStatus status, EquipmentUpgradeSlot slot, float discount = 1)
    {
        switch (status)
        {
            case UpgradeStatus.Available:
                SetUpgradeSlotAvailable(slot, discount);
                break;
            case UpgradeStatus.Locked:
                SetUpgradeSlotLocked(slot);
                break;
            case UpgradeStatus.Purchased:
                SetUpgradeSlotPurchased(slot);
                break;
        }
    }
    public void UpdateUpgradeSlot(UpgradeStatus status, BuildingUpgradeSlot slot)
    {
        switch(status)
        {
            case UpgradeStatus.Available:
                SetUpgradeSlotAvailable(slot);
                break;
            case UpgradeStatus.Locked:
                SetUpgradeSlotLocked(slot);
                break;
            case UpgradeStatus.Purchased:
                SetUpgradeSlotPurchased(slot);
                break;
        }
    }

    public void SetUpgradeSlotPurchased(SkillUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(true);
        slot.icon.sprite = purchasedUpgradeIcon;
        slot.costFrame.gameObject.SetActive(false);
    }
    public void SetUpgradeSlotAvailable(SkillUpgradeSlot slot, float discount)
    {
        slot.background.gameObject.SetActive(false);
        slot.icon.sprite = availableUpgradeIcon;
        slot.costFrame.gameObject.SetActive(true);

        if (DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.Upgrade.Cost[0], discount))
            slot.costFrame.heirloomOneAmount.color = Color.white;
        else
            slot.costFrame.heirloomOneAmount.color = Color.red;

        slot.costFrame.heirloomOneAmount.text = Mathf.RoundToInt(slot.Upgrade.Cost[0].Amount * discount).ToString();
    }
    public void SetUpgradeSlotLocked(SkillUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(false);
        slot.icon.sprite = lockedUpgradeIcon;
        slot.costFrame.gameObject.SetActive(false);
    }
    public void SetUpgradeSlotPurchased(EquipmentUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(true);
        slot.icon.sprite = purchasedUpgradeIcon;
        slot.costFrame.gameObject.SetActive(false);
    }
    public void SetUpgradeSlotAvailable(EquipmentUpgradeSlot slot, float discount)
    {
        slot.background.gameObject.SetActive(false);
        slot.icon.sprite = availableUpgradeIcon;
        slot.costFrame.gameObject.SetActive(true);

        if (DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.Upgrade.Cost[0], discount))
            slot.costFrame.heirloomOneAmount.color = Color.white;
        else
            slot.costFrame.heirloomOneAmount.color = Color.red;

        slot.costFrame.heirloomOneAmount.text = Mathf.RoundToInt(slot.Upgrade.Cost[0].Amount * discount).ToString();
    }
    public void SetUpgradeSlotLocked(EquipmentUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(false);
        slot.icon.sprite = lockedUpgradeIcon;
        slot.costFrame.gameObject.SetActive(false);
    }
    public void SetUpgradeSlotPurchased(BuildingUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(true);
        slot.icon.sprite = purchasedUpgradeIcon;
        slot.costFrame.gameObject.SetActive(false);
    }
    public void SetUpgradeSlotAvailable(BuildingUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(false);
        slot.icon.sprite = availableUpgradeIcon;
        slot.costFrame.gameObject.SetActive(true);

        if (DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.UpgradeInfo.Cost[1]))
            slot.costFrame.heirloomOneAmount.color = Color.white;
        else
            slot.costFrame.heirloomOneAmount.color = Color.red;

        slot.costFrame.heirloomOneAmount.text = slot.UpgradeInfo.Cost[1].Amount.ToString();

        if ( !(slot.costFrame.heirloomTwoIcon == null || slot.UpgradeInfo.Cost.Count < 3) )
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.UpgradeInfo.Cost[2]))
                slot.costFrame.heirloomTwoAmount.color = Color.white;
            else
                slot.costFrame.heirloomTwoAmount.color = Color.red;
            slot.costFrame.heirloomTwoAmount.text = slot.UpgradeInfo.Cost[2].Amount.ToString();
        }
    }
    public void SetUpgradeSlotLocked(BuildingUpgradeSlot slot)
    {
        slot.background.gameObject.SetActive(false);
        slot.icon.sprite = lockedUpgradeIcon;
        slot.costFrame.gameObject.SetActive(false);
    }
}