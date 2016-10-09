using UnityEngine;
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
        {
            if (gameMovie.movie != null)
                gameMovie.Play();
            else
                FinishIntro();
        }
    }

    public void FinishIntro()
    {
        SceneManager.LoadScene("CampaignSelection");
        SceneManager.UnloadScene("Intro");
    }
}
#endif
