using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class Character
{
    public virtual List<SkillArtInfo> SkillArtInfo { get { return new List<SkillArtInfo>(); } }
    public virtual CombatSkill RiposteSkill { get { return null; } }
    public virtual int Size { get { return 1; } }
    public virtual string Name { get { return "Character"; } }
    public virtual string Class { get { return "Class"; } }
    public virtual bool AtDeathsDoor { get { return false; } }
    public virtual bool IsStressed { get { return false; } }
    public virtual bool IsOverstressed { get { return false; } }
    public virtual bool IsVirtued { get { return false; } }
    public virtual bool IsAfflicted { get { return false; } }
    public virtual bool IsMonster { get { return false; } }
    public virtual int RenderRankOverride { get { return 0; } }
    public virtual bool InMode { get { return false; } }
    public virtual CharacterMode Mode { get { return null; } }
    public virtual CommonEffects CommonEffects { get { return null; } }
    public virtual Initiative Initiative { get { return null; } }
    public virtual DisplayModifier DisplayModifier { get { return null; } }
    public virtual TorchlightModifier TorchlightModifier { get { return null; } }
    public virtual HealthbarModifier HealthbarModifier { get { return null; } }
    public virtual DeathClass DeathClass { get { return null; } }
    public virtual DeathDamage DeathDamage { get { return null; } }
    public virtual BattleModifier BattleModifiers { get { return null; } }
    public virtual Companion Companion { get { return null; } }
    public virtual EmptyCaptor EmptyCaptor { get { return null; } }
    public virtual FullCaptor FullCaptor { get { return null; } }
    public virtual Controller ControllerCaptor { get { return null; } }
    public virtual LifeTime LifeTime { get { return null; } }
    public virtual LifeLink LifeLink { get { return null; } }
    public virtual SharedHealth SharedHealth { get { return null; } }
    public virtual Shapeshifter Shapeshifter { get { return null; } }
    public virtual Spawn Spawn { get { return null; } }
    public virtual SkillReaction SkillReaction { get { return null; } }
    public virtual List<MonsterType> MonsterTypes { get { return null; } }
    public virtual List<LootDefinition> Loot { get { return null; } }

    public virtual Trait Trait
    {
        get { return null; }
        protected set { Assert.IsTrue(false, "Only heroes have trait! Character: " + Name + " Trait: " + value); }
    }

    public bool ReadyForAfflictionCheck
    {
        get { return !(IsVirtued || IsAfflicted) && IsOverstressed; }
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

    public float HealthRatio
    {
        get { return GetPairedAttribute(AttributeType.HitPoints).ValueRatio; }
    }

    public float CurrentHealth
    {
        get { return GetPairedAttribute(AttributeType.HitPoints).CurrentValue; }
    }

    public float MaxHealth
    {
        get { return GetPairedAttribute(AttributeType.HitPoints).ModifiedValue; }
    }

    public bool HasZeroHealth
    {
        get { return Mathf.RoundToInt(CurrentHealth) == 0; }
    }

    public PairedAttribute Health
    {
        get { return GetPairedAttribute(AttributeType.HitPoints); }
    }

    public PairedAttribute Stress
    {
        get { return GetPairedAttribute(AttributeType.Stress); }
    }

    protected readonly List<BuffInfo> BuffInfo;
    protected Dictionary<StatusType, StatusEffect> StatusEffects;

    private readonly Dictionary<AttributeType, SingleAttribute> singleAttributes;
    private readonly Dictionary<AttributeType, PairedAttribute> pairedAttributes;

    #region Static Character Data

    private static readonly BuffDurationType[] DisplayableTypes = new BuffDurationType[]
    {
        BuffDurationType.Combat, BuffDurationType.Camp, BuffDurationType.Round, BuffDurationType.Raid
    };

    private static readonly AttributeType[] SingleStats = new AttributeType[]
    {
        AttributeType.DefenseRating, AttributeType.ProtectionRating, AttributeType.SpeedRating,
        AttributeType.AttackRating, AttributeType.CritChance, AttributeType.DamageLow, AttributeType.DamageHigh,
    };

    private static readonly AttributeType[] Modifiers = new AttributeType[]
    {
        AttributeType.HpHealAmount, AttributeType.HpHealPercent, AttributeType.MoveChance, AttributeType.DebuffChance,
        AttributeType.StressHealPercent, AttributeType.DmgReceivedPercent, AttributeType.HpHealReceivedPercent,
        AttributeType.StressDmgReceivedPercent, AttributeType.StressHealReceivedPercent, AttributeType.StunChance,
        AttributeType.PoisonChance, AttributeType.BleedChance, AttributeType.ResolveCheckPercent, AttributeType.StressDmgPercent,
        AttributeType.ScoutingChance, AttributeType.PartySurpriseChance, AttributeType.MonsterSurpirseChance,
        AttributeType.RemoveQuirkChance, AttributeType.FoodConsumption, AttributeType.StarvingDamagePercent,
    };

    private static readonly AttributeType[] HeroDiscounts = new AttributeType[]
    {
        AttributeType.ArmorDiscount,
        AttributeType.WeaponDiscount,
    };

    private static readonly AttributeType[] HeroResistances = new AttributeType[]
    {
        AttributeType.Stun, AttributeType.Poison, AttributeType.Disease,
        AttributeType.DeathBlow, AttributeType.Move, AttributeType.Bleed,
        AttributeType.Debuff, AttributeType.Trap,
    };

    private static readonly AttributeType[] MonsterResistances = new AttributeType[]
    {
        AttributeType.Stun, AttributeType.Poison, AttributeType.Move,
        AttributeType.Bleed, AttributeType.Debuff,
    };

    #endregion

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

    private Character()
    {
        BuffInfo = new List<BuffInfo>();
        pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
        singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
        StatusEffects = new Dictionary<StatusType, StatusEffect>();
        InitializeBasicStatuses(StatusEffects);

        AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute());

        for (int i = 0; i < SingleStats.Length; i++)
            AddSingleAttribute(SingleStats[i], new SingleAttribute());

        for (int i = 0; i < Modifiers.Length; i++)
            AddSingleAttribute(Modifiers[i], new SingleAttribute());
    }

    protected Character(HeroClass heroClass, int level = 0) : this()
    {
        for (int i = 0; i < HeroDiscounts.Length; i++)
            AddSingleAttribute(HeroDiscounts[i], new SingleAttribute());

        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                AddSingleAttribute(HeroResistances[i],
                    new SingleAttribute(heroClass.Resistanses[HeroResistances[i]]));
            else
                AddSingleAttribute(HeroResistances[i],
                    new SingleAttribute(heroClass.Resistanses[HeroResistances[i]] + level * 0.1f));
    }

    protected Character(SaveHeroData saveHeroData) : 
        this(DarkestDungeonManager.Data.HeroClasses[saveHeroData.HeroClass], saveHeroData.ResolveLevel)
    {
        BuffInfo = saveHeroData.Buffs;
    }

    protected Character(MonsterData monsterData) : this()
    {
        this[AttributeType.HitPoints, true].RawValue = monsterData.Attributes[AttributeType.HitPoints];
        this[AttributeType.HitPoints, true].CurrentValue = monsterData.Attributes[AttributeType.HitPoints];

        for (int i = 0; i < SingleStats.Length; i++)
            if (monsterData.Attributes.ContainsKey(SingleStats[i]))
                this[SingleStats[i]].RawValue = monsterData.Attributes[SingleStats[i]];

        for (int i = 0; i < MonsterResistances.Length; i++)
            AddSingleAttribute(MonsterResistances[i],
                new SingleAttribute(monsterData.Attributes[MonsterResistances[i]]));
    }

    protected Character(FormationUnitSaveData unitSaveData, MonsterData monsterData) : this(monsterData)
    {
        BuffInfo = unitSaveData.Buffs;
        StatusEffects = unitSaveData.Statuses;

        this[AttributeType.HitPoints, true].CurrentValue = unitSaveData.CurrentHp;
    }

    public void UpdateRound()
    {
        foreach (var effect in StatusEffects)
            effect.Value.UpdateNextTurn();

        UpdateDurations(BuffDurationType.Round);
    }

    #region Buff Functions

    public void UpdateDurations(BuffDurationType durationType)
    {
        foreach (var buffEntry in BuffInfo.FindAll(roundBuff => roundBuff.DurationType == durationType))
            if (--buffEntry.Duration <= 0)
                RemoveBuff(buffEntry);
    }

    public bool HasBuffs()
    {
        return BuffInfo.Find(info => info.SourceType == BuffSourceType.Adventure && info.Buff.IsPositive()) != null;
    }

    public bool HasDebuffs()
    {
        return BuffInfo.Find(info => info.SourceType == BuffSourceType.Adventure && !info.Buff.IsPositive()) != null;
    }

    public bool HasEventBuffs()
    {
        return BuffInfo.Find(info => info.SourceType == BuffSourceType.Estate) != null;
    }

    public void ApplyStunRecovery()
    {
        var recoveryBuff = DarkestDungeonManager.Data.Buffs["STUNRECOVERYBUFF"];
        int recoveryStackCount = 0;

        for (int i = 0; i < BuffInfo.Count; i++)
        {
            if (BuffInfo[i].Buff == recoveryBuff)
                recoveryStackCount++;
        }

        recoveryStackCount++;

        for (int i = 0; i < recoveryStackCount; i++)
            AddBuff(new BuffInfo(recoveryBuff, BuffDurationType.Round, BuffSourceType.Adventure, 2));
    }

    public void RemoveConditionalBuffs()
    {
        for (int i = BuffInfo.Count - 1; i >= 0; i--)
        {
            if (BuffInfo[i].SourceType == BuffSourceType.Condition)
                RemoveBuff(BuffInfo[i]);
        }
    }

    public void RemoveCampingBuffs()
    {
        for (int i = BuffInfo.Count - 1; i >= 0; i--)
        {
            if (BuffInfo[i].DurationType == BuffDurationType.Camp)
                RemoveBuff(BuffInfo[i]);
        }
    }

    public void RemoveLightBuffs()
    {
        for (int i = BuffInfo.Count - 1; i >= 0; i--)
        {
            if (BuffInfo[i].SourceType == BuffSourceType.Light)
                RemoveBuff(BuffInfo[i]);
        }
    }    

    public void AddBuff(BuffInfo newBuffInfo)
    {
        BuffInfo.Add(newBuffInfo);
        if (newBuffInfo.Buff.RuleType == BuffRule.Always)
            ApplyBuff(newBuffInfo);
    }

    public bool ContainsBuff(Buff buff, BuffSourceType sourceType)
    {
        return BuffInfo.Find(item => item.Buff == buff && item.SourceType == sourceType) != null;
    }

    public void RemoveSourceBuff(Buff revertBuff, BuffSourceType sourceType)
    {
        var revertBuffInfo = BuffInfo.Find(item => item.Buff == revertBuff && item.SourceType == sourceType);
        if (revertBuffInfo != null)
            RemoveBuff(revertBuffInfo);
    }

    public void RemoveCombatDebuffs()
    {
        for (int i = BuffInfo.Count - 1; i >= 0; i--)
            if (BuffInfo[i].SourceType == BuffSourceType.Adventure && BuffInfo[i].Buff.IsPositive() == false)
                RemoveBuff(BuffInfo[i]);
    }

    public void RemoveAllBuffsWithSource(BuffSourceType sourceType)
    {
        for (int i = BuffInfo.Count - 1; i >= 0; i--)
            if (BuffInfo[i].SourceType == sourceType)
                RemoveBuff(BuffInfo[i]);
    }

    public void ApplySingleBuffRule(RaidRuleInfo raidRuleInfo, BuffRule rule)
    {
        for (int i = 0; i < BuffInfo.Count; i++)
            if (BuffInfo[i].Buff.RuleType == rule)
                ApplyBuffRule(BuffInfo[i], raidRuleInfo);
    }

    public void ApplyAllBuffRules(RaidRuleInfo raidRuleInfo)
    {
        for (int i = 0; i < BuffInfo.Count; i++)
            ApplyBuffRule(BuffInfo[i], raidRuleInfo);
    }

    public string DeathsDoorBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = BuffInfo.FindAll(info => info.SourceType == BuffSourceType.DeathsDoor
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }

    public string MortalityBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = BuffInfo.FindAll(info => info.SourceType == BuffSourceType.Mortality
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }

    public string TraitBuffsTooltip()
    {
        string toolTip = "";

        var availableBuffs = BuffInfo.FindAll(info => info.SourceType == BuffSourceType.Trait
            && info.Buff.AttributeType != AttributeType.DamageLow);
        foreach (var buffEntry in availableBuffs)
            toolTip += "\n" + buffEntry.Buff.ToolTip;
        return toolTip.TrimStart('\n');
    }

    public string EventBuffTooltip()
    {
        string toolTip = "";

        var availableBuffs = BuffInfo.FindAll(info => info.SourceType == BuffSourceType.Estate && info.ModifierValue != 0).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();
        availableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);

        for (int i = availableBuffs.Count - 1; i >= 0; i--)
        {
            var sameBuffs = availableBuffs.FindAll(info => info.Buff.IsSameBuff(availableBuffs[i].Buff));
            float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
            int maxRound = sameBuffs.Max(info => info.Duration);
            string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);

            toolTip += "\n" + string.Format(LocalizationManager.GetString(
                "tray_icon_tooltip_buff_duration_quest_end_format"), buffTooltip, maxRound);

            availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
            i -= sameBuffs.Count - 1;
        }
        return toolTip.TrimStart('\n');
    }

    public string CombatBuffTooltip(bool isPositive)
    {
        string toolTip = "";

        foreach (var durationType in DisplayableTypes)
        {
            var availableBuffs = FindDisplayableBuffs(BuffSourceType.Adventure, durationType, isPositive);
            for (int i = availableBuffs.Count - 1; i >= 0; i--)
            {
                var sameBuffs = availableBuffs.FindAll(info => info.Buff.IsSameBuff(availableBuffs[i].Buff));
                float modifierSum = sameBuffs.Sum(info => info.ModifierValue);
                string buffTooltip = availableBuffs[i].Buff.TooltipOverrided(modifierSum);
                int maxRound;

                switch (durationType)
                {
                    case BuffDurationType.Combat:
                        maxRound = sameBuffs.Max(info => info.Duration);
                        toolTip += "\n" + string.Format(LocalizationManager.GetString(
                            "tray_icon_tooltip_buff_duration_combat_end_format"), buffTooltip, maxRound);
                        break;
                    case BuffDurationType.Camp:
                        toolTip += "\n" + string.Format(LocalizationManager.GetString(
                            "tray_icon_tooltip_buff_until_camp_format"), buffTooltip);
                        break;
                    case BuffDurationType.Round:
                        maxRound = sameBuffs.Max(info => info.Duration);
                        toolTip += "\n" + string.Format(LocalizationManager.GetString(
                            "tray_icon_tooltip_buff_duration_round_format"), buffTooltip, maxRound);
                        break;
                    case BuffDurationType.Raid:
                        toolTip += "\n" + string.Format(LocalizationManager.GetString(
                            "tray_icon_tooltip_buff_until_end_of_raid_format"), buffTooltip);
                        break;
                }  

                availableBuffs.RemoveRange(availableBuffs.Count - sameBuffs.Count, sameBuffs.Count);
                i -= sameBuffs.Count - 1;
            }
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
        BuffInfo.Remove(buffEntry);
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
                    if (raidRuleInfo.Unit.Character.HealthRatio > buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.HealthRatio > buffEntry.Buff.SingleParam)
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
                    if (raidRuleInfo.Unit.Character.HealthRatio < buffEntry.Buff.SingleParam)
                        RevertBuff(buffEntry);
                    else
                        ApplyBuff(buffEntry);
                }
                else
                {
                    if (raidRuleInfo.Unit.Character.HealthRatio < buffEntry.Buff.SingleParam)
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
        }
    }

    private List<BuffInfo> FindDisplayableBuffs(BuffSourceType sourceType, BuffDurationType durationType, bool isPositive)
    {
        var displayableBuffs = BuffInfo.FindAll(info => info.SourceType == sourceType &&
            info.ModifierValue != 0 && info.DurationType == durationType && info.Buff.IsPositive() == isPositive).
            OrderBy(info => info.Buff.AttributeType).ThenBy(info => info.Buff.RuleType).ToList();

        displayableBuffs.RemoveAll(buff => buff.Buff.AttributeType == AttributeType.DamageLow);
        return displayableBuffs;
    }

    #endregion

    #region Attributes and Statuses

    public void LoadStatusEffects(Dictionary<StatusType, StatusEffect> newStatusEffects)
    {
        StatusEffects = newStatusEffects;
    }

    public virtual int Heal(float healAmount, bool includeModifier)
    {
        int heal = includeModifier
            ? Mathf.CeilToInt(healAmount * (1 + this[AttributeType.HpHealReceivedPercent].ModifiedValue))
            : Mathf.CeilToInt(healAmount);

        this[AttributeType.HitPoints, true].IncreaseValue(heal);
        return heal;
    }

    public int HealPercent(float healPercent, bool includeModifier)
    {
        return Heal(this[AttributeType.HitPoints, true].ModifiedValue * healPercent, includeModifier);
    }

    public int TakeDamage(float damageAmount)
    {
        int damage = Mathf.RoundToInt(damageAmount);
        GetPairedAttribute(AttributeType.HitPoints).DecreaseValue(damage);
        return damage;
    }

    public int TakeDamagePercent(float damagePercent)
    {
        return TakeDamage(this[AttributeType.HitPoints, true].ModifiedValue * damagePercent);
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
        return StatusEffects[type];
    }

    public StatusEffect this[StatusType type]
    {
        get
        {
            if (type != StatusType.None)
                return StatusEffects[type];
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

    protected void AddSingleAttribute(AttributeType stat, SingleAttribute attribute)
    {
        singleAttributes.Add(stat, attribute);
    }

    protected void AddPairedAttribute(AttributeType stat, PairedAttribute attribute)
    {
        pairedAttributes.Add(stat, attribute);
    }

    protected void UpdateResolve(int level, HeroClass heroClass)
    {
        for (int i = 0; i < HeroResistances.Length; i++)
            if (HeroResistances[i] == AttributeType.DeathBlow)
                GetSingleAttribute(HeroResistances[i]).RawValue = heroClass.Resistanses[HeroResistances[i]];
            else
                GetSingleAttribute(HeroResistances[i]).RawValue = heroClass.Resistanses[HeroResistances[i]] + level * 0.1f;
    }

    #endregion

    public virtual void UpdateSaveData(FormationUnitSaveData saveUnitData)
    {
        saveUnitData.IsHero = false;
        saveUnitData.Class = Class;
        saveUnitData.Name = Name;
        saveUnitData.CurrentHp = CurrentHealth;
        saveUnitData.Buffs = BuffInfo;
        saveUnitData.Statuses = StatusEffects;
    }
}