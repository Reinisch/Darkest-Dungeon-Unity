using UnityEngine;

public abstract class BuildingWindow : MonoBehaviour
{
    public abstract TownManager TownManager { get; set; }

    public abstract void Initialize();
    public abstract void UpdateUpgradeTrees(bool afterPurchase = false);
    public abstract void WindowOpened();
    public abstract void WindowClosed();
}