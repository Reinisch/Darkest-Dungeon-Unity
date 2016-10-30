using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GraveyardWindow : BuildingWindow
{
    public GameObject recordTemplate;
    public RectTransform recordsRect;

    public List<Sprite> resolveIcons;

    public Button closeButton;
    public Scrollbar scrollBar;
    public ScrollRect scrollRect;

    public override TownManager TownManager { get; set; }
    public Graveyard Graveyard { get; private set; }

    private List<DeathRecordSlot> existingRecordSlots { get; set; }

    public override void Initialize()
    {
        Graveyard = DarkestDungeonManager.Campaign.Estate.Graveyard;
        existingRecordSlots = new List<DeathRecordSlot>();

        foreach (var deathRecord in Graveyard.Records)
        {
            var newRecordSlot = Instantiate(recordTemplate);
            newRecordSlot.transform.SetParent(recordsRect, false);
            var newSlot = newRecordSlot.GetComponent<DeathRecordSlot>();
            newSlot.UpgdateRecord(deathRecord, this);
            existingRecordSlots.Add(newSlot);
        }
    }
    public void HeroResurrected(DeathRecord record)
    {
        var resurrectedRecord = existingRecordSlots.Find(existingRecord => existingRecord.Record == record);
        if (resurrectedRecord != null)
            Destroy(resurrectedRecord.gameObject);
    }

    void Update()
    {
        scrollBar.size = 0;
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {

    }
    public override void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "graveyard");
            DarkestSoundManager.PlayOneShot("event:/town/enter_graveyard");
        }
    }
    public override void WindowClosed()
    {
        gameObject.SetActive(false);
        TownManager.BuildingWindowActive = false;
    }
}
