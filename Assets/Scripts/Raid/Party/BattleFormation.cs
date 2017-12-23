using System.Linq;
using UnityEngine;

public class BattleFormation : MonoBehaviour
{
    [SerializeField]
    private FormationRanks ranks;
    [SerializeField]
    private FormationParty party;
    [SerializeField]
    private FormationOverlay overlay;
    [SerializeField]
    private RankPlaceholders rankHolder;

    public RectTransform RectTransform { get; private set; }
    public FormationRanks Ranks { get { return ranks; } }
    public FormationParty Party { get { return party; } }
    public FormationOverlay Overlay { get { return overlay; } }
    public RankPlaceholders RankHolder { get { return rankHolder; } }

    #region Fomation Info

    public int AliveUnitsCount
    {
        get
        {
            return Party.Units.FindAll(unit => unit.Character is Hero ||
                !((Monster)unit.Character).Types.Contains(MonsterType.Corpse)).Count;
        }
    }

    public int AvailableSummonSpace
    {
        get
        {
            return Party.Units.Where(unit => !unit.Character.IsMonster || unit.Character.BattleModifiers == null 
                || !unit.Character.BattleModifiers.CanBeSummonRank).Aggregate(4, (current, unit) => current - unit.Size);
        }
    }

    public int AvailableFreeSpace
    {
        get
        {
            return Party.Units.Aggregate(4, (current, unit) => current - unit.Size);
        }
    }

    public bool IsStallingActive
    {
        get
        {
            if (Party.Units.Any(t => t.Character.BattleModifiers != null && t.Character.BattleModifiers.DisableStallPenalty))
                return false;

            return AliveUnitsCount <= 1;
        }
    }

    public bool CanBeSurprised
    {
        get
        {
            return Party.Units.Any(unit => unit.Character.BattleModifiers.CanBeSurprised);
        }
    }

    public bool AlwaysBeSurprised
    {
        get
        {
            return Party.Units.All(unit => unit.Character.BattleModifiers.AlwaysBeSurprised);
        }
    }

    public bool CanSurprise
    {
        get
        {
            return Party.Units.Any(unit => unit.Character.BattleModifiers.CanSurprise);
        }
    }

    public bool AlwaysSurprises
    {
        get
        {
            return Party.Units.Any(unit => unit.Character.BattleModifiers.AlwaysSurprise);
        }
    }

    #endregion

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    #region Initialization

    public void LoadParty(RaidParty heroParty)
    {
        Party.CreateFormation(heroParty);
        Ranks.DistributeParty(Party);
        Overlay.LockOnUnits(Party);
        Overlay.UpdateOverlay();

        foreach (var unit in Party.Units)
        {
            unit.OverlaySlot.HeroSelected += UpdateHero;
            unit.Formation = this;
        }

        Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });

        Party.Units[0].OverlaySlot.UnitSelected();
    }

    public void LoadParty(RaidParty heroParty, PhotonPlayer player)
    {
        Party.CreateFormation(heroParty, player);
        Ranks.DistributeParty(Party);
        Overlay.LockOnUnits(Party);
        Overlay.UpdateOverlay();

        foreach (var unit in Party.Units)
        {
            unit.OverlaySlot.HeroSelected += UpdateHero;
            unit.Formation = this;
        }

        Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
    }

    public void LoadParty(BattleEncounter encounter)
    {
        Party.CreateFormation(encounter);
        Ranks.DistributeParty(Party);

        foreach (var unit in Party.Units)
            unit.Formation = this;
        Overlay.LockOnUnits(Party);
    }

    public void LoadParty(BattleFormationSaveData formationData, bool isHeroFormation)
    {
        if (isHeroFormation)
        {
            Party.CreateFormation(formationData);
            Ranks.DistributeParty(Party);
            Overlay.LockOnUnits(Party);
            Overlay.UpdateOverlay();

            foreach (var unit in Party.Units)
                unit.Formation = this;

            foreach (var overlaySlot in Overlay.OverlaySlots)
                overlaySlot.HeroSelected += UpdateHero;

            Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
            Party.Units[0].OverlaySlot.UnitSelected();
        }
        else
        {
            Party.CreateFormation(formationData);
            Ranks.DistributeParty(Party);

            foreach (var unit in Party.Units)
                unit.Formation = this;
            Overlay.LockOnUnits(Party);
        }
    }

    #endregion

    public void SwapUnits(FormationUnit swapper, FormationUnit target)
    {
        if (swapper.RankSlot.Ranks != target.RankSlot.Ranks)
            return;

        int swapperRank = swapper.Rank;
        int targetRank = target.Rank;

        swapper.RankSlot.Relocate(targetRank);
        target.RankSlot.Relocate(swapperRank);
        Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });

        swapper.Character.ApplySingleBuffRule(RaidSceneManager.Rules.GetIdleUnitRules(swapper), BuffRule.InRank);
        target.Character.ApplySingleBuffRule(RaidSceneManager.Rules.GetIdleUnitRules(target), BuffRule.InRank);
    }

    public void SpawnCorpse(FormationUnit deadUnit, Monster corpse)
    {
        if (!Party.Units.Contains(deadUnit))
            return;

        deadUnit.Character = corpse;
        deadUnit.OverlaySlot.UpdateUnit();
        deadUnit.RankSlot.UpdateSlot();
        deadUnit.CombatInfo.IsDead = false;
        deadUnit.CombatInfo.PrepareForBattle(deadUnit.CombatInfo.CombatId, corpse, true);
    }

    public void SpawnUnit(FormationUnit spawnUnit, int targetRank)
    {
        Party.AddUnit(spawnUnit, targetRank);
        Ranks.RedistributeParty(Party);
        Overlay.FreeSlot.LockOnUnit(spawnUnit);
    }

    public void DeleteUnit(FormationUnit deadUnit, bool destroy = true)
    {
        if (!Party.Units.Contains(deadUnit))
            return;

        Party.RemoveUnit(deadUnit);
        deadUnit.OverlaySlot.ClearTarget();
        deadUnit.RankSlot.ClearSlot();
        if(destroy)
            Destroy(deadUnit.gameObject);
    }

    public void DeleteUnitDelayed(FormationUnit deadUnit, float delay)
    {
        if (!Party.Units.Contains(deadUnit))
            return;

        Party.RemoveUnit(deadUnit);
        deadUnit.OverlaySlot.ClearTarget();
        deadUnit.RankSlot.ClearSlot();
        Destroy(deadUnit.gameObject, delay);
    }

    public void UpdateBuffRule(BuffRule rule)
    {
        if (Party == null || Party.Units == null)
            return;

        foreach (FormationUnit unit in Party.Units)
            unit.Character.ApplySingleBuffRule(RaidSceneManager.Rules.GetIdleUnitRules(unit), rule);
    }

    public bool ContainsBaseClass(string baseClass)
    {
        return Party.Units.Any(unit => unit.Character.Class == baseClass);
    }

    private void UpdateHero(FormationOverlaySlot unitSlot)
    {
        unitSlot.TargetUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(unitSlot.TargetUnit));
        RaidSceneManager.RaidPanel.SelectHeroUnit(unitSlot.TargetUnit);
        RaidSceneManager.Inventory.UpdateState();
    }
}