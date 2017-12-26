using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StageCoach : Building
{
    public override string Name { get { return "stage_coach"; } }
    public override BuildingType Type { get { return BuildingType.StageCoach; } }
    public int BaseRecruitSlots { get; set; }
    public int RecruitSlots { get; set; }
    public int BaseRosterSlots { get; set; }
    public int RosterSlots { get; set; }

    public List<SlotUpgrade> RecruitSlotUpgrades { get; set; }
    public List<SlotUpgrade> RosterSlotUpgrades { get; set; }
    public List<RecruitUpgrade> RecruitExperienceUpgrades { get; set; }
    public List<Hero> Heroes { get; set; }
    public List<Hero> EventHeroes { get; set; }
    public List<int> GraveIndexes { get; set; }

    private int CurrentRecruitMaxLevel { get; set; }
    private readonly string[] firstHeroClasses = { "plague_doctor", "vestal" };

    public StageCoach()
    {
        RecruitSlotUpgrades = new List<SlotUpgrade>();
        RosterSlotUpgrades = new List<SlotUpgrade>();
        RecruitExperienceUpgrades = new List<RecruitUpgrade>();
        Heroes = new List<Hero>();
        EventHeroes = new List<Hero>();
        GraveIndexes = new List<int>();
    }

    public void RestockHeroes(List<int> rosterIds, Estate estate)
    {
        var heroClasses = DarkestDungeonManager.Data.HeroClasses.Keys.ToList();
        Heroes.AddRange(EventHeroes);
        EventHeroes.Clear();
        GraveIndexes.Clear();
        #region Clear unrecruited heroes
        if(Heroes != null && Heroes.Count > 0)
        {
            for(int i = 0; i < Heroes.Count; i++)
            {
                if (rosterIds.Contains(Heroes[i].RosterId))
                {
                    Debug.LogError("Same id returned while restocking heroes.");
                }
                else
                    rosterIds.Add(Heroes[i].RosterId);

                estate.HeroPurchases.Remove(Heroes[i].RosterId);
            }
        }
        #endregion
        #region Create new recruits
        Heroes = new List<Hero>();
        if(DarkestDungeonManager.RaidManager.Quest != null && DarkestDungeonManager.RaidManager.Quest.Goal.Id == "tutorial_final_room")
        {
            for (int i = 0; i < firstHeroClasses.Length; i++)
            {
                int id = rosterIds[Random.Range(0, rosterIds.Count)];
                var newHero = new Hero(id, firstHeroClasses[i],
                    LocalizationManager.GetString("hero_name_" + Random.Range(0, 556).ToString()));
                Heroes.Add(newHero);
                rosterIds.Remove(id);
                GeneratePurchaseInfo(newHero, estate);
            }
        }
        else
        {
            for (int i = 0; i < RecruitSlots; i++)
            {
                RecruitUpgrade experienceUpgrade = null;

                if(CurrentRecruitMaxLevel > 0)
                {
                    for(int j = 0; j <= RecruitExperienceUpgrades.Count - 1; j++)
                    {
                        if (RecruitExperienceUpgrades[j].Level <= CurrentRecruitMaxLevel &&
                            RandomSolver.CheckSuccess(RecruitExperienceUpgrades[j].Chance))
                        {
                            experienceUpgrade = RecruitExperienceUpgrades[j];
                            break;
                        }
                    }
                }
                int id = rosterIds[Random.Range(0, rosterIds.Count)];
                string heroClass = heroClasses[Random.Range(0, DarkestDungeonManager.Data.HeroClasses.Count)];
                string heroName = LocalizationManager.GetString("hero_name_" + Random.Range(0, 556).ToString());
                var newHero = experienceUpgrade == null ?
                    new Hero(id, heroClass, heroName) :
                    new Hero(id, heroClass, heroName, experienceUpgrade);
                Heroes.Add(newHero);
                rosterIds.Remove(id);
                GeneratePurchaseInfo(newHero, estate);
            }
        }
        #endregion
        #region Add recruits for minimum of one party
        int abominations = DarkestDungeonManager.Campaign.Heroes.FindAll(hero =>
            hero.Class == "abomination").Count + Heroes.FindAll(hero => hero.Class == "abomination").Count;
        int additionalHeroes = 4 - DarkestDungeonManager.Campaign.Heroes.Count - Heroes.Count + abominations;
        if(abominations > 3)
            return;
        for(int i = 0; i < additionalHeroes; i++)
        {
            RecruitUpgrade experienceUpgrade = null;

            if (CurrentRecruitMaxLevel > 0)
            {
                for (int j = 0; j <= RecruitExperienceUpgrades.Count - 1; j++)
                {
                    if (RecruitExperienceUpgrades[j].Level <= CurrentRecruitMaxLevel &&
                        RandomSolver.CheckSuccess(RecruitExperienceUpgrades[j].Chance))
                    {
                        experienceUpgrade = RecruitExperienceUpgrades[j];
                        break;
                    }
                }
            }
            int id = rosterIds[Random.Range(0, rosterIds.Count)];
            string heroClass = "abomination";
            while (heroClass == "abomination")
                heroClass = heroClasses[Random.Range(0, DarkestDungeonManager.Data.HeroClasses.Count)];

            string heroName = LocalizationManager.GetString("hero_name_" + Random.Range(0, 556).ToString());
            var newHero = experienceUpgrade == null ?
                new Hero(id, heroClass, heroName) :
                new Hero(id, heroClass, heroName, experienceUpgrade);
            Heroes.Add(newHero);
            rosterIds.Remove(id);
            GeneratePurchaseInfo(newHero, estate);
        }
        #endregion
    }

    public void RestockBonus(List<int> rosterIds, Estate estate, string bonusClass, int bonusAmount)
    {
        EventHeroes.Clear();
        GraveIndexes.Clear();

        #region Clear unrecruited heroes
        if (EventHeroes.Count > 0)
        {
            for (int i = 0; i < EventHeroes.Count; i++)
            {
                if (rosterIds.Contains(EventHeroes[i].RosterId))
                {
                    Debug.LogError("Same id returned while restocking heroes.");
                }
                else
                    rosterIds.Add(EventHeroes[i].RosterId);

                estate.HeroPurchases.Remove(EventHeroes[i].RosterId);
            }
        }
        #endregion

        #region Create bonus recruits
        for (int i = 0; i < bonusAmount; i++)
        {
            RecruitUpgrade experienceUpgrade = null;

            if (CurrentRecruitMaxLevel > 0)
            {
                for (int j = 0; j <= RecruitExperienceUpgrades.Count - 1; j++)
                {
                    if (RecruitExperienceUpgrades[j].Level <= CurrentRecruitMaxLevel &&
                        RandomSolver.CheckSuccess(RecruitExperienceUpgrades[j].Chance))
                    {
                        experienceUpgrade = RecruitExperienceUpgrades[j];
                        break;
                    }
                }
            }
            int id = rosterIds[Random.Range(0, rosterIds.Count)];
            string heroClass = DarkestDungeonManager.Data.HeroClasses[bonusClass].StringId;
            string heroName = LocalizationManager.GetString("hero_name_" + Random.Range(0, 556).ToString());
            var newHero = experienceUpgrade == null ?
                new Hero(id, heroClass, heroName) :
                new Hero(id, heroClass, heroName, experienceUpgrade);
            EventHeroes.Add(newHero);
            rosterIds.Remove(id);
            GeneratePurchaseInfo(newHero, estate);
        }
        #endregion
    }

    public void RestockFromGrave(List<int> rosterIds, Estate estate, int bonusAmount)
    {
        EventHeroes.Clear();
        GraveIndexes.Clear();

        #region Clear unrecruited heroes
        if (EventHeroes.Count > 0)
        {
            for (int i = 0; i < EventHeroes.Count; i++)
            {
                if (rosterIds.Contains(EventHeroes[i].RosterId))
                {
                    Debug.LogError("Same id returned while restocking heroes.");
                }
                else
                    rosterIds.Add(EventHeroes[i].RosterId);

                estate.HeroPurchases.Remove(EventHeroes[i].RosterId);
            }
        }
        #endregion

        bonusAmount = Mathf.Min(estate.Graveyard.Records.Count, bonusAmount);
        var deadRecruitsRecords = new List<DeathRecord>(estate.Graveyard.Records);
        var heroClasses = DarkestDungeonManager.Data.HeroClasses.Values.ToList();

        #region Create dead recruits
        for (int i = 0; i < bonusAmount; i++)
        {
            int id = rosterIds[Random.Range(0, rosterIds.Count)];
            var deadRecord = deadRecruitsRecords[Random.Range(0, deadRecruitsRecords.Count)];
            string heroClass = heroClasses.Find(deadClass => deadClass.IndexId == deadRecord.HeroClassIndex).StringId;
            var newHero = new Hero(id, heroClass, deadRecord);
            EventHeroes.Add(newHero);
            rosterIds.Remove(id);
            GeneratePurchaseInfo(newHero, estate);
            GraveIndexes.Add(estate.Graveyard.Records.IndexOf(deadRecord));
            deadRecruitsRecords.Remove(deadRecord);
        }
        #endregion
    }

    public void ClearDeadRecruits(List<int> rosterIds, Estate estate)
    {
        EventHeroes.Clear();
        GraveIndexes.Clear();

        #region Clear unrecruited heroes
        if (EventHeroes.Count > 0)
        {
            for (int i = 0; i < EventHeroes.Count; i++)
            {
                if (rosterIds.Contains(EventHeroes[i].RosterId))
                {
                    Debug.LogError("Same id returned while restocking heroes.");
                }
                else
                    rosterIds.Add(EventHeroes[i].RosterId);

                estate.HeroPurchases.Remove(EventHeroes[i].RosterId);
            }
        }
        #endregion
    }

    public override void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        Heroes.Clear();
        EventHeroes.Clear();
        GraveIndexes.Clear();

        Reset();

        for (int i = RosterSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[RosterSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(RosterSlotUpgrades[i].UpgradeCode))
            {
                RosterSlots = RosterSlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        for (int i = RecruitSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[RecruitSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(RecruitSlotUpgrades[i].UpgradeCode))
            {
                RecruitSlots = RecruitSlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        for(int i = RecruitExperienceUpgrades.Count - 1; i>= 0; i--)
        {
            if (purchases[RecruitExperienceUpgrades[i].TreeId].PurchasedUpgrades.Contains(RecruitExperienceUpgrades[i].UpgradeCode))
            {
                CurrentRecruitMaxLevel = RecruitExperienceUpgrades[i].Level;
                break;
            }
        }
    }

    public override void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();

        for (int i = RosterSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[RosterSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(RosterSlotUpgrades[i].UpgradeCode))
            {
                RosterSlots = RosterSlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        for (int i = RecruitSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[RecruitSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(RecruitSlotUpgrades[i].UpgradeCode))
            {
                RecruitSlots = RecruitSlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        for (int i = RecruitExperienceUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[RecruitExperienceUpgrades[i].TreeId].PurchasedUpgrades.Contains(RecruitExperienceUpgrades[i].UpgradeCode))
            {
                CurrentRecruitMaxLevel = RecruitExperienceUpgrades[i].Level;
                break;
            }
        }
    }

    public override List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        List<ITownUpgrade> townUpgrades = new List<ITownUpgrade>();
        townUpgrades.AddRange(RosterSlotUpgrades.FindAll(item => item.UpgradeCode == code && item.TreeId == treeId).Cast<ITownUpgrade>());
        townUpgrades.AddRange(RecruitSlotUpgrades.FindAll(item => item.UpgradeCode == code && item.TreeId == treeId).Cast<ITownUpgrade>());
        townUpgrades.AddRange(RecruitExperienceUpgrades.FindAll(item => item.UpgradeCode == code && item.TreeId == treeId).Cast<ITownUpgrade>());
        return townUpgrades;
    }

    protected override void Reset()
    {
        RecruitSlots = BaseRecruitSlots;
        RosterSlots = BaseRosterSlots;
        CurrentRecruitMaxLevel = 0;
    }

    private void GeneratePurchaseInfo(Hero hero, Estate estate)
    {
        estate.HeroPurchases.Add(hero.RosterId, new Dictionary<string, UpgradePurchases>());
        var trees = DarkestDungeonManager.Data.UpgradeTrees.Values.ToList().
            FindAll(item => item.Id.StartsWith(hero.HeroClass.StringId));

        foreach (var tree in trees)
            estate.HeroPurchases[hero.RosterId].Add(tree.Id, new UpgradePurchases(tree.Id));
        foreach (var skill in hero.HeroClass.CampingSkills)
            estate.HeroPurchases[hero.RosterId].Add(skill.Id, new UpgradePurchases(skill.Id));

        if (hero.Weapon.UpgradeLevel > 1)
        {
            var weaponPurchases = estate.HeroPurchases[hero.RosterId][hero.ClassStringId + ".weapon"];

            for (int i = 0; i < hero.Weapon.UpgradeLevel - 1; i++)
                weaponPurchases.PurchasedUpgrades.Add(i.ToString());
        }
        if (hero.Armor.UpgradeLevel > 1)
        {
            var armorPurchases = estate.HeroPurchases[hero.RosterId][hero.ClassStringId + ".armour"];

            for (int i = 0; i < hero.Weapon.UpgradeLevel - 1; i++)
                armorPurchases.PurchasedUpgrades.Add(i.ToString());
        }

        for (int i = 0; i < hero.CurrentCombatSkills.Length; i++)
        {
            if (hero.CurrentCombatSkills[i] != null)
            {
                string treeName = hero.ClassStringId + "." + hero.CurrentCombatSkills[i].Id;
                var skillTree = DarkestDungeonManager.Data.UpgradeTrees[treeName];

                estate.HeroPurchases[hero.RosterId][treeName].PurchasedUpgrades.Add(skillTree.Upgrades[0].Code);
                if (hero.Resolve.Level > 0)
                {
                    for (int j = 1; j < skillTree.Upgrades.Count; j++)
                    {
                        if ((skillTree.Upgrades[j] as HeroUpgrade).PrerequisiteResolveLevel <= hero.Resolve.Level)
                            estate.HeroPurchases[hero.RosterId][treeName].PurchasedUpgrades.Add(skillTree.Upgrades[j].Code);
                    }
                }
            }
        }
        estate.ReskillCombatHero(hero);
        for (int i = 0; i < hero.CurrentCampingSkills.Length; i++)
            if (hero.CurrentCampingSkills[i] != null)
                estate.HeroPurchases[hero.RosterId][hero.CurrentCampingSkills[i].Id].PurchasedUpgrades.Add("0");
    }
}