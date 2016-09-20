using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class Backdrop : MonoBehaviour
{
    public Image backdropImage;
    public Animator backdropAnimator;

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
