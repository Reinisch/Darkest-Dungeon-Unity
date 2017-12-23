using System.IO;

public class GuardedStatusEffect : StatusEffect
{
    public override StatusType Type { get { return StatusType.Guarded; } }
    public override bool IsApplied { get { return Guard != null && GuardDuration > 0; } }

    public int GuardCombatId { get; private set; }
    public int GuardDuration { get; set; }
    public FormationUnit Guard { get; set; }

    public override void UpdateNextTurn()
    {
    }

    public override void ResetStatus()
    {
        GuardDuration = 0;
        if (Guard == null)
            return;

        var removingGuard = (GuardStatusEffect)Guard.Character[StatusType.Guard];
        for (int i = removingGuard.Targets.Count - 1; i >= 0; i--)
        {
            var guardTarget = (GuardedStatusEffect)removingGuard.Targets[i].Character[StatusType.Guarded];
            if (guardTarget.Guard == Guard)
            {
                guardTarget.Guard = null;
                guardTarget.GuardDuration = 0;
                removingGuard.Targets[i].OverlaySlot.UpdateOverlay();
                removingGuard.Targets.RemoveAt(i);
            }
        }

        Guard = null;
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(GuardDuration);
        bw.Write(Guard == null ? -1 : Guard.CombatInfo.CombatId);
    }

    public override void ReadStatusData(BinaryReader br)
    {
        GuardDuration = br.ReadInt32();
        GuardCombatId = br.ReadInt32();
    }
}