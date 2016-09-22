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