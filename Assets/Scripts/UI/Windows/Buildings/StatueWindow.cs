using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class StatueWindow : BuildingWindow
{
    [SerializeField]
    private Scrollbar scrollBar;
    [SerializeField]
    private List<StatueAudioEntry> audioEntries;

    protected override BuildingType BuildingType { get { return BuildingType.Statue; } }

    private void Update()
    {
        scrollBar.size = 0;
    }

    public override void Initialize()
    {
        for(int i = 0; i < audioEntries.Count; i++)
            audioEntries[i].UpdateCondition();
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
    }

    public override void WindowOpened()
    {
        if (!EstateSceneManager.Instanse.TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            EstateSceneManager.Instanse.TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "statue");
            DarkestSoundManager.PlayOneShot("event:/town/enter_statue");
        }
    }

    public override void WindowClosed()
    {
        gameObject.SetActive(false);
        EstateSceneManager.Instanse.TownManager.BuildingWindowActive = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
    }
}
