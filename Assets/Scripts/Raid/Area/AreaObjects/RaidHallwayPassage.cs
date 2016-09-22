using UnityEngine;

public class RaidHallwayPassage : MonoBehaviour
{
    public RectTransform leftWall;
    public RectTransform rightWall;

    public RectTransform RectTransform { get; set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}