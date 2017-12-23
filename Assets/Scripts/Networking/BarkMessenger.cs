using UnityEngine;
using UnityEngine.UI;

public class BarkMessenger : MonoBehaviour
{
    [SerializeField]
    private InputField chatInputField;
    [SerializeField]
    private Button sendButton;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private Image checkBoxImage;

    private float chatCooldown = 2f;
    private float currentChatCooldown;

    private bool IsChecked { get; set; }

    private void Start()
    {
        IsChecked = false;
        checkBoxImage.enabled = false;
        PhotonGameManager.SkipMessagesOnClick = false;
    }

    private void Update()
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
