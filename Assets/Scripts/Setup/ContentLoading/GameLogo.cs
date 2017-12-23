using UnityEngine;

#if !UNITY_WEBGL
public class GameLogo : MonoBehaviour
{
    private GameIntro gameIntro;

    private void Awake()
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