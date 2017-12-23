using UnityEngine;

public class GuardEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.GuardAlly; } }
    public bool SwapTargets { private get; set; }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
            {
                if (SwapTargets)
                    ApplyInstant(target, performer, effect);
                else
                    ApplyInstant(performer, target, effect);
            }
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var targetGuardStatus = (GuardStatusEffect)target.Character.GetStatusEffect(StatusType.Guard);
        var targetGuardedStatus = (GuardedStatusEffect)target.Character.GetStatusEffect(StatusType.Guarded);

        var performerGuardStatus = (GuardStatusEffect)performer.Character.GetStatusEffect(StatusType.Guard);
        var performerGuardedStatus = (GuardedStatusEffect)performer.Character.GetStatusEffect(StatusType.Guarded);

        if (performerGuardedStatus.IsApplied)
            performerGuardedStatus.ResetStatus();

        if (performerGuardStatus.IsApplied)
        {
            if (performerGuardStatus.Targets.Contains(target))
            {
                targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;
                if (targetGuardedStatus.Guard != performer)
                    Debug.LogError("Alien guard: " + targetGuardedStatus.Guard.name + ". Expected guard: " + performer.name);
            }
            else
            {
                if (targetGuardStatus.IsApplied)
                    targetGuardStatus.ResetStatus();

                if (targetGuardedStatus.IsApplied)
                    targetGuardedStatus.ResetStatus();

                targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;
                targetGuardedStatus.Guard = performer;
                performerGuardStatus.Targets.Add(target);
                target.OverlaySlot.UpdateOverlay();
            }
        }
        else
        {
            if (targetGuardStatus.IsApplied)
                targetGuardStatus.ResetStatus();

            if (targetGuardedStatus.IsApplied)
                targetGuardedStatus.ResetStatus();

            targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;
            targetGuardedStatus.Guard = performer;
            performerGuardStatus.Targets.Add(target);
            performer.OverlaySlot.UpdateOverlay();
            target.OverlaySlot.UpdateOverlay();
        }

        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (SwapTargets)
        {
            if (ApplyInstant(target, performer, effect))
            {
                RaidSceneManager.RaidEvents.ShowPopupMessage(performer, PopupMessageType.Guard);
                performer.OverlaySlot.UpdateOverlay();
                return true;
            }
        }
        else
        {
            if (ApplyInstant(performer, target, effect))
            {
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Guard);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_guard");
    }
}