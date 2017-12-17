using UnityEngine;

public class ControlEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Control; } }
    private int Duration { get; set; }

    public ControlEffect(int duration)
    {
        Duration = duration;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (!performer.Character.IsMonster)
            return false;

        if (((Monster)performer.Character).Data.ControllerCaptor == null)
            return false;

        float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
        debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

        if (RandomSolver.CheckSuccess(debuffChance))
        {
            RaidSceneManager.BattleGround.ControlUnit(target, performer, Duration);
            return true;
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            return true;
        }
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
        return false;
    }
}