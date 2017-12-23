using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if !UNITY_WEBGL
public class MoviePlayer : MonoBehaviour
{
    [SerializeField]
    private MovieTexture movie;

    private GameIntro gameIntro;
    private AudioSource audioSource;
    private IEnumerator videoCoroutine;
    private int skipFrames = 6;
    private float vol;

    private void Awake()
    {
        gameIntro = GetComponentInParent<GameIntro>();
        movie = (MovieTexture)GetComponent<RawImage>().mainTexture;
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = movie.audioClip;
    }

    private void Update()
    {
        if (Input.anyKey)
            FinishVideo();
    }

    public void Play()
    {
        if (movie == null)
        {
            gameIntro.FinishIntro();
            return;
        }

        gameObject.SetActive(true);
        movie.Play();
        audioSource.Play();
        vol = audioSource.volume;
        GetComponent<RawImage>().color = new Color(255, 255, 255, 0);
        movie.filterMode = FilterMode.Point;
        audioSource.volume = 0;
        videoCoroutine = VideoEnd();
        StartCoroutine(videoCoroutine);
    }

    private void FinishVideo()
    {
        StopCoroutine(videoCoroutine);
        movie.Stop();
        audioSource.Stop();
        gameObject.SetActive(false);
        gameIntro.FinishIntro();
    }

    private IEnumerator VideoEnd()
    {
        while (movie.isPlaying)
        {
            if (skipFrames != 0)
            {
                skipFrames--;
                if (skipFrames == 0)
                {
                    GetComponent<RawImage>().color = new Color(255, 255, 255, 255);
                    audioSource.volume = vol;
                }
            }
            yield return 0;
        }

        FinishVideo();
    }
}
#endif