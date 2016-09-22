using UnityEngine;
using System.Collections.Generic;

public delegate void ScrollEvent();

public class RaidEvents : MonoBehaviour
{
    public PopupMessage popupText;
    public ScrollEventLoot loot;
    public ScrollMealEvent meal;
    public ScrollHungerEvent hunger;
    public ScrollCampEvent camp;
    public ScrollEventInteraction itemInteraction;
    public RaidAnnouncment announcment;
    public RoundIndicator roundIndicator;
    public CanvasGroup raidUiCanvasGroup;

    public SkeletonAnimation battleAnnouncment;
    public RectTransform eventRect;
    public MonsterTooltip monsterTooltip;

    public PopupMessage PopupMessage
    {
        get
        {
            return popupText;
        }
    }
    public ScrollCampEvent CampEvent
    {
        get
        {
            return camp;
        }
    }
    public ScrollMealEvent MealEvent
    {
        get
        {
            return meal;
        }
    }
    public ScrollHungerEvent HungerEvent
    {
        get
        {
            return hunger;
        }
    }
    public ScrollEventLoot LootEvent
    {
        get
        {
            return loot;
        }
    }
    public ScrollEventInteraction InteractionEvent
    {
        get
        {
            return itemInteraction;
        }
    }
    public RaidAnnouncment Announcment
    {
        get
        {
            return announcment;
        }
    }
    public RoundIndicator RoundIndicator
    {
        get
        {
            return roundIndicator;
        }
    }
    public MonsterTooltip MonsterTooltip
    {
        get
        {
            return monsterTooltip;
        }
    }

    Dictionary<string, Color> PopupColors;

    public void Initialize()
    {
        #region Popup Colors
        PopupColors = new Dictionary<string, Color>()
        {
            { "harmful", DarkestDungeonManager.Data.FromHexDatabase("harmful") },
            { "notable", DarkestDungeonManager.Data.FromHexDatabase("notable") },
            { "pop_text_stress_reduce", DarkestDungeonManager.Data.FromHexDatabase("pop_text_stress_reduce") },
            { "pop_text_outline_stress_reduce", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_stress_reduce") },
            { "pop_text_stress_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_stress_damage") },
            { "pop_text_outline_stress_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_stress_damage") },
            { "pop_text_miss", DarkestDungeonManager.Data.FromHexDatabase("pop_text_miss") },
            { "pop_text_outline_miss", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_miss") },
            { "pop_text_no_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_no_damage") },
            { "pop_text_outline_no_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_no_damage") },
            { "pop_text_crit_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_crit_damage") },
            { "pop_text_outline_crit_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_crit_damage") },
            { "pop_text_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_damage") },
            { "pop_text_outline_damage", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_damage") },
            { "pop_text_deathblow", DarkestDungeonManager.Data.FromHexDatabase("pop_text_deathblow") },
            { "pop_text_outline_deathblow", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_deathblow") },
            { "pop_text_death_avoided", DarkestDungeonManager.Data.FromHexDatabase("pop_text_death_avoided") },
            { "pop_text_outline_death_avoided", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_death_avoided") },
            { "pop_text_disease_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_disease_resist") },
            { "pop_text_outline_disease_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_disease_resist") },
            { "pop_text_pass", DarkestDungeonManager.Data.FromHexDatabase("pop_text_pass") },
            { "pop_text_outline_pass", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_pass") },
            { "pop_text_heal", DarkestDungeonManager.Data.FromHexDatabase("pop_text_heal") },
            { "pop_text_outline_heal", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_heal") },
            { "pop_text_heal_crit", DarkestDungeonManager.Data.FromHexDatabase("pop_text_heal_crit") },
            { "pop_text_outline_heal_crit", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_heal_crit") },
            { "pop_text_buff", DarkestDungeonManager.Data.FromHexDatabase("pop_text_buff") },
            { "pop_text_outline_buff", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_buff") }, 
            { "pop_text_debuff", DarkestDungeonManager.Data.FromHexDatabase("pop_text_debuff") },
            { "pop_text_outline_debuff", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_debuff") },
            { "pop_text_stun", DarkestDungeonManager.Data.FromHexDatabase("pop_text_stun") },
            { "pop_text_outline_stun", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_stun") },
            { "pop_text_stun_clear", DarkestDungeonManager.Data.FromHexDatabase("pop_text_stun_clear") },
            { "pop_text_outline_stun_clear", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_stun_clear") },
            { "pop_text_poison", DarkestDungeonManager.Data.FromHexDatabase("pop_text_poison") },
            { "pop_text_outline_poison", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_poison") },
            { "pop_text_bleed", DarkestDungeonManager.Data.FromHexDatabase("pop_text_bleed") },
            { "pop_text_outline_bleed", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_bleed") },
            { "pop_text_cured", DarkestDungeonManager.Data.FromHexDatabase("pop_text_cured") },
            { "pop_text_outline_cured", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_cured") },
            { "pop_text_tagged", DarkestDungeonManager.Data.FromHexDatabase("pop_text_tagged") },
            { "pop_text_outline_tagged", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_tagged") },
            { "pop_text_riposte", DarkestDungeonManager.Data.FromHexDatabase("pop_text_riposte") },
            { "pop_text_outline_riposte", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_riposte") },
            { "pop_text_guard", DarkestDungeonManager.Data.FromHexDatabase("pop_text_guard") },
            { "pop_text_outline_guard", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_guard") },
            { "pop_text_full", DarkestDungeonManager.Data.FromHexDatabase("pop_text_full") },
            { "pop_text_outline_full", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_full") },
            { "pop_text_heart_attack", DarkestDungeonManager.Data.FromHexDatabase("pop_text_heart_attack") },
            { "pop_text_outline_heart_attack", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_heart_attack") },
            { "pop_text_move_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_move_resist") },
            { "pop_text_outline_move_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_move_resist") },
            { "pop_text_debuff_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_debuff_resist") },
            { "pop_text_outline_debuff_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_debuff_resist") },
            { "pop_text_stun_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_stun_resist") },
            { "pop_text_outline_stun_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_stun_resist") },
            { "pop_text_poison_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_poison_resist") },
            { "pop_text_outline_poison_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_poison_resist") },
            { "pop_text_bleed_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_bleed_resist") }, 
            { "pop_text_outline_bleed_resist", DarkestDungeonManager.Data.FromHexDatabase("pop_text_outline_bleed_resist") },
        };
        #endregion

        loot.partyInventory.Configuration = InventoryConfiguration.LootInventory;
        loot.Initialize();
        meal.Initialize();
        itemInteraction.Initialize();
        itemInteraction.onScrollOpened += RaidSceneManager.Instanse.DisablePartyMovement;

        battleAnnouncment.Reset();
    }

    public void ShowPopupMessage(FormationUnit unit, PopupMessageType type, string parameter = "", float ripOffset = 0)
    {
        PopupMessage popupMessage = (Instantiate(popupText.gameObject) as GameObject).GetComponent<PopupMessage>();
        popupMessage.rectTransform.SetParent(eventRect, false);
        Spine.Bone bone = unit.CurrentState.Skeleton.FindBone("fxhead");
        if (bone == null)
            bone = unit.CurrentState.Skeleton.FindBone("fxskill");

        switch(type)
        {
            case PopupMessageType.DeathBlow:
                popupMessage.SetColor(PopupColors["pop_text_deathblow"], PopupColors["pop_text_outline_deathblow"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_deathblow"));
                popupMessage.SetRotation(new Vector3(0, 0, 8));
                break;
            case PopupMessageType.DeathsDoor:
                bone = unit.CurrentState.Skeleton.FindBone("fxchest");
                popupMessage.SetColor(PopupColors["pop_text_death_avoided"], PopupColors["pop_text_outline_death_avoided"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_death_avoided"));
                popupMessage.SetIcon("poptext_death_avoided");
                popupMessage.SetRotation(new Vector3(0, 0, 8));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/deaths_door");
                break;
            case PopupMessageType.HeartAttack:
                popupMessage.SetColor(PopupColors["pop_text_heart_attack"], PopupColors["pop_text_outline_heart_attack"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_heart_attack"));
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/heart_attack");
                break;
            case PopupMessageType.RetreatFailed:
                popupMessage.SetColor(PopupColors["pop_text_heart_attack"], PopupColors["pop_text_outline_heart_attack"]);
                popupMessage.SetMessage(LocalizationManager.GetString("retreat_fail_announcement"));
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat_fail");
                break;
            case PopupMessageType.Miss:
                popupMessage.SetColor(PopupColors["pop_text_miss"], PopupColors["pop_text_outline_miss"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_miss"));
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                break;
            case PopupMessageType.Dodge:
                popupMessage.SetColor(PopupColors["pop_text_miss"], PopupColors["pop_text_outline_miss"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_dodge"));
                popupMessage.SetRotation(new Vector3(0, 0, 4));
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                break;
            case PopupMessageType.ZeroDamage:
                popupMessage.SetColor(PopupColors["pop_text_no_damage"], PopupColors["pop_text_outline_no_damage"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_no_damage"));
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                break;
            case PopupMessageType.Damage:
                popupMessage.SetColor(PopupColors["pop_text_damage"], PopupColors["pop_text_outline_damage"]);
                popupMessage.SetMessage(parameter);
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                popupMessage.skillMessage.fontSize = 70;
                break;
            case PopupMessageType.CritDamage:
                popupMessage.SetColor(PopupColors["pop_text_crit_damage"], PopupColors["pop_text_outline_crit_damage"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_crittxt") + "\n" + parameter);
                popupMessage.SetOffset(new Vector3(0, ripOffset, 0));
                popupMessage.skillMessage.fontSize = 72;
                break;
            case PopupMessageType.Stress:
                popupMessage.SetColor(PopupColors["pop_text_stress_damage"], PopupColors["pop_text_outline_stress_damage"]);
                popupMessage.SetMessage(parameter);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/stress_up");
                popupMessage.skillMessage.fontSize = 70;
                break;
            case PopupMessageType.StressHeal:
                popupMessage.SetColor(PopupColors["pop_text_stress_reduce"], PopupColors["pop_text_outline_stress_reduce"]);
                popupMessage.SetMessage(parameter);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/stress_down");
                popupMessage.skillMessage.fontSize = 70;
                break;
            case PopupMessageType.Heal:
                popupMessage.SetColor(PopupColors["pop_text_heal"], PopupColors["pop_text_outline_heal"]);
                popupMessage.SetMessage(parameter);
                popupMessage.skillMessage.fontSize = 70;
                break;
            case PopupMessageType.CritHeal:
                popupMessage.SetColor(PopupColors["pop_text_heal_crit"], PopupColors["pop_text_outline_heal_crit"]);
                popupMessage.SetMessage(parameter);
                popupMessage.skillMessage.fontSize = 72;
                break;
            case PopupMessageType.Pass:
                popupMessage.SetColor(PopupColors["pop_text_pass"], PopupColors["pop_text_outline_pass"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_pass"));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/pass");
                break;
            case PopupMessageType.Tagged:
                popupMessage.SetColor(PopupColors["pop_text_tagged"], PopupColors["pop_text_outline_tagged"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_tagged"));
                popupMessage.SetIcon("poptext_tagged");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/marked");
                break;
            case PopupMessageType.Untagged:
                popupMessage.SetColor(PopupColors["pop_text_tagged"], PopupColors["pop_text_outline_tagged"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_untagged"));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/marked");
                popupMessage.SetRotation(new Vector3(0, 0, 6));
                break;
            case PopupMessageType.Bleed:
                popupMessage.SetColor(PopupColors["pop_text_bleed"], PopupColors["pop_text_outline_bleed"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_bleed"));
                popupMessage.SetIcon("poptext_bleed");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_onset");
                break;
            case PopupMessageType.Poison:
                popupMessage.SetColor(PopupColors["pop_text_poison"], PopupColors["pop_text_outline_poison"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_poison"));
                popupMessage.SetIcon("poptext_poison");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_onset");
                break;
            case PopupMessageType.Buff:
                popupMessage.SetColor(PopupColors["pop_text_buff"], PopupColors["pop_text_outline_buff"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_buff"));
                popupMessage.SetIcon("poptext_buff");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/buff");
                break;
            case PopupMessageType.Debuff:
                popupMessage.SetColor(PopupColors["pop_text_debuff"], PopupColors["pop_text_outline_debuff"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_debuff"));
                popupMessage.SetIcon("poptext_debuff");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/debuff");
                break;
            case PopupMessageType.Stunned:
                popupMessage.SetColor(PopupColors["pop_text_stun"], PopupColors["pop_text_outline_stun"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_stun"));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/stun_onset");
                break;
            case PopupMessageType.Unstun:
                popupMessage.SetColor(PopupColors["pop_text_stun_clear"], PopupColors["pop_text_outline_stun_clear"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_stun"));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/stun_off");
                break;
            case PopupMessageType.Cured:
                popupMessage.SetColor(PopupColors["pop_text_cured"], PopupColors["pop_text_outline_cured"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_cured"));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/cured");
                break;
            case PopupMessageType.BleedResist:
                if(unit.Character.DisplayModifier != null && unit.Character.DisplayModifier.DisabledPopups.Contains("resist"))
                {
                    Destroy(popupMessage.gameObject);
                    return;
                }
                popupMessage.SetColor(PopupColors["pop_text_bleed_resist"], PopupColors["pop_text_outline_bleed_resist"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_bleed_resist"));
                popupMessage.SetIcon("poptext_bleed_resist");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/resist");
                break;
            case PopupMessageType.PoisonResist:
                if (unit.Character.DisplayModifier != null && unit.Character.DisplayModifier.DisabledPopups.Contains("resist"))
                {
                    Destroy(popupMessage.gameObject);
                    return;
                }
                popupMessage.SetColor(PopupColors["pop_text_poison_resist"], PopupColors["pop_text_outline_poison_resist"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_blight_resist"));
                popupMessage.SetIcon("poptext_poison_resist");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/resist");
                break;
            case PopupMessageType.StunResist:
                if (unit.Character.DisplayModifier != null && unit.Character.DisplayModifier.DisabledPopups.Contains("resist"))
                {
                    Destroy(popupMessage.gameObject);
                    return;
                }
                popupMessage.SetColor(PopupColors["pop_text_stun_resist"], PopupColors["pop_text_outline_stun_resist"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_stun_resist"));
                popupMessage.SetIcon("poptext_stun_resist");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/resist");
                break;
            case PopupMessageType.MoveResist:
                if (unit.Character.DisplayModifier != null && unit.Character.DisplayModifier.DisabledPopups.Contains("resist"))
                {
                    Destroy(popupMessage.gameObject);
                    return;
                }
                popupMessage.SetColor(PopupColors["pop_text_move_resist"], PopupColors["pop_text_outline_move_resist"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_move_resist"));
                popupMessage.SetIcon("poptext_move_resist");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/resist");
                break;
            case PopupMessageType.DebuffResist:
                if (unit.Character.DisplayModifier != null && unit.Character.DisplayModifier.DisabledPopups.Contains("resist"))
                {
                    Destroy(popupMessage.gameObject);
                    return;
                }
                popupMessage.SetColor(PopupColors["pop_text_debuff_resist"], PopupColors["pop_text_outline_debuff_resist"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_debuff_resist"));
                popupMessage.SetIcon("poptext_debuff_resist");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/resist");
                break;
            case PopupMessageType.DiseaseResist:
                if (unit.Character.DisplayModifier != null && unit.Character.DisplayModifier.DisabledPopups.Contains("resist"))
                {
                    Destroy(popupMessage.gameObject);
                    return;
                }
                popupMessage.SetColor(PopupColors["pop_text_disease_resist"], PopupColors["pop_text_outline_disease_resist"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_disease_resist"));
                popupMessage.SetIcon("poptext_disease_resist");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/resist");
                break;
            case PopupMessageType.Disease:
                popupMessage.SetColor(PopupColors["pop_text_disease_resist"], PopupColors["pop_text_outline_disease_resist"]);
                popupMessage.SetMessage(parameter);
                popupMessage.SetOffset(new Vector3(0, 70, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/quirk_neg");
                break;
            case PopupMessageType.PositiveQuirk:
                popupMessage.SetColor(PopupColors["notable"], Color.black);
                popupMessage.SetMessage(parameter);
                popupMessage.SetOffset(new Vector3(0, 70, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/quirk_pos");
                break;
            case PopupMessageType.NegativeQuirk:
                popupMessage.SetColor(PopupColors["harmful"], Color.black);
                popupMessage.SetMessage(parameter);
                popupMessage.SetOffset(new Vector3(0, 70, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/quirk_neg");
                break;
            case PopupMessageType.QuirkRemoved:
                popupMessage.SetColor(PopupColors["notable"], Color.black);
                popupMessage.SetMessage(parameter + LocalizationManager.GetString("curio_announcement_purge_format"));
                popupMessage.SetOffset(new Vector3(0, 70, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/quirk_pos");
                break;
            case PopupMessageType.DiseaseCured:
                popupMessage.SetMessage(string.Format(LocalizationManager.GetString("str_ui_disease_cured"), parameter));
                popupMessage.SetColor(PopupColors["pop_text_disease_resist"], PopupColors["pop_text_outline_disease_resist"]);
                popupMessage.SetIcon("poptext_disease_resist");
                popupMessage.SetOffset(new Vector3(0, 30, 0));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/cured");
                break;
            case PopupMessageType.Guard:
                popupMessage.SetColor(PopupColors["pop_text_guard"], PopupColors["pop_text_outline_guard"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_guard"));
                popupMessage.SetIcon("poptext_guard");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/guard");
                break;
            case PopupMessageType.Riposte:
                popupMessage.SetColor(PopupColors["pop_text_riposte"], PopupColors["pop_text_outline_riposte"]);
                popupMessage.SetMessage(LocalizationManager.GetString("str_ui_riposte"));
                popupMessage.SetIcon("poptext_riposte");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/riposte_enabled");
                break;
        }

        Vector3 screenPosition = RaidSceneManager.DungeonPositionToScreen(
            unit.RectTransform.TransformPoint(bone.WorldX, bone.WorldY, 0));
        popupMessage.rectTransform.position = new Vector3(screenPosition.x, screenPosition.y, 0);
        popupMessage.FollowXBone(bone, unit);
        popupMessage.gameObject.SetActive(true);
    }

    public void ShowBattleAnnouncment()
    {
        battleAnnouncment.gameObject.SetActive(true);
        if (battleAnnouncment.state.GetCurrent(0) != null)
        {
            battleAnnouncment.Skeleton.SetToSetupPose();
            battleAnnouncment.AnimationName = "start";
            battleAnnouncment.state.GetCurrent(0).Time = 0;
            battleAnnouncment.state.SetAnimation(0, "start", false).Time = 0;
            battleAnnouncment.Update(0);
        }
        else
        {
            battleAnnouncment.Skeleton.SetToSetupPose();
            battleAnnouncment.AnimationName = "start";
            battleAnnouncment.state.SetAnimation(0, "start", false);
        }
    }
    public void HideBattleAnnouncment()
    {
        battleAnnouncment.gameObject.SetActive(false);
    }
    public void ShowAnnouncment(string message, AnnouncmentPosition position = AnnouncmentPosition.Top)
    {
        announcment.ShowAnnouncment(message, position);
    }
    public void HideAnnouncment()
    {
        announcment.HideAnnouncment();
    }

    public void LoadCampingMeal()
    {
        meal.LoadCampingMeal();
    }
    public void LoadHungerMeal()
    {
        hunger.LoadHungerEventMeal();
    }
    public void LoadCampingSkillEvent()
    {
        camp.PrepareCamping();
    }
    public void LoadInteraction(Obstacle obstacle, RaidHallSector sector)
    {
        itemInteraction.LoadInteraction(obstacle, sector);
    }
    public void LoadInteraction(Curio curio, IRaidArea areaView)
    {
        itemInteraction.LoadInteraction(curio, areaView);
    }
    public void LoadCurioLoot(Curio curio, CurioResult result, bool keepLoot = false)
    {
        loot.LoadCurioLoot(curio, result, RaidSceneManager.Raid, keepLoot);
    }
    public void LoadBattleLoot(List<LootDefinition> battleLoot)
    {
        loot.LoadBattleLoot(battleLoot);
    }
    public void LoadSingleLoot(string code, int amount)
    {
        loot.LoadSingleLoot(code, amount);
    }
}