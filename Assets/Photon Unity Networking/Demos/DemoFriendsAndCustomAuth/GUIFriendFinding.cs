using UnityEngine;
using System.Collections;

public class GUIFriendFinding : MonoBehaviour
{
    private string[] friendListOfSomeCommunity;
    public Rect GuiRect;

	private string ExpectedUsers;

    void Start()
    {
        // If a user should be "findable", the client must set a playerName before connecting.
        // This is then used during connect and the client can be found by others.
        // Setting the playerName before connect, enables others to locate your game:
        PhotonNetwork.playerName = "usr" + (int)Random.Range(0, 9);


        // Photon Cloud does not implement community features for users but can work with external friends lists.
        // We assume you get some list of IDs of your friends.
        friendListOfSomeCommunity = FetchFriendsFromCommunity();


        GuiRect = new Rect(Screen.width / 4, 80, Screen.width / 2, Screen.height - 100);
    }

    
    // In this demo, wo just make up some names instead of connecting somewhere
    public static string[] FetchFriendsFromCommunity()
    {
        string[] friendsList = new string[9];
        int u = 0;
        for (int i = 0; i < friendsList.Length; i++)
        {
            string usrName = "usr" + u++;
            if (usrName.Equals(PhotonNetwork.playerName))
            {
                usrName = "usr" + u++;  // skip friend if the name is yours
            }
            friendsList[i] = usrName;
        }

        return friendsList;
    }


    public void OnUpdatedFriendList()
    {
        Debug.Log("OnUpdatedFriendList is called when the list PhotonNetwork.Friends is refreshed.");
    }


    public void OnGUI()
    {
        if (!PhotonNetwork.connectedAndReady || PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            // this feature is only available on the Master Client. Check either: insideLobby or 
            // PhotonNetwork.connectionStateDetailed == PeerState.Authenticated or 
            // PhotonNetwork.inRoomLobby

            // for simplicity (and cause we know we WILL join the lobby, we can just check that)
            return;
        }


        GUILayout.BeginArea(GuiRect);

        GUILayout.Label("Your (random) name: " + PhotonNetwork.playerName);
        GUILayout.Label("Your friends: " + string.Join(", ",this.friendListOfSomeCommunity));


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Find Friends"))
        {
            PhotonNetwork.FindFriends(this.friendListOfSomeCommunity);
        }
        if (GUILayout.Button("Create Room"))
        {
				PhotonNetwork.CreateRoom(null); // just a random room
        }

		ExpectedUsers = GUILayout.TextField("Expected Users",ExpectedUsers);

        GUILayout.EndHorizontal();


        if (PhotonNetwork.Friends != null)
        {
            foreach (FriendInfo info in PhotonNetwork.Friends)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(info.ToString());
                if (info.IsInRoom)
                {
                    if (GUILayout.Button("join"))
                    {
                        PhotonNetwork.JoinRoom(info.Room);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndArea();
    }
}
