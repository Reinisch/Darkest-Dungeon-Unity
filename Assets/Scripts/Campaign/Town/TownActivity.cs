using UnityEngine;
using System.Collections.Generic;

public interface IActivity
{
    string TreeId { get; set; }
    List<ActivitySlot> ActivitySlots { get; set; }
}

public class TownActivity : IActivity
{
    public string Id { get; set; }
    public string TreeId { get; set; }
    public ActivityType ActivityType { get; private set; }

    public int BaseSlots { get; set; }
    public int BaseStressHeal { get; set; }
    public CurrencyCost BaseCost { get; set; }
    public float BaseAfflictionCure { get; set; }

    public int NumberOfSlots { get; set; }
    public int StressHealAmount { get; set; }
    public CurrencyCost ActivityCost { get; set; }
    public float AfflictionCureChance { get; set; }

    public float SideEffectChance { get; set; }
    public List<TownEffect> SideEffects { get; set; }
    public List<string> IncompatiableQuirks { get; set; }
    public bool CareTakerFriendly { get; set; }

    public List<CostUpgrade> CostUpgrades { get; set; }
    public List<SlotUpgrade> SlotUpgrades { get; set; }
    public List<StressUpgrade> StressUpgrades { get; set; }
    public List<ChanceUpgrade> AfflictionCureUpgrades { get; set; }

    public List<ActivitySlot> ActivitySlots { get; set; }

    void LogActivity(ActivityEffectType effectType, Hero hero, string effectInfo)
    {
        var log = DarkestDungeonManager.Campaign.CurrentLog();
        if(log != null)
            log.HeroRecords.Add(new ActorActivityRecord(ActivityType, effectType, hero, effectInfo, StressHealAmount));
    }

    public TownActivity(string treeId)
    {
        TreeId = treeId;
        switch (TreeId)
        {
            case "abbey.meditation":
                ActivityType = ActivityType.MeditationStressHeal;
                Id = "meditation";
                break;
            case "abbey.prayer":
                ActivityType = ActivityType.PrayerStressHeal;
                Id = "prayer";
                break;
            case "abbey.flagellation":
                ActivityType = ActivityType.FlagellationStressHeal;
                Id = "flagellation";
                break;
            case "tavern.bar":
                ActivityType = ActivityType.BarStressHeal;
                Id = "bar";
                break;
            case "tavern.gambling":
                ActivityType = ActivityType.GambleStressHeal;
                Id = "gambling";
                break;
            case "tavern.brothel":
                ActivityType = ActivityType.BrothelStressHeal;
                Id = "brothel";
                break;
        }
        SideEffects = new List<TownEffect>();
        IncompatiableQuirks = new List<string>();
        CostUpgrades = new List<CostUpgrade>();
        SlotUpgrades = new List<SlotUpgrade>();
        StressUpgrades = new List<StressUpgrade>();
        AfflictionCureUpgrades = new List<ChanceUpgrade>();
        ActivitySlots = new List<ActivitySlot>();
    }

    public void ProvideActivity()
    {
        foreach(var activitySlot in ActivitySlots)
        {
            switch(activitySlot.Status)
            {
                case ActivitySlotStatus.Caretaken:
                    activitySlot.Status = ActivitySlotStatus.Available;
                    activitySlot.Hero = null;
                    break;
                case ActivitySlotStatus.Blocked:
                case ActivitySlotStatus.Checkout:
                    activitySlot.Hero.Status = HeroStatus.Available;
                    activitySlot.Status = ActivitySlotStatus.Available;
                    activitySlot.Hero = null;
                    break;
                case ActivitySlotStatus.Paid:
                    activitySlot.Hero.GetPairedAttribute(AttributeType.Stress).DecreaseValue(StressHealAmount);
                    if(RandomSolver.CheckSuccess(SideEffectChance))
                    {
                        var effect = RandomSolver.ChooseByRandom<TownEffect>(SideEffects);
                        switch(effect.Type)
                        {
                            case TownEffectType.GoMissing:
                                var missingEffect = effect as MissingTownEffect;
                                var duration = RandomSolver.ChooseByRandom<MissingDuration>(missingEffect.Durations);

                                LogActivity(ActivityEffectType.Missing, activitySlot.Hero, "None");

                                activitySlot.Hero.Status = HeroStatus.Missing;
                                activitySlot.Hero.MissingDuration = duration.Duration;
                                activitySlot.Status = ActivitySlotStatus.Available;
                                activitySlot.Hero = null;
                                break;
                            case TownEffectType.ActivityLock:
                                activitySlot.Status = ActivitySlotStatus.Blocked;
                                LogActivity(ActivityEffectType.Lock, activitySlot.Hero, "None");
                                break;
                            case TownEffectType.AddQuirk:
                                var addQuirkEffect = effect as AddQuirkTownEffect;
                                var activityQuirk = RandomSolver.ChooseByRandom<TownActivityQuirk>(addQuirkEffect.QuirkSet);
                                if(activityQuirk != null)
                                {
                                    var quirk = DarkestDungeonManager.Data.Quirks[activityQuirk.QuirkName];
                                    if(activitySlot.Hero.AddQuirk(quirk))
                                        LogActivity(ActivityEffectType.AddQuirk, activitySlot.Hero, quirk.Id);
                                }
                                activitySlot.Hero.Status = HeroStatus.Available;
                                activitySlot.Status = ActivitySlotStatus.Available;
                                activitySlot.Hero = null;
                                break;
                            case TownEffectType.AddTrinket:
                                if(activitySlot.Hero.LeftTrinket != null || activitySlot.Hero.RightTrinket != null)
                                {
                                    var trinket = DarkestDungeonManager.Campaign.Estate.NomadWagon.GenerateTrinket();
                                    DarkestDungeonManager.Campaign.RealmInventory.AddTrinket(trinket);
                                    LogActivity(ActivityEffectType.AddTrinket, activitySlot.Hero, trinket.Id);
                                }
                                activitySlot.Hero.Status = HeroStatus.Available;
                                activitySlot.Status = ActivitySlotStatus.Available;
                                activitySlot.Hero = null;
                                break;
                            case TownEffectType.RemoveTrinket:
                                if (activitySlot.Hero.LeftTrinket != null && activitySlot.Hero.RightTrinket != null)
                                {
                                    if (Random.value > 0.5f)
                                    {
                                        LogActivity(ActivityEffectType.RemoveTrinket, 
                                            activitySlot.Hero, activitySlot.Hero.LeftTrinket.Id);
                                        activitySlot.Hero.Unequip(TrinketSlot.Left);
                                    }
                                    else
                                    {
                                        LogActivity(ActivityEffectType.RemoveTrinket,
                                            activitySlot.Hero, activitySlot.Hero.RightTrinket.Id);
                                        activitySlot.Hero.Unequip(TrinketSlot.Right);
                                    }
                                }
                                else if (activitySlot.Hero.LeftTrinket != null)
                                {
                                    LogActivity(ActivityEffectType.RemoveTrinket, 
                                        activitySlot.Hero, activitySlot.Hero.LeftTrinket.Id);
                                    activitySlot.Hero.Unequip(TrinketSlot.Left);
                                }
                                else if (activitySlot.Hero.RightTrinket != null)
                                {
                                    LogActivity(ActivityEffectType.RemoveTrinket, 
                                        activitySlot.Hero, activitySlot.Hero.RightTrinket.Id);
                                    activitySlot.Hero.Unequip(TrinketSlot.Right);
                                }
                                activitySlot.Hero.Status = HeroStatus.Available;
                                activitySlot.Status = ActivitySlotStatus.Available;
                                activitySlot.Hero = null;
                                break;
                            case TownEffectType.ApplyBuff:
                                var addBuffTownEffect = effect as AddBuffTownEffect;
                                var activityBuff = RandomSolver.ChooseByRandom<TownActivityBuff>(addBuffTownEffect.BuffSets);
                                if(activityBuff != null)
                                {
                                    foreach(var buffName in activityBuff.BuffNames)
                                    {
                                        var buff = DarkestDungeonManager.Data.Buffs[buffName];
                                        if(!activitySlot.Hero.ContainsBuff(buff, BuffSourceType.Estate))
                                            activitySlot.Hero.AddBuff(new BuffInfo(buff, BuffDurationType.Raid, BuffSourceType.Estate));
                                    }
                                    LogActivity(ActivityEffectType.AddBuff, activitySlot.Hero, activityBuff.BuffNames[0]);
                                }
                                activitySlot.Hero.Status = HeroStatus.Available;
                                activitySlot.Status = ActivitySlotStatus.Available;
                                activitySlot.Hero = null;
                                break;
                            case TownEffectType.ChangeCurrency:
                                var changeCurrencyEffect = effect as CurrencyTownEffect;
                                var change = RandomSolver.ChooseByRandom<TownActivityCurrency>(changeCurrencyEffect.Changes);
                                if(change != null)
                                {
                                    if (change.Amount > 0)
                                    {
                                        LogActivity(ActivityEffectType.CurrencyGained,
                                            activitySlot.Hero, change.Amount.ToString());
                                        DarkestDungeonManager.Campaign.Estate.AddGold(change.Amount);
                                    }
                                    else
                                    {
                                        LogActivity(ActivityEffectType.CurrencyLost,
                                            activitySlot.Hero, Mathf.Abs(change.Amount).ToString());
                                        DarkestDungeonManager.Campaign.Estate.RemoveGold(-change.Amount);
                                    }
                                }
                                activitySlot.Hero.Status = HeroStatus.Available;
                                activitySlot.Status = ActivitySlotStatus.Available;
                                activitySlot.Hero = null;
                                break;
                        }
                    }
                    else
                    {
                        LogActivity(ActivityEffectType.Nothing, activitySlot.Hero, "None");
                        activitySlot.Hero.Status = HeroStatus.Available;
                        activitySlot.Status = ActivitySlotStatus.Available;
                        activitySlot.Hero = null;
                    }
                    break;
                default:
                    break;
            }
            
        }
    }
    public void Reset()
    {
        ActivitySlots = new List<ActivitySlot>();
        NumberOfSlots = BaseSlots;
        StressHealAmount = BaseStressHeal;
        ActivityCost = BaseCost;
        AfflictionCureChance = BaseAfflictionCure;
    }

    public ITownUpgrade GetUpgradeByCode(string code)
    {
        ITownUpgrade upgrade = CostUpgrades.Find(item => item.UpgradeCode == code);
        if (upgrade != null)
            return upgrade;
        upgrade = SlotUpgrades.Find(item => item.UpgradeCode == code);
        if (upgrade != null)
            return upgrade;
        upgrade = StressUpgrades.Find(item => item.UpgradeCode == code);
        return upgrade;
    }
}