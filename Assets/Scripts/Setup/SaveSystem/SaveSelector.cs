using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

// ReSharper disable PossibleLossOfFraction

public class SaveSelector : MonoBehaviour
{
    [SerializeField]
    private Button startCampaignButton;
    [SerializeField]
    private Button returnButton;

    public Button StartCampaignButton { get { return startCampaignButton; } }
    public Button ReturnButton { get { return returnButton; } }

    private const int SlotNumber = 3;

    private SaveSlot[] saveSlots;
    private SaveSlot selectedSaveSlot;
    private RectTransform sceneryRect;
    private Transform saveFrame;
    private bool isSelecting;

    private IEnumerator sliderCoroutine;
    private IEnumerator slideBackCoroutine;
    private IEnumerator fadeCoroutine;

    private void Awake()
    {
        saveFrame = transform.Find("SaveFrame");
        saveSlots = saveFrame.GetComponentsInChildren<SaveSlot>();
        for (int i = 0; i < SlotNumber; i++)
            saveSlots[i].SaveSelector = this;

        startCampaignButton = saveFrame.GetComponentInParent<Button>();
        sceneryRect = StartCampaignButton.transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        saveFrame.gameObject.SetActive(false);
        if(SaveLoadManager.ReadSave(1) == null && SaveLoadManager.ReadSave(2) == null)
        {
            SaveLoadManager.WriteStartingSave(new SaveCampaignData(1, "Darkest"));
            SaveLoadManager.WriteTestingSave(new SaveCampaignData(2, "Middle"));
        }
    }

    private void Update()
    {
        if (isSelecting)
        {
            if (selectedSaveSlot != null)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    selectedSaveSlot.RefocusInput();
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

    public void FadeToLoadingScreen()
    {
        for (int i = 0; i < SlotNumber; i++)
            saveSlots[i].DisableInteraction();

        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();
        
        fadeCoroutine = SceneFade(1, 2500);
        StartCoroutine(fadeCoroutine); 
    }

    public void SaveSelectionStart()
    {
        CampaignSelectionManager.OnSelectionStart(CampaignSelection.Singleplayer);
        saveFrame.gameObject.SetActive(true);

        sliderCoroutine = SceneSlider();
        StartCoroutine(sliderCoroutine);
    }

    public void SaveNamingStart(SaveSlot namingSaveSlot)
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/letter_open");
        
        selectedSaveSlot = namingSaveSlot;
        for (int i = 0; i < SlotNumber; i++)
            saveSlots[i].DisableInteraction();
    }

    public void SaveNamingCompleted()
    {
        selectedSaveSlot = null;
        for (int i = 0; i < SlotNumber; i++)
            saveSlots[i].EnableInteraction();
    }

    public void ReturnButtonClicked()
    {
        if (isSelecting)
        {
            isSelecting = false;
            slideBackCoroutine = SliderBack();
            StartCoroutine(slideBackCoroutine);
        }
    }

    private IEnumerator SceneSlider()
    {
        DarkestSoundManager.PlayOneShot("event:/general/title_screen/campaign_button");

        while (true)
        {
            if (470 < sceneryRect.offsetMax.y)
                break;

            Vector2 offsetMax = sceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * 1200;
            sceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = sceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * 1200;
            sceneryRect.offsetMin = offsetMin;
            yield return null;
        }
        isSelecting = true;

        for (int i = 0; i < SlotNumber; i++)
        {
            saveSlots[i].LoadSaveFrame();
        }
    }

    private IEnumerator SliderBack()
    {
        saveFrame.gameObject.SetActive(false);
        ReturnButton.gameObject.SetActive(false);
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
            yield return null;
        }
        isSelecting = false;
        CampaignSelectionManager.OnSelectionReturn();
    }

    private IEnumerator SceneFade(float seconds, float speed)
    {
        CampaignSelectionManager.Instanse.TitleRect.SetParent(sceneryRect, false);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(sceneryRect,
            new Vector2(Screen.width / 2, Screen.height), DarkestDungeonManager.Instanse.MainUICamera, out localPoint);
        CampaignSelectionManager.Instanse.TitleRect.localPosition = localPoint;

        if (DarkestSoundManager.TitleMusicInstanse != null)
        {
            float titleVolume;
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
            seconds -= Time.deltaTime;

            Vector2 offsetMax = sceneryRect.offsetMax;
            offsetMax.y += Time.deltaTime * speed;
            sceneryRect.offsetMax = offsetMax;
            Vector2 offsetMin = sceneryRect.offsetMin;
            offsetMin.y += Time.deltaTime * speed;
            sceneryRect.offsetMin = offsetMin;
            yield return null;
        }
        SceneManager.LoadScene("LoadingScreen");
    }
}