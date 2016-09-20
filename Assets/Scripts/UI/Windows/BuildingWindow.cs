using UnityEngine;

public abstract class BuildingWindow : MonoBehaviour
{
    abstract public TownManager TownManager { get; set; }
    abstract public void Initialize();

    abstract public void WindowClosed();
}