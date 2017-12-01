using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PickupDemoGui : MonoBehaviour
{
    public bool ShowScores;
    public bool ShowDropButton;
    public bool ShowTeams;
    public float DropOffset = 0.5f;



    public void OnGUI()
    {
        if (!PhotonNetwork.inRoom)
        {
            return;
        }


        if (this.ShowScores)
        {
            GUILayout.Label("Your Score: " + PhotonNetwork.player.GetScore());
        }


        if (this.ShowDropButton)
        {
            foreach (PickupItem item in PickupItem.DisabledPickupItems)
            {
                if (item.PickupIsMine && item.SecondsBeforeRespawn <= 0)
                {
                    if (GUILayout.Button("Drop " + item.name))
                    {
                        item.Drop();    // drops the item at the place where it originates
                    }
                    
                    GameObject playerCharGo = PhotonNetwork.player.TagObject as GameObject;
                    if (playerCharGo != null && GUILayout.Button("Drop here " + item.name))
                    {
                        // drop the item at some other place. next to the user's character would be fine...
                        Vector3 random = Random.insideUnitSphere;
                        random.y = 0;
                        random = random.normalized;
                        Vector3 itempos = playerCharGo.transform.position + this.DropOffset * random;
                        
                        item.Drop(itempos);
                    }
                }
            }
        }


        if (this.ShowTeams)
        {
            foreach (var teamName in PunTeams.PlayersPerTeam.Keys)
            {
                GUILayout.Label("Team: " + teamName.ToString());
                List<PhotonPlayer> teamPlayers = PunTeams.PlayersPerTeam[teamName];
                foreach (PhotonPlayer player in teamPlayers)
                {
                    GUILayout.Label("  " + player.ToStringFull() + " Score: " + player.GetScore());
                }
            }

            if (GUILayout.Button("to red"))
            {
                PhotonNetwork.player.SetTeam(PunTeams.Team.red);
            }
            if (GUILayout.Button("to blue"))
            {
                PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
            }
        }
    }
}