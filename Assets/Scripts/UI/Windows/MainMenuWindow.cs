using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuWindow : MonoBehaviour
{
    public Button closeButton;
    public CanvasGroup uiCanvasGroup;

    public event WindowEvent onWindowClose;

    public bool IsOpened
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        DarkestDungeonManager.GamePaused = true;
        uiCanvasGroup.blocksRaycasts = false;
    }

    public void WindowClosed()
    {
        DarkestDungeonManager.GamePaused = false;
        gameObject.SetActive(false);

        if (onWindowClose != null)
            onWindowClose();
        uiCanvasGroup.blocksRaycasts = true;
    }

    public void ReturnToCampaignSelection()
    {
        if(SceneManager.GetActiveScene().name == "EstateManagement")
        {
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
        SceneManager.LoadScene("CampaignSelection");

        WindowClosed();
        Destroy(DarkestDungeonManager.Instanse.gameObject);
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().name == "EstateManagement")
        {
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
        Application.Quit();
    }
}
