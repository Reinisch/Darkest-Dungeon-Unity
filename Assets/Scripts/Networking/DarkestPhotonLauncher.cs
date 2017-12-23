using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    #region Static fields and methods

    /// <summary>
    /// Client's version number. Users are separated from each other by gameversion.
    /// </summary>
    public static string GameVersion { get { return "1.0.3"; } }
    public static DarkestPhotonLauncher Instanse { get; private set; }

    private static CloudRegionCode selectedRegion = CloudRegionCode.eu;
    private static PhotonLogLevel LogLevel = PhotonLogLevel.Informational;
    private static byte MaxPlayersPerRoom = 2;

    private static readonly List<CloudRegionCode> AvailableRegions = new List<CloudRegionCode>
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

    private static string RegionToString(CloudRegionCode code)
    {
        switch(code)
        {
            case CloudRegionCode.eu:
                return "Europe";
            case CloudRegionCode.us:
                return "US East";
            case CloudRegionCode.asia:
                return "Asia";
            case CloudRegionCode.jp:
                return "Japan";
            case CloudRegionCode.au:
                return "Australia";
            case CloudRegionCode.usw:
                return "US West";
            case CloudRegionCode.sa:
                return "South America";
            case CloudRegionCode.cae:
                return "Canada East";
            case CloudRegionCode.kr:
                return "South Korea";
            case CloudRegionCode.@in:
                return "India";
            default:
                goto case CloudRegionCode.eu;
        }
    }

    #endregion

    private bool isRandomConnecting;
    private bool isNamedConnecting;
    private string targetRoomName;

    [SerializeField]
    private RoomSelector launcherPanel;
    [SerializeField]
    private CharacterWindow characterWindow;
    [SerializeField]
    private MultiplayerPartyPanel multiplayerPartyPanel;

    public static CharacterWindow CharacterWindow { get { return Instanse.characterWindow; } }
    public static List<Hero> HeroPool { get; private set; }
    public static List<int> HeroSeeds { get; private set; }

    private void Awake()
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

    private void Start()
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
        multiplayerPartyPanel.LoadInitialComposition(initialParty);

        launcherPanel.ProgressLabel.text = "Disconnected!";
        CampaignSelectionManager.Instanse.RoomSelector.RegionLabel.text = "Region: " + RegionToString(selectedRegion);
    }

    #region PunBehaviour callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Darkest Photon Network: OnConnectedToMaster() was called!");
        launcherPanel.ProgressLabel.text = "Connected!";

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
            PhotonNetwork.JoinOrCreateRoom(targetRoomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("Darkest Photon Network: OnPhotonRandomJoinFailed() was called!");
        launcherPanel.ProgressLabel.text = "No available rooms found!";

        // #Critical: we failed to join a random room,
        // Maybe none exists or they are all full. No worries, we create a new room.
        if(PhotonNetwork.CreateRoom(CampaignSelectionManager.Instanse.RoomSelector.GenerateRoomName(),
            new RoomOptions { MaxPlayers = MaxPlayersPerRoom }, null))
        {
            CampaignSelectionManager.Instanse.RoomSelector.DisableInteraction();
        }
        else
        {
            CampaignSelectionManager.Instanse.RoomSelector.EnableInteraction();

            isRandomConnecting = false;
            isNamedConnecting = false;
        }
    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        launcherPanel.ProgressLabel.text = "Can't create room!";

        CampaignSelectionManager.Instanse.RoomSelector.EnableInteraction();

        isRandomConnecting = false;
        isNamedConnecting = false;
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        launcherPanel.ProgressLabel.text = "Disconnected! ";
        switch(cause)
        {
            case DisconnectCause.DisconnectByClientTimeout:
            case DisconnectCause.DisconnectByServerTimeout:
                launcherPanel.ProgressLabel.text += "Timeout!";
                break;
            case DisconnectCause.MaxCcuReached:
                launcherPanel.ProgressLabel.text += "Max players amount reached!";
                break;
            case DisconnectCause.InvalidRegion:
                launcherPanel.ProgressLabel.text += "Invalid region!";
                break;
        }

        if (isNamedConnecting || isRandomConnecting)
        {
            targetRoomName = null;

            CampaignSelectionManager.Instanse.RoomSelector.RefreshRoomList();
            CampaignSelectionManager.Instanse.RoomSelector.EnableInteraction();

            isRandomConnecting = false;
            isNamedConnecting = false;
        }
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        isNamedConnecting = false;
        Debug.Log("Darkest Photon Network: OnPhotonJoinFailed() was called!");

        // #Critical: we failed to join aroom,
        launcherPanel.ProgressLabel.text = "Room no longer available!";

        CampaignSelectionManager.Instanse.RoomSelector.EnableInteraction();

        isRandomConnecting = false;
        isNamedConnecting = false;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Darkest Photon Network: OnJoinedRoom() was called!");
        launcherPanel.ProgressLabel.text = "Joined " + (PhotonNetwork.room != null ? PhotonNetwork.room.Name : "")  + "!";

        launcherPanel.FadeToLoadingScreen();
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("Darkest Photon Network: OnDisconnectedFromPhoton() was called!");

        if(isNamedConnecting || isRandomConnecting)
        {
            CampaignSelectionManager.Instanse.RoomSelector.RefreshRoomList();
            CampaignSelectionManager.Instanse.RoomSelector.EnableInteraction();

            isRandomConnecting = false;
            isNamedConnecting = false;
        }
    }

    public override void OnConnectedToPhoton()
    {
        Debug.Log("Darkest Photon Network: OnConnectedToPhoton() was called!");

        launcherPanel.ProgressLabel.text = "Connected!";
    }

    public override void OnCreatedRoom()
    {
        launcherPanel.ProgressLabel.text = "Creating " + (PhotonNetwork.room != null ? PhotonNetwork.room.Name : "room") + "!";
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Darkest Photon Network: OnJoinedLobby() was called!");
        launcherPanel.ProgressLabel.text = "Connected!";
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
            PhotonNetwork.JoinOrCreateRoom(targetRoomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        OnConnectionFail(cause);
    }

    #endregion

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - If not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void RandomConnect()
    {
        if (!CheckSelectedSkills())
            return;

        CampaignSelectionManager.Instanse.RoomSelector.DisableInteraction();

        launcherPanel.ProgressLabel.enabled = true;
        launcherPanel.ProgressPanel.enabled = true;
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
            PhotonNetwork.ConnectToRegion(selectedRegion, GameVersion);
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

        CampaignSelectionManager.Instanse.RoomSelector.DisableInteraction();

        launcherPanel.ProgressLabel.enabled = true;
        launcherPanel.ProgressPanel.enabled = true;
        // keep track of the will to join a room, because when we come back from the game
        // we will get a callback that we are connected, so we need to know what to do then
        isNamedConnecting = true;
        targetRoomName = targetRoom.Name;

        UpdateCustomProperties();

        // We check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected)
        {
            // #Critical: We need at this point to attempt joining a Random Room.
            // If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRoom(targetRoom.Name);
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            //PhotonNetwork.ConnectUsingSettings(GameVersion);
            PhotonNetwork.ConnectToRegion(selectedRegion, GameVersion);
        }
    }

    public void ConnectToMaster()
    {
        if (!PhotonNetwork.connected)
            PhotonNetwork.ConnectToRegion(selectedRegion, GameVersion);
    }

    public bool CreateNamedRoom(string roomName)
    {
        isNamedConnecting = true;
        targetRoomName = roomName;

        UpdateCustomProperties();

        if (!PhotonNetwork.connected)
        {
            launcherPanel.ProgressLabel.text = "Can't create room! No connection!";
            return PhotonNetwork.ConnectToRegion(selectedRegion, GameVersion);
        }

        return PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public bool CheckSelectedSkills()
    {
        foreach (MultiplayerPartySlot slot in multiplayerPartyPanel.PartySlots)
            if (slot.SelectedHero.SelectedCombatSkills.Count < 4)
            {
                launcherPanel.ProgressLabel.text = "Choose at least 4 skills for eath hero!";
                return false;
            }
        return true;
    }

    public void NextRegion()
    {
        int targetIndex = AvailableRegions.IndexOf(selectedRegion) + 1;
        if (targetIndex > AvailableRegions.Count - 1)
            targetIndex = 0;

        selectedRegion = AvailableRegions[targetIndex];
        CampaignSelectionManager.Instanse.RoomSelector.RegionLabel.text = "Region: " + RegionToString(selectedRegion);

        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        CampaignSelectionManager.Instanse.RoomSelector.CleanRoomList();
        launcherPanel.ProgressLabel.text = "Disconnected!";
    }

    public void PrevRegion()
    {
        int targetIndex = AvailableRegions.IndexOf(selectedRegion) - 1;
        if (targetIndex < 0)
            targetIndex = AvailableRegions.Count - 1;

        selectedRegion = AvailableRegions[targetIndex];
        CampaignSelectionManager.Instanse.RoomSelector.RegionLabel.text = "Region: " + RegionToString(selectedRegion);

        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        CampaignSelectionManager.Instanse.RoomSelector.CleanRoomList();
        launcherPanel.ProgressLabel.text = "Disconnected!";
    }

    private void UpdateCustomProperties()
    {
        var playerProps = new ExitGames.Client.Photon.Hashtable();

        for (int i = 0; i < multiplayerPartyPanel.PartySlots.Count; i++)
        {
            var hero = multiplayerPartyPanel.PartySlots[i].SelectedHero;
            playerProps.Add("HC" + (i + 1), multiplayerPartyPanel.PartySlots[i].SelectedHero.ClassStringId);
            playerProps.Add("HN" + (i + 1), multiplayerPartyPanel.PartySlots[i].SelectedHero.Name);
            playerProps.Add("HS" + (i + 1), HeroSeeds[HeroPool.IndexOf(hero)]);

            var skillFlags = PlayerSkillFlags.Empty;
            for (int j = 0; j < hero.CurrentCombatSkills.Length; j++)
                if (hero.CurrentCombatSkills[j] != null && hero.SelectedCombatSkills.Contains(hero.CurrentCombatSkills[j]))
                    skillFlags |= (PlayerSkillFlags)Mathf.Pow(2, j + 1);

            playerProps.Add("HF" + (i + 1), skillFlags);
        }
        PhotonNetwork.player.SetCustomProperties(playerProps, playerProps);
    }
}