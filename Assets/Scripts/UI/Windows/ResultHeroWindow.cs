using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ResultHeroWindow : MonoBehaviour
{
    public List<ResultHeroSlot> resultSlots;

    public void PreparePropotions()
    {
        for(int i = 0; i < RaidSceneManager.Raid.RaidParty.HeroInfo.Count; i++)
            resultSlots[i].UpdateResult(RaidSceneManager.Raid.RaidParty.HeroInfo[i]);
        for (int i = RaidSceneManager.Raid.RaidParty.HeroInfo.Count; i < resultSlots.Count; i++)
            resultSlots[i].SetEmpty();
    }
}
