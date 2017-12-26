using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GraveyardWindow : BuildingWindow
{
    [SerializeField]
    private GameObject recordTemplate;
    [SerializeField]
    private RectTransform recordsRect;
    [SerializeField]
    private List<Sprite> resolveIcons;
    [SerializeField]
    private Scrollbar scrollBar;

    public List<Sprite> ResolveIcons { get { return resolveIcons; } }
    protected override BuildingType BuildingType { get { return BuildingType.Graveyard; } }
    private TownManager TownManager { get { return EstateSceneManager.Instanse.TownManager; } }
    private List<DeathRecordSlot> ExistingRecordSlots { get; set; }
    private Graveyard Graveyard { get; set; }

    private void Update()
    {
        scrollBar.size = 0;
    }

    public override void Initialize()
    {
        Graveyard = DarkestDungeonManager.Campaign.Estate.Graveyard;
        ExistingRecordSlots = new List<DeathRecordSlot>();

        foreach (var deathRecord in Graveyard.Records)
        {
            var newRecordSlot = Instantiate(recordTemplate);
            newRecordSlot.transform.SetParent(recordsRect, false);
            var newSlot = newRecordSlot.GetComponent<DeathRecordSlot>();
            newSlot.UpgdateRecord(deathRecord, this);
            ExistingRecordSlots.Add(newSlot);
        }
    }

    public void HeroResurrected(DeathRecord record)
    {
        var resurrectedRecord = ExistingRecordSlots.Find(existingRecord => existingRecord.Record == record);
        if (resurrectedRecord != null)
            Destroy(resurrectedRecord.gameObject);
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
