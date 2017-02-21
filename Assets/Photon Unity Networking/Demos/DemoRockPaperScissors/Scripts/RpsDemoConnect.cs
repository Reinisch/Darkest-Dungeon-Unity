using Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RpsDemoConnect : PunBehaviour
{
    public InputField InputField;
    public string UserId;
    public string previousRoom;
    private const string MainSceneName = "DemoRPS-Scene";

	const string NickNamePlayerPrefsKey = "NickName";


	void Start()
	{
		InputField.text = PlayerPrefs.HasKey(NickNamePlayerPrefsKey)?PlayerPrefs.GetString(NickNamePlayerPrefsKey):"";
	}

    public void ApplyUserIdAndConnect()
    {
		string nickName = "DemoNick";
        if (this.InputField != null && !string.IsNullOrEmpty(this.InputField.text))
        {
            nickName = this.InputField.text;
			PlayerPrefs.SetString(NickNamePlayerPrefsKey,nickName);
        }
        //if (string.IsNullOrEmpty(UserId))
        //{
        //    this.UserId = nickName + "ID";
        //}
        Debug.Log("Nickname: " + nickName + " userID: " + this.UserId,this);


        if (PhotonNetwork.AuthValues == null)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
        }
        //else
        //{
        //    Debug.Log("Re-using AuthValues. UserId: " + PhotonNetwork.AuthValues.UserId);
        //}

        PhotonNetwork.playerName = nickName;
        PhotonNetwork.ConnectUsingSettings("0.5");
        
        // this way we can force timeouts by pausing the client (in editor)
        PhotonHandler.StopFallbackSendAckThread();
    }


    public override void OnConnectedToMaster()
    {
        // after connect 
        this.UserId = PhotonNetwork.player.userId;
        ////Debug.Log("UserID " + this.UserId);


        // after timeout: re-join "old" room (if one is known)
        if (!string.IsNullOrEmpty(this.previousRoom))
        {
            Debug.Log("ReJoining previous room: " + this.previousRoom);
            PhotonNetwork.ReJoinRoom(this.previousRoom);
            this.previousRoom = null;       // we only will try to re-join once. if this fails, we will get into a random/new room
        }
        else
        {
            // else: join a random room
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedLobby()
    {
        OnConnectedToMaster(); // this way, it does not matter if we join a lobby or not
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2, PlayerTtl = 5000 }, null);
    }

    public override void OnJoinedRoom()
    {
		Debug.Log("Joined room: " + PhotonNetwork.room.name);
        this.previousRoom = PhotonNetwork.room.name;

    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        this.previousRoom = null;
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        Debug.Log("Disconnected due to: " + cause + ". this.previousRoom: " + this.previousRoom);
    }
}
