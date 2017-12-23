using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuWindow : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup uiCanvasGroup;

    public CanvasGroup UICanvasGroup { private get { return uiCanvasGroup; } set { uiCanvasGroup = value; } }
    public bool IsOpened { get { return gameObject.activeSelf; } }

    public event Action EventWindowClosed;

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        DarkestDungeonManager.GamePaused = true;
        UICanvasGroup.blocksRaycasts = false;
    }

    public void WindowClosed()
    {
        DarkestDungeonManager.GamePaused = false;
        gameObject.SetActive(false);

        if (EventWindowClosed != null)
            EventWindowClosed();
        UICanvasGroup.blocksRaycasts = true;
    }

    public void ReturnToCampaignSelection()
    {
        if(SceneManager.GetActiveScene().name == "DungeonMultiplayer")
        {
            WindowClosed();
            RaidSceneManager.Instanse.AbandonButtonClicked();
            return;
        }
        else if(SceneManager.GetActiveScene().name == "EstateManagement")
        {
            EstateSceneManager.Instanse.OnSceneLeave();
            DarkestDungeonManager.SaveData.UpdateFromEstate();
            DarkestDungeonManager.Instanse.SaveGame();
        }
        else if (SceneManager.GetActiveScene().name == "Dungeon")
        {
            if (!RaidSceneManager.HasAnyEvents)
            {
                DarkestDungeonManager.SaveData.UpdateFromRaid();
                DarkestDungeonManager.Instanse.SaveGame();
            }
            RaidSceneManager.Instanse.OnSceneLeave();
        }
        DarkestSoundManager.SilenceNarrator();
        SceneManager.LoadScene("CampaignSelection");
        WindowClosed();
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().name == "DungeonMultiplayer")
        {
            RaidSceneManager.Instanse.OnSceneLeave();
            PhotonGameManager.Instanse.LeaveRoom();
            WindowClosed();
            return;
        }
        else if (SceneManager.GetActiveScene().name == "EstateManagement")
        {
            EstateSceneManager.Instanse.OnSceneLeave();
            DarkestDungeonManager.SaveData.UpdateFromEstate();
            DarkestDungeonManager.Instanse.SaveGame();
        }
        else if (SceneManager.GetActiveScene().name == "Dungeon")
        {
            if(!RaidSceneManager.HasAnyEvents)
            {
                DarkestDungeonManager.SaveData.UpdateFromRaid();
                DarkestDungeonManager.Instanse.SaveGame();
            }
            RaidSceneManager.Instanse.OnSceneLeave();
        }
        DarkestSoundManager.SilenceNarrator();
        Application.Quit();
    }
}