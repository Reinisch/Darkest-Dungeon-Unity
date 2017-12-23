using UnityEngine;
using UnityEngine.SceneManagement;

#if !UNITY_WEBGL
public class GameIntro : MonoBehaviour
{
    [SerializeField]
    private MoviePlayer gameMovie;

    private GameLogo[] gameLogos;
    private int currentLogo;

    private void Awake()
    {
        gameLogos = transform.GetComponentsInChildren<GameLogo>(true);
    }

    private void Start()
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
    }
}
#endif
