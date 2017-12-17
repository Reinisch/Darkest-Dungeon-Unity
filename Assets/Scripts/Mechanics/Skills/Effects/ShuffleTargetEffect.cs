using System.Collections.Generic;
using UnityEngine;

public class ShuffleTargetEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Shuffle; } }
    private bool IsPartyShuffle { get; set; }

    public ShuffleTargetEffect(bool isPartyShuffle)
    {
        IsPartyShuffle = isPartyShuffle;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Party.Units.Count < 2)
            return false;

        if (IsPartyShuffle)
        {
            var shuffleUnits = new List<FormationUnit>(target.Party.Units);
            foreach (var unit in shuffleUnits)
            {
                var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < unit.Rank)
                    unit.Pull(unit.Rank - shuffleRoll.Rank);
                else
                    unit.Push(shuffleRoll.Rank - unit.Rank);
            }
            shuffleUnits.Clear();
            return true;
        }
        else
        {
            float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
            if (performer != null && performer.Character is Hero)
                moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

            moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
            if (RandomSolver.CheckSuccess(moveChance))
            {
                var shuffleTargets = target.Party.Units.FindAll(unit => unit != target);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < target.Rank)
                    target.Pull(target.Rank - shuffleRoll.Rank);
                else
                    target.Push(shuffleRoll.Rank - target.Rank);
                return true;
            }
            return false;
        }
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Party.Units.Count < 2)
            return false;

        if (IsPartyShuffle)
        {
            foreach (var unit in target.Party.Units)
            {
                float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                moveChance -= unit.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
                if (RandomSolver.CheckSuccess(moveChance))
                {
                    var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                    var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                    if (shuffleRoll.Rank < unit.Rank)
                        unit.Pull(unit.Rank - shuffleRoll.Rank);
                    else
                        unit.Push(shuffleRoll.Rank - unit.Rank);
                    return true;
                }
            }
            return true;
        }
        else
        {
            float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
            if (performer != null && performer.Character is Hero)
                moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

            moveChance = Mathf.Clamp(moveChance, 0, 0.95f);
            if (RandomSolver.CheckSuccess(moveChance))
            {
                var shuffleTargets = target.Party.Units.FindAll(unit => unit != target);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < target.Rank)
                    target.Pull(target.Rank - shuffleRoll.Rank);
                else
                    target.Push(shuffleRoll.Rank - target.Rank);
                return true;
            }
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.MoveResist);
            return false;
        }
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString(IsPartyShuffle ? "effect_tooltip_shuffle_party" : "effect_tooltip_shuffle_single");
    }
}