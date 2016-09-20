using UnityEngine;
using System.Collections;

public class PreambleSkipper : MonoBehaviour
{
    RectTransform rect;
    IEnumerator slideCoroutine;
    bool isSliding = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
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
        while (true)
        {
            Vector2 offsetMax = rect.offsetMax;
            offsetMax.y += Time.deltaTime * 1000;
            rect.offsetMax = offsetMax;
            Vector2 offsetMin = rect.offsetMin;
            offsetMin.y += Time.deltaTime * 1000;
            rect.offsetMin = offsetMin;

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
