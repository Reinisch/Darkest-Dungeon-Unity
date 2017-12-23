using UnityEngine;
using System.Collections;

public class PreambleSkipper : MonoBehaviour
{
    private RectTransform rect;
    private IEnumerator slideCoroutine;
    private bool isSliding;

    private float waitForPreable = 3f;
    private float appearDelay = 1f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        DarkestSoundManager.PlayTitleMusic(true);
    }

    private void Update()
    {
        if (appearDelay > 0)
        {
            appearDelay -= Time.deltaTime;
            if(appearDelay <= 0)
                DarkestDungeonManager.ScreenFader.Appear();
        }
        if (waitForPreable > 0)
        {
            waitForPreable -= Time.deltaTime;
            return;
        }

        if (isSliding == false)
        {
            if (Input.anyKey)
            {
                isSliding = true;
                slideCoroutine = StartSceneSlider();
                StartCoroutine(slideCoroutine);
            }
        }
    }

    private IEnumerator StartSceneSlider()
    {
        float distance = rect.offsetMax.y/2;

        while (true)
        {
            Vector2 offsetMax = rect.offsetMax;
            offsetMax.y += Time.deltaTime * 1500;
            rect.offsetMax = offsetMax;
            Vector2 offsetMin = rect.offsetMin;
            offsetMin.y += Time.deltaTime * 1500;
            rect.offsetMin = offsetMin;

            if (distance < 0)
            {
                if(offsetMax.y > distance)
                {
                    distance = 1;
                    DarkestSoundManager.PlayTitleMusic(false);
                }
            }

            if (rect.offsetMax.y >= 0 || rect.offsetMin.y >= 0)
            {
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                break;
            }
            yield return 0;
        }

        CampaignSelectionManager.Instanse.TitleRect.SetParent(CampaignSelectionManager.Instanse.OverlayTitleRect, false);
    }
}
