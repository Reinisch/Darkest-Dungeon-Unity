using UnityEngine;

public abstract class BuildingWindow : MonoBehaviour
{
    abstract public TownManager TownManager { get; set; }
    abstract public void Initialize();

    abstract public void UpdateUpgradeTrees(bool afterPurchase = false);

    abstract public void WindowOpened();
    abstract public void WindowClosed();
}