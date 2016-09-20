using UnityEngine;
using System.Collections;
using FMODUnity;

public class DarkestSoundManager : MonoBehaviour
{
    public static DarkestSoundManager Instanse { get; private set; }
    public static FMOD.Studio.System Studio { get; private set; }

    public static FMOD.Studio.EventInstance DungeonInstanse { get; set; }
    public static FMOD.Studio.EventInstance BattleInstanse { get; set; }
    public static FMOD.Studio.EventInstance CampingInstanse { get; set; }

    void Awake()
    {
        if (Instanse == null)
        {
            Studio = FMODUnity.RuntimeManager.StudioSystem;
            Instanse = this;
        }
    }
    void Start()
    {
        if (Instanse.gameObject != gameObject)
            return;
    }

    public static void StartDungeonSoundtrack(string dungeonName)
    {
        DungeonInstanse = RuntimeManager.CreateInstance("event:/ambience/dungeon/" + dungeonName);
        if (DungeonInstanse != null)
            DungeonInstanse.start();
    }
    public static void StopDungeonSoundtrack()
    {
        if (DungeonInstanse != null)
        {
            DungeonInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            DungeonInstanse.release();
        }
    }
    public static void StopCampingSoundtrack()
    {
        if (CampingInstanse != null)
        {
            CampingInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            CampingInstanse.release();
        }
    }
}
