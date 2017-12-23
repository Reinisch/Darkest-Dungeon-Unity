using UnityEngine;
using System.Collections.Generic;

public class RecruitPanel : MonoBehaviour
{
    [SerializeField]
    private HeroRosterPanel rosterPanel;
    [SerializeField]
    private List<RecruitSlot> recruitSlots;

    private void Awake()
    {
        for(int i = 0; i < recruitSlots.Count; i++)
        {
            recruitSlots[i].HeroRoster = rosterPanel;
        }
    }

    public void UpdateRecruitPanel(List<Hero> heroes)
    {
        int updatableSlots = Mathf.Min(recruitSlots.Count, heroes.Count);
        for (int i = 0; i < updatableSlots; i++)
            recruitSlots[i].UpdateSlot(heroes[i]);

        for (int i = updatableSlots; i < recruitSlots.Count; i++)
            recruitSlots[i].RemoveSlot();
    }
}