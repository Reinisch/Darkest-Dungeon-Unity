using UnityEngine;

public class RaidHallwayPassage : MonoBehaviour
{
    public RectTransform LeftWall;
    public RectTransform RightWall;

    public RectTransform RectTransform { get; set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}