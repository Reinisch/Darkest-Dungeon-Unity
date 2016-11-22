using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BarkMessenger : MonoBehaviour
{
    public InputField chatInputField;
    public Button sendButton;
    public Text buttonText;
    public Image sendIcon;

    public Image checkBoxImage;
    public bool IsChecked { get; set; }

    float chatCooldown = 2f;
    float currentChatCooldown = 0;

    void Start()
    {
        IsChecked = false;
        checkBoxImage.enabled = false;
        PhotonGameManager.SkipMessagesOnClick = false;
    }

    void Update()
    {
        if(currentChatCooldown > 0)
        {
            currentChatCooldown -= Time.deltaTime;

            if(currentChatCooldown <= 0)
            {
                chatInputField.interactable = true;
                sendButton.interactable = true;
                buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 1);
            }
        }
    }

    public void CheckBoxClicked()
    {
        IsChecked = !IsChecked;
        PhotonGameManager.SkipMessagesOnClick = IsChecked;
        checkBoxImage.enabled = IsChecked;
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
    }

    public void SendButtonClicked()
    {
        if (chatInputField.text.Trim(' ').Length == 0)
            return;

        if (PhotonNetwork.otherPlayers.Length == 0)
        {
            chatInputField.text = "You are the only one in the room!";
            return;
        }

        PhotonGameManager.Instanse.photonView.RPC("ExecuteBarkMessage", PhotonTargets.All, 
            PhotonNetwork.isMasterClient ? (int)Team.Heroes : (int)Team.Monsters, chatInputField.text);

        chatInputField.interactable = false;
        sendButton.interactable = false;
        buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0.5f);
        chatInputField.text = "";
        currentChatCooldown = chatCooldown;
    }
}
