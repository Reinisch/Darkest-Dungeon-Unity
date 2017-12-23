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