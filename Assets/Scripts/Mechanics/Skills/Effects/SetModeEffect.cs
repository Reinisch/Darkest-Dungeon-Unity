public class SetModeEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Mode; } }
    private string Mode { get; set; }

    public SetModeEffect(string mode)
    {
        Mode = mode;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character.IsMonster)
            return false;

        var heroTarget = (Hero)target.Character;

        target.SetCombatAnimation(false);

        heroTarget.CurrentMode = heroTarget.HeroClass.Modes.Find(mode => mode.Id == Mode);

        target.SetCombatAnimation(true);

        if (RaidSceneManager.RaidInterface.RaidPanel.SelectedUnit == target)
            RaidSceneManager.RaidInterface.RaidPanel.BannerPanel.SkillPanel.UpdateSkillPanel();

        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        return ApplyInstant(performer, target, effect);
    }

    public override string Tooltip(Effect effect)
    {
        var modName = LocalizationManager.GetString("actor_mode_name_" + Mode);
        return string.Format(LocalizationManager.GetString("effect_tooltip_set_actor_mode_format"), modName);
    }
}