using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image labelBackground;
    [SerializeField]
    private Text nameLabel;
    [SerializeField]
    private Text buildingDescription;
    [SerializeField]
    private SkeletonAnimation currentState;

    public void BuildingSelected()
    {
        labelBackground.enabled = false;
        nameLabel.enabled = false;
        buildingDescription.enabled = false;
        currentState.state.ClearTracks();
        currentState.state.SetAnimation(0, "idle", false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EstateSceneManager.Instanse.TownManager.AnyWindowsOpened)
        {
            labelBackground.enabled = true;
            nameLabel.enabled = true;
            buildingDescription.enabled = true;
            currentState.state.ClearTracks();
            currentState.state.SetAnimation(0, "active", false);
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!EstateSceneManager.Instanse.TownManager.AnyWindowsOpened)
        {
            labelBackground.enabled = false;
            nameLabel.enabled = false;
            buildingDescription.enabled = false;
            currentState.state.ClearTracks();
            currentState.state.SetAnimation(0, "idle", false);
        }
    }
}
