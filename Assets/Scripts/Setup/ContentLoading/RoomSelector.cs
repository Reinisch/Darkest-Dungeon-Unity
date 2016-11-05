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

    public RectTransform saveFrame;

    public RectTransform sceneryRect;
    public RectTransform rect;

    #region Multiplayer Launcher UI

    public Button playButton;
    public InputField nicknameField;
    public Image progressPanel;
    public Text progressLabel;

    #endregion

    bool isSelecting = false;

    MultiplayerRoomSlot selectedRoomSlot;

    IEnumerator sliderCoroutine;
    IEnumerator slideBackCoroutine;
    IEnumerator fadeCoroutine;

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
                slideBackCoroutine = SliderBack();
                StartCoroutine(slideBackCoroutine);
            }
        }
    }

    IEnumerator SceneSlider()
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/campaign_button");

        while (true)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            if (Screen.height * 0.3f < screenPos.y)
                break;

            Vector2 offsetMax = sceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * 1200;
            sceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = sceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * 1200;
            sceneryRect.offsetMin = offsetMin;
            yield return 0;
        }
        isSelecting = true;

        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].LoadSaveFrame();

        yield break;
    }
    IEnumerator SliderBack()
    {
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
        SceneManager.LoadScene("LoadingScreen");
        yield break;
    }

    public void FadeToLoadingScreen()
    {
        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].DisableInteraction();
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

        selectedRoomSlot = namingSaveSlot;
        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].DisableInteraction();
    }

    public void RoomNamingCompleted()
    {
        selectedRoomSlot = null;
        for (int i = 0; i < roomSlots.Count; i++)
            roomSlots[i].EnableInteraction();
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

    public void PlayButtonClicked()
    {
        DarkestPhotonLauncher.Instanse.Connect();
    }
}