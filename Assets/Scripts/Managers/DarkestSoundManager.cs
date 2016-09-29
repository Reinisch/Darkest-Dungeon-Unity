using UnityEngine;
using FMODUnity;

public class DarkestSoundManager : MonoBehaviour
{
    public static DarkestSoundManager Instanse { get; private set; }
    public static FMOD.Studio.System Studio { get; private set; }

    public static FMOD.Studio.EventInstance DungeonInstanse { get; private set; }
    public static FMOD.Studio.EventInstance BattleInstanse { get; private set; }
    public static FMOD.Studio.EventInstance CampingInstanse { get; private set; }
    public static FMOD.Studio.EventInstance CampingMusicInstanse { get; private set; }

    void Awake()
    {
        if (Instanse == null)
        {
            Studio = RuntimeManager.StudioSystem;
            Instanse = this;
        }
    }

    public static void StartDungeonSoundtrack(string dungeonName)
    {
        StopDungeonSoundtrack();

        if(dungeonName == "darkestdungeon")
            DungeonInstanse = RuntimeManager.CreateInstance("event:/ambience/dungeon/quest/" + RaidSceneManager.Raid.Quest.Id);
        else
            DungeonInstanse = RuntimeManager.CreateInstance("event:/ambience/dungeon/" + dungeonName);

        if (DungeonInstanse != null)
            DungeonInstanse.start();
    }
    public static void ContinueDungeonSoundtrack(string dungeonName)
    {
        if (DungeonInstanse != null)
            DungeonInstanse.setPaused(false);
        else
            StartDungeonSoundtrack(dungeonName);
    }
    public static void PauseDungeonSoundtrack()
    {
        if (DungeonInstanse != null)
            DungeonInstanse.setPaused(true);
    }
    public static void StopDungeonSoundtrack()
    {
        if (DungeonInstanse != null)
        {
            DungeonInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            DungeonInstanse.release();
        }
    }

    public static void StartBattleSoundtrack(string dungeonName, bool isRoom)
    {
        StopBattleSoundtrack();

        BattleInstanse = RuntimeManager.CreateInstance("event:/music/mus_battle_" +
            dungeonName + (isRoom ? "_room" : "_hallway"));
        if (BattleInstanse != null)
            BattleInstanse.start();
    }
    public static void StopBattleSoundtrack()
    {
        if (BattleInstanse != null)
        {
            BattleInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            BattleInstanse.release();
        }
    }

    public static void StartCampingSoundtrack()
    {
        StopCampingSoundtrack();

        CampingInstanse = RuntimeManager.CreateInstance("event:/ambience/local/campfire");
        if (CampingInstanse != null)
            CampingInstanse.start();
        CampingMusicInstanse = RuntimeManager.CreateInstance("event:/music/mus_camp");
        if (CampingMusicInstanse != null)
            CampingMusicInstanse.start();
    }
    public static void StopCampingSoundtrack()
    {
        if (CampingInstanse != null)
        {
            CampingInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            CampingInstanse.release();
        }
        if (CampingMusicInstanse != null)
        {
            CampingMusicInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            CampingMusicInstanse.release();
        }
    }
}
