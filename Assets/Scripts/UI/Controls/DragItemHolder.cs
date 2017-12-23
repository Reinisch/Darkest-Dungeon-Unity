using UnityEngine;
using UnityEngine.UI;

public class DragItemHolder : MonoBehaviour
{
    [SerializeField]
    private Image backIcon;
    [SerializeField]
    private Image itemIcon;
    [SerializeField]
    private RectTransform rectTransform;

    public Image BackIcon { get { return backIcon; } }
    public Image ItemIcon { get { return itemIcon; } }
    public RectTransform RectTransform { get { return rectTransform; } }
}
