using System.Collections.Generic;

public class Monster : Character
{
    public override List<SkillArtInfo> SkillArtInfo { get { return Data.SkillArtInfo; } }
    public override CombatSkill RiposteSkill { get { return Data.RiposteSkill; } }
    public override int RenderRankOverride { get { return Data.RenderingRankOverride; } }
    public override bool IsMonster { get { return true; } }
    public override string Name { get { return Data.StringId; } }
    public override string Class { get { return Data.TypeId; } }
    public override int Size { get { return Data.Size; } }
    public List<MonsterType> Types { get { return Data.EnemyTypes; } }
    public override CommonEffects CommonEffects { get { return Data.CommonEffects; } }
    public override Initiative Initiative { get { return Data.Initiative; } }
    public override DisplayModifier DisplayModifier { get { return Data.DisplayModifier; } }
    public override TorchlightModifier TorchlightModifier { get { return Data.TorchlightModifier; } }
    public override HealthbarModifier HealthbarModifier { get { return Data.HealthbarModifier; } }
    public override DeathClass DeathClass { get { return Data.DeathClass; } }
    public override DeathDamage DeathDamage { get { return Data.DeathDamage; } }
    public override BattleModifier BattleModifiers { get { return Data.Modifiers; } }
    public override Companion Companion { get { return Data.Companion; } }
    public override EmptyCaptor EmptyCaptor { get { return Data.EmptyCaptor; } }
    public override FullCaptor FullCaptor { get { return Data.FullCaptor; } }
    public override Controller ControllerCaptor { get { return Data.ControllerCaptor; } }
    public override LifeTime LifeTime { get { return Data.LifeTime; } }
    public override LifeLink LifeLink { get { return Data.LifeLink; } }
    public override SharedHealth SharedHealth { get { return Data.SharedHealth; } }
    public override Shapeshifter Shapeshifter { get { return Data.Shapeshifter; } }
    public override Spawn Spawn { get { return Data.Spawn; } }
    public override SkillReaction SkillReaction { get { return Data.SkillReaction; } }
    public override List<LootDefinition> Loot { get { return Data.Loot; } }
    public override List<MonsterType> MonsterTypes { get { return Data.EnemyTypes; } }
    public MonsterData Data { get; private set; }
    public MonsterBrain Brain { get; private set; }

    public Monster(MonsterData monsterData) : base(monsterData)
    {
        Data = monsterData;
        Brain = DarkestDungeonManager.Data.Brains[monsterData.MonsterBrainId];
    }

    public Monster(FormationUnitSaveData unitSaveData) :
        base (unitSaveData, DarkestDungeonManager.Data.Monsters[unitSaveData.Name])
    {
        Data = DarkestDungeonManager.Data.Monsters[unitSaveData.Name];
        Brain = DarkestDungeonManager.Data.Brains[Data.MonsterBrainId];
    }

    public override void UpdateSaveData(FormationUnitSaveData saveUnitData)
    {
        saveUnitData.IsHero = false;
        saveUnitData.Class = Class;
        saveUnitData.Name = Name;
        saveUnitData.CurrentHp = CurrentHealth;
        saveUnitData.Buffs = BuffInfo;
        saveUnitData.Statuses = StatusEffects;
    }
}