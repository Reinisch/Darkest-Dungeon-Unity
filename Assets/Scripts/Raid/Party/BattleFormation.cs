using UnityEngine;

public class BattleFormation : MonoBehaviour
{
    public FormationRanks ranks;
    public FormationParty party;
    public FormationOverlay overlay;
    public RankPlaceholders rankHolder;

    public RectTransform RectTransform { get; set; }

    public int AliveUnitsCount
    {
        get
        {
            return party.Units.FindAll(unit => (unit.Character is Hero) ||
                !(unit.Character as Monster).Types.Contains(MonsterType.Corpse)).Count;
        }
    }
    public int AvailableSummonSpace
    {
        get
        {
            int freeSpace = 4;
            for(int i = 0; i < party.Units.Count; i++)
                if (!(party.Units[i].Character.IsMonster && party.Units[i].Character.BattleModifiers != null
                    && party.Units[i].Character.BattleModifiers.CanBeSummonRank))
                    freeSpace -= party.Units[i].Size;
            return freeSpace;
        }
    }
    public int AvailableFreeSpace
    {
        get
        {
            int freeSpace = 4;
            for (int i = 0; i < party.Units.Count; i++)
                    freeSpace -= party.Units[i].Size;
            return freeSpace;
        }
    }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    void UpdateHero(FormationOverlaySlot unitSlot)
    {
        unitSlot.TargetUnit.Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(unitSlot.TargetUnit));
        RaidSceneManager.RaidPanel.SelectHeroUnit(unitSlot.TargetUnit);
        RaidSceneManager.Inventory.UpdateState();
    }

    public void LoadParty(RaidParty heroParty)
    {
        party.CreateFormation(heroParty);
        ranks.DistributeParty(party);
        overlay.LockOnUnits(party);
        overlay.UpdateOverlay();

        foreach (var unit in party.Units)
        {
            unit.OverlaySlot.onHeroSelected += UpdateHero;
            unit.Formation = this;
        }

        party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });

        party.Units[0].OverlaySlot.UnitSelected();
    }

    public void LoadParty(BattleEncounter encounter)
    {
        party.CreateFormation(encounter);
        ranks.DistributeParty(party);

        foreach (var unit in party.Units)
            unit.Formation = this;
        overlay.LockOnUnits(party);
    }

    public void LoadParty(BattleFormationSaveData formationData, bool isHeroFormation)
    {
        if (isHeroFormation)
        {
            party.CreateFormation(formationData);
            ranks.DistributeParty(party);
            overlay.LockOnUnits(party);
            overlay.UpdateOverlay();

            foreach (var unit in party.Units)
                unit.Formation = this;

            foreach (var overlaySlot in overlay.OverlaySlots)
                overlaySlot.onHeroSelected += UpdateHero;

            party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
            party.Units[0].OverlaySlot.UnitSelected();
        }
        else
        {
            party.CreateFormation(formationData);
            ranks.DistributeParty(party);

            foreach (var unit in party.Units)
                unit.Formation = this;
            overlay.LockOnUnits(party);
        }

    }

    public void UpdateBuffRule(BuffRule rule)
    {
        if (party == null || party.Units == null)
            return;

        for (int i = 0; i < party.Units.Count; i++)
            party.Units[i].Character.ApplySingleBuffRule(RaidSceneManager.Rules.GetIdleUnitRules(party.Units[i]), rule);
    }


    public void SwapUnits(FormationUnit swapper, FormationUnit target)
    {
        if (swapper.RankSlot.Ranks != target.RankSlot.Ranks)
            return;

        int swapperRank = swapper.Rank;
        int targetRank = target.Rank;

        swapper.RankSlot.Relocate(targetRank);
        target.RankSlot.Relocate(swapperRank);
        party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });

        swapper.Character.ApplySingleBuffRule(RaidSceneManager.Rules.GetIdleUnitRules(swapper), BuffRule.InRank);
        target.Character.ApplySingleBuffRule(RaidSceneManager.Rules.GetIdleUnitRules(target), BuffRule.InRank);
    }

    public void SpawnCorpse(FormationUnit deadUnit, Monster corpse)
    {
        if (!party.Units.Contains(deadUnit))
            return;

        deadUnit.Character = corpse;
        deadUnit.OverlaySlot.UpdateUnit();
        deadUnit.RankSlot.UpdateSlot();
        deadUnit.CombatInfo.IsDead = false;
        deadUnit.CombatInfo.PrepareForBattle(deadUnit.CombatInfo.CombatId, corpse, true);
    }

    public void SpawnUnit(FormationUnit spawnUnit, int targetRank)
    {
        party.AddUnit(spawnUnit, targetRank);
        ranks.RedistributeParty(party);
        overlay.FreeSlot.LockOnUnit(spawnUnit);
    }

    public void DeleteUnit(FormationUnit deadUnit, bool destroy = true)
    {
        if (!party.Units.Contains(deadUnit))
            return;

        party.RemoveUnit(deadUnit);
        deadUnit.OverlaySlot.ClearTarget();
        deadUnit.RankSlot.ClearSlot();
        if(destroy)
            Destroy(deadUnit.gameObject);
    }

    public void DeleteUnitDelayed(FormationUnit deadUnit, float delay)
    {
        if (!party.Units.Contains(deadUnit))
            return;

        party.RemoveUnit(deadUnit);
        deadUnit.OverlaySlot.ClearTarget();
        deadUnit.RankSlot.ClearSlot();
        Destroy(deadUnit.gameObject, delay);
    }


    public bool IsStallingActive()
    {
        for (int i = 0; i < party.Units.Count; i++)
            if (party.Units[i].Character.BattleModifiers != null && party.Units[i].Character.BattleModifiers.DisableStallPenalty)
                return false;

        if (AliveUnitsCount > 1)
            return false;

        return true;
    }

    public bool ContainsBaseClass(string baseClass)
    {
        return party.Units.Find(unit => unit.Character.Class == baseClass) != null;
    }

    public bool CanBeSurprised()
    {
        for (int i = 0; i < party.Units.Count; i++)
            if (party.Units[i].Character.BattleModifiers.CanBeSurprised)
                return true;

        return false;
    }

    public bool AlwaysBeSurprised()
    {
        for (int i = 0; i < party.Units.Count; i++)
            if (party.Units[i].Character.BattleModifiers.AlwaysBeSurprised == false)
                return false;

        return true;
    }

    public bool CanSurprise()
    {
        for (int i = 0; i < party.Units.Count; i++)
            if (party.Units[i].Character.BattleModifiers.CanSurprise)
                return true;

        return false;
    }

    public bool AlwaysSurprises()
    {
        for (int i = 0; i < party.Units.Count; i++)
            if (party.Units[i].Character.BattleModifiers.AlwaysSurprise)
                return true;

        return false;
    }
}