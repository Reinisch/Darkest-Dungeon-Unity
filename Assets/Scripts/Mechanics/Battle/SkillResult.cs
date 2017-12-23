using System.Collections.Generic;

public class SkillResult
{
    public CombatSkill Skill { get; set; }
    public SkillArtInfo ArtInfo { get; set; }
    public SkillResultEntry Current { get; set; }
   
    public bool HasHit { get; set; }
    public bool HasZeroHealth { get; set; }
    public List<Effect> AppliedEffects { get; private set; }
    public List<SkillResultEntry> SkillEntries { get; private set; }

    public bool HasCritEffect
    {
        get
        {
            for (int i = 0; i < SkillEntries.Count; i++)
                if (SkillEntries[i].Type == SkillResultType.Crit && SkillEntries[i].CanCritReleaf)
                    return true;
            return false;
        }
    }

    public bool HasDeadEffect
    {
        get
        {
            for (int i = 0; i < SkillEntries.Count; i++)
                if (SkillEntries[i].IsZeroed && SkillEntries[i].CanKillReleaf)
                    return true;
            return false;
        }
    }

    public SkillResult()
    {
        AppliedEffects = new List<Effect>();
        SkillEntries = new List<SkillResultEntry>();
    }

    public void Reset()
    {
        Current = null;
        HasHit = false;
        HasZeroHealth = false;
        AppliedEffects.Clear();
        SkillEntries.Clear();
    }

    public SkillResult Copy()
    {
        SkillResult copy = new SkillResult();
        copy.Skill = Skill;
        copy.ArtInfo = ArtInfo;
        copy.Current = Current;
        copy.HasHit = HasHit;
        copy.HasZeroHealth = HasZeroHealth;
        copy.AppliedEffects = new List<Effect>(AppliedEffects);
        copy.SkillEntries = new List<SkillResultEntry>(SkillEntries);
        return copy;
    }

    public void AddResultEntry(SkillResultEntry entry)
    {
        Current = entry;
        SkillEntries.Add(entry);
        if (entry.Type != SkillResultType.Dodge && entry.Type != SkillResultType.Miss)
            HasHit = true;
        if (entry.IsZeroed)
            HasZeroHealth = true;
    }

    public void AddEffectEntry(Effect entry)
    {
        AppliedEffects.Add(entry);
    }
}