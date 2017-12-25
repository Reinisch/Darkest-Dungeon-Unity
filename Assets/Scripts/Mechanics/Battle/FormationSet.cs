using System.Collections.Generic;

public class FormationSet
{
    public bool IsMultitarget { get; private set; }
    public bool IsRandomTarget { get; private set; }
    public bool IsSelfFormation { get; private set; }
    public bool IsSelfTarget { get; private set; }

    public List<int> Ranks { get; private set; }

    public SkillTargetType SkillTargetType
    {
        get
        {
            if (IsSelfTarget)
                return SkillTargetType.Self;

            return IsSelfFormation ? SkillTargetType.Party : SkillTargetType.Enemy;
        }
    }

    public bool IsLaunchableFrom(int rank, int size)
    {
        return Ranks.Exists(r => r >= rank && r <= rank + size - 1);
    }

    public bool IsTargetableUnit(FormationUnit unit)
    {
        return Ranks.Exists(r => r >= unit.Rank && r <= unit.Rank + unit.Size - 1);
    }

    public FormationSet(string formationString)
    {
        FromString(formationString);
    }

    private void FromString(string formationString)
    {
        IsMultitarget = false;
        IsRandomTarget = false;
        IsSelfFormation = false;

        Ranks = new List<int>();

        if (formationString == "")
        {
            IsSelfTarget = true;
            IsSelfFormation = true;
            return;
        }

        while(formationString[0] == '@' || formationString[0] == '~' || formationString[0] == '?')
        {
            if (formationString[0] == '@')
                IsSelfFormation = true;
            if (formationString[0] == '~')
                IsMultitarget = true;
            if (formationString[0] == '?')
                IsRandomTarget = true;

            formationString = formationString.Substring(1);
        }       

        for (int i = 0; i < formationString.Length; i++)
            Ranks.Add(int.Parse(formationString[i].ToString()));

        Ranks.Sort();
    }
}
