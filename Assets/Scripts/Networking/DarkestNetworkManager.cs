using UnityEngine;
using System.Collections;

public class DarkestNetworkManager : Photon.PunBehaviour
{
    #region Public Static Variables

    /// <summary>
    /// Singletone instance for network manager.
    /// </summary>
    public static DarkestNetworkManager Instanse { get; private set; }

    #endregion

    #region Private Static Variables

    /// <summary>
    /// Client's version number. Users are separated from each other by gameversion.
    /// </summary>
    private static string GameVersion
    {
        get
        {
            return "1";
        }
    }

    #endregion

    #region Public Variables

    /// <summary>
    /// The PUN log level.
    /// </summary>
    public PhotonLogLevel LogLevel = PhotonLogLevel.Informational;

    /// <summary>
    /// The maximum number of players per room. When a room is full, 
    /// it can't be joined by new players, and so new room will be created.
    /// </summary>
    public byte MaxPlayersPerRoom = 2;

    #endregion

    #region MonoBehavior callbacks

    /// <summary>
    /// MonoBehaviour method called during early initialization phase.
    /// </summary>
    void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;
            DontDestroyOnLoad(gameObject);

            // Force log level
            PhotonNetwork.logLevel = LogLevel;

            // We don't join the lobby, not need to join a lobby to get the list of rooms
            PhotonNetwork.autoJoinLobby = false;

            // Auto sync loaded level with master client
            PhotonNetwork.automaticallySyncScene = true;
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// MonoBehaviour method called during initialization phase.
    /// </summary>
    void Start()
    {
    }

    #endregion

    #region PunBehaviour callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Darkest Photon Network: OnConnectedToMaster() was called!");
        // #Critical: The first we try to do is to join a potential existing room.
        // If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed() 
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("Darkest Photon Network: OnPhotonRandomJoinFailed() was called!");

        // #Critical: we failed to join a random room,
        // Maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Darkest Photon Network: OnJoinedRoom() was called!");
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("Darkest Photon Network: OnDisconnectedFromPhoton() was called!");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - If not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        // We check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected)
        {
            // #Critical: We need at this point to attempt joining a Random Room.
            // If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings(GameVersion);
        }
    }

    #endregion
}