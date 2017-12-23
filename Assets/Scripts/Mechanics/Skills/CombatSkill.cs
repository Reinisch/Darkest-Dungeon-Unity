using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class CombatSkill : Skill
{
    public int Level { get; private set; }
    public string Type { get; private set; }
    public SkillCategory Category { get; private set; }

    public float Accuracy { get; private set; }
    public float DamageMin { get; private set; }
    public float DamageMax { get; private set; }
    public float DamageMod { get; private set; }
    public float CritMod { get; private set; }

    public bool IsCritValid { get; private set; }
    public bool IsSelfValid { get; private set; }
    public bool IsGenerationGuaranteed { get; private set; }
    public bool? IsKnowledgeable { get; private set; }
    public bool? CanMiss { get; private set; }
    public float ExtraTargetsChance { get; private set; }

    public HealComponent Heal { get; private set; }
    public MoveComponent Move { get; private set; }
    public List<Effect> Effects { get; private set; }
    public FormationSet LaunchRanks { get; private set; }
    public FormationSet TargetRanks { get; private set; }

    public List<string> ValidModes { get; private set; }
    public Dictionary<string, List<Effect>> ModeEffects { get; private set; }

    public bool IsContinueTurn { get; private set; }
    public int? LimitPerTurn { get; private set; }
    public int? LimitPerBattle { get; private set; }

    public bool IsBuffSkill
    {
        get
        {
            return Effects.Find(effect => effect.SubEffects.Find(subEffect =>
                subEffect.Type == EffectSubType.Buff || subEffect.Type == EffectSubType.StatBuff) != null) != null;
        }
    }

    public CombatSkill(List<string> data, bool isHeroSkill)
    {
        Level = 0;
        IsCritValid = true;
        IsSelfValid = true;
        Category = SkillCategory.Damage;

        ValidModes = new List<string>();
        Effects = new List<Effect>();
        ModeEffects = new Dictionary<string, List<Effect>>();

        LoadData(data, isHeroSkill);
    }

    public List<FormationUnit> GetAvailableTargets(FormationUnit performer, FormationParty friends, FormationParty enemies)
    {
        if (TargetRanks.IsSelfTarget)
            return new List<FormationUnit>(new[] { performer });

        if (TargetRanks.IsSelfFormation)
            return friends.Units.FindAll(unit =>
                ((unit == performer && TargetRanks.IsTargetableUnit(unit) && IsSelfValid) ||
                (unit != performer && TargetRanks.IsTargetableUnit(unit))) && (unit.Character.BattleModifiers == null ||
                unit.Character.BattleModifiers.IsValidFriendlyTarget));

        return enemies.Units.FindAll(unit => unit != performer && TargetRanks.IsTargetableUnit(unit));
    }

    public bool HasAvailableTargets(FormationUnit performer, FormationParty friends, FormationParty enemies)
    {
        if (TargetRanks.IsSelfTarget)
            return true;

        if (TargetRanks.IsSelfFormation)
            for (int i = 0; i < friends.Units.Count; i++)
            {
                if (performer == friends.Units[i])
                {
                    if (IsSelfValid && TargetRanks.IsTargetableUnit(friends.Units[i]))
                        return true;
                }
                else if (TargetRanks.IsTargetableUnit(friends.Units[i]))
                    return true;
            }
        else 
            for (int i = 0; i < enemies.Units.Count; i++)
                if (TargetRanks.IsTargetableUnit(enemies.Units[i]))
                    return true;

        return false;
    }

    public string HeroSkillTooltip(Hero hero)
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString(ToolTipManager.GetConcat("combat_skill_name_", hero.HeroClass.StringId, "_", Id)));
        sb.AppendFormat("</color><color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
        
        if(TargetRanks.IsRandomTarget)
        {
            sb.Append("\n" + LocalizationManager.GetString("skill_random_target"));
        }

        if(Type != null)
        {
            sb.Append("\n" + LocalizationManager.GetString("str_" + Type));
        }

        if (Heal != null)
        {
            if(TargetRanks.IsSelfFormation && TargetRanks.IsMultitarget)
                sb.AppendFormat("\n" + LocalizationManager.GetString("skill_party_heal_format"), Heal.MinAmount, Heal.MaxAmount);
            else
                sb.AppendFormat("\n" + LocalizationManager.GetString("skill_heal_format"), Heal.MinAmount, Heal.MaxAmount);
        }

        if (Move != null)
        {
            if(Move.Pullforward != 0)
                sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_forward"), Move.Pullforward);
            else if (Move.Pushback != 0)
                sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_back"), Move.Pushback);
        }

        if (Accuracy != 0)
        {
            sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_acc_base"), Accuracy * 100);
        }

        if (DamageMod != 0)
        {
            sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_dmg_mod"), DamageMod * 100);
        }

        if (CritMod != 0)
        {
            sb.AppendFormat("\n" + LocalizationManager.GetString("str_skill_tooltip_crit_mod"), CritMod * 100);
        }

        bool hasTargetLabel = false;
        bool hasSelfLabel = false;
        bool hasPartyLabel = false;
        bool hasPartyOtherLabel = false;

        if(ValidModes.Count > 1)
        {
            foreach (var modeName in ValidModes)
            {
                hasTargetLabel = false;
                hasSelfLabel = false;
                hasPartyLabel = false;
                hasPartyOtherLabel = false;

                if (!ModeEffects.ContainsKey(modeName))
                    continue;

                sb.Append("\n" + LocalizationManager.GetString("str_skill_mode_info_" + modeName));

                #region Effects
                foreach (var effect in ModeEffects[modeName])
                {
                    switch (effect.TargetType)
                    {
                        case EffectTargetType.Target:
                            if (TargetRanks.IsSelfFormation && TargetRanks.IsMultitarget && TargetRanks.Ranks.Count == 4)
                            {
                                if (hasPartyLabel)
                                    break;
                                sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_party"));
                                hasPartyLabel = true;
                            }
                            else if (TargetRanks.IsSelfTarget)
                            {
                                if (hasSelfLabel)
                                    break;
                                sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_self"));
                                hasSelfLabel = true;
                            }
                            else
                            {
                                if (hasTargetLabel)
                                    break;
                                sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_target"));
                                hasTargetLabel = true;
                            }
                            break;
                        case EffectTargetType.Performer:
                            if (hasSelfLabel)
                                break;
                            sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_self"));
                            hasSelfLabel = true;
                            break;
                        case EffectTargetType.PerformersOther:
                            if (hasPartyOtherLabel)
                                break;
                            sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_party_other"));
                            hasPartyOtherLabel = true;
                            break;
                    }
                    string effectTooltip = effect.Tooltip();
                    if (effectTooltip.Length > 0)
                        sb.Append("\n" + effectTooltip);
                }
                #endregion
            }
        }

        #region Effects
        foreach (var effect in Effects)
        {
            switch(effect.TargetType)
            {
                case EffectTargetType.Target:
                    if (TargetRanks.IsSelfFormation && TargetRanks.IsMultitarget && TargetRanks.Ranks.Count == 4)
                    {
                        if (hasPartyLabel)
                            break;
                        sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_party"));
                        hasPartyLabel = true;
                    }
                    else if (TargetRanks.IsSelfTarget)
                    {
                        if (hasSelfLabel)
                            break;
                        sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_self"));
                        hasSelfLabel = true;
                    }
                    else
                    {
                        if (hasTargetLabel)
                            break;
                        sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_target"));
                        hasTargetLabel = true;
                    }
                    break;
                case EffectTargetType.Performer:
                    if (hasSelfLabel)
                        break;
                    sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_self"));
                    hasSelfLabel = true;
                    break;
                case EffectTargetType.PerformersOther:
                    if (hasPartyOtherLabel)
                        break;
                    sb.Append("\n" + LocalizationManager.GetString("effect_tooltip_party_other"));
                    hasPartyOtherLabel = true;
                    break;
            }
            string effectTooltip = effect.Tooltip();
            if(effectTooltip.Length > 0)
                sb.Append("\n" + effectTooltip);
        }
        #endregion

        sb.Append("</color>");
        return sb.ToString();
    }

    private void LoadData(List<string> data, bool isHeroSkill)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".id":
                    Id = data[++i];
                    break;
                case ".level":
                    Level = int.Parse(data[++i]);
                    break;
                case ".type":
                    Type = data[++i];
                    break;
                case ".atk":
                    Accuracy = float.Parse(data[++i]) / 100;
                    break;
                case ".dmg":
                    if (isHeroSkill)
                        DamageMod = float.Parse(data[++i]) / 100;
                    else
                    {
                        DamageMin = float.Parse(data[++i]);
                        DamageMax = float.Parse(data[++i]);
                    }
                    break;
                case ".crit":
                    CritMod = float.Parse(data[++i]) / 100;
                    break;
                case ".launch":
                    LaunchRanks = new FormationSet(data[++i]);
                    break;
                case ".target":
                    if (++i < data.Count && data[i--][0] != '.')
                        TargetRanks = new FormationSet(data[++i]);
                    else
                        TargetRanks = new FormationSet("");
                    break;
                case ".is_crit_valid":
                    IsCritValid = bool.Parse(data[++i].ToLower());
                    break;
                case ".self_target_valid":
                    IsSelfValid = bool.Parse(data[++i].ToLower());
                    break;
                case ".extra_targets_count":
                    ++i;
                    break;
                case ".extra_targets_chance":
                    ExtraTargetsChance = float.Parse(data[++i]);
                    break;
                case ".is_user_selected_targets":
                    ++i;
                    break;
                case ".can_miss":
                    CanMiss = bool.Parse(data[++i].ToLower());
                    break;
                case ".is_continue_turn":
                    IsContinueTurn = bool.Parse(data[++i].ToLower());
                    break;
                case ".per_turn_limit":
                    LimitPerTurn = int.Parse(data[++i]);
                    break;
                case ".per_battle_limit":
                    LimitPerBattle = int.Parse(data[++i]);
                    break;
                case ".valid_modes":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        ValidModes.Add(data[i]);
                    }
                    break;
                case ".human_effects":
                    var humanEffects = new List<Effect>();
                    ModeEffects.Add("human", humanEffects);
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            humanEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill " + Id);
                    }
                    break;
                case ".beast_effects":
                    var beastEffects = new List<Effect>();
                    ModeEffects.Add("beast", beastEffects);
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            beastEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill " + Id);
                    }
                    break;
                case ".effect":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            Effects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill " + Id);
                    }
                    break;
                case ".generation_guaranteed":
                    IsGenerationGuaranteed = bool.Parse(data[++i].ToLower());
                    break;
                case ".is_knowledgeable":
                    IsKnowledgeable = bool.Parse(data[++i].ToLower());
                    break;
                case ".heal":
                    Heal = new HealComponent(int.Parse(data[++i]), int.Parse(data[++i]));
                    break;
                case ".move":
                    Move = new MoveComponent(int.Parse(data[++i]), int.Parse(data[++i]));
                    break;
                case "combat_skill:":
                    break;
                default:
                    Debug.LogError("Unexpected token in combat skill: " + data[i]);
                    break;
            }
        }
        if (Accuracy == 0 || TargetRanks.IsSelfFormation || TargetRanks.IsSelfTarget)
        {
            if (Heal == null)
                Category = SkillCategory.Support;
            else
                Category = SkillCategory.Heal;
        }
    }
}