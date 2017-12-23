using UnityEngine;

public class RectLocker : MonoBehaviour
{
    [SerializeField]
    private RectTransform target;
    [SerializeField]
    private RectTransform rect;

    private void FixedUpdate()
    {
        rect.localPosition = target.localPosition;
    }
}
