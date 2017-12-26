using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Estate
{
    public string EstateTitle { get; private set; }

    public Abbey Abbey { get; private set; }
    public Tavern Tavern { get; private set; }
    public Sanitarium Sanitarium { get; private set; }
    public Blacksmith Blacksmith { get; private set; }
    public Guild Guild { get; private set; }
    public CampingTrainer CampingTrainer { get; private set; }
    public NomadWagon NomadWagon { get; private set; }
    public StageCoach StageCoach { get; private set; }
    public Graveyard Graveyard { get; private set; }
    public Statue Statue { get; private set; }

    public Dictionary<BuildingType, Building> Buildings { get; private set; }
    public Dictionary<int, Dictionary<string, UpgradePurchases>> HeroPurchases { get; private set; }
    public Dictionary<string, UpgradePurchases> TownPurchases { get; private set; }
    public Dictionary<string, int> Currencies { get; private set; }

    private List<int> RosterIds { get; set; }

	public Estate(SaveCampaignData saveData)
    {
        RosterIds = new List<int>();
        for (int i = 1; i < 100; i++)
            RosterIds.Add(i);

        EstateTitle = saveData.HamletTitle;

        Currencies = new Dictionary<string, int>();
        Currencies.Add("gold", saveData.GoldAmount);
        Currencies.Add("bust", saveData.BustsAmount);
        Currencies.Add("deed", saveData.DeedsAmount);
        Currencies.Add("portrait", saveData.PortraitsAmount);
        Currencies.Add("crest", saveData.CrestsAmount);

        HeroPurchases = saveData.InstancedPurchases;
        TownPurchases = saveData.BuildingUpgrades;

        Buildings = new Dictionary<BuildingType, Building>();
        Abbey = DarkestDungeonManager.Data.Buildings["abbey"] as Abbey;
        Buildings.Add(BuildingType.Abbey, Abbey);
        Tavern = DarkestDungeonManager.Data.Buildings["tavern"] as Tavern;
        Buildings.Add(BuildingType.Tavern, Tavern);
        Sanitarium = DarkestDungeonManager.Data.Buildings["sanitarium"] as Sanitarium;
        Buildings.Add(BuildingType.Sanitarium, Sanitarium);
        Blacksmith = DarkestDungeonManager.Data.Buildings["blacksmith"] as Blacksmith;
        Buildings.Add(BuildingType.Blacksmith, Blacksmith);
        Guild = DarkestDungeonManager.Data.Buildings["guild"] as Guild;
        Buildings.Add(BuildingType.Guild, Guild);
        NomadWagon = DarkestDungeonManager.Data.Buildings["nomad_wagon"] as NomadWagon;
        Buildings.Add(BuildingType.NomadWagon, NomadWagon);
        StageCoach = DarkestDungeonManager.Data.Buildings["stage_coach"] as StageCoach;
        Buildings.Add(BuildingType.StageCoach, StageCoach);
        CampingTrainer = DarkestDungeonManager.Data.Buildings["camping_trainer"] as CampingTrainer;
        Buildings.Add(BuildingType.CampingTrainer, CampingTrainer);
        Graveyard = new Graveyard();
        Buildings.Add(BuildingType.Graveyard, Graveyard);
        Statue = new Statue();
        Buildings.Add(BuildingType.Statue, Statue);

        Abbey.InitializeBuilding(TownPurchases);
        Tavern.InitializeBuilding(TownPurchases);
        Sanitarium.InitializeBuilding(TownPurchases);
        Blacksmith.InitializeBuilding(TownPurchases);
        Guild.InitializeBuilding(TownPurchases);
        CampingTrainer.InitializeBuilding(TownPurchases);
        NomadWagon.InitializeBuilding(TownPurchases);
        StageCoach.InitializeBuilding(TownPurchases);
        Graveyard.Records.AddRange(saveData.DeathRecords);
    }

    public void RedeployCaretaker()
    {
        var availableSlots = new List<ActivitySlot>();
        foreach (var activity in Tavern.Activities.Concat(Abbey.Activities))
            availableSlots.AddRange(activity.ActivitySlots.FindAll(slot =>
                slot.IsUnlocked && slot.Status == ActivitySlotStatus.Available));

        if (availableSlots.Count > 0)
            availableSlots[Random.Range(0, availableSlots.Count)].Status = ActivitySlotStatus.Caretaken;
    }

    public void RedeployCrier()
    {
        var availableSlots = new List<ActivitySlot>();
        foreach (var activity in Tavern.Activities.Concat(Abbey.Activities))
            availableSlots.AddRange(activity.ActivitySlots.FindAll(slot =>
                slot.IsUnlocked && slot.Status == ActivitySlotStatus.Available));

        if (availableSlots.Count > 0)
            availableSlots[Random.Range(0, availableSlots.Count)].Status = ActivitySlotStatus.Crierd;
    }

    public void KickCrier()
    {
        foreach (var activity in Tavern.Activities.Concat(Abbey.Activities))
        {
            var crierSlot = activity.ActivitySlots.Find(slot => slot.Status == ActivitySlotStatus.Crierd);
            if(crierSlot != null)
            {
                crierSlot.Status = ActivitySlotStatus.Available;
                return;
            }
        }
    }

    public void ExecuteProgress()
    {
        NomadWagon.RestockTrinkets();
        StageCoach.RestockHeroes(RosterIds, this);

        Abbey.ProvideActivity();
        Tavern.ProvideActivity();
        Sanitarium.ProvideActivity();

        RedeployCaretaker();
    }

    public void RestockBonus(string bonusClass, int bonusAmount)
    {
        StageCoach.RestockBonus(RosterIds, this, bonusClass, bonusAmount);
    }

    public void RestockFromGrave(int bonusAmount)
    {
        StageCoach.RestockFromGrave(RosterIds, this, bonusAmount);
    }

    public void ReturnRosterId(int id)
    {
        if (!RosterIds.Contains(id))
            RosterIds.Add(id);

        if(HeroPurchases.ContainsKey(id))
            HeroPurchases.Remove(id);
    }

    public bool PickRosterId(int id)
    {
        if(RosterIds.Contains(id))
        {
            RosterIds.Remove(id);
            return true;
        }
        return false;
    }

    #region Upgrade Helpers

    public float GetBuildingUpgradeRatio(BuildingType building)
    {
        switch(building)
        {
            case BuildingType.Abbey:
                float purchased = TownPurchases["abbey.meditation"].PurchasedUpgrades.Count
                    + TownPurchases["abbey.prayer"].PurchasedUpgrades.Count
                    + TownPurchases["abbey.flagellation"].PurchasedUpgrades.Count;
                int all = DarkestDungeonManager.Data.UpgradeTrees["abbey.meditation"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["abbey.prayer"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["abbey.flagellation"].Upgrades.Count;
                return purchased / all;
            case BuildingType.Tavern:
                float purchasedTavern = TownPurchases["tavern.bar"].PurchasedUpgrades.Count
                    + TownPurchases["tavern.gambling"].PurchasedUpgrades.Count
                    + TownPurchases["tavern.brothel"].PurchasedUpgrades.Count;
                int allTavern = DarkestDungeonManager.Data.UpgradeTrees["tavern.bar"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["tavern.gambling"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["tavern.brothel"].Upgrades.Count;
                return purchasedTavern / allTavern;
            case BuildingType.Sanitarium:
                float purchasedSan = TownPurchases["sanitarium.cost"].PurchasedUpgrades.Count
                    + TownPurchases["sanitarium.disease_quirk_cost"].PurchasedUpgrades.Count
                    + TownPurchases["sanitarium.slots"].PurchasedUpgrades.Count;
                int allSan = DarkestDungeonManager.Data.UpgradeTrees["sanitarium.cost"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["sanitarium.disease_quirk_cost"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["sanitarium.slots"].Upgrades.Count;
                return purchasedSan / allSan;
            case BuildingType.Blacksmith:
                float purchasedBlack = TownPurchases["blacksmith.weapon"].PurchasedUpgrades.Count
                    + TownPurchases["blacksmith.armour"].PurchasedUpgrades.Count
                    + TownPurchases["blacksmith.cost"].PurchasedUpgrades.Count;
                int allBlack = DarkestDungeonManager.Data.UpgradeTrees["blacksmith.weapon"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["blacksmith.armour"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["blacksmith.cost"].Upgrades.Count;
                return purchasedBlack / allBlack;
            case BuildingType.Guild:
                float purchasedGuild = TownPurchases["guild.skill_levels"].PurchasedUpgrades.Count
                    + TownPurchases["guild.cost"].PurchasedUpgrades.Count;
                int allGuild = DarkestDungeonManager.Data.UpgradeTrees["guild.skill_levels"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["guild.cost"].Upgrades.Count;
                return purchasedGuild / allGuild;
            case BuildingType.CampingTrainer:
                float purchasedCamp = TownPurchases["camping_trainer.cost"].PurchasedUpgrades.Count;
                int allCamp = DarkestDungeonManager.Data.UpgradeTrees["camping_trainer.cost"].Upgrades.Count;
                return purchasedCamp / allCamp;
            case BuildingType.NomadWagon:
                float purchasedWagon = TownPurchases["nomad_wagon.numitems"].PurchasedUpgrades.Count
                    + TownPurchases["nomad_wagon.cost"].PurchasedUpgrades.Count;
                int allWagon = DarkestDungeonManager.Data.UpgradeTrees["nomad_wagon.numitems"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["nomad_wagon.cost"].Upgrades.Count;
                return purchasedWagon / allWagon;
            case BuildingType.StageCoach:
                float purchasedCoach = TownPurchases["stage_coach.numrecruits"].PurchasedUpgrades.Count
                    + TownPurchases["stage_coach.rostersize"].PurchasedUpgrades.Count
                    + TownPurchases["stage_coach.upgraded_recruits"].PurchasedUpgrades.Count;
                int allCoach = DarkestDungeonManager.Data.UpgradeTrees["stage_coach.numrecruits"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["stage_coach.rostersize"].Upgrades.Count
                    + DarkestDungeonManager.Data.UpgradeTrees["stage_coach.upgraded_recruits"].Upgrades.Count;
                return purchasedCoach / allCoach;
            case BuildingType.Graveyard:
            case BuildingType.Statue:
                return 1;
            default:
                Debug.LogError("Building type " + building.ToString() + " not handled in estate upgrade ratio.");
                return 0;
        }
    }

    public bool IsRequirementMet(int rosterId, PrerequisiteReqirement requirement)
    {
        if (HeroPurchases[rosterId].ContainsKey(requirement.TreeId))
        {
            if (HeroPurchases[rosterId][requirement.TreeId].PurchasedUpgrades.Contains(requirement.RequirementCode))
                return true;
        }
        if (TownPurchases.ContainsKey(requirement.TreeId))
        {
            if (TownPurchases[requirement.TreeId].PurchasedUpgrades.Contains(requirement.RequirementCode))
                return true;
        }
        return false;
    }

    public bool IsRequirementMet(PrerequisiteReqirement requirement)
    {
        if (TownPurchases.ContainsKey(requirement.TreeId))
        {
            if (TownPurchases[requirement.TreeId].PurchasedUpgrades.Contains(requirement.RequirementCode))
                return true;
        }
        return false;
    }

    public int GetUpgradeLevel(PrerequisiteReqirement requirement)
    {
        if (DarkestDungeonManager.Data.UpgradeTrees.ContainsKey(requirement.TreeId))
        {
            var upgradeLevel = DarkestDungeonManager.Data.UpgradeTrees[requirement.TreeId].
                Upgrades.FindIndex(item => item.Code == requirement.RequirementCode);

            if (DarkestDungeonManager.Data.UpgradeTrees[requirement.TreeId].Tags.Contains("first_level_not_upgrade"))
                return upgradeLevel + 2;
            else
                return upgradeLevel + 1;
        }
        return 0;
    }

    public int GetUpgradedArmorLevel(int rosterId, string classId)
    {
        if(HeroPurchases.ContainsKey(rosterId))
            return HeroPurchases[rosterId][classId + ".armour"].PurchasedUpgrades.Count + 1;
        return 1;
    }

    public int GetUpgradedSkillLevel(int rosterId, string classId, string skillId)
    {
        if (HeroPurchases.ContainsKey(rosterId))
            return HeroPurchases[rosterId][classId + "." + skillId].PurchasedUpgrades.Count - 1;
        return -1;
    }

    public bool GetUpgradedCampingStatus(int rosterId, string skillId)
    {
        if (HeroPurchases.ContainsKey(rosterId))
            return HeroPurchases[rosterId][skillId].PurchasedUpgrades.Contains("0");
        return false;
    }

    public int GetUpgradedWeaponLevel(int rosterId, string classId)
    {
        if (HeroPurchases.ContainsKey(rosterId))
            return HeroPurchases[rosterId][classId + ".weapon"].PurchasedUpgrades.Count + 1;
        return 1;
    }

    public UpgradeStatus GetUpgradeStatus(string treeId, Hero hero, HeroUpgrade upgrade)
    {
        if (upgrade.PrerequisiteResolveLevel > hero.Resolve.Level)
            return UpgradeStatus.Locked;

        if (HeroPurchases[hero.RosterId].ContainsKey(treeId))
        {
            if (HeroPurchases[hero.RosterId][treeId].PurchasedUpgrades.Contains(upgrade.Code))
                return UpgradeStatus.Purchased;
        }
        else
            return UpgradeStatus.Locked;



        for (int i = 0; i < upgrade.Prerequisites.Count; i++)
        {
            if (HeroPurchases[hero.RosterId].ContainsKey(upgrade.Prerequisites[i].TreeId))
            {
                if (!HeroPurchases[hero.RosterId][upgrade.Prerequisites[i].TreeId].PurchasedUpgrades.
                    Contains(upgrade.Prerequisites[i].RequirementCode))
                    return UpgradeStatus.Locked;
            }
            if (TownPurchases.ContainsKey(upgrade.Prerequisites[i].TreeId))
            {
                if (!TownPurchases[upgrade.Prerequisites[i].TreeId].PurchasedUpgrades.
                    Contains(upgrade.Prerequisites[i].RequirementCode))
                    return UpgradeStatus.Locked;
            }
        }
        return UpgradeStatus.Available;
    }

    public UpgradeStatus GetUpgradeStatus(string treeId, TownUpgrade upgrade)
    {
        if (TownPurchases[treeId].PurchasedUpgrades.Contains(upgrade.Code))
            return UpgradeStatus.Purchased;

        for (int i = 0; i < upgrade.Prerequisites.Count; i++)
        {
            if (!TownPurchases[upgrade.Prerequisites[i].TreeId].PurchasedUpgrades.
                Contains(upgrade.Prerequisites[i].RequirementCode))
                return UpgradeStatus.Locked;
        }
        return UpgradeStatus.Available;
    }

    public bool BuyUpgrade(string treeId, Hero hero, TownUpgrade upgrade, float discount, bool isFree)
    {
        if (!isFree && !CanPayPrice(upgrade.Cost, discount))
            return false;

        if (!HeroPurchases.ContainsKey(hero.RosterId))
        {
            HeroPurchases.Add(hero.RosterId, new Dictionary<string, UpgradePurchases>());
        }

        if (!HeroPurchases[hero.RosterId].ContainsKey(treeId))
        {
            HeroPurchases[hero.RosterId].Add(treeId, new UpgradePurchases(treeId, upgrade.Code));
        }
        else
        {
            if (HeroPurchases[hero.RosterId][treeId].PurchasedUpgrades.Contains(upgrade.Code))
                return false;

            HeroPurchases[hero.RosterId][treeId].PurchasedUpgrades.Add(upgrade.Code);
        }
        if (!isFree)
            RemoveCurrency(upgrade.Cost, discount);

        DarkestSoundManager.PlayOneShot("event:/ui/town/buy");
        return true;
    }

    public bool BuyUpgrade(CampingSkill skill, Hero hero, float discount)
    {
        if (!CanPayPrice(skill.CurrencyCost, discount))
            return false;

        if (!HeroPurchases.ContainsKey(hero.RosterId))
            return false;

        if (!HeroPurchases[hero.RosterId].ContainsKey(skill.Id))
            return false;

        if (HeroPurchases[hero.RosterId][skill.Id].PurchasedUpgrades.Contains("0"))
            return false;

        HeroPurchases[hero.RosterId][skill.Id].PurchasedUpgrades.Add("0");

        RemoveCurrency(skill.CurrencyCost, discount);
        DarkestSoundManager.PlayOneShot("event:/ui/town/buy");
        return true;
    }

    public bool BuyUpgrade(string treeId, TownUpgrade upgrade, bool isFree)
    {
        if (!isFree && !CanPayPrice(upgrade.Cost))
            return false;

        if (!TownPurchases.ContainsKey(treeId))
        {
            TownPurchases.Add(treeId, new UpgradePurchases(treeId, upgrade.Code));
        }
        else
        {
            if (TownPurchases[treeId].PurchasedUpgrades.Contains(upgrade.Code))
                return false;

            TownPurchases[treeId].PurchasedUpgrades.Add(upgrade.Code);
        }

        if (!isFree)
        {
            RemoveCurrency(upgrade.Cost);
            if (upgrade.Cost.Find(cost => cost.Type != "gold" && cost.Amount > 0) != null)
                EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("heirloom");
        }

        if (DarkestDungeonManager.Data.UpgradeTrees.ContainsKey(treeId))
            DarkestSoundManager.ExecuteNarration("upgrade_building", NarrationPlace.Town,
                DarkestDungeonManager.Data.UpgradeTrees[treeId].Tags.ToArray());

        DarkestSoundManager.PlayOneShot("event:/ui/town/buy");
        return true;
    }

    #endregion

    public DeathRecord RecruitHero(Hero hero)
    {
        if (StageCoach.Heroes.Remove(hero))
            return null;

        if (StageCoach.EventHeroes.Contains(hero))
        {
            int removedIndex = StageCoach.EventHeroes.IndexOf(hero);
            StageCoach.EventHeroes.Remove(hero);

            if (StageCoach.GraveIndexes.Count > 0)
            {
                int deadRecordIndex = StageCoach.GraveIndexes[removedIndex];
                var deathRecord = Graveyard.Records[deadRecordIndex];
                Graveyard.Records.RemoveAt(deadRecordIndex);
                StageCoach.ClearDeadRecruits(RosterIds, this);
                return deathRecord;
            }
            return null;
        }
        return null;
    }

    public void ReequipHero(Hero hero)
    {
        Equipment weapon = hero.HeroClass.Weapons.Find(wep => 
            wep.UpgradeLevel == GetUpgradedWeaponLevel(hero.RosterId, hero.HeroClass.StringId));
        hero.Equip(weapon, HeroEquipmentSlot.Weapon);
        Equipment armor = hero.HeroClass.Armors.Find(arm =>
            arm.UpgradeLevel == GetUpgradedArmorLevel(hero.RosterId, hero.HeroClass.StringId));
        hero.Equip(armor, HeroEquipmentSlot.Armor);
    }

    public void ReskillCombatHero(Hero hero)
    {
        for (int i = 0; i < hero.HeroClass.CombatSkills.Count; i++)
        {
            hero.CurrentCombatSkills[i] = hero.HeroClass.CombatSkillVariants.Find(skill => skill.Id == hero.HeroClass.CombatSkills[i].Id
                && skill.Level == GetUpgradedSkillLevel(hero.RosterId, hero.ClassStringId, hero.HeroClass.CombatSkills[i].Id));
        }

        for(int i = 0; i < hero.SelectedCombatSkills.Count; i++)
        {
            hero.SelectedCombatSkills[i] = hero.CurrentCombatSkills[hero.HeroClass.CombatSkills
                .FindIndex(skill => skill.Id == hero.SelectedCombatSkills[i].Id)];
        }
    }

    public void ReskillCampingHero(Hero hero)
    {
        for (int i = 0; i < hero.HeroClass.CampingSkills.Count; i++)
            hero.CurrentCampingSkills[i] = HeroPurchases[hero.RosterId][hero.HeroClass.CampingSkills[i].Id].PurchasedUpgrades.Contains("0")?
                hero.HeroClass.CampingSkills[i] : null;
    }

    #region Currency Helpers

    public bool CanPayPrice(IEnumerable<CurrencyCost> currencies, float discount)
    {
        foreach (var cost in currencies)
        {
            if (Currencies[cost.Type] < cost.Amount * discount)
                return false;
        }
        return true;
    }

    public bool CanPayPrice(IEnumerable<CurrencyCost> currencies)
    {
        foreach(var cost in currencies)
        {
            if (Currencies[cost.Type] < cost.Amount)
                return false;
        }
        return true;
    }

    public bool CanPayPrice(CurrencyCost cost, float discount)
    {
        return Currencies[cost.Type] >= cost.Amount * discount;
    }

    public bool CanPayPrice(CurrencyCost cost)
    {
        return Currencies[cost.Type] >= cost.Amount;
    }

    public bool CanPayGold(int goldAmount)
    {
        return Currencies["gold"] >= goldAmount;
    }

    public bool RemoveCurrency(IEnumerable<CurrencyCost> currencies)
    {
        foreach(var cost in currencies)
        {
            Currencies[cost.Type] -= cost.Amount;
        }
            
        return true;
    }

    public bool RemoveCurrency(IEnumerable<CurrencyCost> currencies, float discount)
    {
        foreach (var cost in currencies)
        {
            Currencies[cost.Type] -= Mathf.RoundToInt(cost.Amount * discount);
        }
        return true;
    }

    public bool RemoveCurrency(CurrencyCost cost)
    {
        Currencies[cost.Type] -= cost.Amount;
        return true;
    }

    public bool RemoveCurrency(CurrencyCost cost, float discount)
    {
        Currencies[cost.Type] -= Mathf.RoundToInt(cost.Amount * discount);
        return true;
    }

    public void AddHeirlooms(int crest, int deed, int portrait, int bust)
    {
        Currencies["crest"] = Mathf.Clamp(Currencies["crest"] + crest, 0, int.MaxValue);
        Currencies["deed"] = Mathf.Clamp(Currencies["deed"] + deed, 0, int.MaxValue);
        Currencies["portrait"] = Mathf.Clamp(Currencies["portrait"] + portrait, 0, int.MaxValue);
        Currencies["bust"] = Mathf.Clamp(Currencies["bust"] + bust, 0, int.MaxValue);
    }

    public void AddGold(int goldAmount)
    {
        Currencies["gold"] = Mathf.Clamp(Currencies["gold"] + goldAmount, 0, int.MaxValue);
    }

    public bool RemoveGold(int goldAmount)
    {
        Currencies["gold"] = Mathf.Clamp(Currencies["gold"] - goldAmount, 0, int.MaxValue);
        return true;
    }

    #endregion
}