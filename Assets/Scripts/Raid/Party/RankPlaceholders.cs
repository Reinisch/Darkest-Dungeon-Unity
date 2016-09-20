using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankPlaceholders : MonoBehaviour
{
    public List<Animator> rankMarkSlots;
    public List<int> MarkedRanks { get; set; }

    void Awake()
    {
        MarkedRanks = new List<int>();
    }

    public void ClearMarks()
    {
        MarkedRanks.Clear();
        for (int i = 0; i < rankMarkSlots.Count; i++)
            rankMarkSlots[i].SetBool("IsMarked", false);
    }
    public void MarkRank(int rank)
    {
        if (!MarkedRanks.Contains(rank))
            MarkedRanks.Add(rank);
        if (rank - 1 < rankMarkSlots.Count)
            rankMarkSlots[rank - 1].SetBool("IsMarked", true);
    }
}
