using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class PopupDialog : MonoBehaviour
{
    [SerializeField]
    private Animator dialogAnimator;
    [SerializeField]
    private Text dialogText;

    public bool IsActive { get; private set; }

    private string Speech { get; set; }
    private bool Skipable { get; set; }

    private readonly StringBuilder currentText = new StringBuilder("", 100);
    private float additionalShowTime = 1f;
    private float charPerSec = 30;
    private float charTime;
    private int currentIndex;

    private void Update()
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