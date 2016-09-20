using UnityEngine;
using System.Collections;

public class RectLocker : MonoBehaviour
{
    public RectTransform target;
    public RectTransform rect;

    void FixedUpdate()
    {
        rect.localPosition = target.localPosition;
    }
}
