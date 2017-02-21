using UnityEngine;
using System.Collections;

public class GUIFriendsInRoom : MonoBehaviour
{
    public Rect GuiRect;


    void Start()
    {
        GuiRect = new Rect(Screen.width / 4, 80, Screen.width / 2, Screen.height - 100);
    }

    
    public void OnGUI()
    {
        if (!PhotonNetwork.inRoom)
        {
            return;
        }

        GUILayout.BeginArea(GuiRect);

        GUILayout.Label("In-Game");
        GUILayout.Label("For simplicity, this demo just shows the players in this room. The list will expand when more join.");
        GUILayout.Label("Your (random) name: " + PhotonNetwork.playerName);
        GUILayout.Label(PhotonNetwork.playerList.Length + " players in this room.");
        GUILayout.Label("The others are:");
        foreach (PhotonPlayer player in PhotonNetwork.otherPlayers)
        {
            GUILayout.Label(player.ToString());
        }

        if (GUILayout.Button("Leave"))
        {
            PhotonNetwork.LeaveRoom();
        }
        GUILayout.EndArea();
    }
}
