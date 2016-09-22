using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MonsterTooltip : MonoBehaviour
{
    public Animator animator;

    public Text monsterLabel;
    public Text monsterDodge;
    public Text monsterProt;
    public Text monsterSpeed;
    public Text monsterHealth;
    public List<Text> monsterTypes;

    public Text heroHitLabel;
    public Text heroCritLabel;
    public Text heroDmgLabel;

    public Text stunAmountLabel;
    public Text blightAmountLabel;
    public Text bleedAmountLabel;
    public Text debuffAmountLabel;
    public Text moveAmountLabel;

    public List<MonsterSkillSlot> monsterSkills;

    public RectTransform monsterIndicator;

    public bool IsDisabled
    {
        get
        {
            return isDisabled;
        }
        set
        {
            if (Slot != null && Slot.IsHovered && isDisabled == true && value == false)
            {
                isDisabled = false;
                Slot.OnPointerEnter(null);
            }
            else
                isDisabled = value;
        }
    }
    public FormationOverlaySlot Slot { get; set; }

    private float velocity = 0;
    private float targetPosition;
    private bool isDisabled;

    void Awake()
    {
        targetPosition = monsterIndicator.position.x;
    }
    void Update()
    {
        if (targetPosition != monsterIndicator.position.x)
            monsterIndicator.position = new Vector3(Mathf.SmoothDamp(monsterIndicator.position.x,
                targetPosition, ref velocity, 0.2f), monsterIndicator.position.y, monsterIndicator.position.z);
    }

    public void UpdateTooltiop(FormationUnit monsterUnit)
    {
        Monster monster = monsterUnit.Character as Monster;

        monsterLabel.text = LocalizationManager.GetString("str_monstername_" + monster.Data.StringId);
        monsterDodge.text = string.Format(LocalizationManager.GetString("monster_tooltip_dodge_format"),
            System.Math.Round((double)monster.Dodge, 3));
        monsterProt.text = string.Format(LocalizationManager.GetString("monster_tooltip_prot_format"),
            System.Math.Round((double)monster.Protection, 3));
        monsterSpeed.text = string.Format(LocalizationManager.GetString("monster_tooltip_speed_format"), monster.Speed);
        monsterHealth.text = string.Format(LocalizationManager.GetString("monster_tooltip_hp_format"),
            Mathf.RoundToInt(monster.Health.CurrentValue), Mathf.RoundToInt(monster.Health.ModifiedValue));

        int monsterTypesCounter = Mathf.Min(monsterTypes.Count, monster.Data.EnemyTypes.Count);
        for (int i = 0; i < monsterTypesCounter; i++)
        {
            monsterTypes[i].enabled = true;
            monsterTypes[i].text = LocalizationManager.GetString(
                CharacterLocalizationHelper.MonsterTooltipTypeString(monster.Data.EnemyTypes[i]));
        }
        for (int i = monsterTypesCounter; i < monsterTypes.Count; i++)
            monsterTypes[i].enabled = false;

        heroHitLabel.text = "";
        heroCritLabel.text = "";
        heroDmgLabel.text = "";

        stunAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Stun].ModifiedValue);
        blightAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Poison].ModifiedValue);
        bleedAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Bleed].ModifiedValue);
        debuffAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Debuff].ModifiedValue);
        moveAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Move].ModifiedValue);

        int monsterSkillCounter = Mathf.Min(monsterSkills.Count, monster.Data.CombatSkills.Count);
        for (int i = 0; i < monsterSkillCounter; i++)
        {
            if (monster.Data.CombatSkills[i].IsKnowledgeable == false)
            {
                monsterSkillCounter--;
                i--;
                continue;
            }

            monsterSkills[i].UpdateSkill(monster.Data.CombatSkills[i]);
        }
        for (int i = monsterSkillCounter; i < monsterSkills.Count; i++)
            monsterSkills[i].ResetSkill();
    }
    public void UpdateTooltip(FormationUnit monsterUnit, float hitChance, float critChance, int minDamage, int maxDamage)
    {
        Monster monster = monsterUnit.Character as Monster;

        monsterLabel.text = LocalizationManager.GetString("str_monstername_" + monster.Data.StringId);
        monsterDodge.text = string.Format(LocalizationManager.GetString("monster_tooltip_dodge_format"),
            System.Math.Round((double)monster.Dodge, 3));
        monsterProt.text = string.Format(LocalizationManager.GetString("monster_tooltip_prot_format"),
            System.Math.Round((double)monster.Protection, 3));
        monsterSpeed.text = string.Format(LocalizationManager.GetString("monster_tooltip_speed_format"), monster.Speed);
        monsterHealth.text = string.Format(LocalizationManager.GetString("monster_tooltip_hp_format"), 
            Mathf.RoundToInt(monster.Health.CurrentValue), Mathf.RoundToInt(monster.Health.ModifiedValue));

        int monsterTypesCounter = Mathf.Min(monsterTypes.Count, monster.Data.EnemyTypes.Count);
        for (int i = 0; i < monsterTypesCounter; i++)
        {
            monsterTypes[i].enabled = true;
            monsterTypes[i].text = LocalizationManager.GetString(
                CharacterLocalizationHelper.MonsterTooltipTypeString(monster.Data.EnemyTypes[i]));
        }
        for (int i = monsterTypesCounter; i < monsterTypes.Count; i++)
            monsterTypes[i].enabled = false;

        heroHitLabel.text = LocalizationManager.GetString("str_ui_hero_to_hit") + string.Format(": {0:#.#%;0.#%}", hitChance);
        heroCritLabel.text = LocalizationManager.GetString("str_ui_hero_crit") + string.Format(": {0:#.#%;0.#%}", critChance);
        heroDmgLabel.text = LocalizationManager.GetString("str_ui_hero_dmg") + string.Format(": {0} - {1}", minDamage, maxDamage);

        stunAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Stun].ModifiedValue);
        blightAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Poison].ModifiedValue);
        bleedAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Bleed].ModifiedValue);
        debuffAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Debuff].ModifiedValue);
        moveAmountLabel.text = string.Format("{0:0.#%;-0.#%}", monster[AttributeType.Move].ModifiedValue);

        int monsterSkillCounter = Mathf.Min(monsterSkills.Count, monster.Data.CombatSkills.Count);
        for (int i = 0; i < monsterSkillCounter; i++)
        {
            if (monster.Data.CombatSkills[i].IsKnowledgeable == false)
                continue;

            monsterSkills[i].UpdateSkill(monster.Data.CombatSkills[i]);
        }
        for (int i = monsterSkillCounter; i < monsterSkills.Count; i++)
            monsterSkills[i].ResetSkill();
    }

    public void Show(FormationUnit monsterUnit)
    {
        Slot = monsterUnit.OverlaySlot;

        if (IsDisabled)
            return;

        Vector3 screenPoint = RaidSceneManager.DungeonPositionToScreen(monsterUnit.RectTransform.position);
        targetPosition = screenPoint.x;
        animator.SetBool("IsActive", true);

        if(RaidSceneManager.BattleGround.Round.TurnType == TurnType.HeroTurn)
        {
            if(Slot.TargetUnit.IsTargetable && RaidSceneManager.RaidPanel.SelectedUnit != null)
            {
                var combatSkill = (CombatSkill)RaidSceneManager.RaidPanel.bannerPanel.skillPanel.SelectedSkill;
                if(combatSkill != null)
                {
                    BattleSolver.CalculateSkillPotential(RaidSceneManager.RaidPanel.SelectedUnit, monsterUnit, combatSkill);
                    if (BattleSolver.HeroActionInfo.IsValid)
                    {
                        UpdateTooltip(monsterUnit, BattleSolver.HeroActionInfo.ChanceToHit, BattleSolver.HeroActionInfo.ChanceToCrit,
                            BattleSolver.HeroActionInfo.MinDamage, BattleSolver.HeroActionInfo.MaxDamage);
                        return;
                    }
                }
            }
        }

        UpdateTooltiop(monsterUnit);
    }
    public bool IsActive()
    {
        return animator.GetBool("IsActive");
    }
    public void Hide()
    {
        animator.SetBool("IsActive", false);
    }
}
