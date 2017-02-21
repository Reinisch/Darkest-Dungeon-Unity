using UnityEngine;
using System.Collections;

public class MessageOverlay : MonoBehaviour
{
    public GameObject[] Objects;

    public void Start()
    {
        SetActive(true);
    }

    public void OnJoinedRoom()
    {
        SetActive(false);
    }

    public void OnLeftRoom()
    {
        SetActive(true);
    }

    void SetActive(bool enable)
    {
        foreach (GameObject o in Objects)
        {
            #if UNITY_3_5
            o.SetActiveRecursively(enable);
            #else
            o.SetActive(enable);
            #endif
        }
    }
}
