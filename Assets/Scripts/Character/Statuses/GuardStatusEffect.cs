using System.Collections.Generic;
using System.IO;

public class GuardStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.Guard; } }
    public override bool IsApplied { get { return Targets.Count > 0; } }

    public List<FormationUnit> Targets { get; private set; }

    public GuardStatusEffect()
    {
        Targets = new List<FormationUnit>();
    }

    public override void UpdateNextTurn()
    {
        for (int i = Targets.Count - 1; i >= 0; i--)
        {
            var targetStatus = (GuardedStatusEffect)Targets[i].Character.GetStatusEffect(StatusType.Guarded);
            if (--targetStatus.GuardDuration <= 0)
            {
                targetStatus.Guard = null;
                Targets[i].OverlaySlot.UpdateOverlay();
                Targets.RemoveAt(i);
            }
        }
    }

    public override void ResetStatus()
    {
        for (int i = Targets.Count - 1; i >= 0; i--)
        {
            var guardedTarget = (GuardedStatusEffect)Targets[i].Character.GetStatusEffect(StatusType.Guarded);
            guardedTarget.Guard = null;
            guardedTarget.GuardDuration = 0;
            Targets[i].OverlaySlot.UpdateOverlay();
        }
        Targets.Clear();
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
    }

    public override void ReadStatusData(BinaryReader br)
    {
    }
}