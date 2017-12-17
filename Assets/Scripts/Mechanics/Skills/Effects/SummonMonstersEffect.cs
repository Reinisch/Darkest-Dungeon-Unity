using System.Collections.Generic;
using UnityEngine;

public class SummonMonstersEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Summon; } }

    public int SummonCount { private get; set; }
    public bool CanSpawnLoot { private get; set; }

    public List<string> SummonMonsters { get; private set; }
    public List<float> SummonChances { get; private set; }
    public List<int> SummonLimits { get; private set; }
    public List<FormationSet> SummonRanks { get; private set; }
    public List<int> SummonRollInitiatives { get; private set; }

    public SummonMonstersEffect()
    {
        SummonMonsters = new List<string>();
        SummonChances = new List<float>();
        SummonLimits = new List<int>();
        SummonRanks = new List<FormationSet>();
        SummonRollInitiatives = new List<int>();
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        List<int> summonPool = new List<int>();
        List<float> chancePool = new List<float>(SummonChances);
        for (int i = 0; i < SummonMonsters.Count; i++)
            summonPool.Add(i);

        for (int i = 0; i < SummonCount; i++)
        {
            if (summonPool.Count == 0)
                break;

            int rolledIndex = RandomSolver.ChooseRandomIndex(chancePool);
            int summonIndex = summonPool[rolledIndex];
            if (SummonLimits.Count > 0)
            {
                if (SummonLimits[summonIndex] <= performer.Party.Units.FindAll(unit =>
                     unit.Character.Name == SummonMonsters[summonIndex]).Count)
                {
                    i--;
                    summonPool.RemoveAt(rolledIndex);
                    chancePool.RemoveAt(rolledIndex);
                    continue;
                }
            }
            if (performer.Formation.AvailableSummonSpace < DarkestDungeonManager.Data.Monsters[SummonMonsters[summonIndex]].Size)
            {
                i--;
                summonPool.RemoveAt(rolledIndex);
                chancePool.RemoveAt(rolledIndex);
                continue;
            }
            MonsterData summonData = DarkestDungeonManager.Data.Monsters[SummonMonsters[summonIndex]];
            GameObject summonObject = Resources.Load("Prefabs/Monsters/" + summonData.TypeId) as GameObject;
            bool rollInitiative = SummonRollInitiatives.Count > 0;
            if (SummonRanks.Count > 0)
                RaidSceneManager.BattleGround.SummonUnit(summonData, summonObject, SummonRanks[summonIndex].
                    Ranks[RandomSolver.Next(SummonRanks[summonIndex].Ranks.Count)], rollInitiative, CanSpawnLoot);
            else
                RaidSceneManager.BattleGround.SummonUnit(summonData, summonObject, 1, rollInitiative, CanSpawnLoot);
        }
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}