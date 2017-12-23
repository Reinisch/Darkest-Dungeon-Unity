using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class PartyActivityRecord : ActivityRecord
{
    public string Description { get { return cachedDescription ?? GenerateDescription(); } }
    public List<string> Names { get; private set; }
    public List<string> Classes { get; private set; }
    public List<bool> Alive { get; private set; }
    public bool IsSuccessfull { get; private set; }

    private PartyActionType PartyActionType { get; set; }
    private string QuestType { get; set; }
    private string QuestDifficulty { get; set; }
    private string QuestLength { get; set; }
    private string Dungeon { get; set; }

    private string cachedDescription;

    private string GenerateDescription()
    {
        StringBuilder sb = ToolTipManager.TipBody;
        switch (PartyActionType)
        {
            case PartyActionType.Tutorial:
                #region Tutorial
                if (!Alive[0] || !Alive[1])
                {
                    if (!Alive[0] && !Alive[1])
                    {
                        string tutAllPerish = string.Format(LocalizationManager.GetString("str_party_members_2"),
                            DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1]);
                        sb.AppendFormat(LocalizationManager.GetString("str_tutorial_all_perished"), tutAllPerish);
                    }
                    else
                    {
                        string tutSurv = string.Format(LocalizationManager.GetString("str_party_members_1"),
                            DarkestDungeonManager.Data.HexColors["notable"], Alive[0] ? Names[0] : Names[1]);
                        string tutPerish = string.Format(LocalizationManager.GetString("str_party_members_1"),
                            DarkestDungeonManager.Data.HexColors["harmful"], Alive[1] ? Names[0] : Names[1]);
                        sb.AppendFormat(LocalizationManager.GetString("str_tutorial_alive_perished_mixed"), tutSurv, tutPerish);
                    }
                }
                else
                {
                    string tutWinners = string.Format(LocalizationManager.GetString("str_party_members_2"),
                        DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1]);
                    sb.AppendFormat(LocalizationManager.GetString("str_tutorial_all_alive"), tutWinners);
                }
                #endregion
                break;
            case PartyActionType.Embark:
                #region Embark
                string strEmbarkers = string.Format(LocalizationManager.GetString("str_party_members_4"),
                    DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1], Names[2], Names[3]);
                string embarkId = "str_embarked_on_";
                switch(QuestType)
                {
                    case "explore":
                    case "cleanse":
                    case "kill_boss":
                    case "gather":
                    case "inventory_activate":
                        embarkId += QuestType + "_" + Dungeon;
                        break;
                    default:
                        embarkId += QuestType;
                        break;
                }
                sb.AppendFormat(LocalizationManager.GetString(embarkId), strEmbarkers,
                    LocalizationManager.GetString("str_difficulty_" + QuestDifficulty),
                    LocalizationManager.GetString("town_quest_length_" + QuestLength));
                #endregion
                break;
            case PartyActionType.Result:
                #region Result
                string strRaiders = string.Format(LocalizationManager.GetString("str_party_members_4"),
                    DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1], Names[2], Names[3]);
                string returnId = "str_returned_from_";
                switch(QuestType)
                {
                    case "explore":
                    case "cleanse":
                    case "kill_boss":
                    case "gather":
                    case "inventory_activate":
                        returnId += QuestType + "_" + Dungeon;
                        break;
                    default:
                        returnId += QuestType;
                        break;
                }
                if (IsSuccessfull)
                    returnId += "_success";
                else
                    returnId += "_failure";

                sb.AppendFormat(LocalizationManager.GetString(returnId), strRaiders,
                    LocalizationManager.GetString("str_difficulty_" + QuestDifficulty),
                    LocalizationManager.GetString("town_quest_length_" + QuestLength));

                if(Alive.Contains(false))
                {
                    if(Alive.Contains(true))
                    {
                        string deadHeroes = "";
                        switch(Alive.FindAll(x => !x).Count)
                        {
                            case 1:
                                deadHeroes = string.Format(LocalizationManager.GetString("str_party_members_1"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    Alive[0] ? Alive[1] ? Alive[2] ? Names[3] : Names[2] : Names[1] : Names[0]);
                                break;
                            case 2:
                                int deadOne = 0;
                                int deadTwo = 0;
                                for(int i = 0; i < Alive.Count; i++)
                                    if(!Alive[i]) deadTwo = i;
                                for (int i = Alive.Count - 1; i >= 0; i--)
                                    if(!Alive[i]) deadOne = i;
                                deadHeroes = string.Format(LocalizationManager.GetString("str_party_members_2"),
                                    DarkestDungeonManager.Data.HexColors["harmful"], Names[deadOne], Names[deadTwo]);
                                break;
                            case 3:
                                int dead = 0;
                                int live = Alive.IndexOf(true);
                                deadHeroes = string.Format(LocalizationManager.GetString("str_party_members_3"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    dead++ == live ? Names[dead++] : Names[dead - 1],
                                    dead++ == live ? Names[dead++] : Names[dead - 1],
                                    dead++ == live ? Names[dead] : Names[dead - 1]);
                                break;
                        }
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_perished"), deadHeroes);
                    }
                    else
                    {
                        string deadParty = string.Format(LocalizationManager.GetString("str_party_members_4"),
                            DarkestDungeonManager.Data.HexColors["harmful"], Names[0], Names[1], Names[2], Names[3]);
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_allperished"), deadParty);
                    }
                }
                #endregion
                break;
        }
        cachedDescription = sb.ToString();
        return cachedDescription;
    }

    public PartyActivityRecord()
    {
        Names = new List<string>();
        Classes = new List<string>();
        Alive = new List<bool>();
    }

    public PartyActivityRecord(PartyActionType actionType, string questType, string questDifficulty, 
        string questLength, string dungeon, List<Hero> heroes)
    {
        PartyActionType = actionType;
        QuestType = questType;
        QuestDifficulty = questDifficulty;
        QuestLength = questLength;
        Dungeon = dungeon;

        Names = new List<string>(heroes.Select(item => item.HeroName));
        Classes = new List<string>(heroes.Select(item => item.Class));
        Alive = new List<bool>();
        for (int i = 0; i < Names.Count; i++)
            Alive.Add(true);
    }

    public PartyActivityRecord(PartyActionType actionType, RaidManager raidManager)
    {
        PartyActionType = actionType;
        QuestType = raidManager.Quest.Type;
        QuestDifficulty = raidManager.Quest.Difficulty.ToString();
        QuestLength = raidManager.Quest.Length.ToString();
        Dungeon = raidManager.Quest.Dungeon;
        IsSuccessfull = raidManager.Status == RaidStatus.Success;

        Names = new List<string>(raidManager.RaidParty.HeroInfo.Select(item => item.Hero.HeroName));
        Classes = new List<string>(raidManager.RaidParty.HeroInfo.Select(item => item.Hero.Class));
        if (actionType == PartyActionType.Result)
            Alive = new List<bool>(raidManager.RaidParty.HeroInfo.Select(item => item.IsAlive));
        else
        {
            Alive = new List<bool>();
            for (int i = 0; i < Names.Count; i++)
                Alive.Add(true);
        }
    }

    public PartyActivityRecord(PartyActionType actionType, string questType, string questDifficulty, string questLength,
        string dungeon, List<SaveHeroData> heroes, bool[] aliveStatus, bool isSuccessfull)
    {
        PartyActionType = actionType;
        QuestType = questType;
        QuestDifficulty = questDifficulty;
        QuestLength = questLength;
        Dungeon = dungeon;
        IsSuccessfull = isSuccessfull;

        Names = new List<string>(heroes.Select(item => item.Name));
        Classes = new List<string>(heroes.Select(item => item.HeroClass));
        Alive = new List<bool>(aliveStatus);
    }

    public PartyActivityRecord(PartyActivityRecord embarkRecord, bool[] aliveStatus, bool isSuccessfull)
    {
        PartyActionType = PartyActionType.Result;
        QuestType = embarkRecord.QuestType;
        QuestDifficulty = embarkRecord.QuestDifficulty;
        QuestLength = embarkRecord.QuestLength;
        Dungeon = embarkRecord.Dungeon;
        IsSuccessfull = isSuccessfull;

        Names = embarkRecord.Names.ToList();
        Classes = embarkRecord.Classes.ToList();
        Alive = new List<bool>(aliveStatus);
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);

        bw.Write(IsSuccessfull);
        bw.Write(QuestType);
        bw.Write(QuestDifficulty);
        bw.Write(QuestLength);
        bw.Write(Dungeon);
        bw.Write((int)PartyActionType);

        Names.Write(bw);
        Classes.Write(bw);
        Alive.Write(bw);
    }

    public override void Read(BinaryReader br)
    {
        base.Read(br);

        IsSuccessfull = br.ReadBoolean();
        QuestType = br.ReadString();
        QuestDifficulty = br.ReadString();
        QuestLength = br.ReadString();
        Dungeon = br.ReadString();
        PartyActionType = (PartyActionType)br.ReadInt32();

        Names.Read(br);
        Classes.Read(br);
        Alive.Read(br);
    }
}