using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MonsterSkillSlot : MonoBehaviour
{
    [SerializeField]
    private Text skillLabel;
    [SerializeField]
    private List<Image> skillAttributeIcons;

    public void UpdateSkill(CombatSkill skill)
    {
        gameObject.SetActive(true);
        skillLabel.text = LocalizationManager.GetString("str_monster_skill_" + skill.Id);

        UpdateAttributes(skill);
    }

    public void UpdateHeroSkill(Hero hero, CombatSkill skill)
    {
        gameObject.SetActive(true);

        skillLabel.text = LocalizationManager.GetString("combat_skill_name_" + hero.Class + "_" + skill.Id);
        UpdateAttributes(skill);
    }

    public void ResetSkill()
    {
        gameObject.SetActive(false);
    }

    private void UpdateAttributes(CombatSkill skill)
    {
        int freeIconIndex = skillAttributeIcons.Count - 1;
        if (skill.Heal != null)
        {
            skillAttributeIcons[freeIconIndex].enabled = true;
            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_heal"];
            freeIconIndex--;
        }
        for (int i = 0; i < skill.Effects.Count; i++)
        {
            for (int j = 0; j < skill.Effects[i].SubEffects.Count; j++)
            {
                if (freeIconIndex >= 0)
                {
                    switch (skill.Effects[i].SubEffects[j].Type)
                    {
                        case EffectSubType.Bleeding:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_bleed"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Poison:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_poison"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Heal:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_heal"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Pull:
                        case EffectSubType.Push:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_move"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Stress:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_stress"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Disease:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_disease"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.GuardAlly:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_guard"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Stun:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_stun"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Tag:
                            skillAttributeIcons[freeIconIndex].enabled = true;
                            skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_tag"];
                            freeIconIndex--;
                            break;
                        case EffectSubType.Buff:
                            #region Buff and Debuff
                            BuffEffect buffEffect = skill.Effects[i].SubEffects[j] as BuffEffect;
                            if (buffEffect.Buffs.Count > 0)
                            {
                                if (buffEffect.Buffs[0].IsPositive())
                                {
                                    skillAttributeIcons[freeIconIndex].enabled = true;
                                    skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_buff"];
                                    freeIconIndex--;
                                }
                                else
                                {
                                    skillAttributeIcons[freeIconIndex].enabled = true;
                                    skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_debuff"];
                                    freeIconIndex--;
                                }
                            }
                            break;
                            #endregion
                        case EffectSubType.StatBuff:
                            #region Stat Buff and Debuff
                            CombatStatBuffEffect statBuffEffect = skill.Effects[i].SubEffects[j] as CombatStatBuffEffect;
                            if (statBuffEffect.StatAddBuffs.Count == 0 && statBuffEffect.StatMultBuffs.Count == 0)
                                break;

                            if (statBuffEffect.TargetMonsterType != MonsterType.None || statBuffEffect.TargetStatus != StatusType.None)
                                break;

                            if (statBuffEffect.IsPositive())
                            {
                                skillAttributeIcons[freeIconIndex].enabled = true;
                                skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_buff"];
                                freeIconIndex--;
                            }
                            else
                            {
                                skillAttributeIcons[freeIconIndex].enabled = true;
                                skillAttributeIcons[freeIconIndex].sprite = DarkestDungeonManager.Data.Sprites["skill_attribute_debuff"];
                                freeIconIndex--;
                            }
                            break;
                            #endregion
                    }
                }
                else
                    return;
            }
        }

        for (int i = freeIconIndex; i >= 0; i--)
            skillAttributeIcons[i].enabled = false;
    }
}
