using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DarkestPhotonLauncher : Photon.PunBehaviour
{
    #region Public Static Variables

    /// <summary>
    /// Singletone instance for network manager.
    /// </summary>
    public static DarkestPhotonLauncher Instanse { get; private set; }

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

    #region Private Variables

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    #endregion

    #region Public Variables

    /// <summary>
    /// Reference to network selector UI.
    /// </summary>
    public RoomSelector launcherPanel;

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

            // Force log level
            PhotonNetwork.logLevel = LogLevel;

            // We don't join the lobby, not need to join a lobby to get the list of rooms
            PhotonNetwork.autoJoinLobby = false;

            // Auto sync loaded level with master client
            PhotonNetwork.automaticallySyncScene = false;
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// MonoBehaviour method called during initialization phase.
    /// </summary>
    void Start()
    {
        launcherPanel.progressLabel.enabled = false;
        launcherPanel.progressPanel.enabled = true;
    }

    #endregion

    #region PunBehaviour callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Darkest Photon Network: OnConnectedToMaster() was called!");
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if(isConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room.
            // If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed() 
            PhotonNetwork.JoinRandomRoom();
        }
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

        launcherPanel.FadeToLoadingScreen();
    }

    public override void OnDisconnectedFromPhoton()
    {
        launcherPanel.progressLabel.enabled = false;
        launcherPanel.progressPanel.enabled = true;
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
        launcherPanel.progressLabel.enabled = true;
        launcherPanel.progressPanel.enabled = true;
        // keep track of the will to join a room, because when we come back from the game
        // we will get a callback that we are connected, so we need to know what to do then
        isConnecting = true;

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