using UnityEngine;

public abstract class BaseSlot : MonoBehaviour
{
    protected RectTransform RectTransform { get { return rectTran ?? (rectTran = GetComponent<RectTransform>()); } }

    private RectTransform rectTran;
}
