using UnityEngine.UI;

public class StatueWindow : BuildingWindow
{
    public Button closeButton;
    public Scrollbar scrollBar;

    public override TownManager TownManager { get; set; }
    public Statue Statue { get; private set; }

    public override void Initialize()
    {
        Statue = DarkestDungeonManager.Campaign.Estate.Statue;
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
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
    }
}
