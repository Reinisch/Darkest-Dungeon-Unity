using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GlossaryWindow : MonoBehaviour
{
    public RectTransform recordsRect;
    public Camera mainCamera;
    public RectTransform overlayRect;
    public Button closeButton;

    public List<GlossaryRecord> Records { get; set; }

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
