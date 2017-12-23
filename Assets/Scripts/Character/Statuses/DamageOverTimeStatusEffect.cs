using System.Collections.Generic;
using System.IO;
using System.Linq;

public abstract class DamageOverTimeStatusEffect : StatusEffect
{
    public override bool IsApplied { get { return doTs.Count > 0; } }

    private readonly List<DamageOverTimeInstanse> doTs = new List<DamageOverTimeInstanse>();

    public int CurrentTickDamage { get { return doTs.Count > 0 ? doTs.Sum(dot => dot.TickDamage) : 0; } }
    public int CombinedDamage { get { return doTs.Count > 0 ? doTs.Sum(dot => dot.TicksLeft * dot.TickDamage) : 0; } }
    public int ExpirationTime { get { return doTs.Count > 0 ? doTs.Max(dot => dot.TicksLeft) : 0; } }

    public override void UpdateNextTurn()
    {
        for (int i = doTs.Count - 1; i >= 0; i--)
            if (doTs[i].CheckExpiration())
                doTs.RemoveAt(i);
    }

    public override void ResetStatus()
    {
        RemoveDoT();
    }

    public override void WriteStatusData(BinaryWriter bw)
    {
        bw.Write(doTs.Count);
        for (int i = 0; i < doTs.Count; i++)
        {
            bw.Write(doTs[i].TickDamage);
            bw.Write(doTs[i].TicksAmount);
            bw.Write(doTs[i].TicksLeft);
        }
    }

    public override void ReadStatusData(BinaryReader br)
    {
        int dotCount = br.ReadInt32();
        doTs.Clear();
        for (int i = 0; i < dotCount; i++)
        {
            var newDot = new DamageOverTimeInstanse();
            newDot.TickDamage = br.ReadInt32();
            newDot.TicksAmount = br.ReadInt32();
            newDot.TicksLeft = br.ReadInt32();
            doTs.Add(newDot);
        }
    }

    public void AddInstanse(DamageOverTimeInstanse newInstance)
    {
        doTs.Add(newInstance);
    }

    public void RemoveDoT()
    {
        doTs.Clear();
    }
}