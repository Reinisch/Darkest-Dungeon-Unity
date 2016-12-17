using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[System.Flags]
public enum PlayerSkillFlags
{
    Empty = 0,
    One = 1,
    Two = 2,
    Three = 4,
    Four = 8,
    Five = 16,
    Six = 32,
    Seven = 64,
}

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
    public static string GameVersion
    {
        get
        {
            return "1.0.3";
        }
    }

    public static List<CloudRegionCode> AvailableRegions = new List<CloudRegionCode>()
    {
        CloudRegionCode.eu,
        CloudRegionCode.us,
        CloudRegionCode.asia,
        CloudRegionCode.jp,
        CloudRegionCode.au,
        CloudRegionCode.usw,
        CloudRegionCode.sa,
        CloudRegionCode.cae,
        CloudRegionCode.kr,
        CloudRegionCode.@in,
    };

    public static CloudRegionCode SelectedRegion = CloudRegionCode.eu;

    public static string RegionToString(CloudRegionCode code)
    {
        switch(code)
        {
            /// <summary>European servers in Amsterdam.</summary>
            case CloudRegionCode.eu:
                return "Europe";
            /// <summary>US servers (East Coast).</summary>
            case CloudRegionCode.us:
                return "US East";
            /// <summary>Asian servers in Singapore.</summary>
            case CloudRegionCode.asia:
                return "Asia";
            /// <summary>Japanese servers in Tokyo.</summary>
            case CloudRegionCode.jp:
                return "Japan";
            /// <summary>Australian servers in Melbourne.</summary>
            case CloudRegionCode.au:
                return "Australia";
            ///<summary>USA West, San José, usw</summary>
            case CloudRegionCode.usw:
                return "US West";
            ///<summary>South America	, Sao Paulo, sa</summary>
            case CloudRegionCode.sa:
                return "South America";
            ///<summary>Canada East, Montreal, cae</summary>
            case CloudRegionCode.cae:
                return "Canada East";
            ///<summary>South Korea, Seoul, kr</summary>
            case CloudRegionCode.kr:
                return "South Korea";
            ///<summary>India, Chennai, in</summary>
            case CloudRegionCode.@in:
                return "India";
            default:
                goto case CloudRegionCode.eu;
        }
    }
    #endregion

    #region Private Variables

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isRandomConnecting;

    bool isNamedConnecting;

    string targetRoomName;

    #endregion

    #region Public Variables

    /// <summary>
    /// Reference to network selector UI.
    /// </summary>
    public RoomSelector launcherPanel;

    public CharacterWindow characterWindow;

    public MultiplayerPartyPanel multiplayerPartyPanel;

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

    #region Properties
    public static CharacterWindow CharacterWindow
    {
        get
        {
            return Instanse.characterWindow;
        }
    }

    public static MultiplayerPartyPanel MultiplayerPartyPanel
    {
        get
        {
            return Instanse.multiplayerPartyPanel;
        }
    }

    public List<Hero> HeroPool { get; set; }

    public List<int> HeroSeeds { get; set; }
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
            PhotonNetwork.autoJoinLobby = true;

            // Auto sync loaded level with master client
            PhotonNetwork.automaticallySyncScene = false;

            PhotonNetwork.MaxResendsBeforeDisconnect = 10;

            PhotonNetwork.QuickResends = 3;
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// MonoBehaviour method called during initialization phase.
    /// </summary>
    void Start()
    {
        HeroPool = new List<Hero>();
        HeroSeeds = new List<int>();

        foreach (var heroClass in DarkestDungeonManager.Data.HeroClasses.Values.ToList())
        {
            for (int i = 0; i < 4; i++)
            {
                string generatedName = LocalizationManager.GetString("hero_name_" + RandomSolver.Next(0, 556).ToString());
                int heroSeed = GetInstanceID() + System.DateTime.Now.Millisecond + (int)System.DateTime.Now.Ticks + i + HeroPool.Count;
                RandomSolver.SetRandomSeed(heroSeed);
                HeroSeeds.Add(heroSeed);
                HeroPool.Add(new Hero(heroClass.StringId, generatedName));
            }
        }

        var initialParty = new List<Hero>(HeroPool).OrderBy(x => RandomSolver.NextDouble()).Take(4).ToList();
        MultiplayerPartyPanel.LoadInitialComposition(initialParty);

        launcherPanel.progressLabel.text = "Disconnected!";
        CampaignSelectionManager.Instanse.roomSelector.regionLabel.text = "Region: " + RegionToString(SelectedRegion);
    }

    #endregion

    #region PunBehaviour callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Darkest Photon Network: OnConnectedToMaster() was called!");
        launcherPanel.progressLabel.text = "Connected!";

        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isRandomConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room.
            // If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed() 
            PhotonNetwork.JoinRandomRoom();
        }
        else if (isNamedConnecting)
        {
            PhotonNetwork.JoinOrCreateRoom(targetRoomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("Darkest Photon Network: OnPhotonRandomJoinFailed() was called!");
        launcherPanel.progressLabel.text = "No available rooms found!";

        // #Critical: we failed to join a random room,
        // Maybe none exists or they are all full. No worries, we create a new room.
        if(PhotonNetwork.CreateRoom(CampaignSelectionManager.Instanse.roomSelector.GenerateRoomName(),
            new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null))
        {
            CampaignSelectionManager.Instanse.roomSelector.DisableInteraction();
        }
        else
        {
            CampaignSelectionManager.Instanse.roomSelector.EnableInteraction();

            isRandomConnecting = false;
            isNamedConnecting = false;
        }

    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        launcherPanel.progressLabel.text = "Can't create room!";

        CampaignSelectionManager.Instanse.roomSelector.EnableInteraction();

        isRandomConnecting = false;
        isNamedConnecting = false;
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        launcherPanel.progressLabel.text = "Disconnected! ";
        switch(cause)
        {
            case DisconnectCause.DisconnectByClientTimeout:
            case DisconnectCause.DisconnectByServerTimeout:
                launcherPanel.progressLabel.text += "Timeout!";
                break;
            case DisconnectCause.MaxCcuReached:
                launcherPanel.progressLabel.text += "Max players amount reached!";
                break;
            case DisconnectCause.InvalidRegion:
                launcherPanel.progressLabel.text += "Invalid region!";
                break;
        }

        if (isNamedConnecting || isRandomConnecting)
        {
            targetRoomName = null;

            CampaignSelectionManager.Instanse.roomSelector.RefreshRoomList();
            CampaignSelectionManager.Instanse.roomSelector.EnableInteraction();

            isRandomConnecting = false;
            isNamedConnecting = false;
        }
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        isNamedConnecting = false;
        Debug.Log("Darkest Photon Network: OnPhotonJoinFailed() was called!");

        // #Critical: we failed to join aroom,
        launcherPanel.progressLabel.text = "Room no longer available!";

        CampaignSelectionManager.Instanse.roomSelector.EnableInteraction();

        isRandomConnecting = false;
        isNamedConnecting = false;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Darkest Photon Network: OnJoinedRoom() was called!");
        launcherPanel.progressLabel.text = "Joined " + (PhotonNetwork.room != null ? PhotonNetwork.room.name : "")  + "!";

        launcherPanel.FadeToLoadingScreen();
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("Darkest Photon Network: OnDisconnectedFromPhoton() was called!");

        if(isNamedConnecting || isRandomConnecting)
        {
            CampaignSelectionManager.Instanse.roomSelector.RefreshRoomList();
            CampaignSelectionManager.Instanse.roomSelector.EnableInteraction();

            isRandomConnecting = false;
            isNamedConnecting = false;
        }
    }

    public override void OnConnectedToPhoton()
    {
        Debug.Log("Darkest Photon Network: OnConnectedToPhoton() was called!");

        launcherPanel.progressLabel.text = "Connected!";
    }

    public override void OnCreatedRoom()
    {
        launcherPanel.progressLabel.text = "Creating " + (PhotonNetwork.room != null ? PhotonNetwork.room.name : "room") + "!";
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Darkest Photon Network: OnJoinedLobby() was called!");
        launcherPanel.progressLabel.text = "Connected!";
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isRandomConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room.
            // If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed() 
            PhotonNetwork.JoinRandomRoom();
        }
        else if (isNamedConnecting)
        {
            PhotonNetwork.JoinOrCreateRoom(targetRoomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        OnConnectionFail(cause);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - If not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void RandomConnect()
    {
        if (!CheckSelectedSkills())
            return;

        CampaignSelectionManager.Instanse.roomSelector.DisableInteraction();

        launcherPanel.progressLabel.enabled = true;
        launcherPanel.progressPanel.enabled = true;
        // keep track of the will to join a room, because when we come back from the game
        // we will get a callback that we are connected, so we need to know what to do then
        isRandomConnecting = true;

        UpdateCustomProperties();

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
            //PhotonNetwork.ConnectUsingSettings(GameVersion);
            PhotonNetwork.ConnectToRegion(SelectedRegion, GameVersion);
        }
    }

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining target room
    /// - If not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect(RoomInfo targetRoom)
    {
        if(!CheckSelectedSkills())
            return;

        CampaignSelectionManager.Instanse.roomSelector.DisableInteraction();

        launcherPanel.progressLabel.enabled = true;
        launcherPanel.progressPanel.enabled = true;
        // keep track of the will to join a room, because when we come back from the game
        // we will get a callback that we are connected, so we need to know what to do then
        isNamedConnecting = true;
        targetRoomName = targetRoom.name;

        UpdateCustomProperties();

        // We check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected)
        {
            // #Critical: We need at this point to attempt joining a Random Room.
            // If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRoom(targetRoom.name);
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            //PhotonNetwork.ConnectUsingSettings(GameVersion);
            PhotonNetwork.ConnectToRegion(SelectedRegion, GameVersion);
        }
    }

    public void ConnectToMaster()
    {
        if (!PhotonNetwork.connected)
            PhotonNetwork.ConnectToRegion(SelectedRegion, GameVersion);
    }

    public bool CreateNamedRoom(string roomName)
    {
        isNamedConnecting = true;
        targetRoomName = roomName;

        UpdateCustomProperties();

        if (!PhotonNetwork.connected)
        {
            launcherPanel.progressLabel.text = "Can't create room! No connection!";
            return PhotonNetwork.ConnectToRegion(SelectedRegion, GameVersion);
        }

        return PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public bool CheckSelectedSkills()
    {
        for (int i = 0; i < MultiplayerPartyPanel.PartySlots.Count; i++)
        {
            if (MultiplayerPartyPanel.PartySlots[i].SelectedHero.SelectedCombatSkills.Count < 4)
            {
                launcherPanel.progressLabel.text = "Choose at least 4 skills for eath hero!";
                return false;
            }
        }
        return true;
    }

    public void UpdateCustomProperties()
    {
        var playerProps = new ExitGames.Client.Photon.Hashtable();

        for (int i = 0; i < MultiplayerPartyPanel.PartySlots.Count; i++)
        {
            var hero = MultiplayerPartyPanel.PartySlots[i].SelectedHero;
            playerProps.Add("HC" + (i + 1).ToString(), MultiplayerPartyPanel.PartySlots[i].SelectedHero.ClassStringId);
            playerProps.Add("HN" + (i + 1).ToString(), MultiplayerPartyPanel.PartySlots[i].SelectedHero.Name);
            playerProps.Add("HS" + (i + 1).ToString(), HeroSeeds[HeroPool.IndexOf(hero)]);

            var skillFlags = PlayerSkillFlags.Empty;
            for (int j = 0; j < hero.CurrentCombatSkills.Length; j++)
                if (hero.CurrentCombatSkills[j] != null && hero.SelectedCombatSkills.Contains(hero.CurrentCombatSkills[j]))
                    skillFlags |= (PlayerSkillFlags)Mathf.Pow(2, j + 1);

            playerProps.Add("HF" + (i + 1).ToString(), skillFlags);
        }
        PhotonNetwork.player.SetCustomProperties(playerProps, playerProps, false);
    }

    public void NextRegion()
    {
        int targetIndex = AvailableRegions.IndexOf(SelectedRegion) + 1;
        if (targetIndex > AvailableRegions.Count - 1)
            targetIndex = 0;

        SelectedRegion = AvailableRegions[targetIndex];
        CampaignSelectionManager.Instanse.roomSelector.regionLabel.text = "Region: " + RegionToString(SelectedRegion);

        if(PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        CampaignSelectionManager.Instanse.roomSelector.CleanRoomList();
        launcherPanel.progressLabel.text = "Disconnected!";
    }
    
    public void PrevRegion()
    {
        int targetIndex = AvailableRegions.IndexOf(SelectedRegion) - 1;
        if (targetIndex < 0)
            targetIndex = AvailableRegions.Count - 1;

        SelectedRegion = AvailableRegions[targetIndex];
        CampaignSelectionManager.Instanse.roomSelector.regionLabel.text = "Region: " + RegionToString(SelectedRegion);

        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        CampaignSelectionManager.Instanse.roomSelector.CleanRoomList();
        launcherPanel.progressLabel.text = "Disconnected!";
    }

    #endregion
}