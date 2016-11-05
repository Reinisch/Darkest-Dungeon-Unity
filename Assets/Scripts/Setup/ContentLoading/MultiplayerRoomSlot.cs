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

    public Text title;
    public InputField titleInput;
    public Text location;
    public Text currentWeek;

    public RectTransform nukeFrame;
    public Button nukeSaveButton;

    public Animator saveEnvelopeAnimator;

    public RoomSelector RoomSelector { get; set; }

    void FillEmptySave()
    {
        Color color;
        ColorUtility.TryParseHtmlString("#323232FF", out color);
        title.color = color;
        title.text = "Click here to create room...";
        location.text = "";
        currentWeek.text = "";
        saveEnvelopeAnimator.SetBool("Opened", false);
        nukeFrame.gameObject.SetActive(false);
    }
    void FillPopulatedSave()
    {
        Color color;
        ColorUtility.TryParseHtmlString("#FFDB77FF", out color);
        title.color = color;
        title.text = "";
        location.text = String.Format("In: {0}", "Ruins");
        currentWeek.text = String.Format("Round {0}", "10");
        saveEnvelopeAnimator.SetBool("Opened", true);
        nukeFrame.gameObject.SetActive(true);
    }

    public void LoadSaveFrame()
    {
        FillEmptySave();
    }
    public void RoomButtonClick()
    {
        title.color = Color.white;
        titleInput.interactable = true;
        titleInput.enabled = true;
        titleInput.Select();
    }
    public void NukeButtonClick()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        FillEmptySave();
    }

    public void SaveNamingCompleted()
    {
        if (titleInput.text.Length == 0)
        {
            FillEmptySave();
            return;
        }
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        RoomSelector.RoomNamingCompleted();
    }

    public void RefocusInput()
    {
        titleInput.Select();
        titleInput.ActivateInputField();
    }

    public void EnableInteraction()
    {
        saveSlotButton.interactable = true;
        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = true;
    }
    public void DisableInteraction()
    {
        saveSlotButton.interactable = false;
        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = false;
    }
}