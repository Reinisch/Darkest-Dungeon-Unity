using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public enum SpeedHackMode { Slower, Faster }

public class SpeedHackControl : MonoBehaviour, IPointerClickHandler
{
    public SpeedHackMode speedHackMode;

    private const int clickCount = 3;
    private int clicksLeft = clickCount;

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (--clicksLeft == 0)
        {
            clicksLeft = clickCount;
            Time.timeScale += speedHackMode == SpeedHackMode.Faster ? 0.4f : -0.4f;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0.2f, 9f);
        }
#endif
    }
}