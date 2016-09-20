using System.Collections.Generic;
using System.Linq;
using System.IO;

public enum StatusType { None, Stun, Bleeding, Poison, Marked, Riposte, Guard, Guarded, DeathsDoor, DeathRecovery }
public enum DurationType { Round, Combat }

public interface IDamageOverTimeEffect
{
    int CurrentTickDamage { get; }
    int CombinedDamage { get; }
    int ExpirationTime { get; }

    void RemoveDoT();

    void AddInstanse(DamageOverTimeInstanse newInstance);
}

public class DamageOverTimeInstanse
{
    public int TickDamage { get; set; }
    public int TicksLeft { get; set; }
    public int TicksAmount { get; set; }

    public bool CheckExpiration()
    {
        return --TicksLeft <= 0;
    }
}

public abstract class StatusEffect
{
    public abstract bool IsApplied { get; }
    public abstract StatusType Type { get; }

    public abstract void UpdateNextTurn();
    public abstract void ResetStatus();
    public abstract void WriteStatusData(BinaryWriter bw);
    public abstract void ReadStatusData(BinaryReader br);
}

public class StunStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return StunApplied;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Stun;
        }
    }
    public bool StunApplied { get; set; }

    public override void UpdateNextTurn()
    {
    }
    public override void ResetStatus()
    {
        StunApplied = false;
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(StunApplied);
    }
    public override void ReadStatusData(BinaryReader br)
    {
        StunApplied = br.ReadBoolean();
    }
}
public class RiposteStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return RiposteDuration > 0;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Riposte;
        }
    }

    public DurationType DurationType { get; set; }
    public int RiposteDuration { get; set; }

    public override void UpdateNextTurn()
    {
        if (DurationType == DurationType.Combat)
            return;

        if (RiposteDuration > 0)
            RiposteDuration--;
    }
    public override void ResetStatus()
    {
        RiposteDuration = 0;
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write((int)DurationType);
        bw.Write(RiposteDuration);
    }
    public override void ReadStatusData(BinaryReader br)
    {
        DurationType = (DurationType)br.ReadInt32();
        RiposteDuration = br.ReadInt32();
    }
}
public class MarkStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return MarkDuration > 0;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Marked;
        }
    }

    public DurationType DurationType { get; set; }
    public int MarkDuration { get; set; }

    public override void UpdateNextTurn()
    {
        if (DurationType == DurationType.Combat)
            return;

        if (MarkDuration > 0)
            MarkDuration--;
    }
    public override void ResetStatus()
    {
        MarkDuration = 0;
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write((int)DurationType);
        bw.Write(MarkDuration);
    }
    public override void ReadStatusData(BinaryReader br)
    {
        DurationType = (DurationType)br.ReadInt32();
        MarkDuration = br.ReadInt32();
    }
}
public class DeathsDoorStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return AtDeathsDoor;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.DeathsDoor;
        }
    }

    public bool AtDeathsDoor { get; set; }

    public override void UpdateNextTurn()
    {
        return;
    }
    public override void ResetStatus()
    {
        AtDeathsDoor = false;
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(AtDeathsDoor);
    }
    public override void ReadStatusData(BinaryReader br)
    {
        AtDeathsDoor = br.ReadBoolean();
    }
}
public class DeathRecoveryStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return AtDeathRecovery;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.DeathRecovery;
        }
    }

    public bool AtDeathRecovery { get; set; }

    public override void UpdateNextTurn()
    {
        return;
    }
    public override void ResetStatus()
    {
        AtDeathRecovery = false;
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(AtDeathRecovery);
    }
    public override void ReadStatusData(BinaryReader br)
    {
        AtDeathRecovery = br.ReadBoolean();
    }
}
public class GuardStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return Targets.Count > 0;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Guard;
        }
    }

    public List<FormationUnit> Targets { get; set; }

    public GuardStatusEffect()
    {
        Targets = new List<FormationUnit>();
    }

    public override void UpdateNextTurn()
    {
        for (int i = Targets.Count - 1; i >= 0; i--)
        {
            var targetStatus = Targets[i].Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
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
        for (int i = Targets.Count - 1; i >= 0; i-- )
        {
            var guardedTarget = Targets[i].Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
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
public class GuardedStatusEffect : StatusEffect
{
    public override bool IsApplied
    {
        get
        {
            return Guard != null && GuardDuration > 0;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Guarded;
        }
    }

    public int GuardDuration { get; set; }
    public int GuardCombatId { get; set; }
    public FormationUnit Guard { get; set; }

    public override void UpdateNextTurn()
    {
        
    }
    public override void ResetStatus()
    {
        GuardDuration = 0;
        if(Guard != null)
        {
            var removingGuard = Guard.Character.GetStatusEffect(StatusType.Guard) as GuardStatusEffect;
            for(int i = removingGuard.Targets.Count - 1; i >= 0; i--)
            {
                var guardTarget = removingGuard.Targets[i].Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
                if(guardTarget.Guard == Guard)
                {
                    guardTarget.Guard = null;
                    guardTarget.GuardDuration = 0;
                    removingGuard.Targets[i].OverlaySlot.UpdateOverlay();
                    removingGuard.Targets.RemoveAt(i);
                }
            }
            Guard = null;
        }
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
public class BleedingStatusEffect : StatusEffect, IDamageOverTimeEffect
{
    public override bool IsApplied
    {
        get
        {
            return DoTs.Count > 0;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Bleeding;
        }
    }

    List<DamageOverTimeInstanse> DoTs = new List<DamageOverTimeInstanse>();
    
    public int CurrentTickDamage
    {
        get 
        {
            if (DoTs.Count > 0)
                return DoTs.Sum(dot => dot.TickDamage);
            else
                return 0;
        }
    }
    public int CombinedDamage
    {
        get
        {
            if (DoTs.Count > 0)
                return DoTs.Sum(dot => dot.TicksLeft * dot.TickDamage);
            else
                return 0;
        }
    }
    public int ExpirationTime
    {
        get
        {
            if (DoTs.Count > 0)
                return DoTs.Max(dot => dot.TicksLeft);
            else
                return 0;
        }
    }

    public override void UpdateNextTurn()
    {
        for (int i = DoTs.Count - 1; i >= 0; i--)
            if (DoTs[i].CheckExpiration())
                DoTs.RemoveAt(i);
    }
    public override void ResetStatus()
    {
        RemoveDoT();
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(DoTs.Count);
        for(int i = 0; i < DoTs.Count; i++)
        {
            bw.Write(DoTs[i].TickDamage);
            bw.Write(DoTs[i].TicksAmount);
            bw.Write(DoTs[i].TicksLeft);
        }
    }
    public override void ReadStatusData(BinaryReader br)
    {
        int dotCount = br.ReadInt32();
        DoTs.Clear();
        for (int i = 0; i < dotCount; i++)
        {
            var newDot = new DamageOverTimeInstanse();
            newDot.TickDamage = br.ReadInt32();
            newDot.TicksAmount = br.ReadInt32();
            newDot.TicksLeft = br.ReadInt32();
            DoTs.Add(newDot);
        }
    }

    public void AddInstanse(DamageOverTimeInstanse newInstance)
    {
        DoTs.Add(newInstance);
    }
    public void RemoveDoT()
    {
        DoTs.Clear();
    }
}
public class PoisonStatusEffect : StatusEffect, IDamageOverTimeEffect
{
    public override bool IsApplied
    {
        get
        {
            return DoTs.Count > 0;
        }
    }
    public override StatusType Type
    {
        get
        {
            return StatusType.Poison;
        }
    }

    List<DamageOverTimeInstanse> DoTs = new List<DamageOverTimeInstanse>();

    public int CurrentTickDamage
    {
        get
        {
            return DoTs.Sum(dot => dot.TickDamage);
        }
    }
    public int CombinedDamage
    {
        get
        {
            return DoTs.Sum(dot => dot.TicksLeft * dot.TickDamage);
        }
    }
    public int ExpirationTime
    {
        get
        {
            return DoTs.Max(dot => dot.TicksLeft);
        }
    }

    public override void UpdateNextTurn()
    {
        for (int i = DoTs.Count - 1; i >= 0; i--)
            if (DoTs[i].CheckExpiration())
                DoTs.RemoveAt(i);
    }
    public override void ResetStatus()
    {
        RemoveDoT();
    }
    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(DoTs.Count);
        for (int i = 0; i < DoTs.Count; i++)
        {
            bw.Write(DoTs[i].TickDamage);
            bw.Write(DoTs[i].TicksAmount);
            bw.Write(DoTs[i].TicksLeft);
        }
    }
    public override void ReadStatusData(BinaryReader br)
    {
        int dotCount = br.ReadInt32();
        DoTs.Clear();
        for (int i = 0; i < dotCount; i++)
        {
            var newDot = new DamageOverTimeInstanse();
            newDot.TickDamage = br.ReadInt32();
            newDot.TicksAmount = br.ReadInt32();
            newDot.TicksLeft = br.ReadInt32();
            DoTs.Add(newDot);
        }
    }

    public void AddInstanse(DamageOverTimeInstanse newInstance)
    {
        DoTs.Add(newInstance);
    }
    public void RemoveDoT()
    {
        DoTs.Clear();
    }
}