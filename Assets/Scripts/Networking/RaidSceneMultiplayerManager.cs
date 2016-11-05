using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class RaidSceneMultiplayerManager : RaidSceneManager
{
    protected override void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;
    #if !(UNITY_ANDROID || UNITY_IOS)
            escapeButton.gameObject.SetActive(false);
#endif
            RaidEvents.Initialize();

            return;

            currentRaid = new RaidInfo();
            currentRaid.Quest = DarkestDungeonManager.Instanse.RaidingManager.Quest;
            if (currentRaid.Quest.IsPlotQuest && (currentRaid.Quest as PlotQuest).RaidMap != null)
                currentRaid.Dungeon = SaveLoadManager.LoadDungeonMap((currentRaid.Quest as PlotQuest).RaidMap, currentRaid.Quest);
            else
                currentRaid.Dungeon = DungeonGenerator.GenerateDungeon(currentRaid.Quest);
            currentRaid.RaidParty = DarkestDungeonManager.RaidManager.RaidParty;

            if (currentRaid.Quest.IsPlotQuest)
                DarkestSoundManager.ExecuteNarration("quest_start", NarrationPlace.Raid, currentRaid.Quest.Id);
            else
                DarkestSoundManager.ExecuteNarration("quest_start", NarrationPlace.Raid,
                    currentRaid.Quest.Type, currentRaid.Quest.Dungeon);


            DarkestDungeonManager.Data.LoadDungeon(currentRaid.Quest.Dungeon, currentRaid.Quest.Id);
            Rules = new RaidRuleInfo(currentRaid.Quest.Dungeon, BattleGround, TorchMeter);
        }
        else
            Destroy(Instanse.gameObject);
    }
    protected override void Start()
    {
        if (Instanse != this)
            return;

        CharacterWindow.onWindowClose += CharacterWindow_onWindowClose;
        CharacterWindow.onNextButtonClick += CharacterWindow_onNextButtonClick;
        CharacterWindow.onPreviousButtonClick += CharacterWindow_onPreviousButtonClick;

        RaidInterface.UpdateRaidScene();
        Inventory.SetDeactivated();
        RaidPanel.bannerPanel.skillPanel.SetMode(SkillPanelMode.Combat);
        RaidPanel.bannerPanel.SetPeacefulState();
        TorchMeter.Initialize(100);
        DarkestDungeonManager.ScreenFader.StartFaded();
        DarkestDungeonManager.ScreenFader.Appear(2);
        return;

        MapPanel.LoadDungeon(currentRaid.Dungeon);
        QuestPanel.UpdateQuest(currentRaid.Quest);
        DarkestSoundManager.StartDungeonSoundtrack(currentRaid.Dungeon.Name);
        Formations.Initialize();

        currentEvent = RoomLoadingEvent(currentRaid.Dungeon.StartingRoom, RoomTransitionType.Entrance);
        StartCoroutine(currentEvent);
    }
    protected override void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            OnEscapePressed();
    }
}
