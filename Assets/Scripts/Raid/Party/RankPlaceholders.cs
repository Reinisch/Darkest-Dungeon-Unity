using UnityEngine;
using System.Collections.Generic;

public class RankPlaceholders : MonoBehaviour
{
    [SerializeField]
    private List<Animator> rankMarkSlots;

    public List<int> MarkedRanks { get; private set; }
    public List<Animator> RankMarkSlots { get { return rankMarkSlots; } }

    private void Awake()
    {
        MarkedRanks = new List<int>();
    }

    public void MarkRank(int rank)
    {
        if (!MarkedRanks.Contains(rank))
            MarkedRanks.Add(rank);
        if (rank - 1 < rankMarkSlots.Count)
            rankMarkSlots[rank - 1].SetBool("IsMarked", true);
    }

    public void ClearMarks()
    {
        MarkedRanks.Clear();
        RankMarkSlots.ForEach(slot => slot.SetBool("IsMarked", false));
    }
}