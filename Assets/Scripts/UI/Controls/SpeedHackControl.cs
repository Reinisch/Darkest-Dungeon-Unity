using UnityEngine;
using UnityEngine.EventSystems;

public enum SpeedHackMode { Slower, Faster }

public class SpeedHackControl : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private SpeedHackMode speedHackMode;

    private const int ClickCount = 3;
    private int clicksLeft = ClickCount;

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (--clicksLeft == 0)
        {
            clicksLeft = ClickCount;
            Time.timeScale += speedHackMode == SpeedHackMode.Faster ? 0.4f : -0.4f;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0.2f, 9f);
        }
#endif
    }
}