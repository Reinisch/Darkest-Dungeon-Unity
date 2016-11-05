using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (ChatGui))]
public class NamePickGui : MonoBehaviour
{
    private const string UserNamePlayerPref = "NamePickUserName";

    public ChatGui chatNewComponent;
    public RectTransform AppIdInputPanel;
    public RectTransform NameInputPanel;
    public InputField idInput;

    public void Start()
    {
        this.chatNewComponent = FindObjectOfType<ChatGui>();

        bool appIdMissing = false; // string.IsNullOrEmpty(ChatSettings.Instance.AppId);
        this.AppIdInputPanel.gameObject.SetActive(appIdMissing);
        this.NameInputPanel.gameObject.SetActive(!appIdMissing);

        string prefsName = PlayerPrefs.GetString(NamePickGui.UserNamePlayerPref);
        if (!string.IsNullOrEmpty(prefsName))
        {
            this.idInput.text = prefsName;
        }
    }


    public void OpenDashboard()
    {
        Application.OpenURL("https://www.exitgames.com/en/Chat/Dashboard");
    }


    // new UI will fire "EndEdit" event also when loosing focus. So check "enter" key and only then StartChat.
    public void EndEditOnEnter()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            this.StartChat();
        }
    }

    public void StartChat()
    {
        ChatGui chatNewComponent = FindObjectOfType<ChatGui>();
        chatNewComponent.UserName = this.idInput.text.Trim();
        chatNewComponent.enabled = true;
        enabled = false;
        this.AppIdInputPanel.gameObject.SetActive(false);
        this.NameInputPanel.gameObject.SetActive(false);

        PlayerPrefs.SetString(NamePickGui.UserNamePlayerPref, chatNewComponent.UserName);
    }
}