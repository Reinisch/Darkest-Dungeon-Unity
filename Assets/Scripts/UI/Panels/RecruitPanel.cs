using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecruitPanel : MonoBehaviour
{
    public HeroRosterPanel rosterPanel;
    public List<RecruitSlot> recruitSlots;

    void Awake()
    {
        for(int i = 0; i < recruitSlots.Count; i++)
        {
            recruitSlots[i].HeroRoster = rosterPanel;
        }
    }

    public void UpdateRecruitPanel()
    {
        int currentIndex = 0;
        for (int i = 0; i < DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes.Count; i++)
        {
            currentIndex = i;
            recruitSlots[i].UpdateSlot(DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes[i]);
        }
        for(int i = currentIndex + 1; i < recruitSlots.Count; i++)
        {
            recruitSlots[i].RemoveSlot();
        }
    }
}
