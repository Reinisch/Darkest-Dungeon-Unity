using System.Collections.Generic;

public sealed class BonusInitiativeAllyLastDamaged : BonusInitiativeDesire
{
    private string AllyBaseClass { get; set; }
    private bool IgnoreIfStun { get; set; }

    public BonusInitiativeAllyLastDamaged(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if(IgnoreIfStun && performer.Character[StatusType.Stun].IsApplied)
            return false;

        if (AllyBaseClass != null && RaidSceneManager.BattleGround.LastDamaged.Contains(AllyBaseClass))
        {
            RaidSceneManager.BattleGround.LastDamaged.Clear();
            return true;
        }

        return false;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "ally_base_class_id":
                    AllyBaseClass = (string)dataSet["ally_base_class_id"];
                    break;
                case "ignore_if_stun":
                    IgnoreIfStun = (bool)dataSet["ignore_if_stun"];
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}