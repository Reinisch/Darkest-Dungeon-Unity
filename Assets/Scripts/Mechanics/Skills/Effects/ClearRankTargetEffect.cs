public class ClearRankTargetEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.ClearTargetRanks; } }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (performer == null)
            return false;

        if (target.Team == Team.Heroes)
            RaidSceneManager.BattleGround.MonsterFormation.RankHolder.ClearMarks();
        else
            RaidSceneManager.BattleGround.HeroFormation.RankHolder.ClearMarks();

        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}