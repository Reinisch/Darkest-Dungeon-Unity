using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class PopupDialog : MonoBehaviour
{
    public Animator dialogAnimator;
    public RectTransform textRectTransform;
    public Text dialogText;

    public string Speech { get; private set; }
    public bool IsActive { get; private set; }
    public bool Skipable { get; set; }

    StringBuilder currentText = new StringBuilder("", 100);
    float additionalShowTime = 1f;
    float charPerSec = 30;
    float charTime = 0;
    int currentIndex = 0;

    void Update()
    {
        if (IsActive)
        {
            if (Skipable && Input.GetMouseButtonDown(0))
            {
                dialogAnimator.SetBool("IsShown", false);
                return;
            }

            if (currentIndex < Speech.Length)
            {
                charTime += charPerSec * Time.deltaTime;
                if (charTime >= 1)
                {
                    currentIndex += Mathf.Min((int)charTime, Speech.Length - currentIndex);
                    charTime -= (int)charTime;

                    currentText.Replace("<color=#000000>", "");
                    currentText.Insert(currentIndex, "<color=#000000>");
                    dialogText.text = currentText.ToString();
                }
            }
            else
            {
                if (charTime >= additionalShowTime)
                    dialogAnimator.SetBool("IsShown", false);
                else
                    charTime += Time.deltaTime;
            }
        }
    }

    public void SetCurrentDialog(string speech, bool skipable = true)
    {
        IsActive = true;
        Speech = speech;
        Skipable = skipable;
        currentText.Length = 0;
        currentIndex = 0;
        charTime = 0;
        dialogText.text = "";
        currentText.Append(speech);
        currentText.Append("</color>");
        dialogAnimator.SetBool("IsShown", true);
    }
    public void DeactivateDialog()
    {
        IsActive = false;
    }
}