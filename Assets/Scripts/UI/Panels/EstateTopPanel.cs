using System;
using UnityEngine;

public class EstateTopPanel : MonoBehaviour
{
    public event Action EventReturnButtonClick;

    public void ReturnButtonClicked()
    {
        if (EventReturnButtonClick != null)
            EventReturnButtonClick();
    }
}
