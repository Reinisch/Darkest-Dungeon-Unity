using ExitGames.Client.Photon;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// A minimal UI to show connection info in a demo.
/// </summary>
public class IELdemo : MonoBehaviour
{
    public GUISkin Skin;

    public void OnGUI()
    {
        if (this.Skin != null)
        {
            GUI.skin = this.Skin;
        }

        if (PhotonNetwork.isMasterClient)
        {
            GUILayout.Label("Controlling client.\nPing: " + PhotonNetwork.GetPing());
            if (GUILayout.Button("disconnect", GUILayout.ExpandWidth(false)))
            {
                PhotonNetwork.Disconnect();
            }
        }
        else if (PhotonNetwork.isNonMasterClientInRoom)
        {
            GUILayout.Label("Receiving updates.\nPing: " + PhotonNetwork.GetPing());
            if (GUILayout.Button("disconnect", GUILayout.ExpandWidth(false)))
            {
                PhotonNetwork.Disconnect();
            }
        }
        else
        {
            GUILayout.Label("Not in room yet\n" + PhotonNetwork.connectionStateDetailed);
        }
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
        {
            if (GUILayout.Button("connect", GUILayout.Width(80)))
            {
                PhotonNetwork.ConnectUsingSettings(null);   // using null as parameter, re-uses previously set version.
            }
        }
    }
}
