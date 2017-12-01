using UnityEngine;
using System.Collections;

using UnityEngine.UI;


/// <summary>
/// This is used in the Editor Splash to properly inform the developer about the chat AppId requirement.
/// </summary>
[ExecuteInEditMode]
public class ChatAppIdCheckerUI : MonoBehaviour
{
    public Text Description;

    public void Update()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID))
        {
            Description.text = "<Color=Red>WARNING:</Color>\nTo run this demo, please set the Chat AppId in the PhotonServerSettings file.";
        }
        else
        {
            Description.text = string.Empty;
        }
    }
}