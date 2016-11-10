using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class MultiplayerRoomSlot : MonoBehaviour
{
    public Button saveSlotButton;
    public Image roomFrame;

    public Text title;
    public InputField titleInput;
    public Text location;
    public Text currentWeek;

    public RectTransform nukeFrame;
    public Button nukeSaveButton;

    public Animator saveEnvelopeAnimator;

    public RoomSelector RoomSelector { get; set; }

    RoomInfo photonRoom;

    void FillEmptyRoom()
    {
        photonRoom = null;

        Color color;
        ColorUtility.TryParseHtmlString("#323232FF", out color);
        titleInput.textComponent.color = color;
        titleInput.text = "";
        (titleInput.placeholder as Text).text = "Click here to create room...";
        location.text = "";
        currentWeek.text = "";
        saveEnvelopeAnimator.SetBool("Opened", false);
        nukeFrame.gameObject.SetActive(false);
    }
    void FillPopulatedSave()
    {
        if (photonRoom == null)
            FillEmptyRoom();

        Color color;
        ColorUtility.TryParseHtmlString("#FFDB77FF", out color);
        titleInput.textComponent.color = color;
        titleInput.text = photonRoom.name;
        location.text = String.Format("Players");
        currentWeek.text = photonRoom.playerCount + "/" + photonRoom.maxPlayers;
        saveEnvelopeAnimator.SetBool("Opened", true);
        nukeFrame.gameObject.SetActive(true);
    }

    public void LoadSaveFrame(RoomInfo listedRoom)
    {
        photonRoom = listedRoom;

        if (listedRoom == null)
            FillEmptyRoom();
        else
            FillPopulatedSave();
    }
    public void RoomButtonClick()
    {
        if(photonRoom == null)
        {
            RoomSelector.SaveNamingStart(this);
            roomFrame.material = DarkestDungeonManager.HighlightMaterial;
            titleInput.interactable = true;
            titleInput.enabled = true;
            titleInput.textComponent.raycastTarget = true;
            titleInput.placeholder.raycastTarget = true;
            (titleInput.placeholder as Text).text = "";
            titleInput.Select();
        }
        else
        {
            DarkestPhotonLauncher.Instanse.Connect(photonRoom);
        }
    }
    public void NukeButtonClick()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        FillEmptyRoom();
    }

    public void SaveNamingCompleted()
    {
        roomFrame.material = roomFrame.defaultMaterial;

        if (titleInput.text.Length == 0)
        {
            FillEmptyRoom();
            RoomSelector.RoomNamingCompleted();
            return;
        }
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        RoomSelector.RoomNamingCompleted();

        if (!DarkestPhotonLauncher.Instanse.CreateNamedRoom(titleInput.text))
            FillEmptyRoom();
    }

    public void RefocusInput()
    {
        titleInput.Select();
        titleInput.ActivateInputField();
    }

    public void EnableInteraction()
    {
        saveSlotButton.interactable = true;
        titleInput.interactable = true;
        titleInput.textComponent.raycastTarget = false;
        titleInput.placeholder.raycastTarget = false;

        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = true;
    }
    public void DisableInteraction()
    {
        saveSlotButton.interactable = false;
        titleInput.interactable = false;

        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = false;
    }
}