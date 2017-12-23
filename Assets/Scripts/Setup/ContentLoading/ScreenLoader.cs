using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class ScreenLoader : MonoBehaviour
{
    [SerializeField]
    private Image loadingImage;
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text description;

    private AsyncOperation async;

    private void Awake()
    {
        DarkestDungeonManager.ScreenFader.StartFaded();

        if (DarkestDungeonManager.LoadingInfo.NextScene == "EstateManagement")
        {
            title.text = LocalizationManager.GetString("str_town_title");
            description.text = LocalizationManager.GetString("str_town_tip");
        }
        else if (DarkestDungeonManager.LoadingInfo.NextScene == "Dungeon")
        {
            var currentQuest = DarkestDungeonManager.SaveData != null && DarkestDungeonManager.SaveData.InRaid ?
                DarkestDungeonManager.SaveData.Quest : DarkestDungeonManager.RaidManager.Quest;

            if (currentQuest.IsPlotQuest)
            {
                DarkestSoundManager.ExecuteNarration("loading_screen_start", NarrationPlace.Raid, currentQuest.Id);

                if (currentQuest.Id != "tutorial")
                {
                    title.text = LocalizationManager.GetString("dungeon_name_" + currentQuest.Dungeon);
                    description.text = LocalizationManager.GetString("str_" + currentQuest.Id + "_tip");
                }
                else
                {
                    title.text = LocalizationManager.GetString("dungeon_name_tutorial");
                    description.text = LocalizationManager.GetString("town_quest_goal_start_plural_tutorial_room");
                }
            }
            else
            {
                DarkestSoundManager.ExecuteNarration("loading_screen_start", NarrationPlace.Raid, currentQuest.Type, currentQuest.Dungeon);

                title.text = LocalizationManager.GetString("dungeon_name_" + currentQuest.Dungeon);
                description.text = LocalizationManager.GetString("str_" + currentQuest.Dungeon + "_tip");
            }
        }

        if (!DarkestDungeonManager.SkipTransactions)
        {
            var loadingScreen = Resources.Load<Sprite>(DarkestDungeonManager.LoadingInfo.TextureName);
            if (loadingScreen != null)
                loadingImage.sprite = loadingScreen;
        }
        else
            SceneManager.LoadScene(DarkestDungeonManager.LoadingInfo.NextScene);
    }

    private void Start()
    {
        if (!DarkestDungeonManager.SkipTransactions)
            StartCoroutine(SceneLoading());
	}

    private IEnumerator SceneLoading()
    {
        Resources.UnloadUnusedAssets();
        GC.Collect();

        yield return new WaitForEndOfFrame();
        DarkestDungeonManager.ScreenFader.Appear();
        yield return new WaitForSeconds(1f);
        async = SceneManager.LoadSceneAsync(DarkestDungeonManager.LoadingInfo.NextScene);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            if (async.progress >= 0.8f)
                break;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        while (DarkestSoundManager.NarrationQueue.Count > 0)
            yield return null;

        DarkestDungeonManager.ScreenFader.Fade();
        yield return new WaitForSeconds(1f);
        async.allowSceneActivation = true;
    }
}