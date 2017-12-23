using UnityEngine;
using UnityEngine.UI;

public class ScrollBarResizer : MonoBehaviour
{
    private Scrollbar scrollBar;

    private void Awake()
    {
        scrollBar = GetComponent<Scrollbar>();
    }

    private void LateUpdate()
    {
        scrollBar.size = 0;
    }
}