using UnityEngine.UI;
using System.Collections.Generic;

public class StatueWindow : BuildingWindow
{
    public Button closeButton;
    public Scrollbar scrollBar;

    public List<StatueAudioEntry> audioEntries;

    public override TownManager TownManager { get; set; }
    public Statue Statue { get; private set; }

    void Update()
    {
        scrollBar.size = 0;
    }

    public override void Initialize()
    {
        Statue = DarkestDungeonManager.Campaign.Estate.Statue;
        for(int i = 0; i < audioEntries.Count; i++)
            audioEntries[i].UpdateCondition();
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
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "statue");
            DarkestSoundManager.PlayOneShot("event:/town/enter_statue");
        }
    }

    public override void WindowClosed()
    {
        gameObject.SetActive(false);
        TownManager.BuildingWindowActive = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
    }
}
