using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public enum TraySlotType
{
    Afflicted,
    Virtued,
    DeathsDoor,
    DeathRecovery,
    Buff,
    Debuff,
    Bleed,
    Poison,
    Guard,
    Riposte,
    Tag,
    Trap,
    Event
}

public class TraySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private TraySlotType type;

    public TrayPanel TrayPanel { private get; set; }

    public void UpdateTraySlot()
    {
        if (TrayPanel == null || TrayPanel.TargetUnit == null)
            return;

        switch(type)
        {
            case TraySlotType.Afflicted:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.IsAfflicted);
                break;
            case TraySlotType.Virtued:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.IsVirtued);
                break;
            case TraySlotType.DeathsDoor:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.DeathsDoor).IsApplied);
                break;
            case TraySlotType.DeathRecovery:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.DeathRecovery).IsApplied);
                break;
            case TraySlotType.Buff:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.HasBuffs());
                break;
            case TraySlotType.Debuff:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.HasDebuffs());
                break;
            case TraySlotType.Bleed:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Bleeding).IsApplied);
                break;
            case TraySlotType.Poison:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Poison).IsApplied);
                break;
            case TraySlotType.Guard:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Guarded).IsApplied);
                break;
            case TraySlotType.Riposte:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Riposte).IsApplied);
                break;
            case TraySlotType.Tag:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Marked).IsApplied);
                break;
            case TraySlotType.Event:
                gameObject.SetActive(TrayPanel.TargetUnit.Character.HasEventBuffs());
                break;
            case TraySlotType.Trap:
                gameObject.SetActive(false);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TrayPanel.TargetUnit != null && TrayPanel.TargetUnit.OverlaySlot != null)
            if (TrayPanel.TargetUnit.OverlaySlot.SlotAnimator.GetBool("Hidden"))
                return;
            
        StringBuilder sb = ToolTipManager.TipBody;
        switch(type)
        {
            case TraySlotType.Afflicted:
                if (TrayPanel.TargetUnit.Character is Hero)
                {
                    var hero = TrayPanel.TargetUnit.Character as Hero;
                    if(hero.Trait != null && hero.Trait.Type == OverstressType.Affliction)
                    {
                        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                        sb.Append(LocalizationManager.GetString("str_affliction_name_" + hero.Trait.Id));
                        sb.Append("\n" + LocalizationManager.GetString("str_affliction_description_" + hero.Trait.Id));
                        sb.Append("</color>");
                        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                        sb.Append("\n" + TrayPanel.TargetUnit.Character.TraitBuffsTooltip());
                        sb.Append("</color>");
                    }
                }
                break;
            case TraySlotType.Virtued:
                if (TrayPanel.TargetUnit.Character is Hero)
                {
                    var hero = TrayPanel.TargetUnit.Character as Hero;
                    if (hero.Trait != null && hero.Trait.Type == OverstressType.Virtue)
                    {
                        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
                        sb.Append(LocalizationManager.GetString("str_virtue_name_" + hero.Trait.Id));
                        sb.Append("</color>");
                        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                        sb.Append("\n" + LocalizationManager.GetString("str_virtue_description_" + hero.Trait.Id));
                        sb.Append("</color>");
                    }
                }
                break;
            case TraySlotType.DeathsDoor:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append(LocalizationManager.GetString("tray_icon_tooltip_deathsdoor_title"));
                sb.Append("</color>");
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.Append(LocalizationManager.GetString("tray_icon_tooltip_deathsdoor"));
                sb.Append("\n" + TrayPanel.TargetUnit.Character.DeathsDoorBuffsTooltip());
                sb.Append("</color>");
                break;
            case TraySlotType.DeathRecovery:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append(LocalizationManager.GetString(LocalizationManager.GetString("buff_bsrc_deathsdoor_recovery")));
                sb.Append("</color>");
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.Append("\n" + TrayPanel.TargetUnit.Character.MortalityBuffsTooltip());
                sb.Append("</color>");
                break;
            case TraySlotType.Buff:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.Append(TrayPanel.TargetUnit.Character.CombatBuffTooltip(true));
                sb.Append("</color>");
                break;
            case TraySlotType.Debuff:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append(TrayPanel.TargetUnit.Character.CombatBuffTooltip(false));
                sb.Append("</color>");
                break;
            case TraySlotType.Bleed:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.Append(LocalizationManager.GetString("effect_tooltip_dot_bleed"));
                sb.AppendFormat("\n" + LocalizationManager.GetString("tray_icon_tooltip_bleed_format"), 
                    (TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect).CurrentTickDamage,
                    (TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect).ExpirationTime);
                sb.Append("</color>");
                break;
            case TraySlotType.Poison:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.Append(LocalizationManager.GetString("effect_tooltip_dot_poison"));
                sb.AppendFormat("\n" + LocalizationManager.GetString("tray_icon_tooltip_poison_format"), 
                    (TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect).CurrentTickDamage,
                    (TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect).ExpirationTime);
                sb.Append("</color>");
                break;
            case TraySlotType.Guard:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                var guardEffect = TrayPanel.TargetUnit.Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
                var guard = guardEffect.Guard;
                string guardText = string.Format(LocalizationManager.GetString("tray_icon_tooltip_guard"), guard.Character.IsMonster ?
                    LocalizationManager.GetString("str_monstername_" + guard.Character.Name) :
                    guard.Character.Name, guardEffect.GuardDuration);
                if (guard.Party.Units.FindAll(unit => unit.Character.Name == guard.Character.Name).Count > 1)
                    guardText = string.Format(LocalizationManager.GetString(
                        "tray_icon_tooltip_guard_name_with_rank_format"), guardText, guard.Rank);
                sb.Append(guardText);
                sb.Append("</color>");
                break;
            case TraySlotType.Riposte:
                var ripostEffect = TrayPanel.TargetUnit.Character[StatusType.Riposte] as RiposteStatusEffect;
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.AppendFormat(LocalizationManager.GetString("tray_icon_tooltip_riposte"), ripostEffect.RiposteDuration);
                sb.Append("</color>");
                break;
            case TraySlotType.Tag:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                var markedEffect = TrayPanel.TargetUnit.Character[StatusType.Marked] as MarkStatusEffect;
                if(markedEffect.DurationType == DurationType.Round)
                    sb.AppendFormat(LocalizationManager.GetString("tray_icon_tooltip_tag_title_round"), markedEffect.MarkDuration);
                else
                    sb.AppendFormat(LocalizationManager.GetString("tray_icon_tooltip_tag_title_combat_end"), markedEffect.MarkDuration);

                sb.AppendFormat("</color><color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                if(TrayPanel.TargetUnit.Character is Hero)
                    sb.Append("\n" + LocalizationManager.GetString("tray_icon_tooltip_tag_hero"));
                else
                    sb.Append("\n" + LocalizationManager.GetString("tray_icon_tooltip_tag_monster"));
                sb.Append("</color>");
                break;
            case TraySlotType.Event:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.Append(TrayPanel.TargetUnit.Character.EventBuffTooltip());
                sb.Append("</color>");
                break;
            case TraySlotType.Trap:
                if (TrayPanel.TargetUnit.Character is Hero)
                    sb.AppendFormat(LocalizationManager.GetString("buff_stat_tooltip_resistance_trap"),
                        TrayPanel.TargetUnit.Character.GetSingleAttribute(AttributeType.Trap).ModifiedValue);
                break;
        }
        ToolTipManager.Instanse.Show(sb.ToString(), rectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}