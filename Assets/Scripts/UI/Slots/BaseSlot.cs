using UnityEngine;

public abstract class BaseSlot : MonoBehaviour
{
    private RectTransform rectTran;

    public RectTransform RectTransform
    {
        get
        {
            if(rectTran == null)
                rectTran = GetComponent<RectTransform>();

            return rectTran;
        }
        protected set
        {
            rectTran = value;
        }
    }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}
