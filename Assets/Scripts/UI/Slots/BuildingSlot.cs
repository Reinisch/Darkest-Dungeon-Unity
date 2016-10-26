using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string building;

    public Button buildingButton;
    public Image labelBackground;
    public Text nameLabel;
    public Text buildingDescription;

    public List<SkeletonAnimation> states;
    public SkeletonAnimation currentState;

    public TownManager TownManager { get; set; }

    public void BuildingSelected()
    {
        labelBackground.enabled = false;
        nameLabel.enabled = false;
        buildingDescription.enabled = false;
        currentState.state.ClearTracks();
        currentState.state.SetAnimation(0, "idle", false);
        DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, building);
        DarkestSoundManager.PlayOneShot("event:/town/enter_" + building);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!TownManager.AnyWindowsOpened)
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
        if (!TownManager.AnyWindowsOpened)
        {
            labelBackground.enabled = false;
            nameLabel.enabled = false;
            buildingDescription.enabled = false;
            currentState.state.ClearTracks();
            currentState.state.SetAnimation(0, "idle", false);
        }
    }
}
