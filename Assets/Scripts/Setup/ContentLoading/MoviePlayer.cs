using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Diagnostics;

#if !UNITY_WEBGL
public class MoviePlayer : MonoBehaviour
{
    GameIntro gameIntro;
    MovieTexture movie;
    AudioSource audioSource;
    IEnumerator videoCoroutine;
    int skipFrames = 6;
    float vol;

    void Awake()
    {
        gameIntro = GetComponentInParent<GameIntro>();
        movie = (MovieTexture)GetComponent<RawImage>().mainTexture;
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = movie.audioClip;
    }

    void Update()
    {
        if (Input.anyKey)
            FinishVideo();
    }

    public void Play()
    {
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

    void FinishVideo()
    {
        StopCoroutine(videoCoroutine);
        movie.Stop();
        audioSource.Stop();
        gameObject.SetActive(false);
        gameIntro.FinishIntro();
    }

    IEnumerator VideoEnd()
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
        yield break;
    }
}
#endif