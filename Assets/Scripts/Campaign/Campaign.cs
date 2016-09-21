using System.Collections.Generic;

public class Campaign
{
    public int SaveSlotId { get; private set; }
    public int CurrentWeek { get; set; }
    public int QuestsComleted { get; set; }

    public Estate Estate { get; set; }

    public RealmInventory RealmInventory { get; set; }
    public List<WeekActivityLog> Logs { get; set; }
    public List<Hero> Heroes { get; set; }
    public List<Quest> Quests { get; set; }
    public List<string> CompletedPlot { get; set; }

    public bool AreQuestsReady { get; set; }

    public Dictionary<string, DungeonProgress> Dungeons { get; set; }

    public void SearchMissingHeroes()
    {
        for (int i = 0; i < Heroes.Count; i++)
            if (Heroes[i].Status == HeroStatus.Missing)
            {
                Heroes[i].MissingDuration--;
                if (Heroes[i].MissingDuration == 0)
                    Heroes[i].Status = HeroStatus.Available;
            }
    }
    public void ExecuteProgress()
    {
        Estate.ExecuteProgress();
        GenerateQuests();
        SearchMissingHeroes();
    }
    public void AdvanceNextWeek()
    {
        CurrentWeek++;
        Logs.Add(new WeekActivityLog(CurrentWeek));
    }

    public Campaign()
    {

    }

    public void Load(SaveCampaignData saveData)
    {
        SaveSlotId = saveData.saveId;
        CurrentWeek = saveData.currentWeek;

        QuestsComleted = saveData.questsCompleted;

        Estate = new Estate(saveData);
        RealmInventory = new RealmInventory(saveData);
        CompletedPlot = saveData.completedPlot;

        Heroes = new List<Hero>();
        for (int i = 0; i < saveData.saveHeroData.Length; i++)
            Heroes.Add(new Hero(Estate, saveData.saveHeroData[i]));
        for (int i = 0; i < saveData.stageCoachData.Length; i++)
            Estate.StageCoach.Heroes.Add(new Hero(Estate, saveData.stageCoachData[i]));
        for (int i = 0; i < saveData.wagonData.Count; i++)
            Estate.NomadWagon.Trinkets.Add(DarkestDungeonManager.Data.Items["trinket"][saveData.wagonData[i]] as Trinket);

        Estate.Abbey.UpdateActivitySlots(saveData);
        Estate.Tavern.UpdateActivitySlots(saveData);
        Estate.Sanitarium.QuirkActivity.UpdateActivitySlots(saveData);
        Estate.Sanitarium.DiseaseActivity.UpdateActivitySlots(saveData);

        Dungeons = saveData.saveDungeonData;
        Quests = saveData.generatedQuests;
        if (Quests.Count == 0)
            GenerateQuests();

        Logs = new List<WeekActivityLog>(saveData.activityLog);
        if (Logs.Count == 0)
            Logs.Add(new WeekActivityLog(CurrentWeek));
    } 

    public WeekActivityLog CurrentLog()
    {
        return Logs.Count > 0 ? Logs[Logs.Count - 1] : null;
    }
    public WeekActivityLog PreviousLog()
    {
        return Logs.Count > 1 ? Logs[Logs.Count - 2] : null;
    }
    public void GenerateQuests()
    {
        Quests = QuestGenerator.GenerateQuests(DarkestDungeonManager.Campaign);
        AreQuestsReady = true;
    }
}
