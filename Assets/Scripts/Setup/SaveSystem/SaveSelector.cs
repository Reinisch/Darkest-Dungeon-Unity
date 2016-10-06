using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveSelector : MonoBehaviour
{
    SaveSlot[] saveSlots;
    SaveSlot selectedSaveSlot;
    Button startCampaignButton;
    RectTransform sceneryRect;
    RectTransform rect;
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
        rect = saveFrame.GetComponent<RectTransform>();
    }
	void Start ()
    {
        saveFrame.gameObject.SetActive(false);
        if(SaveLoadManager.ReadSave(1) == null && SaveLoadManager.ReadSave(2) == null)
        {
            SaveLoadManager.WriteStartingSave(new SaveCampaignData(1, "Darkest"));
            SaveLoadManager.WriteTestingSave(new SaveCampaignData(2, "Middle"));
        }
    }
	void Update ()
    {
        if (isSelecting == true)
        {
            if (selectedSaveSlot != null)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    selectedSaveSlot.RefocusInput();
                }
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    selectedSaveSlot.SaveNamingCompleted();
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

        for (int i = 0; i < slotNumber; i++)
        {
            saveSlots[i].LoadSaveFrame();
        }

        yield break;
    }
    IEnumerator SliderBack()
    {
        saveFrame.gameObject.SetActive(false);
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
        startCampaignButton.interactable = true;
        yield break;
    }
    IEnumerator SceneFade(float seconds, float speed)
    {
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
        DarkestSoundManager.StopTitleMusic();
        SceneManager.LoadScene("LoadingScreen");
        yield break;
    }

    public void FadeToLoadingScreen()
    {
        for (int i = 0; i < slotNumber; i++)
            saveSlots[i].DisableInteraction();
        fadeCoroutine = SceneFade(1, 2500);
        StartCoroutine(fadeCoroutine); 
    }

    public void SaveSelectionStart()
    {
        startCampaignButton.interactable = false;
        saveFrame.gameObject.SetActive(true);

        sliderCoroutine = SceneSlider();
        StartCoroutine(sliderCoroutine);

        DarkestSoundManager.PlayOneShot("event:/general/title_screen/campaign_button");
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
}