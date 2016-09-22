using UnityEngine;
using System.Collections.Generic;

public class MonsterData
{
    public string StringId { get; set; }
    public string TypeId { get; set; }

    public int Size { get; set; }
    public int PreferableSkill { get; set; }
    public int RenderingRankOverride { get; set; }
    public string MonsterBrainId { get; set; }

    public Dictionary<AttributeType, float> Attributes { get; set; }

    public List<MonsterType> EnemyTypes { get; set; }
    public List<string> Tags { get; set; }

    public List<SkillArtInfo> SkillArtInfo { get; set; }
    public List<CombatSkill> CombatSkills { get; set; }
    public CombatSkill RiposteSkill { get; set; }
    public Vector2 DefendOffset { get; set; }

    public CommonEffects CommonEffects { get; set; }
    public Initiative Initiative { get; set; }
    public DisplayModifier DisplayModifier { get; set; }
    public TorchlightModifier TorchlightModifier { get; set; }
    public HealthbarModifier HealthbarModifier { get; set; }
    
    public List<LootDefinition> Loot { get; set; }

    public DeathClass DeathClass { get; set; }
    public DeathDamage DeathDamage { get; set; }
    public BattleModifier Modifiers { get; set; }

    public Companion Companion { get; set; }
    public EmptyCaptor EmptyCaptor { get; set; }
    public FullCaptor FullCaptor { get; set; }
    public Controller ControllerCaptor { get; set; }

    public LifeTime LifeTime { get; set; }
    public LifeLink LifeLink { get; set; }

    public SharedHealth SharedHealth { get; set; }
    public Shapeshifter Shapeshifter { get; set; }

    public Spawn Spawn { get; set; }
    public SkillReaction SkillReaction { get; set; }
    public string BattleStage { get; set; }
    public string BattleBackdrop { get; set; }
    public int? AudioIntensity { get; set; }

    public MonsterData()
    {
        Attributes = new Dictionary<AttributeType, float>();
        EnemyTypes = new List<MonsterType>();
        SkillArtInfo = new List<SkillArtInfo>();
        CombatSkills = new List<CombatSkill>();
        Loot = new List<LootDefinition>();
        Tags = new List<string>();
    }
}