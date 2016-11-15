using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RoomSelector : MonoBehaviour
{
    public List<MultiplayerRoomSlot> roomSlots;
    public Button startCampaignButton;
    public Button returnButton;
    public Button refreshButton;

    public RectTransform saveFrame;

    public RectTransform sceneryRect;
    public RectTransform rect;

    #region Multiplayer Launcher UI

    public Button playButton;
    public InputField nicknameField;
    public Image progressPanel;
    public Text progressLabel;

    #endregion

    static List<string> roomNameTemplates = new List<string>()
    {
        "Lepoundmaster", "Lepersader", "Leprusader", "Cruleper",
        "Crusoundmaster", "Crusaster", "Lepster", "Jesleper",
        "Jesteper", "Jesterer", "Jesterper", "Jesteraster",
        "Hourusader", "Manatahunter", "Mancultist", "Moccultist",
        "Manataltist", "Manatarmist" ,"Manatartist", "Bountester",
        "Occulatarms", "Occulthunter", "Occultiser", "Vestalhunter",
        "Gravestal", "Graverosader", "Abomirobber", "Abominestal",
        "Abomisader", "Grabomination", "Vestalnation", "Arbalestal",
    };

    bool isSelecting = false;

    MultiplayerRoomSlot selectedRoomSlot;

    IEnumerator sliderCoroutine;
    IEnumerator slideBackCoroutine;
    IEnumerator fadeCoroutine;

    void Awake()
    {
        Random.InitState(GetInstanceID() + System.DateTime.Now.Millisecond);

        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].RoomSelector = this;
    }
    void Start()
    {
        saveFrame.gameObject.SetActive(false);
    }
    void Update()
    {
        if (isSelecting == true)
        {
            if (selectedRoomSlot != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    selectedRoomSlot.RefocusInput();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                isSelecting = false;
                StopAllCoroutines();
                slideBackCoroutine = SliderBack();
                StartCoroutine(slideBackCoroutine);
            }
        }
    }

    IEnumerator SceneSlider()
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/campaign_button");
        if(PhotonNetwork.connected)
            progressLabel.text = "Connected!";
        else
            progressLabel.text = "Disconnected!";

        DarkestPhotonLauncher.Instanse.ConnectToMaster();

        DisableInteraction();

        while (true)
        {
            if (470 < sceneryRect.offsetMax.y)
                break;

            Vector2 offsetMax = sceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * 800;
            sceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = sceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * 800;
            sceneryRect.offsetMin = offsetMin;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        isSelecting = true;

        RefreshRoomList();
        EnableInteraction();
        yield break;
    }
    IEnumerator SliderBack()
    {
        PhotonNetwork.Disconnect();
        saveFrame.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);

        while (true)
        {
            if (sceneryRect.offsetMax.y <= 0 || sceneryRect.offsetMin.y <= 0)
            {
                sceneryRect.offsetMax = Vector2.zero;
                sceneryRect.offsetMin = Vector2.zero;
                break;
            }

            Vector2 offsetMax = sceneryRect.offsetMax;
            offsetMax.y -= Time.deltaTime * 1200;
            sceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = sceneryRect.offsetMin;
            offsetMin.y -= Time.deltaTime * 1200;
            sceneryRect.offsetMin = offsetMin;
            yield return 0;
        }
        isSelecting = false;
        CampaignSelectionManager.OnSelectionReturn();
        yield break;
    }
    IEnumerator SceneFade(float seconds, float speed)
    {
        float titleVolume = 0;

        if (DarkestSoundManager.TitleMusicInstanse != null)
        {
            DarkestSoundManager.TitleMusicInstanse.getVolume(out titleVolume);

            for (float nextVolume = titleVolume; nextVolume >= 0; nextVolume -= Time.deltaTime * 3f)
            {
                DarkestSoundManager.TitleMusicInstanse.setVolume(nextVolume);
                yield return null;
            }
        }

        DarkestSoundManager.StopTitleMusic();
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/start_game");

        while (true)
        {
            if (seconds <= 0)
                break;
            else
                seconds -= Time.deltaTime;

            Vector2 offsetMax = sceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * speed;
            sceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = sceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * speed;
            sceneryRect.offsetMin = offsetMin;
            yield return 0;
        }

        PhotonNetwork.LoadLevel("DungeonMultiplayer");
        yield break;
    }

    public string GenerateRoomName()
    {
        return roomNameTemplates[Random.Range(0, roomNameTemplates.Count)]
            + "#" + Random.Range(1, 1000).ToString().PadLeft(3, '0');
    }

    public void RefreshRoomList()
    {
        DarkestPhotonLauncher.Instanse.ConnectToMaster();

        if (PhotonNetwork.insideLobby)
        {
            var roomList = PhotonNetwork.GetRoomList();
            int roomsUpdated = Mathf.Min(roomSlots.Count, roomList.Length);

            for (int i = 0; i < roomsUpdated; i++)
                roomSlots[i].LoadSaveFrame(roomList[i]);

            for (int i = roomsUpdated; i < roomSlots.Count; i++)
                roomSlots[i].LoadSaveFrame(null);
        }
        else
        {
            for (int i = 0; i < roomSlots.Count; i++)
                roomSlots[i].LoadSaveFrame(null);
        }
    }

    public void FadeToLoadingScreen()
    {
        DisableInteraction();
        fadeCoroutine = SceneFade(1, 2500);
        StartCoroutine(fadeCoroutine);
    }

    public void SaveSelectionStart()
    {
        CampaignSelectionManager.OnSelectionStart(CampaignSelection.Multiplayer);
        saveFrame.gameObject.SetActive(true);

        sliderCoroutine = SceneSlider();
        StartCoroutine(sliderCoroutine);
    }

    public void SaveNamingStart(MultiplayerRoomSlot namingSaveSlot)
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/letter_open");
        namingSaveSlot.titleInput.text = GenerateRoomName();
        selectedRoomSlot = namingSaveSlot;
        DisableInteraction();
    }

    public void RoomNamingCompleted()
    {
        selectedRoomSlot = null;
    }

    public void ReturnButtonClicked()
    {
        if(isSelecting)
        {
            isSelecting = false;
            slideBackCoroutine = SliderBack();
            StartCoroutine(slideBackCoroutine);
        }
    }

    public void DisableInteraction()
    {
        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].DisableInteraction();

        playButton.interactable = false;
        nicknameField.interactable = false;
        returnButton.interactable = false;
        refreshButton.interactable = false;
    }

    public void EnableInteraction()
    {
        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].EnableInteraction();

        playButton.interactable = true;
        nicknameField.interactable = true;
        returnButton.interactable = true;
        refreshButton.interactable = true;
    }

    public void PlayButtonClicked()
    {
        DarkestPhotonLauncher.Instanse.RandomConnect();
    }
}