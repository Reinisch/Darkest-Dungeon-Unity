using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveSelector : MonoBehaviour
{
    SaveSlot[] saveSlots;
    SaveSlot selectedSaveSlot;
    public Button startCampaignButton;
    public Button returnButton;

    RectTransform sceneryRect;
    Transform saveFrame;
    bool isSelecting = false;
    const int slotNumber = 3;

    IEnumerator sliderCoroutine;
    IEnumerator slideBackCoroutine;
    IEnumerator fadeCoroutine;

    void Awake()
    {
        saveFrame = transform.FindChild("SaveFrame");
        saveSlots = saveFrame.GetComponentsInChildren<SaveSlot>();
        for (int i = 0; i < slotNumber; i++)
            saveSlots[i].SaveSelector = this;

        startCampaignButton = saveFrame.GetComponentInParent<Button>();
        sceneryRect = startCampaignButton.transform.parent.GetComponent<RectTransform>();
    }
	void Start()
    {
        saveFrame.gameObject.SetActive(false);
        if(SaveLoadManager.ReadSave(1) == null && SaveLoadManager.ReadSave(2) == null)
        {
            SaveLoadManager.WriteStartingSave(new SaveCampaignData(1, "Darkest"));
            SaveLoadManager.WriteTestingSave(new SaveCampaignData(2, "Middle"));
        }
    }
	void Update()
    {
        if (isSelecting == true)
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

    IEnumerator SceneSlider()
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
            yield return 0;
        }
        isSelecting = true;

        for (int i = 0; i < slotNumber; i++)
        {
            saveSlots[i].LoadSaveFrame();
        }

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
        CampaignSelectionManager.Instanse.titleRect.SetParent(sceneryRect, false);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(sceneryRect,
            new Vector2(Screen.width / 2, Screen.height), DarkestDungeonManager.Instanse.MainUICamera, out localPoint);
        CampaignSelectionManager.Instanse.titleRect.localPosition = localPoint;

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
        for (int i = 0; i < slotNumber; i++)
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
        for (int i = 0; i < slotNumber; i++)
            saveSlots[i].DisableInteraction();
    }

    public void SaveNamingCompleted()
    {
        selectedSaveSlot = null;
        for (int i = 0; i < slotNumber; i++)
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
}