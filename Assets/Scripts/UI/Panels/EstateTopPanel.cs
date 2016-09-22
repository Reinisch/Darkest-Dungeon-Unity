using UnityEngine;

public class EstateTopPanel : MonoBehaviour
{
    public event WindowEvent onReturnButtonClick;

    public void ReturnButtonClicked()
    {
        if (onReturnButtonClick != null)
            onReturnButtonClick();
    }
}
