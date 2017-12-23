using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;

public class DarkestSoundManager : MonoBehaviour
{
    public static DarkestSoundManager Instanse { get; private set; }

    public static FMOD.Studio.EventInstance DungeonInstanse { get; private set; }
    public static FMOD.Studio.EventInstance BattleInstanse { get; private set; }
    public static FMOD.Studio.EventInstance CampingInstanse { get; private set; }
    public static FMOD.Studio.EventInstance CampingMusicInstanse { get; private set; }
    public static FMOD.Studio.EventInstance TownInstanse { get; private set; }
    public static FMOD.Studio.EventInstance TownMusicInstanse { get; private set; }
    public static FMOD.Studio.EventInstance TitleMusicInstanse { get; private set; }

    public static List<FMOD.Studio.EventInstance> NarrationQueue { get; private set; }
    public static FMOD.Studio.EventInstance CurrentNarration { get; private set; }

    private static FMOD.Studio.PLAYBACK_STATE narrationState;

    private void Awake()
    {
        if (Instanse == null)
        {
            NarrationQueue = new List<FMOD.Studio.EventInstance>();
            Instanse = this;
        }
    }

    private void Update()
    {
        if (CurrentNarration == null)
        {
            if (NarrationQueue.Count > 0)
            {
                CurrentNarration = NarrationQueue[0];
                CurrentNarration.start();
            }
        }
        else
        {
            CurrentNarration.getPlaybackState(out narrationState);
            if (narrationState == FMOD.Studio.PLAYBACK_STATE.STOPPED || narrationState == FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                CurrentNarration.release();
                NarrationQueue.Remove(CurrentNarration);
                CurrentNarration = null;
            }
        }
    }

    public static void ExecuteNarration(string id, NarrationPlace place, params string[] tags)
    {
        if (!DarkestDungeonManager.Data.Narration.ContainsKey(id))
            return;

        NarrationEntry narrationEntry = DarkestDungeonManager.Data.Narration[id];

        if (!RandomSolver.CheckSuccess(narrationEntry.Chance))
            return;

        var possibleEvents = narrationEntry.AudioEvents.FindAll(audioEvent => audioEvent.IsPossible(place, tags));
        if (possibleEvents.Count == 0)
            return;

        float maxPriority = possibleEvents.Max(audio => audio.Priority);
        possibleEvents.RemoveAll(lowPriorityEvent => lowPriorityEvent.Priority < maxPriority);

        NarrationAudioEvent narrationEvent = id == "combat_start" ?
            possibleEvents[0] : possibleEvents[RandomSolver.Next(possibleEvents.Count)];

        if (narrationEvent.QueueOnlyOnEmpty && NarrationQueue.Count > 0)
            return;

        if (id == "town_visit_start")
        {
            for (int i = 0; i < 3; i++)
            {
                if (RandomSolver.CheckSuccess(narrationEvent.Chance))
                    break;
                else
                    narrationEvent = possibleEvents[RandomSolver.Next(possibleEvents.Count)];

                if (i == 2)
                    return;
            }
        }
        else if (!RandomSolver.CheckSuccess(narrationEvent.Chance))
            return;

        var narrationInstanse = RuntimeManager.CreateInstance("event:" + narrationEvent.AudioEvent);
        if (narrationInstanse != null)
            NarrationQueue.Add(narrationInstanse);

        switch (place)
        {
            case NarrationPlace.Campaign:
                if (narrationEvent.MaxCampaignOccurrences > 0)
                {
                    if (!DarkestDungeonManager.Campaign.NarrationCampaignInfo.ContainsKey(narrationEvent.AudioEvent))
                        DarkestDungeonManager.Campaign.NarrationCampaignInfo.Add(narrationEvent.AudioEvent, 0);

                    DarkestDungeonManager.Campaign.NarrationCampaignInfo[narrationEvent.AudioEvent]++;
                }
                break;
            case NarrationPlace.Raid:
                if (narrationEvent.MaxRaidOccurrences > 0)
                {
                    if (!DarkestDungeonManager.Campaign.NarrationRaidInfo.ContainsKey(narrationEvent.AudioEvent))
                        DarkestDungeonManager.Campaign.NarrationRaidInfo.Add(narrationEvent.AudioEvent, 0);

                    DarkestDungeonManager.Campaign.NarrationRaidInfo[narrationEvent.AudioEvent]++;
                }
                goto case NarrationPlace.Campaign;
            case NarrationPlace.Town:
                if (narrationEvent.MaxTownVisitOccurrences > 0)
                {
                    if (!DarkestDungeonManager.Campaign.NarrationTownInfo.ContainsKey(narrationEvent.AudioEvent))
                        DarkestDungeonManager.Campaign.NarrationTownInfo.Add(narrationEvent.AudioEvent, 0);

                    DarkestDungeonManager.Campaign.NarrationTownInfo[narrationEvent.AudioEvent]++;
                }
                goto case NarrationPlace.Campaign;
        }
    }

    public static void PlayStatueAudioEntry(string id)
    {
        if (CurrentNarration != null && NarrationQueue.Count > 0)
            return;

        var narrationInstanse = RuntimeManager.CreateInstance(id);
        if (narrationInstanse != null)
            NarrationQueue.Add(narrationInstanse);
    }

    public static void SilenceNarrator()
    {
        if (CurrentNarration != null)
        {
            CurrentNarration.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            CurrentNarration.release();
            CurrentNarration = null;
            NarrationQueue.Clear();
        }
    }

    public static void PlayOneShot(string eventId)
    {
        RuntimeManager.PlayOneShot(eventId);
    }

    public static void PlayTitleMusic(bool isIntro)
    {
        StopTitleMusic();

        if (isIntro)
            TitleMusicInstanse = RuntimeManager.CreateInstance("event:/music/_music_assets/title_intro");
        else
            TitleMusicInstanse = RuntimeManager.CreateInstance("event:/music/_music_assets/title_outro");

        if (TitleMusicInstanse != null)
            TitleMusicInstanse.start();
    }

    public static void StopTitleMusic()
    {
        if (TitleMusicInstanse != null)
        {
            TitleMusicInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            TitleMusicInstanse.release();
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

    public static void StartTownSoundtrack()
    {
        StopTownSoundtrack();

        TownInstanse = RuntimeManager.CreateInstance("event:/ambience/town/general");
        if (TownInstanse != null)
            TownInstanse.start();
        TownMusicInstanse = RuntimeManager.CreateInstance("event:/music/mus_town");
        if (TownMusicInstanse != null)
            TownMusicInstanse.start();
    }

    public static void StopTownSoundtrack()
    {
        if (TownInstanse != null)
        {
            TownInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            TownInstanse.release();
        }
        if (TownMusicInstanse != null)
        {
            TownMusicInstanse.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            TownMusicInstanse.release();
        }
    }
}
