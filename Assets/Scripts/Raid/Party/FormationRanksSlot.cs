using UnityEngine;
using UnityEngine.UI;

public class FormationRanksSlot : MonoBehaviour
{
    public FormationRanks Ranks { get; private set; }
    public RectTransform RectTransform { get; private set; }

    private FormationUnit Unit { get; set; }
    private LayoutElement LayoutElement { get; set; }

    public bool HasUnit { get { return Unit != null; } }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        LayoutElement = GetComponent<LayoutElement>();
        Ranks = GetComponentInParent<FormationRanks>();
    }

    public void PutInSlot(FormationUnit unit)
    {
        Unit = unit;
        Unit.RankSlot = this;
        if (Unit.Character.RenderRankOverride == 0)
            Unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Rank);
        else
            Unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - unit.Character.RenderRankOverride);

        LayoutElement.preferredWidth = FormationRanks.SlotSize * unit.Size;
        if (Ranks.FacingRight)
            RectTransform.SetSiblingIndex(4 - unit.Rank);
        else
            RectTransform.SetSiblingIndex(unit.Rank - 1);
        gameObject.SetActive(true);
    }

    public void UpdateSlot()
    {
        if(HasUnit)
        {
            if (Unit.Character.RenderRankOverride == 0)
                Unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - Unit.Rank);
            else
                Unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - Unit.Character.RenderRankOverride);

            LayoutElement.preferredWidth = FormationRanks.SlotSize * Unit.Size;
            if (Ranks.FacingRight)
                RectTransform.SetSiblingIndex(4 - Unit.Rank);
            else
                RectTransform.SetSiblingIndex(Unit.Rank - 1);
        }
    }

    public void Relocate(int targetRank, bool changeUnitSorting = true)
    {
        Unit.Rank = targetRank;
        if(Unit.Character.RenderRankOverride == 0)
        {
            if (changeUnitSorting)
                Unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - targetRank);
        }
        else
        {
            if (changeUnitSorting)
                Unit.SetSortingOrder(PartyFormationManager.ShowoffOrder - Unit.Character.RenderRankOverride);
        }
        if (Ranks.FacingRight)
            RectTransform.SetSiblingIndex(4 - Unit.Rank);
        else
            RectTransform.SetSiblingIndex(Unit.Rank - 1);
    }

    public void ClearSlot()
    {
        Unit = null;
        gameObject.SetActive(false);
    }

    public void Teleport()
    {
        if(Unit != null)
            Unit.InstantRelocation();   
    }
}
