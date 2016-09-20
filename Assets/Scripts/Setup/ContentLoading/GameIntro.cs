using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

#if !UNITY_WEBGL
public class GameIntro : MonoBehaviour
{
    GameLogo[] gameLogos;
    public MoviePlayer gameMovie;

    int currentLogo = 0;

    void Awake()
    {
        gameLogos = transform.GetComponentsInChildren<GameLogo>(true);
    }

	void Start()
    {
        if (currentLogo < gameLogos.Length)
            gameLogos[currentLogo].Play();
	}

    public void LogoEnded()
    {
        if (++currentLogo < gameLogos.Length)
            gameLogos[currentLogo].Play();
        else
            gameMovie.Play();
    }

    public void FinishIntro()
    {
        SceneManager.LoadScene("CampaignSelection");
        SceneManager.UnloadScene("Intro");
    }
}
#endif
