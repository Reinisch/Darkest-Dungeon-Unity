using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterBrain
{
    public string Id { get; set; }

    public List<SkillCooldown> SkillCooldowns { get; set; }
    public List<SkillSelectionDesire> SkillDesireSet { get; set; }
    public List<TargetSelectionDesire> TargetDesireSet { get; set; }
    public List<BonusInitiativeDesire> BonusDesireSet { get; set; }

    public MonsterBrain()
    {
        SkillCooldowns = new List<SkillCooldown>();
        SkillDesireSet = new List<SkillSelectionDesire>();
        TargetDesireSet = new List<TargetSelectionDesire>();
        BonusDesireSet = new List<BonusInitiativeDesire>();
    }
}

public class SkillCooldown
{
    public string SkillId { get; private set; }
    public int Amount { get; private set; }

    public SkillCooldown(string skillId, int amount)
    {
        SkillId = skillId;
        Amount = amount;
    }

    public bool ReduceCooldown()
    {
        return --Amount <= 0;
    }
    public SkillCooldown Copy()
    {
        return new SkillCooldown(SkillId, Amount);
    }
}
