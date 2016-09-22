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

    public override void Initialize()
    {
        Graveyard = DarkestDungeonManager.Campaign.Estate.Graveyard;
        
        foreach(var deathRecord in Graveyard.Records)
        {
            var newRecordSlot = Instantiate(recordTemplate);
            newRecordSlot.transform.SetParent(recordsRect, false);
            newRecordSlot.GetComponent<DeathRecordSlot>().UpgdateRecord(deathRecord, this);
        }
    }

    void Update()
    {
        scrollBar.size = 0;
    }

    public void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
        }
    }

    public override void WindowClosed()
    {
        gameObject.SetActive(false);
        TownManager.BuildingWindowActive = false;
    }
}
