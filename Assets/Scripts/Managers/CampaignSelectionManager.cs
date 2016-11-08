using UnityEngine;
using System.Collections;

public enum CampaignSelection { Singleplayer, Multiplayer }

public class CampaignSelectionManager : MonoBehaviour
{
    public static CampaignSelectionManager Instanse { get; private set; }

    public SaveSelector saveSelector;
    public RoomSelector roomSelector;

    public static void OnSelectionStart(CampaignSelection selection)
    {
        Instanse.saveSelector.startCampaignButton.interactable = false;
        Instanse.saveSelector.returnButton.gameObject.SetActive(selection == CampaignSelection.Singleplayer);
        Instanse.roomSelector.startCampaignButton.interactable = false;
        Instanse.roomSelector.returnButton.gameObject.SetActive(selection == CampaignSelection.Multiplayer);
    }

    public static void OnSelectionReturn()
    {
        Instanse.saveSelector.startCampaignButton.interactable = true;
        Instanse.saveSelector.returnButton.gameObject.SetActive(false);
        Instanse.roomSelector.startCampaignButton.interactable = true;
        Instanse.roomSelector.returnButton.gameObject.SetActive(false);
    }

    void Awake()
    {
        if (Instanse == null)
            Instanse = this;
        else
            Destroy(gameObject);
    }
}
