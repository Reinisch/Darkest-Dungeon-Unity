using UnityEngine;
using System.Collections.Generic;

public class ResultHeroWindow : MonoBehaviour
{
    [SerializeField]
    private List<ResultHeroSlot> resultSlots;

    public void PreparePropotions()
    {
        for(int i = 0; i < RaidSceneManager.Raid.RaidParty.HeroInfo.Count; i++)
            resultSlots[i].UpdateResult(RaidSceneManager.Raid.RaidParty.HeroInfo[i]);
        for (int i = RaidSceneManager.Raid.RaidParty.HeroInfo.Count; i < resultSlots.Count; i++)
            resultSlots[i].SetEmpty();
    }
}
