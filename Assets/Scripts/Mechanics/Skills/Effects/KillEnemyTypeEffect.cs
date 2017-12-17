public class KillEnemyTypeEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.KillType; } }
    private MonsterType EnemyType { get; set; }

    public KillEnemyTypeEffect(MonsterType monsterType)
    {
        EnemyType = monsterType;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character.IsMonster && ((Monster)target.Character).Types.Contains(EnemyType))
        {
            target.Character.TakeDamagePercent(1.0f);
            target.CombatInfo.MarkedForDeath = true;
            return true;
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }

    public override string Tooltip(Effect effect)
    {
        switch (EnemyType)
        {
            case MonsterType.Man:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_man"));
            case MonsterType.Beast:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_beast"));
            case MonsterType.Unholy:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_unholy"));
            case MonsterType.Eldritch:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_eldritch"));
            case MonsterType.Corpse:
                return string.Format(LocalizationManager.GetString("effect_tooltip_kill_enemy_type"),
                    LocalizationManager.GetString("enemy_type_name_corpse"));
            default:
                return "";
        }
    }
}