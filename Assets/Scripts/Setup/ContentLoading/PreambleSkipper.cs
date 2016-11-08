using UnityEngine;
using System.Collections;

public class PreambleSkipper : MonoBehaviour
{
    RectTransform rect;
    IEnumerator slideCoroutine;
    bool isSliding = false;

    float waitForPreable = 3f;
    float appearDelay = 1f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        DarkestSoundManager.PlayTitleMusic(true);
    }

    void Update()
    {
        if (appearDelay > 0)
        {
            appearDelay -= Time.deltaTime;
            if(appearDelay <= 0)
                DarkestDungeonManager.ScreenFader.Appear(1);
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

    IEnumerator StartSceneSlider()
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
        yield break;
    }
}
