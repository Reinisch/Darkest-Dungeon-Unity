using UnityEngine;
using System.Collections.Generic;

public class GlossaryWindow : MonoBehaviour
{
    [SerializeField]
    private RectTransform recordsRect;
    [SerializeField]
    private Camera mainCamera;

    public Camera MainCamera { get { return mainCamera; } }
    private List<GlossaryRecord> Records { get; set; }

    public void Initialize()
    {
        Records = new List<GlossaryRecord>(recordsRect.GetComponentsInChildren<GlossaryRecord>(true));
        for (int i = 0; i < Records.Count; i++)
        {
            Records[i].Window = this;
            Records[i].UpdateRecord(i + 1);
        }
    }
}
