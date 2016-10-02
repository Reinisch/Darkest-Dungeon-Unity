using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenLoader : MonoBehaviour
{
    AsyncOperation async;

    public Image loadingImage;
    public Animator imageAnimator;
    public Text title;
    public Text description;
    public Text continueText;

    void Awake()
    {
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
                title.text = LocalizationManager.GetString("dungeon_name_" + currentQuest.Dungeon);
                description.text = LocalizationManager.GetString("str_" + currentQuest.Dungeon + "_tip");
            }
        }

        if (!DarkestDungeonManager.SkipTransactions)
            loadingImage.sprite = Resources.Load<Sprite>(DarkestDungeonManager.LoadingInfo.TextureName);
        else
        {
            async = SceneManager.LoadSceneAsync(DarkestDungeonManager.LoadingInfo.NextScene);
            imageAnimator.SetBool("LoadingEnded", true);
            Faded();
        }
    }
	void Start()
    {
        if (!DarkestDungeonManager.SkipTransactions)
        {
            StartCoroutine(SceneLoading());
        }
	}
    void Faded()
    {
        async.allowSceneActivation = true;
    }

    IEnumerator SceneLoading()
    {
        async = SceneManager.LoadSceneAsync(DarkestDungeonManager.LoadingInfo.NextScene);
        async.allowSceneActivation = false;
        while (!async.isDone)
        {
            if (async.progress >= 0.6f)
                break;
            yield return null;
        }
        imageAnimator.SetBool("LoadingEnded", true);
    }
}