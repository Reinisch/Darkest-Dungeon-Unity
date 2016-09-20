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
        if (!DarkestDungeonManager.SkipTransactions)
            loadingImage.sprite = Resources.Load<Sprite>(DarkestDungeonManager.LoadingInfo.TextureName);
        else
            Faded();


        if(DarkestDungeonManager.LoadingInfo.NextScene == "EstateManagement")
        {
            title.text = LocalizationManager.GetString("str_town_title");
            description.text = LocalizationManager.GetString("str_town_tip");
        }
        else if (DarkestDungeonManager.LoadingInfo.NextScene == "Dungeon")
        {
            if(DarkestDungeonManager.SaveData != null && DarkestDungeonManager.SaveData.InRaid)
            {
                if(DarkestDungeonManager.SaveData.Quest.IsPlotQuest)
                {
                    var plot = DarkestDungeonManager.SaveData.Quest as PlotQuest;
                    if(plot.Id != "tutorial")
                    {
                        title.text = LocalizationManager.GetString("dungeon_name_" + DarkestDungeonManager.SaveData.Quest.Dungeon);
                        description.text = LocalizationManager.GetString("str_" + plot.Id + "_tip");
                    }
                    else
                    {
                        title.text = LocalizationManager.GetString("dungeon_name_tutorial");
                        description.text = LocalizationManager.GetString("town_quest_goal_start_plural_tutorial_room");
                    }
                }        
            }
            else
            {
                if(DarkestDungeonManager.RaidManager.Quest != null)
                {
                    title.text = LocalizationManager.GetString("dungeon_name_" + DarkestDungeonManager.RaidManager.Quest.Dungeon);
                    description.text = LocalizationManager.GetString("str_" + DarkestDungeonManager.RaidManager.Quest.Dungeon + "_tip");
                }
            }
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