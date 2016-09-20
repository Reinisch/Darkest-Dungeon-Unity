using UnityEngine;
using System.Collections;

#if !UNITY_WEBGL
public class GameLogo : MonoBehaviour
{
    GameIntro gameIntro;

    void Awake()
    {
        gameIntro = transform.GetComponentInParent<GameIntro>();
    }

    public void Play()
    {
        gameObject.SetActive(true);
    }

    public void LogoEnded()
    {
        gameObject.SetActive(false);
        gameIntro.LogoEnded();
    }
}
#endif