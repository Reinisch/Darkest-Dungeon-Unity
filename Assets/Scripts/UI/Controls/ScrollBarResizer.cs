using UnityEngine;
using UnityEngine.UI;

public class ScrollBarResizer : MonoBehaviour
{
    Scrollbar scrollBar;

    void Awake()
    {
        scrollBar = GetComponent<Scrollbar>();
    }

    void LateUpdate()
    {
        scrollBar.size = 0;
    }
}