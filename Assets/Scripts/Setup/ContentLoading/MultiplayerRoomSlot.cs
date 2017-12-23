using UnityEngine;
using UnityEngine.UI;

public class MultiplayerRoomSlot : MonoBehaviour
{
    [SerializeField]
    private Button saveSlotButton;
    [SerializeField]
    private Image roomFrame;
    [SerializeField]
    private InputField titleInput;
    [SerializeField]
    private Text location;
    [SerializeField]
    private Text currentWeek;
    [SerializeField]
    private RectTransform nukeFrame;
    [SerializeField]
    private Button nukeSaveButton;
    [SerializeField]
    private Animator saveEnvelopeAnimator;

    public RoomSelector RoomSelector { private get; set; }
    public InputField TitleInput { get { return titleInput; } }

    private RoomInfo photonRoom;

    private void FillEmptyRoom()
    {
        photonRoom = null;

        Color color;
        ColorUtility.TryParseHtmlString("#323232FF", out color);
        TitleInput.textComponent.color = color;
        TitleInput.text = "";
        ((Text)TitleInput.placeholder).text = "Click here to create room...";
        location.text = "";
        currentWeek.text = "";
        saveEnvelopeAnimator.SetBool("Opened", false);
        nukeFrame.gameObject.SetActive(false);
    }

    private void FillPopulatedSave()
    {
        if (photonRoom == null)
            FillEmptyRoom();

        Color color;
        ColorUtility.TryParseHtmlString("#FFDB77FF", out color);
        TitleInput.textComponent.color = color;
        TitleInput.text = photonRoom.Name;
        location.text = "Players";
        currentWeek.text = photonRoom.PlayerCount + "/" + photonRoom.MaxPlayers;
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
        if (!DarkestPhotonLauncher.Instanse.CheckSelectedSkills())
            return;

        if (photonRoom == null)
        {
            RoomSelector.SaveNamingStart(this);
            roomFrame.material = DarkestDungeonManager.HighlightMaterial;
            TitleInput.interactable = true;
            TitleInput.enabled = true;
            TitleInput.textComponent.raycastTarget = true;
            TitleInput.placeholder.raycastTarget = true;
            (TitleInput.placeholder as Text).text = "";
            TitleInput.Select();
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

        if (TitleInput.text.Length == 0)
        {
            FillEmptyRoom();
            RoomSelector.RoomNamingCompleted();
            RoomSelector.EnableInteraction();
            return;
        }
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
        RoomSelector.RoomNamingCompleted();

        if (!DarkestPhotonLauncher.Instanse.CreateNamedRoom(TitleInput.text))
            FillEmptyRoom();
    }

    public void RefocusInput()
    {
        TitleInput.Select();
        TitleInput.ActivateInputField();
    }

    public void EnableInteraction()
    {
        saveSlotButton.interactable = true;
        TitleInput.interactable = true;
        TitleInput.textComponent.raycastTarget = false;
        TitleInput.placeholder.raycastTarget = false;

        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = true;
    }

    public void DisableInteraction()
    {
        saveSlotButton.interactable = false;
        TitleInput.interactable = false;

        if (nukeFrame.gameObject.activeSelf)
            nukeSaveButton.interactable = false;
    }
}