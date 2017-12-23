using System.Collections.Generic;
using System.Linq;

public class Round
{
    public int RoundNumber { get; set; }

    public RoundStatus RoundStatus { get; set; }
    public TurnType TurnType { get; set; }
    public TurnStatus TurnStatus { get; set; }

    public HeroTurnAction HeroAction { get; set; }
    public FormationUnit SelectedUnit { get; set; }
    public FormationUnit SelectedTarget { get; set; }

    public List<FormationUnit> OrderedUnits { get; private set; }

    public Round()
    {
        OrderedUnits = new List<FormationUnit>();
    }

    public void PreHeroTurn(FormationUnit heroUnit)
    {
        RaidSceneManager.BattleGround.LastSkillUsed = null;
        heroUnit.CombatInfo.UpdateNextTurn();
        TurnType = TurnType.HeroTurn;
        TurnStatus = TurnStatus.PreTurn;

        HeroAction = HeroTurnAction.Waiting;
        SelectedUnit = heroUnit;
        SelectedTarget = null;

        OrderedUnits.Remove(heroUnit);
    }

    public void OnHeroTurn()
    {
        TurnStatus = TurnStatus.Progress;
    }

    public void PostHeroTurn()
    {
        TurnStatus = TurnStatus.PostTurn;

        HeroAction = HeroTurnAction.Waiting;
        SelectedUnit = null;
        SelectedTarget = null;
    }

    public void PreMonsterTurn(FormationUnit monsterUnit)
    {
        RaidSceneManager.BattleGround.LastSkillUsed = null;
        monsterUnit.CombatInfo.UpdateNextTurn();
        TurnType = TurnType.HeroTurn;
        TurnStatus = TurnStatus.PreTurn;

        HeroAction = HeroTurnAction.Waiting;
        SelectedUnit = monsterUnit;
        SelectedTarget = null;

        OrderedUnits.Remove(monsterUnit);
    }

    public void OnMonsterTurn()
    {
        TurnStatus = TurnStatus.Progress;
    }

    public void PostMonsterTurn()
    {
        TurnStatus = TurnStatus.PostTurn;

        HeroAction = HeroTurnAction.Waiting;
        SelectedUnit = null;
        SelectedTarget = null;
    }

    public void HeroActionSelected(HeroTurnAction actionType, FormationUnit selectedTarget)
    {
        HeroAction = actionType;
        SelectedTarget = selectedTarget;
    }

    public void HeroActionSelected(HeroTurnAction actionType, FormationUnit selectedTarget, int rngSeed)
    {
        HeroAction = actionType;
        SelectedTarget = selectedTarget;
        RandomSolver.SetRandomSeed(rngSeed);
    }

    public void StartBattle(BattleGround battleground)
    {
        RoundNumber = 0;
        RoundNumber = NextRound(battleground);
    }

    public int NextRound(BattleGround battleground)
    {
        RoundStatus = RoundStatus.Start;
        OrderedUnits.Clear();

        if(RoundNumber == 0 || RoundNumber == 1)
        {
            battleground.HeroFormation.UpdateBuffRule(BuffRule.FirstRound);
            battleground.MonsterFormation.UpdateBuffRule(BuffRule.FirstRound);
        }

        foreach (var unit in battleground.HeroParty.Units)
        {
            unit.CombatInfo.UpdateNextRound();
            OrderedUnits.Add(unit);

            if(SceneManagerHelper.ActiveSceneName == "DungeonMultiplayer")
                if(unit.Character.Class == "antiquarian")
                    OrderedUnits.Add(unit);
        }

        foreach (var unit in battleground.MonsterParty.Units)
        {
            unit.CombatInfo.UpdateNextRound();
            if (unit.Character.IsMonster)
                for (int i = 0; i < unit.Character.Initiative.NumberOfTurns; i++)
                    OrderedUnits.Add(unit);
            else
                OrderedUnits.Add(unit);

            if (SceneManagerHelper.ActiveSceneName == "DungeonMultiplayer")
                if (unit.Character.Class == "antiquarian")
                    OrderedUnits.Add(unit);
        }
        
        if(RoundNumber == 0)
        {
            if (RaidSceneManager.BattleGround.SurpriseStatus == SurpriseStatus.HeroesSurprised)
                OrderedUnits = new List<FormationUnit>(OrderedUnits.OrderByDescending(unit => unit.Character.IsMonster ?
                    unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble() :
                    unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble() - 100));
            else if (RaidSceneManager.BattleGround.SurpriseStatus == SurpriseStatus.MonstersSurprised)
                OrderedUnits = new List<FormationUnit>(OrderedUnits.OrderByDescending(unit => unit.Character.IsMonster ?
                    unit.Character.BattleModifiers != null && unit.Character.BattleModifiers.CanBeSurprised ?
                    unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble() - 100 :
                    unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble() :
                    unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble()));
            else
                OrderedUnits = new List<FormationUnit>(OrderedUnits.OrderByDescending(unit =>
                unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble()));
        }
        else
            OrderedUnits = new List<FormationUnit>(OrderedUnits.OrderByDescending(unit =>
                unit.Character.Speed + RandomSolver.Next(0, 3) + RandomSolver.NextDouble()));

        return ++RoundNumber;
    }

    public void InsertInitiativeRoll(FormationUnit unit)
    {
        if (unit.Character.Initiative == null || unit.Character.Initiative.NumberOfTurns < 1)
            return;

        for(int i = 0; i < OrderedUnits.Count; i++)
        {
            if (OrderedUnits[i].Character.Speed < unit.Character.Speed - 2)
            {
                OrderedUnits.Insert(i, unit);
                return;
            }
        }
        OrderedUnits.Add(unit);
    }
}