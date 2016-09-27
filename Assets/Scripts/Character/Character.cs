using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum AttributeType
{
    Undefined, HitPoints, Stress, HpHealAmount, HpHealPercent, DmgReceivedPercent, HpHealReceivedPercent,
    StressDmgReceivedPercent, StressDmgPercent, StressHealPercent, StressHealReceivedPercent,
    ResolveCheckPercent, ResolveXpPercent, StunChance, PoisonChance, BleedChance, MoveChance,
    DebuffChance, ScoutingChance, PartySurpriseChance, MonsterSurpirseChance, RemoveQuirkChance,
    FoodConsumption, StarvingDamagePercent, DefenseRating, ProtectionRating, SpeedRating,
    AttackRating, CritChance, DamageLow, DamageHigh, ArmorDiscount, WeaponDiscount, Stun,
    Poison, Disease, DeathBlow, Move, Bleed, Debuff, Trap,
}

public enum AttributeCategory
{
    Undefined, CombatStat, Modifier, Discount, Resistance
}

public class Character
{
    protected List<BuffInfo> buffInfo;
    protected Dictionary<StatusType, StatusEffect> statusEffects;
    protected Dictionary<AttributeType, SingleAttribute> singleAttributes;
    protected Dictionary<AttributeType, PairedAttribute> pairedAttributes;

    #region Basic Stats
    private static AttributeType[] SingleStats = new AttributeType[]
    {
        AttributeType.DefenseRating, AttributeType.ProtectionRating, AttributeType.SpeedRating,
        AttributeType.AttackRating, AttributeType.CritChance, AttributeType.DamageLow, AttributeType.DamageHigh,
    };
    #endregion
    #region Modifiers
    private static AttributeType[] Modifiers = new AttributeType[]
    {
        AttributeType.HpHealAmount, AttributeType.HpHealPercent, AttributeType.MoveChance, AttributeType.DebuffChance,
        AttributeType.StressHealPercent, AttributeType.DmgReceivedPercent, AttributeType.HpHealReceivedPercent,
        AttributeType.StressDmgReceivedPercent, AttributeType.StressHealReceivedPercent, AttributeType.StunChance,
        AttributeType.PoisonChance, AttributeType.BleedChance, AttributeType.ResolveCheckPercent, AttributeType.StressDmgPercent, 
        AttributeType.ScoutingChance, AttributeType.PartySurpriseChance, AttributeType.MonsterSurpirseChance,
        AttributeType.RemoveQuirkChance, AttributeType.FoodConsumption, AttributeType.StarvingDamagePercent,
    };
    #endregion
    #region Hero Discounts
    private static AttributeType[] HeroDiscounts = new AttributeType[]
    {
        AttributeType.ArmorDiscount,
        AttributeType.WeaponDiscount,
    };
    #endregion
    #region Hero Resistances
    private static AttributeType[] HeroResistances = new AttributeType[]
    {
        AttributeType.Stun, AttributeType.Poison, AttributeType.Disease,
        AttributeType.DeathBlow, AttributeType.Move, AttributeType.Bleed,
        AttributeType.Debuff, AttributeType.Trap,
    };
    #endregion
    #region Monster Resistances
    private static AttributeType[] MonsterResistances = new AttributeType[]
    {
        AttributeType.Stun, AttributeType.Poison, AttributeType.Move,
        AttributeType.Bleed, AttributeType.Debuff,
    };
    #endregion

    public virtual List<SkillArtInfo> SkillArtInfo
    {
        get
        {
            return new List<SkillArtInfo>();
        }
    }
    public virtual CombatSkill RiposteSkill
    {
        get
        {
            return null;
        }
    }
    public virtual int Size
    {
        get
        {
            return 1;
        }
    }
    public virtual string Name
    {
        get
        {
            return "Character";
        }
    }
    public virtual string Class
    {
        get
        {
            return "Class";
        }
    }
    public virtual bool AtDeathsDoor
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsStressed
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsOverstressed
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsVirtued
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsAfflicted
    {
        get
        {
            return false;
        }
    }
    public virtual bool IsMonster
    {
        get
        {
            return false;
        }
    }
    public virtual int RenderRankOverride
    {
        get
        {
            return 0;
        }
    }
    public virtual bool InMode
    {
        get
        {
            return false;
        }
    }
    public virtual CharacterMode Mode
    {
        get
        {
            return null;
        }
    }
    public virtual Trait Trait
    {
        get
        {
            return null;
        }
        protected set
        {

        }
    }

    public virtual CommonEffects CommonEffects
    {
        get
        {
            return null;
        }
    }
    public virtual Initiative Initiative
    {
        get
        {
            return null;
        }
    }
    public virtual DisplayModifier DisplayModifier
    {
        get
        {
            return null;
        }
    }
    public virtual TorchlightModifier TorchlightModifier
    {
        get
        {
            return null;
        }
    }
    public virtual HealthbarModifier HealthbarModifier
    { 
        get
        {
            return null;
        }
    }
    public virtual DeathClass DeathClass
    { 
        get
        {
            return null;
        }
    }
    public virtual DeathDamage DeathDamage
    {
        get
        {
            return null;
        }
    }
    public virtual BattleModifier BattleModifiers
    {
        get
        {
            return null;
        }
    }
    public virtual Companion Companion
    {
        get
        {
            return null;
        }
    }
    public virtual EmptyCaptor EmptyCaptor
    {
        get
        {
            return null;
        }
    }
    public virtual FullCaptor FullCaptor
    {
        get
        {
            return null;
        }
    }
    public virtual Controller ControllerCaptor
    {
        get
        {
            return null;
        }
    }
    public virtual LifeTime LifeTime
    {
        get
        {
            return null;
        }
    }
    public virtual LifeLink LifeLink
    {
        get
        {
            return null;
        }
    }
    public virtual SharedHealth SharedHealth
    {
        get
        {
            return null;
        }
    }
    public virtual Shapeshifter Shapeshifter
    {
        get
        {
            return null;
        }
    }
    public virtual Spawn Spawn
    {
        get
        {
            return null;
        }
    }
    public virtual SkillReaction SkillReaction
    {
        get
        {
            return null;
        }
    }
    public virtual List<MonsterType> MonsterTypes
    {
        get
        {
            return null;
        }
    }
    public virtual List<LootDefinition> Loot
    {
        get
        {
            return null;
        }
    }

    public bool ReadyForAfflictionCheck
    {
        get
        {
            return !(IsVirtued || IsAfflicted) && IsOverstressed;
        }
    }
    public float FoodConsumption
    {
        get
        {
            var food = GetSingleAttribute(AttributeType.FoodConsumption);
            if (food != null)
                return Mathf.Clamp(food.ModifiedValue, -1f, float.MaxValue);
            else
                return 0;
        }
    }
    public float Speed
    {
        get
        {
            var speed = GetSingleAttribute(AttributeType.SpeedRating);
            if (speed != null)
                return Mathf.Clamp(speed.ModifiedValue, 0, float.MaxValue);
            else
                return 0;
        }
    }
    public float Crit
    {
        get
        {
            var crit = GetSingleAttribute(AttributeType.CritChance);
            if (crit != null)
                return Mathf.Clamp(crit.ModifiedValue, 0, 1);
            else
                return 0;
        }
    }
    public float Accuracy
    {
        get
        {
            var acc = GetSingleAttribute(AttributeType.AttackRating);
            if (acc != null)
                return Mathf.Clamp(acc.ModifiedValue, -1, 2);
            else
                return 0;
        }
    }
    public float Dodge
    {
        get
        {
            var dodge = GetSingleAttribute(AttributeType.DefenseRating);
            if (dodge != null)
                return Mathf.Clamp(dodge.ModifiedValue, 0, Mathf.Max(3, dodge.RawValue));
            else
                return 0;
        }
    }
    public float Protection
    {
        get
        {
            var prot = GetSingleAttribute(AttributeType.ProtectionRating);
            if (prot != null)
                return Mathf.Clamp(prot.ModifiedValue, -1, Mathf.Max(0.85f, prot.RawValue));
            else
                return 0;
        }
    }
    public float MinDamage
    {
        get
        {
            var dmgLow = GetSingleAttribute(AttributeType.DamageLow);
            if (dmgLow != null)
                return Mathf.Clamp(dmgLow.ModifiedValue, 0, float.MaxValue);
            else
                return 0;
        }
    }
    public float MaxDamage
    {
        get
        {
            var dmgHigh = GetSingleAttribute(AttributeType.DamageHigh);
            if (dmgHigh != null)
                return Mathf.Clamp(dmgHigh.ModifiedValue, MinDamage, float.MaxValue);
            else
                return 0;
        }
    }
    public float DamageMod
    {
        get
        {
            var dmg = GetSingleAttribute(AttributeType.DamageHigh);
            if (dmg != null)
                return Mathf.Clamp(dmg.Multiplier, -0.95f, float.MaxValue);
            else
                return 1;
        }
    }

    public static void InitializeBasicStatuses(Dictionary<StatusType, StatusEffect> targetDictionary)
    {
        targetDictionary.Clear();

        targetDictionary.Add(StatusType.Stun, new StunStatusEffect());
        targetDictionary.Add(StatusType.Marked, new MarkStatusEffect());
        targetDictionary.Add(StatusType.Riposte, new RiposteStatusEffect());
        targetDictionary.Add(StatusType.Bleeding, new BleedingStatusEffect());
        targetDictionary.Add(StatusType.Poison, new PoisonStatusEffect());
        targetDictionary.Add(StatusType.Guard, new GuardStatusEffect());
        targetDictionary.Add(StatusType.Guarded, new GuardedStatusEffect());
        targetDictionary.Add(StatusType.DeathsDoor, new DeathsDoorStatusEffect());
        targetDictionary.Add(StatusType.DeathRecovery, new DeathRecoveryStatusEffect());
    }

    protected void AddSingleAttribute(AttributeType stat, SingleAttribute attribute)
    {
        singleAttributes.Add(stat, attribute);
    }
    protected void AddPairedAttribute(AttributeType stat, PairedAttribute attribute)
    {
        pairedAttributes.Add(stat, attribute);
    }

    public Character(HeroClass heroClass, int level)
    {
        buffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute(AttributeCategory.Discount));

        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                AddSingleAttribute(HeroResistances[i],
                    new SingleAttribute(heroClass.Resistanses[HeroResistances[i]], AttributeCategory.Resistance));
            else
                AddSingleAttribute(HeroResistances[i],
                    new SingleAttribute(heroClass.Resistanses[HeroResistances[i]] + level * 0.1f, AttributeCategory.Resistance));
    }
    public Character(HeroClass heroClass)
    {
        buffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute(AttributeCategory.Discount));

        for (int i = 0; i < HeroResistances.Length; i++)
            AddSingleAttribute(HeroResistances[i],
                new SingleAttribute(heroClass.Resistanses[HeroResistances[i]], AttributeCategory.Resistance));
    }
    public Character(SaveHeroData saveHeroData)
    {
        buffInfo = saveHeroData.buffs;
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        HeroClass heroClass = DarkestDungeonManager.Data.HeroClasses[saveHeroData.heroClass];

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute(AttributeCategory.Discount));

        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                AddSingleAttribute(HeroResistances[i],
                    new SingleAttribute(heroClass.Resistanses[HeroResistances[i]], AttributeCategory.Resistance));
            else
                AddSingleAttribute(HeroResistances[i],
                    new SingleAttribute(heroClass.Resistanses[HeroResistances[i]]
                    + saveHeroData.resolveLevel * 0.1f, AttributeCategory.Resistance));
    }
    public Character(MonsterData monsterData)
    {
        buffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(statusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(monsterData.Attributes[AttributeType.HitPoints],
            monsterData.Attributes[AttributeType.HitPoints], true, AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            if(monsterData.Attributes.ContainsKey(SingleStats[i]))
                AddSingleAttribute(SingleStats[i],
                    new SingleAttribute(monsterData.Attributes[SingleStats[i]], AttributeCategory.CombatStat));
            else
                AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < MonsterResistances.Length; i++)
            AddSingleAttribute(MonsterResistances[i],
                new SingleAttribute(monsterData.Attributes[MonsterResistances[i]], AttributeCategory.Resistance));
    }
    public Character(FormationUnitSaveData unitSaveData, MonsterData monsterData)
    {
        buffInfo = unitSaveData.Buffs;
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        statusEffects = unitSaveData.Statuses;

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute(unitSaveData.CurrentHp,
            monsterData.Attributes[AttributeType.HitPoints], true, AttributeCategory.CombatStat));

        for (int i = 0; i < SingleStats.Length; i++)
            if (monsterData.Attributes.ContainsKey(SingleStats[i]))
                AddSingleAttribute(SingleStats[i],
                    new SingleAttribute(monsterData.Attributes[SingleStats[i]], AttributeCategory.CombatStat));
            else
                AddSingleAttribute(SingleStats[i], new SingleAttribute(AttributeCategory.CombatStat));

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute(AttributeCategory.Modifier));

        for (int i = 0; i < MonsterResistances.Length; i++)
            AddSingleAttribute(MonsterResistances[i],
                new SingleAttribute(monsterData.Attributes[MonsterResistances[i]], AttributeCategory.Resistance));
    }

    protected void UpdateResolve(int level, HeroClass heroClass)
    {
        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                GetSingleAttribute(HeroResistances[i]).RawValue = heroClass.Resistanses[HeroResistances[i]];
            else
                GetSingleAttribute(HeroResistances[i]).RawValue = heroClass.Resistanses[HeroResistances[i]] + level * 0.1f;
    }

    public void LoadStatusEffects(Dictionary<StatusType, StatusEffect> newStatusEffects)
    {
        statusEffects = newStatusEffects;
    }
    public void UpdateDurations(BuffDurationType durationType)
    {
        foreach (var buffEntry in buffInfo.FindAll(roundBuff => roundBuff.DurationType == durationType))
            if (--buffEntry.Duration <= 0)
                RemoveBuff(buffEntry);
    }
    public void UpdateRound()
    {
        foreach (var effect in statusEffects)
            effect.Value.UpdateNextTurn();

        UpdateDurations(BuffDurationType.Round);
    }

    public PairedAttribute Health
    {
        get
        {
            return GetPairedAttribute(AttributeType.HitPoints);
        }
    }
    public PairedAttribute Stress
    {
        get
        {
            return GetPairedAttribute(AttributeType.Stress);
        }
    }

    public bool HasBuffs()
    {
        return buffInfo.Find(info => info.SourceType == BuffSourceType.Adventure && info.Buff.IsPositive()) != null;
    }
    public bool HasDebuffs()
    {
        return buffInfo.Find(info => info.SourceType == BuffSourceType.Adventure && !info.Buff.IsPositive()) != null;
    }
    public void ApplyStunRecovery()
    {
        var recoveryBuff = DarkestDungeonManager.Data.Buffs["STUNRECOVERYBUFF"];
        int recoveryStackCount = 0;

        for(int i = 0; i < buffInfo.Count; i++)
        {
            if (buffInfo[i].Buff == recoveryBuff)
                recoveryStackCount++;
        }

        recoveryStackCount++;

        for(int i = 0; i < recoveryStackCount; i++)
            AddBuff(new BuffInfo(recoveryBuff, BuffDurationType.Round, BuffSourceType.Adventure, 2));
    }
    public void RemoveConditionalBuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
        {
            if (buffInfo[i].SourceType == BuffSourceType.Condition)
                RemoveBuff(buffInfo[i]);
        }
    }
    public void RemoveCampingBuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
        {
            if (buffInfo[i].DurationType == BuffDurationType.Camp)
                RemoveBuff(buffInfo[i]);
        }
    }
    public void RemoveLightBuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
        {
            if (buffInfo[i].SourceType == BuffSourceType.Light)
                RemoveBuff(buffInfo[i]);
        }
    }

    public string DeathsDoorBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.DeathsDoor
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach(var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }
    public string MortalityBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Mortality
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }
    public string TraitBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Trait
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }
    public string CombatBuffTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Combat && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_duration_combat_end_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Camp && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {

            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_until_camp_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Round && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_duration_round_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Raid && info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_until_end_of_raid_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }
        return toolTip.TrimStart('\n');
    }
    public string CombatDebuffTooltip()
    {
        string toolTip = "";

        var availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Combat && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_duration_combat_end_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Camp && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString("tray_icon_tooltip_buff_until_camp_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Round && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_duration_round_format"), buffTooltip, maxRound);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }

        availableBuffs = buffInfo.FindAll(info => info.SourceType == BuffSourceType.Adventure &&
            info.ModifierValue != 0 && info.DurationType == BuffDurationType.Raid && !info.Buff.IsPositive()).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.AttributeType == availableBuffs[i].Buff.AttributeType
                && info.Buff.RuleType == availableBuffs[i].Buff.RuleType && info.Buff.IsFalseRule == availableBuffs[i].Buff.IsFalseRule);
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
            toolTip += "\n" + "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_until_end_of_raid_format"), buffTooltip);
            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }
        return toolTip.TrimStart('\n');
    }

    protected void ApplyBuff(BuffInfo buffEntry)
    {
        if (buffEntry.IsApplied)
            return;
        buffEntry.IsApplied = true;

        if(buffEntry.Buff.Type == BuffType.StatAdd)
            GetAttribute(buffEntry.Buff.AttributeType).FlatAddition += buffEntry.ModifierValue;
        else if(buffEntry.Buff.Type == BuffType.StatMultiply)
            GetAttribute(buffEntry.Buff.AttributeType).Multiplier += buffEntry.ModifierValue;
    }
    protected void RevertBuff(BuffInfo buffEntry)
    {
        if (!buffEntry.IsApplied)
            return;
        buffEntry.IsApplied = false;

        if (buffEntry.Buff.Type == BuffType.StatAdd)
            GetAttribute(buffEntry.Buff.AttributeType).FlatAddition -= buffEntry.ModifierValue;
        else if (buffEntry.Buff.Type == BuffType.StatMultiply)
            GetAttribute(buffEntry.Buff.AttributeType).Multiplier -= buffEntry.ModifierValue;
    }
    protected void RemoveBuff(BuffInfo buffEntry)
    {
        buffInfo.Remove(buffEntry);
        RevertBuff(buffEntry);
    }
    protected void ApplyBuffRule(BuffInfo buffEntry, RaidRuleInfo raidRuleInfo)
    {
        switch (buffEntry.Buff.RuleType)
        {
            case BuffRule.Afflicted: // done
                #region Afflicted
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.IsAfflicted)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.IsAfflicted)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Always: // done
                #region Always
                if (buffEntry.Buff.IsFalseRule)
                {
                    RevertBuff(buffEntry);
                }
                else
                {
                    ApplyBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.DeathsDoor:  // done
                #region DeathsDoor
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.AtDeathsDoor)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.AtDeathsDoor)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.EnemyType: // done
                #region EnemyType
                if (raidRuleInfo.Target == null || raidRuleInfo.Target.Character.IsMonster == false)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Target.Character.MonsterTypes.
                        Contains(CharacterHelper.StringToMonsterType(buffEntry.Buff.StringParam)))
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Target.Character.MonsterTypes.
                        Contains(CharacterHelper.StringToMonsterType(buffEntry.Buff.StringParam)))
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.FirstRound: // done
                #region FirstRound
                if (raidRuleInfo.BattleGround.BattleStatus != BattleStatus.Fighting)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.BattleGround.Round.RoundNumber == 0)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.BattleGround.Round.RoundNumber == 0)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.HpAbove: // done
                #region HpAbove
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio > buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.HpBelow:  // done
                #region HpAbove
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Health.ValueRatio < buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InActivity:
                #region InActivity
                RevertBuff(buffEntry);
                break;
                #endregion
            case BuffRule.InCamp:
                #region InCamp
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.IsDoingCamping)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.IsDoingCamping)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InCorridor:  // done
                #region InCorridor
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InDungeon:  // done
                #region InDungeon
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Dungeon == buffEntry.Buff.StringParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Dungeon == buffEntry.Buff.StringParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InMode:  // done
                #region InMode
                if (raidRuleInfo.Unit.Character.InMode == false)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Mode.Id == buffEntry.Buff.StringParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Mode.Id == buffEntry.Buff.StringParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.InRank: // done
                #region InRank
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Rank == buffEntry.Buff.SingleParam + 1)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Rank == buffEntry.Buff.SingleParam + 1)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.LightAbove: // done
                #region LightAbove
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount > buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.LightBelow: // done
                #region LightBelow
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.TorchMeter.TorchAmount < buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Melee:  // done
                #region Melee
                if (raidRuleInfo.Skill == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Skill.Type == "melee")
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Skill.Type == "melee")
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Ranged:  // done
                #region Ranged
                if (raidRuleInfo.Skill == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Skill.Type == "ranged")
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Skill.Type == "ranged")
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Riposting:  // done
                #region Riposting
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.IsRiposting)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.IsRiposting)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Size:  // done
                #region Size
                if (raidRuleInfo.Target == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Target.Size == buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Target.Size == buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Skill:  // done
                #region Skill
                if (raidRuleInfo.Skill == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Skill.Id == buffEntry.Buff.StringParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Skill.Id == buffEntry.Buff.StringParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Status:  // done
                #region Status
                if (raidRuleInfo.Target == null)
                {
                    RevertBuff(buffEntry);
                    break;
                }
                var targetStatus = CharacterHelper.StringToStatusType(buffEntry.Buff.StringParam);
                if (targetStatus == StatusType.None)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Target.Character[targetStatus].IsApplied)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Target.Character[targetStatus].IsApplied)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.StressAbove:  // done
                #region StressAbove
                if (raidRuleInfo.Unit.Character.IsMonster)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue > buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.StressBelow:  // done
                #region StressBelow
                if (raidRuleInfo.Unit.Character.IsMonster)
                {
                    RevertBuff(buffEntry);
                    break;
                }

                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.Stress.CurrentValue < buffEntry.Buff.SingleParam)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.Virtued: // done
                #region Virtued
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.Unit.Character.IsVirtued)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.IsVirtued)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            case BuffRule.WalkBack: // done
                #region WalkBack
                if (buffEntry.Buff.IsFalseRule)
                {
                    if (raidRuleInfo.IsWalkingBack)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.IsWalkingBack)
                        ApplyBuff(buffEntry);
                    else
                        RevertBuff(buffEntry);
                }
                break;
                #endregion
            default:
                break;
        }
    }

    public void AddBuff(BuffInfo newBuffInfo)
    {
        buffInfo.Add(newBuffInfo);
        if(newBuffInfo.Buff.RuleType == BuffRule.Always)
            ApplyBuff(newBuffInfo);
    }
    public bool ContainsBuff(Buff buff, BuffSourceType sourceType)
    {
        return buffInfo.Find(item => item.Buff == buff && item.SourceType == sourceType) != null;
    }

    public void RemoveSourceBuff(Buff revertBuff, BuffSourceType sourceType)
    {
        var revertBuffInfo = buffInfo.Find(item => item.Buff == revertBuff && item.SourceType == sourceType);
        if (revertBuffInfo != null)
            RemoveBuff(revertBuffInfo);
    }
    public void RemoveCombatDebuffs()
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
            if (buffInfo[i].SourceType == BuffSourceType.Adventure && buffInfo[i].Buff.IsPositive() == false)
                RemoveBuff(buffInfo[i]);
    }
    public void RemoveAllBuffsWithSource(BuffSourceType sourceType)
    {
        for (int i = buffInfo.Count - 1; i >= 0; i--)
            if (buffInfo[i].SourceType == sourceType)
                RemoveBuff(buffInfo[i]);
    }

    public void ApplySingleBuffRule(RaidRuleInfo raidRuleInfo, BuffRule rule)
    {
        for (int i = 0; i < buffInfo.Count; i++)
            if(buffInfo[i].Buff.RuleType == rule)
                ApplyBuffRule(buffInfo[i], raidRuleInfo);
    }
    public void ApplyAllBuffRules(RaidRuleInfo raidRuleInfo)
    {
        for (int i = 0; i < buffInfo.Count; i++)
            ApplyBuffRule(buffInfo[i], raidRuleInfo);
    }

    public SingleAttribute GetSingleAttribute(AttributeType stat)
    {
        if (singleAttributes.ContainsKey(stat))
            return singleAttributes[stat];
        else
            return null;
    }
    public PairedAttribute GetPairedAttribute(AttributeType stat)
    {
        if (pairedAttributes.ContainsKey(stat))
            return pairedAttributes[stat];
        else
            return null;
    }
    public BaseAttribute GetAttribute(AttributeType stat)
    {
        if (singleAttributes.ContainsKey(stat))
            return singleAttributes[stat];
        else if (pairedAttributes.ContainsKey(stat))
            return pairedAttributes[stat];
        else
        {
            Debug.Log("Attribute not found: " + stat.ToString());
            return null;
        }
    }
    public StatusEffect GetStatusEffect(StatusType type)
    {
        return statusEffects[type];
    }

    public StatusEffect this[StatusType type]
    {
        get
        {
            if (type != StatusType.None)
                return statusEffects[type];
            else
                return null;
        }
    }
    public SingleAttribute this[AttributeType stat]
    {
        get
        {
            if (singleAttributes.ContainsKey(stat))
                return singleAttributes[stat];
            else
                return null;
        }
    }
    public PairedAttribute this[AttributeType stat, bool paired]
    {
        get
        {
            if (pairedAttributes.ContainsKey(stat))
                return pairedAttributes[stat];
            else
                return null;
        }
        set
        {
            if (pairedAttributes.ContainsKey(stat))
                pairedAttributes[stat] = value;
            else
                pairedAttributes.Add(stat, value);
        }
    }

    public virtual void UpdateSaveData(FormationUnitSaveData saveUnitData)
    {
        saveUnitData.IsHero = false;
        saveUnitData.Class = Class;
        saveUnitData.Name = Name;
        saveUnitData.CurrentHp = Health.CurrentValue;
        saveUnitData.Buffs = buffInfo;
        saveUnitData.Statuses = statusEffects;
    }
}