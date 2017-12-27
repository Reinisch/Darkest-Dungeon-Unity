using System.Collections.Generic;

public sealed class BonusInitiativeDeath : BonusInitiativeDesire
{
    public BonusInitiativeDeath(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        return performer.Character.HasZeroHealth;
    }
}