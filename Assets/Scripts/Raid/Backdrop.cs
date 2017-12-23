using UnityEngine;
using UnityEngine.UI;

public class Backdrop : MonoBehaviour
{
    [SerializeField]
    private Image backdropImage;
    [SerializeField]
    private Animator backdropAnimator;

    public void Activate(string backdropName)
    {
        backdropImage.sprite = Resources.Load<Sprite>("Dungeons/shared/" + backdropName);
        backdropAnimator.SetBool("IsActive", true);
    }

    public void Deactivate()
    {
        backdropAnimator.SetBool("IsActive", false);
    }
}
