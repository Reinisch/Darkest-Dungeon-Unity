using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class SaveSlot : MonoBehaviour
{
    Button saveSlotButton;

    Text title;
    InputField titleInput;
    Text location;
    Text currentWeek;

    RectTransform nukeFrame;
    Button nukeSaveButton;

    Animator saveEnvelopeAnimator;

    SaveCampaignData saveData;
    public int slotId;

    public SaveSelector SaveSelector { get; set; }

    void Awake()
    {
        saveSlotButton = transform.FindChild("Save").GetComponent<Button>();
        title = saveSlotButton.transform.FindChild("Title").GetComponent<Text>();
        titleInput = saveSlotButton.transform.FindChild("Title").GetComponent<InputField>();

        location = saveSlotButton.transform.FindChild("Current Location").GetComponent<Text>();
        currentWeek = saveSlotButton.transform.FindChild("Current Week").GetComponent<Text>();

        nukeFrame = transform.FindChild("NukeSaveFrame").GetComponent<RectTransform>();
        nukeSaveButton = nukeFrame.GetComponentInChildren<Button>();

        saveEnvelopeAnimator = transform.FindChild("SaveEnvelope").GetComponent<Animator>();
    }

    void FillEmptySave()
    {
        saveData = null;
        Color color;
        ColorUtility.TryParseHtmlString("#323232FF", out color);
        title.color = color;
        title.text = "Click here to begin campaign...";
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
        title.text = saveData.hamletTitle;
        location.text = String.Format("In: {0}", saveData.locationName);
        currentWeek.text = String.Format("Week {0}", saveData.currentWeek);
        saveEnvelopeAnimator.SetBool("Opened", true);
        nukeFrame.gameObject.SetActive(true);
    }

    public void LoadSaveFrame()
    {
        if(!Directory.Exists(Application.persistentDataPath + "\\Saves\\"))
            Directory.CreateDirectory(Application.persistentDataPath + "\\Saves\\");

        if (File.Exists(Application.persistentDataPath + "\\Saves\\DarkestSave" + slotId + ".darkestsave"))
            saveData = SaveLoadManager.ReadSave(slotId);
        else
            saveData = null;

        if (saveData == null)
            FillEmptySave();
        else
            FillPopulatedSave();
    }
    public void SaveButtonClick()
    {
        if(saveData == null)
        {
            title.color = Color.white;
            titleInput.interactable = true;
            titleInput.enabled = true;
            titleInput.Select();
            SaveSelector.SaveNamingStart(this);
        }
        else
        {
            DarkestDungeonManager.SaveData = saveData;
            if(DarkestDungeonManager.SaveData.InRaid)
            {
                if (!DarkestDungeonManager.SaveData.Quest.IsPlotQuest || 
                    DarkestDungeonManager.SaveData.Quest.Goal.Id == "tutorial_final_room")
                    DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon",
                        "Screen/loading_screen." + DarkestDungeonManager.SaveData.Quest.Dungeon + "_0");
                else
                    DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon",
                        "Screen/loading_screen.plot_" + (DarkestDungeonManager.SaveData.Quest as PlotQuest).Id);
            }
            else
                DarkestDungeonManager.LoadingInfo.SetNextScene("EstateManagement", "Screen/loading_screen.town_visit");

            DarkestDungeonManager.Instanse.LoadSave();
            SaveSelector.FadeToLoadingScreen();
        }
    }
    public void NukeButtonClick()
    {
        SaveLoadManager.DeleteSave(slotId);
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        FillEmptySave();
    }

    public void SaveNamingCompleted()
    {
        if(titleInput.text.Length == 0)
        {
            FillEmptySave();
            return;
        }
        saveData = new SaveCampaignData();
        saveData.hamletTitle = titleInput.text;
        saveData.saveId = slotId;
        SaveLoadManager.WriteStartingSave(saveData);
        saveData = SaveLoadManager.ReadSave(slotId);
        FillPopulatedSave();

        titleInput.interactable = false;
        titleInput.enabled = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        SaveSelector.SaveNamingCompleted();
    }

    public void RefocusInput()
    {
        titleInput.Select();
        titleInput.ActivateInputField();
    }

    public void EnableInteraction()
    {
        saveSlotButton.interactable = true;
        if(nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = true;
    }
    public void DisableInteraction()
    {
        saveSlotButton.interactable = false;
        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = false;
    }
}