using UnityEngine;

public enum CampaignSelection
{
    Singleplayer,
    Multiplayer
}

public class CampaignSelectionManager : MonoBehaviour
{
    public static CampaignSelectionManager Instanse { get; private set; }

    [SerializeField]
    private SaveSelector saveSelector;
    [SerializeField]
    private RoomSelector roomSelector;
    [SerializeField]
    private RectTransform titleRect;
    [SerializeField]
    private RectTransform overlayTitleRect;

    public RoomSelector RoomSelector { get { return roomSelector; } }
    public RectTransform TitleRect { get { return titleRect; } }
    public RectTransform OverlayTitleRect { get { return overlayTitleRect; } }

    public static void OnSelectionStart(CampaignSelection selection)
    {
        Instanse.saveSelector.StartCampaignButton.interactable = false;
        Instanse.saveSelector.ReturnButton.gameObject.SetActive(selection == CampaignSelection.Singleplayer);
        Instanse.RoomSelector.StartCampaignButton.interactable = false;
        Instanse.RoomSelector.ReturnButton.gameObject.SetActive(selection == CampaignSelection.Multiplayer);
    }

    public static void OnSelectionReturn()
    {
        Instanse.saveSelector.StartCampaignButton.interactable = true;
        Instanse.saveSelector.ReturnButton.gameObject.SetActive(false);
        Instanse.RoomSelector.StartCampaignButton.interactable = true;
        Instanse.RoomSelector.ReturnButton.gameObject.SetActive(false);
    }

    private void Awake()
    {
        if (Instanse == null)
            Instanse = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        DarkestDungeonManager.Instanse.UpdateSceneOverlay(FindObjectOfType<Camera>());
    }
}
