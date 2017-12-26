using UnityEngine;

public abstract class BuildingWindow : MonoBehaviour
{
    protected abstract BuildingType BuildingType { get; }

    public abstract void Initialize();
    public abstract void UpdateUpgradeTrees(bool afterPurchase = false);
    public abstract void WindowOpened();
    public abstract void WindowClosed();
}