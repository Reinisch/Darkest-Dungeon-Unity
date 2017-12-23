using System.Collections.Generic;

public class RaidInfo
{
    public bool QuestCompleted { get; set; }

    public Quest Quest { get; set; }
    public Dungeon Dungeon { get; set; }
    public RaidParty RaidParty { get; set; }

    public CampingPhase CampingPhase { get; set; }
    public int CampingTimeLeft { get; set; }
    public float NightAmbushReduced { get; set; }
    public int HungerCooldown { get; set; }

    public int ExploredRoomCount { get; set; }
    public Area CurrentLocation { get; set; }
    public DungeonRoom LastRoom { get; set; }
    public HallSector LastSector { get; set; }

    public HallSector PreviousLastSector { get; private set; }
    public List<string> KilledMonsters { get; private set; }
    public List<string> InvestigatedCurios { get; private set; }

    public int AncestorTalk { get; set; }

    private int defaultHungerCooldown = 18;

    public RaidInfo()
    {
        KilledMonsters = new List<string>();
        InvestigatedCurios = new List<string>();
        HungerCooldown = defaultHungerCooldown;
    }

    public RaidInfo(SaveCampaignData saveData)
    {
        QuestCompleted = saveData.QuestCompleted;
        Quest = saveData.Quest;
        Dungeon = saveData.Dungeon;
        RaidParty = new RaidParty(saveData.RaidParty);

        ExploredRoomCount = saveData.ExploredRoomCount;
        if (Dungeon.Rooms.ContainsKey(saveData.CurrentLocation))
            CurrentLocation = Dungeon.Rooms[saveData.CurrentLocation];
        else
        {
            foreach (var hallway in Dungeon.Hallways)
            {
                CurrentLocation = hallway.Value.Halls.Find(hallSector => hallSector.Id == saveData.CurrentLocation);
                if (CurrentLocation != null)
                    break;
            }
        }
        if (saveData.LastRoom != "")
            LastRoom = Dungeon.Rooms[saveData.LastRoom];
        if (saveData.PreviousLastSector != "")
            foreach (var hallway in Dungeon.Hallways)
            {
                PreviousLastSector = hallway.Value.Halls.Find(hallSector => hallSector.Id == saveData.PreviousLastSector);
                if (PreviousLastSector != null)
                    break;
            }
        if (saveData.LastSector != "")
            foreach (var hallway in Dungeon.Hallways)
            {
                LastSector = hallway.Value.Halls.Find(hallSector => hallSector.Id == saveData.LastSector);
                if (LastSector != null)
                    break;
            }

        KilledMonsters = new List<string>(saveData.KilledMonsters);
        InvestigatedCurios = new List<string>(saveData.InvestigatedCurios);
    }

    public bool CheckQuestGoals()
    {
        return Quest.Goal.QuestData.IsQuestCompleted();
    }

    public void ResetHungerCooldown()
    {
        HungerCooldown = defaultHungerCooldown;
    }

    public void ResetRoundSector(HallSector targetSector)
    {
        PreviousLastSector = targetSector;
        LastSector = targetSector;
    }

    public void EnteredSector(HallSector enterSector)
    {
        if(PreviousLastSector == null)
        {
            PreviousLastSector = enterSector;
            LastSector = enterSector;
            return;
        }

        if(enterSector != PreviousLastSector && enterSector != LastSector)
        {
            PreviousLastSector = LastSector;
            LastSector = enterSector;
            RaidSceneManager.Instanse.AdvanceThroughDungeon();
        }
        else
        {
            if(LastSector != enterSector)
            {
                PreviousLastSector = LastSector;
                LastSector = enterSector;
            }
        }
    }
}
