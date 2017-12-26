using UnityEngine;
using System.Collections.Generic;

public class TownManager : MonoBehaviour
{
    [SerializeField]
    private List<BuildingSlot> buildingSlots;
    [SerializeField]
    private List<BuildingWindow> buildingWindows;
    [SerializeField]
    private Sprite purchasedUpgradeIcon;
    [SerializeField]
    private Sprite lockedUpgradeIcon;
    [SerializeField]
    private Sprite availableUpgradeIcon;

    public bool BuildingWindowActive { get; set; }
    public bool AnyWindowsOpened { get { return EstateSceneManager.Instanse.AnyWindowsOpened || BuildingWindowActive; } }
    public List<BuildingWindow> BuildingWindows { get { return buildingWindows; } }

    public HeroSlot GetHeroSlot(Hero hero)
    {
        return EstateSceneManager.Instanse.RosterPanel.HeroSlots.Find(heroSlot => heroSlot.Hero == hero);
    }

    public void CloseBuildingWindow()
    {
        foreach (BuildingWindow window in BuildingWindows)
            if (window.isActiveAndEnabled)
                window.WindowClosed();
    }

    public void InitializeBuildings()
    {
        for (int i = 0; i < buildingSlots.Count; i++)
        {
            BuildingWindows[i].Initialize();
        }
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

    #region Upgarade Info Helpers

    private void SetUpgradeSlotPurchased(SkillUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(true);
        slot.Icon.sprite = purchasedUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(false);
    }

    private void SetUpgradeSlotAvailable(SkillUpgradeSlot slot, float discount)
    {
        slot.Background.gameObject.SetActive(false);
        slot.Icon.sprite = availableUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(true);

        bool isFree = false;
        for (int i = 0; i < slot.Tree.Tags.Count; i++)
            if (DarkestDungeonManager.Campaign.EventModifiers.HasFreeUpgrade(slot.Tree.Tags[i]))
                isFree = true;

        if (isFree || DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.Upgrade.Cost[0], discount))
            slot.CostFrame.HeirloomOneAmount.color = Color.white;
        else
            slot.CostFrame.HeirloomOneAmount.color = Color.red;

        if (isFree)
        {
            slot.CostFrame.HeirloomOneAmount.text = "0";
        }
        else
        {
            slot.CostFrame.HeirloomOneAmount.text = Mathf.RoundToInt(slot.Upgrade.Cost[0].Amount * discount).ToString();
        }
    }

    private void SetUpgradeSlotLocked(SkillUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(false);
        slot.Icon.sprite = lockedUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(false);
    }

    private void SetUpgradeSlotPurchased(EquipmentUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(true);
        slot.Icon.sprite = purchasedUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(false);
    }

    private void SetUpgradeSlotAvailable(EquipmentUpgradeSlot slot, float discount)
    {
        slot.Background.gameObject.SetActive(false);
        slot.Icon.sprite = availableUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(true);

        bool isFree = false;
        for (int i = 0; i < slot.Tree.Tags.Count; i++)
            if (DarkestDungeonManager.Campaign.EventModifiers.HasFreeUpgrade(slot.Tree.Tags[i]))
                isFree = true;

        if (isFree || DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.Upgrade.Cost[0], discount))
            slot.CostFrame.HeirloomOneAmount.color = Color.white;
        else
            slot.CostFrame.HeirloomOneAmount.color = Color.red;

        if(isFree)
        {
            slot.CostFrame.HeirloomOneAmount.text = "0";
        }
        else
        {
            slot.CostFrame.HeirloomOneAmount.text = Mathf.RoundToInt(slot.Upgrade.Cost[0].Amount * discount).ToString();
        }
    }

    private void SetUpgradeSlotLocked(EquipmentUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(false);
        slot.Icon.sprite = lockedUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(false);
    }

    private void SetUpgradeSlotPurchased(BuildingUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(true);
        slot.Icon.sprite = purchasedUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(false);
    }

    private void SetUpgradeSlotAvailable(BuildingUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(false);
        slot.Icon.sprite = availableUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(true);

        bool isFree = false;
        for (int i = 0; i < slot.Tree.Tags.Count; i++)
            if (DarkestDungeonManager.Campaign.EventModifiers.HasFreeUpgrade(slot.Tree.Tags[i]))
                isFree = true;

        if (isFree || DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.UpgradeInfo.Cost[1]))
            slot.CostFrame.HeirloomOneAmount.color = Color.white;
        else
            slot.CostFrame.HeirloomOneAmount.color = Color.red;

        if (isFree)
        {
            slot.CostFrame.HeirloomOneAmount.text = "0";
        }
        else
        {
            slot.CostFrame.HeirloomOneAmount.text = slot.UpgradeInfo.Cost[1].Amount.ToString();
        }

        if ( !(slot.CostFrame.HeirloomTwoIcon == null || slot.UpgradeInfo.Cost.Count < 3) )
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayPrice(slot.UpgradeInfo.Cost[2]))
                slot.CostFrame.HeirloomTwoAmount.color = Color.white;
            else
                slot.CostFrame.HeirloomTwoAmount.color = Color.red;

            if(isFree)
                slot.CostFrame.HeirloomTwoAmount.text = "0";
            else
                slot.CostFrame.HeirloomTwoAmount.text = slot.UpgradeInfo.Cost[2].Amount.ToString();
        }
    }

    private void SetUpgradeSlotLocked(BuildingUpgradeSlot slot)
    {
        slot.Background.gameObject.SetActive(false);
        slot.Icon.sprite = lockedUpgradeIcon;
        slot.CostFrame.gameObject.SetActive(false);
    }

    #endregion
}