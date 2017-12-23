using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public enum HeroStatus { Available = 0, RaidParty, Missing = 10, Tavern, Sanitarium, Abbey }

public class Hero : Character
{
    public HeroStatus Status { get; set; }
    public string InActivity { get; set; }
    public int MissingDuration { get; set; }

    public int RosterId { get; private set; }
    public string HeroName { get; private set; }
    public string ClassStringId { get; private set; }
    public int ClassIndexId { get; private set; }
    public Resolve Resolve { get; private set; }

    public int WeaponLevel { get; private set; }
    public int ArmorLevel { get; private set; }
    public Equipment Armor { get; private set; }
    public Equipment Weapon { get; private set; }

    public Trinket LeftTrinket { get; private set; }
    public Trinket RightTrinket { get; private set; }

    public HeroClass HeroClass { get; set; }
    public CharacterMode CurrentMode { get; set; }
    public override Trait Trait { get; protected set; }

    public float DeathResist
    {
        get
        {
            var deathResist = GetSingleAttribute(AttributeType.DeathBlow);
            return deathResist != null ? Mathf.Clamp(deathResist.ModifiedValue, 0.0f, 0.87f) : 0.5f;
        }
    }

    public override List<SkillArtInfo> SkillArtInfo { get { return HeroClass.SkillArtInfo; } }
    public override CombatSkill RiposteSkill { get { return HeroClass.RiposteSkill; } }
    public override bool AtDeathsDoor { get { return GetStatusEffect(StatusType.DeathsDoor).IsApplied; } }
    public override bool IsStressed { get { return Stress.CurrentValue >= 50; } }
    public override bool IsOverstressed { get { return Stress.CurrentValue >= 100; } }
    public override bool IsVirtued { get { return Trait != null && Trait.Type == OverstressType.Virtue; } }
    public override bool IsAfflicted { get { return Trait != null && Trait.Type == OverstressType.Affliction; } }
    public override int RenderRankOverride { get { return HeroClass.RenderingRankOverride; } }
    public override bool IsMonster { get { return false; } }
    public override string Name { get { return HeroName; } }
    public override string Class { get { return ClassStringId; } }
    public override bool InMode { get { return CurrentMode != null; } }
    public override CharacterMode Mode { get { return CurrentMode; } }
    public override CommonEffects CommonEffects { get { return HeroClass.CommonEffects; } }

    public CombatSkill[] CurrentCombatSkills { get; private set; }
    public CampingSkill[] CurrentCampingSkills { get; private set; }
    public List<CombatSkill> SelectedCombatSkills { get; private set; }
    public List<CampingSkill> SelectedCampingSkills { get; private set; }

    public ReadOnlyCollection<QuirkInfo> Quirks
    {
        get { return quirkData.AsReadOnly(); }
    }

    public ReadOnlyCollection<QuirkInfo> Diseases
    {
        get { return quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease).AsReadOnly(); }
    }

    public ReadOnlyCollection<QuirkInfo> NegativeQuirks
    {
        get
        {
            return quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease).AsReadOnly();
        }
    }

    public ReadOnlyCollection<QuirkInfo> PositiveQuirks
    {
        get { return quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive).AsReadOnly(); }
    }

    public ReadOnlyCollection<QuirkInfo> LockedPositiveQuirks
    {
        get { return quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive && quirkInfo.IsLocked).AsReadOnly(); }
    }

    private readonly List<QuirkInfo> quirkData = new List<QuirkInfo>();

    #region Constructors

    public Hero(int heroIndex, PhotonPlayer player)
        : base(DarkestDungeonManager.Data.HeroClasses[(string)player.CustomProperties["HC" + heroIndex]])
    {
        RandomSolver.SetRandomSeed((int)player.CustomProperties["HS" + heroIndex]);

        InitializeHeroInfo(0, (string)player.CustomProperties["HN" + heroIndex],
            (string)player.CustomProperties["HC" + heroIndex], 0, 30);

        InitializeEquipment(1, 1);
        InitializeQuirks();

        CurrentCombatSkills = new CombatSkill[HeroClass.CombatSkills.Count];
        for (int i = 0; i < CurrentCombatSkills.Length; i++)
            CurrentCombatSkills[i] = HeroClass.CombatSkills[i];

        var playerSkillFlags = (PlayerSkillFlags)player.CustomProperties["HF" + heroIndex];
        SelectedCombatSkills = new List<CombatSkill>();
        for (int i = 0; i < CurrentCombatSkills.Length; i++)
        {
            if ((playerSkillFlags & (PlayerSkillFlags)Mathf.Pow(2, i + 1)) != PlayerSkillFlags.Empty)
                SelectedCombatSkills.Add(CurrentCombatSkills[i]);
        }

        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];
        SelectedCampingSkills = new List<CampingSkill>();
    }

    public Hero(string classId, string generatedName)
        : base(DarkestDungeonManager.Data.HeroClasses[classId])
    {
        InitializeHeroInfo(0, generatedName, classId, 0, 30);
        InitializeEquipment(1, 1);
        InitializeQuirks();
        InitializeCombatSkills(HeroClass.CombatSkills.Count);

        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];
        SelectedCampingSkills = new List<CampingSkill>();
    }

    public Hero(int rosterId, string classId, string generatedName)
        : base(DarkestDungeonManager.Data.HeroClasses[classId])
    {
        InitializeHeroInfo(rosterId, generatedName, classId, 0, 10);
        InitializeEquipment(1, 1);
        InitializeQuirks();
        InitializeCombatSkills();
        InitializeCampingSkills();
    }

    public Hero(int rosterId, string classId, string generatedName, RecruitUpgrade expUpgrade)
        : base(DarkestDungeonManager.Data.HeroClasses[classId], expUpgrade.Level)
    {
        InitializeHeroInfo(rosterId, generatedName, classId, expUpgrade.Level, 10);

        int equipLevel = DarkestDungeonManager.Data.UpgradeTrees[classId + ".weapon"].Upgrades.FindAll(upgrade =>
            upgrade is HeroUpgrade && ((HeroUpgrade)upgrade).PrerequisiteResolveLevel <= expUpgrade.Level).Count + 1;

        InitializeEquipment(equipLevel, equipLevel);
        InitializeQuirks(expUpgrade);
        InitializeCombatSkills(expUpgrade.ExtraCombatSkills);
        InitializeCampingSkills(expUpgrade.ExtraCampingSkills);
    }

    public Hero(int rosterId, string classId, DeathRecord deathRecord)
        : base(DarkestDungeonManager.Data.HeroClasses[classId], deathRecord.ResolveLevel)
    {
        InitializeHeroInfo(rosterId, deathRecord.HeroName, classId, deathRecord.ResolveLevel, 10);

        int equipLevel = DarkestDungeonManager.Data.UpgradeTrees[classId + ".weapon"].Upgrades.FindAll(upgrade =>
            upgrade is HeroUpgrade && ((HeroUpgrade) upgrade).PrerequisiteResolveLevel <= deathRecord.ResolveLevel).Count + 1;

        InitializeEquipment(equipLevel, equipLevel);
        InitializeQuirks();
        InitializeCombatSkills();
        InitializeCampingSkills();
    }

    public Hero(Estate estate, SaveHeroData saveHeroData) : base(saveHeroData)
    {
        InitializeHeroInfo(saveHeroData.RosterId, saveHeroData.Name, saveHeroData.HeroClass,
            saveHeroData.ResolveLevel, saveHeroData.StressLevel);

        Status = saveHeroData.Status;
        InActivity = saveHeroData.InActivity;
        MissingDuration = saveHeroData.MissingDuration;
        Resolve.CurrentXP = saveHeroData.ResolveXP;

        if (!estate.PickRosterId(RosterId))
            Debug.LogError("Missing id " + RosterId + " in estate from hero " + HeroName);
        
        InitializeEquipment(estate.GetUpgradedWeaponLevel(RosterId, HeroClass.StringId),
            estate.GetUpgradedArmorLevel(RosterId, HeroClass.StringId));

        if (saveHeroData.LeftTrinketId != "")
        {
            Trinket trinket = (Trinket)DarkestDungeonManager.Data.Items["trinket"][saveHeroData.LeftTrinketId];
            Equip(trinket, TrinketSlot.Left);
        }
        if (saveHeroData.RightTrinketId != "")
        {
            Trinket trinket = (Trinket)DarkestDungeonManager.Data.Items["trinket"][saveHeroData.RightTrinketId];
            Equip(trinket, TrinketSlot.Right);
        }

        foreach (var quirkEntry in saveHeroData.Quirks)
        {
            quirkData.Add(quirkEntry);
            ApplyQuirk(quirkEntry.Quirk);
        }

        CurrentCombatSkills = new CombatSkill[7];
        SelectedCombatSkills = new List<CombatSkill>();

        for (int i = 0; i < 7; i++)
            CurrentCombatSkills[i] = HeroClass.CombatSkillVariants.Find(skill => skill.Id == HeroClass.CombatSkills[i].Id
                && skill.Level == estate.GetUpgradedSkillLevel(RosterId, HeroClass.StringId, HeroClass.CombatSkills[i].Id));

        foreach (int skillIndex in saveHeroData.SelectedCombatSkillIndexes)
            if (CurrentCombatSkills[skillIndex] != null)
                SelectedCombatSkills.Add(CurrentCombatSkills[skillIndex]);

        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];
        SelectedCampingSkills = new List<CampingSkill>();

        for (int i = 0; i < CurrentCampingSkills.Length; i++)
            if (estate.GetUpgradedCampingStatus(RosterId, HeroClass.CampingSkills[i].Id))
                CurrentCampingSkills[i] = HeroClass.CampingSkills[i];

        foreach (int skillIndex in saveHeroData.SelectedCampingSkillIndexes)
            if (CurrentCampingSkills[skillIndex] != null)
                SelectedCampingSkills.Add(CurrentCampingSkills[skillIndex]);

        if (saveHeroData.Trait != "")
        {
            var heroTrait = DarkestDungeonManager.Data.Traits.Find(trait => trait.Id == saveHeroData.Trait);
            if (heroTrait != null)
                ApplyTrait(heroTrait);
        }

        GetPairedAttribute(AttributeType.HitPoints).CurrentValue = saveHeroData.CurrentHp;
    }

    private void InitializeHeroInfo(int rosterId, string heroName, string classId, int resolveLevel, float stress)
    {
        RosterId = rosterId;
        HeroName = heroName;
        ClassStringId = classId;
        Status = HeroStatus.Available;
        Resolve = new Resolve(resolveLevel, 0);
        HeroClass = DarkestDungeonManager.Data.HeroClasses[classId];
        ClassIndexId = HeroClass.IndexId;
        AddPairedAttribute(AttributeType.Stress, new PairedAttribute(stress, 200, true));
    }

    private void InitializeEquipment(int weaponLevel, int armorLevel)
    {
        Equipment weapon = HeroClass.Weapons.Find(wep => wep.UpgradeLevel == weaponLevel);
        Equip(weapon, HeroEquipmentSlot.Weapon);
        Equipment armor = HeroClass.Armors.Find(arm => arm.UpgradeLevel == armorLevel);
        Equip(armor, HeroEquipmentSlot.Armor);
    }

    private void InitializeQuirks(RecruitUpgrade expUpgrade = null)
    {
        int positiveQuirkNumber = RandomSolver.Next(HeroClass.Generation.NumberOfPositiveQuirksMin,
            HeroClass.Generation.NumberOfPositiveQuirksMax + 1);

        int negativeQuirkNumber = RandomSolver.Next(HeroClass.Generation.NumberOfNegativeQuirksMin,
            HeroClass.Generation.NumberOfNegativeQuirksMax + 1);

        if (expUpgrade != null)
        {
            positiveQuirkNumber += expUpgrade.ExtraPositiveQuirks;
            negativeQuirkNumber += expUpgrade.ExtraNegativeQuirks;
        }

        for (int i = 0; i < positiveQuirkNumber; i++)
        {
            var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsPositive &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);
            var availableQuirk = availableQuirks[RandomSolver.Next(availableQuirks.Count)];
            quirkData.Add(new QuirkInfo(availableQuirk, false, 1, false));
            ApplyQuirk(availableQuirk);
        }

        for (int i = 0; i < negativeQuirkNumber; i++)
        {
            var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => !newQuirk.IsPositive &&
                !newQuirk.IsDisease && quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);
            var availableQuirk = availableQuirks[RandomSolver.Next(availableQuirks.Count)];
            quirkData.Add(new QuirkInfo(availableQuirk, false, 1, false));
            ApplyQuirk(availableQuirk);
        }
    }

    private void InitializeCombatSkills(int bonusSkills = 0)
    {
        var availableSkills = new List<CombatSkill>(HeroClass.CombatSkills);
        int skillsRequired = Mathf.Clamp(HeroClass.Generation.NumberOfRandomCombatSkills + bonusSkills, 0, HeroClass.CombatSkills.Count);

        CurrentCombatSkills = new CombatSkill[HeroClass.CombatSkills.Count];
        foreach (var guaranteedSkill in availableSkills.FindAll(skill => skill.IsGenerationGuaranteed))
        {
            CurrentCombatSkills[HeroClass.CombatSkills.IndexOf(guaranteedSkill)] = guaranteedSkill;
            availableSkills.Remove(guaranteedSkill);
            skillsRequired--;
        }

        for (int i = skillsRequired; i > 0; i--)
        {
            int generatedIndex = RandomSolver.Next(availableSkills.Count);
            CurrentCombatSkills[HeroClass.CombatSkills.IndexOf(availableSkills[generatedIndex])] = availableSkills[generatedIndex];
            availableSkills.RemoveAt(generatedIndex);
        }

        SelectedCombatSkills = new List<CombatSkill>();
        var selectionList = new List<CombatSkill>(CurrentCombatSkills);
        selectionList.RemoveAll(skill => skill == null);
        int selectedSkills = Mathf.Clamp(HeroClass.NumberOfSelectedCombatSkills, 0, selectionList.Count);
        for (int i = 0; i < selectedSkills; i++)
        {
            int selectedItem = RandomSolver.Next(selectionList.Count);
            SelectedCombatSkills.Add(selectionList[selectedItem]);
            selectionList.RemoveAt(selectedItem);
        }
    }

    private void InitializeCampingSkills(int bonusSkills = 0)
    {
        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];

        var availableGeneralSkills = HeroClass.CampingSkills.FindAll(skill => skill.Classes.Count > 4);
        int generalSkillsRequired = HeroClass.Generation.NumberOfSharedCampingSkills;
        foreach (var skill in availableGeneralSkills.OrderBy(x => RandomSolver.NextDouble())
            .Take(Mathf.Min(generalSkillsRequired, availableGeneralSkills.Count)))
        {
            int skillIndex = HeroClass.CampingSkills.IndexOf(skill);
            CurrentCampingSkills[skillIndex] = skill;
        }
        var availableSpecificSkills = HeroClass.CampingSkills.FindAll(skill => skill.Classes.Count <= 4);
        int specificSkillsRequired = HeroClass.Generation.NumberOfSpecificCampingSkills + bonusSkills;

        foreach (var skill in availableSpecificSkills.OrderBy(x => RandomSolver.NextDouble())
            .Take(Mathf.Min(specificSkillsRequired, availableSpecificSkills.Count)))
        {
            int skillIndex = HeroClass.CampingSkills.IndexOf(skill);
            CurrentCampingSkills[skillIndex] = skill;
        }

        var availableGeneratedSkills = new List<CampingSkill>(CurrentCampingSkills);
        availableGeneratedSkills.RemoveAll(skill => skill == null);

        SelectedCampingSkills = availableGeneratedSkills.OrderBy(x => RandomSolver.NextDouble())
            .Take(Mathf.Min(4, availableGeneratedSkills.Count)).ToList();
    }

    #endregion

    public void ApplyDeathDoor()
    {
        var deathDoorStatus = GetStatusEffect(StatusType.DeathsDoor) as DeathsDoorStatusEffect;
        if (deathDoorStatus.IsApplied)
            return;
        else
            deathDoorStatus.AtDeathsDoor = true;

        RevertMortality();

        for(int i = 0; i < HeroClass.DeathDoor.Buffs.Count; i++)
            AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs[HeroClass.DeathDoor.Buffs[i]],
                BuffDurationType.Permanent, BuffSourceType.DeathsDoor));
    }

    public void RevertDeathsDoor()
    {
        if (GetStatusEffect(StatusType.DeathsDoor).IsApplied)
        {
            GetStatusEffect(StatusType.DeathsDoor).ResetStatus();
            foreach (var removedBuff in BuffInfo.FindAll((buffEntry => buffEntry.SourceType == BuffSourceType.DeathsDoor)))
                RemoveBuff(removedBuff);

            ApplyMortality();
        }
    }

    public void ApplyMortality()
    {
        var mortalityStatus = GetStatusEffect(StatusType.DeathRecovery) as DeathRecoveryStatusEffect;
        if (mortalityStatus.IsApplied)
            return;
        else
            mortalityStatus.AtDeathRecovery = true;

        for (int i = 0; i < HeroClass.DeathDoor.Buffs.Count; i++)
            AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs[HeroClass.DeathDoor.RecoveryBuffs[i]],
                BuffDurationType.Permanent, BuffSourceType.Mortality));
    }

    public void RevertMortality()
    {
        if (GetStatusEffect(StatusType.DeathRecovery).IsApplied)
        {
            GetStatusEffect(StatusType.DeathRecovery).ResetStatus();
            foreach (var removedBuff in BuffInfo.FindAll((buffEntry => buffEntry.SourceType == BuffSourceType.Mortality)))
                RemoveBuff(removedBuff);
        }
    }

    public void ApplyTrait(Trait trait)
    {
        if (Trait != null)
            RevertTrait();
        Trait = trait;
        for (int i = 0; i < trait.Buffs.Count; i++)
            AddBuff(new BuffInfo(trait.Buffs[i], BuffDurationType.Permanent, BuffSourceType.Trait));
    }

    public void RevertTrait()
    {
        foreach (var removedBuff in BuffInfo.FindAll((buffEntry => buffEntry.SourceType == BuffSourceType.Trait)))
            RemoveBuff(removedBuff);
    }

    #region Quirk Helpers

    public bool AddQuirk(Quirk newQuirk)
    {
        if (quirkData.Any(item => item.Quirk == newQuirk))
            return false;
        if (quirkData.Any(quirkInfo => quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)))
            return false;

        var replacableQuirks = newQuirk.IsDisease ? quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease) :
            newQuirk.IsPositive ? quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive) :
            quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease);

        return AddOrReplaceQuirk(newQuirk, replacableQuirks) != null;
    }

    public bool LockQuirk(string quirkId)
    {
        var quirk = quirkData.Find(quirkInfo => quirkInfo.Quirk.Id == quirkId);
        if (quirk != null)
        {
            quirk.IsLocked = true;
            quirk.IsNew = false;
            quirk.IsReplaced = false;
            quirkData.Sort((x,y) => x.IsLocked ? y.IsLocked ? 0 : -1 : y.IsLocked ? 1 : 0);
            return true;
        }
        return false;
    }

    public Quirk AddPositiveQuirk()
    {
        var replacableQuirks = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive);
        var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsPositive &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.All(quirkInfo => quirkInfo.Quirk != newQuirk));

        var addedQuirk = availableQuirks[RandomSolver.Next(availableQuirks.Count)];
        return AddOrReplaceQuirk(addedQuirk, replacableQuirks);
    }

    public Quirk AddNegativeQuirk()
    {
        var replacableQuirks = quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease);
        var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk =>
                !newQuirk.IsPositive && !newQuirk.IsDisease && quirkData.TrueForAll(quirkInfo =>
                !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.All(quirkInfo => quirkInfo.Quirk != newQuirk));

        var addedQuirk = availableQuirks[RandomSolver.Next(availableQuirks.Count)];
        return AddOrReplaceQuirk(addedQuirk, replacableQuirks);
    }

    public Quirk AddRandomDisease()
    {
        var replacableQuirks = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease);
        var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsDisease &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.All(quirkInfo => quirkInfo.Quirk != newQuirk));

        var addedQuirk = availableQuirks[RandomSolver.Next(availableQuirks.Count)];
        return AddOrReplaceQuirk(addedQuirk, replacableQuirks);
    }

    public Quirk RemoveQuirk(string quirkId)
    {
        var quirk = quirkData.Find(quirkInfo => quirkInfo.Quirk.Id == quirkId);
        if(quirk != null)
        {
            RevertQuirk(quirk.Quirk);
            quirkData.Remove(quirk);
            return quirk.Quirk;
        }
        return null;
    }

    public Quirk RemovePositiveQuirk()
    {
        var positiveQuirks = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive && !quirkInfo.IsLocked);
        if(positiveQuirks.Count > 0)
        {
            var removedQuirk = positiveQuirks[RandomSolver.Next(positiveQuirks.Count)];
            RevertQuirk(removedQuirk.Quirk);
            quirkData.Remove(removedQuirk);
            return removedQuirk.Quirk;
        }
        return null;
    }

    public Quirk RemoveNegativeQuirk()
    {
        var negativeQuirks = quirkData.FindAll(quirkInfo => 
            !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease && !quirkInfo.IsLocked);
        if (negativeQuirks.Count == 0)
            negativeQuirks = quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease);

        if (negativeQuirks.Count > 0)
        {
            var removedQuirk = negativeQuirks[RandomSolver.Next(negativeQuirks.Count)];
            RevertQuirk(removedQuirk.Quirk);
            quirkData.Remove(removedQuirk);
            return removedQuirk.Quirk;
        }
        return null;
    }

    public Quirk RemoveDiseaseQuirk()
    {
        var diseases = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease);

        if (diseases.Count > 0)
        {
            var removedDisease = diseases[RandomSolver.Next(diseases.Count)];
            RevertQuirk(removedDisease.Quirk);
            quirkData.Remove(removedDisease);
            return removedDisease.Quirk;
        }
        return null;
    }

    public QuirkInfo GetQuirkInfo(string quirkId)
    {
        return quirkData.Find(quirkInfo => quirkInfo.Quirk.Id == quirkId);
    }

    public List<QuirkInfo> RemoveDiseases()
    {
        var diseases = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease);
        for (int i = 0; i < diseases.Count; i++)
        {
            RevertQuirk(diseases[i].Quirk);
            quirkData.Remove(diseases[i]);
        }
        return diseases;
    }

    private Quirk AddOrReplaceQuirk(Quirk addedQuirk, List<QuirkInfo> replacableQuirks)
    {
        if (replacableQuirks.Count < (addedQuirk.IsDisease ? 3 : 5))
        {
            quirkData.Add(new QuirkInfo(addedQuirk, false, 1, true));
            ApplyQuirk(addedQuirk);
        }
        else
        {
            var replacements = replacableQuirks.FindAll(quirkInfo => !quirkInfo.IsLocked);
            if (replacements.Count > 0)
            {
                int replaceIndex = RandomSolver.Next(replacements.Count);
                RevertQuirk(replacements[replaceIndex].Quirk);
                replacements[replaceIndex].ReplaceBy(addedQuirk);
                ApplyQuirk(addedQuirk);
            }
            else
                return null;
        }
        return addedQuirk;
    }

    private void ApplyQuirk(Quirk quirk)
    {
        for (int i = 0; i < quirk.Buffs.Count; i++)
            AddBuff(new BuffInfo(quirk.Buffs[i], BuffDurationType.Permanent, BuffSourceType.Quirk));
    }

    private void RevertQuirk(Quirk quirk)
    {
        for (int i = 0; i < quirk.Buffs.Count; i++)
            RemoveSourceBuff(quirk.Buffs[i], BuffSourceType.Quirk);
    }

    #endregion

    public void Equip(Equipment equipment, HeroEquipmentSlot slot)
    {
        switch(slot)
        {
            case HeroEquipmentSlot.Weapon:
                if (Weapon != null)
                    Unequip(slot);

                equipment.ApplyModifiers(this);
                WeaponLevel = equipment.UpgradeLevel;
                Weapon = equipment;
                break;
            case HeroEquipmentSlot.Armor:
                if (Armor != null)
                    Unequip(slot);
                equipment.ApplyModifiers(this);
                ArmorLevel = equipment.UpgradeLevel;
                Armor = equipment;
                break;
        }
    }

    public void Equip(Trinket trinket, TrinketSlot slot)
    {
        switch(slot)
        {
            case TrinketSlot.Left:
                if (LeftTrinket != null)
                    Unequip(slot);
                for (int i = 0; i < trinket.Buffs.Count; i++)
                    AddBuff(new BuffInfo(trinket.Buffs[i], BuffDurationType.Permanent, BuffSourceType.Trinket));
                LeftTrinket = trinket;
                break;
            case TrinketSlot.Right:
                if (RightTrinket != null)
                    Unequip(slot);
                for (int i = 0; i < trinket.Buffs.Count; i++)
                    AddBuff(new BuffInfo(trinket.Buffs[i], BuffDurationType.Permanent, BuffSourceType.Trinket));
                RightTrinket = trinket;
                break;
        }
    }

    public void Unequip(HeroEquipmentSlot slot)
    {
        switch (slot)
        {
            case HeroEquipmentSlot.Weapon:
                Weapon.RevertModifiers(this);
                Weapon = null;
                break;
            case HeroEquipmentSlot.Armor:
                Armor.RevertModifiers(this);
                Armor = null;
                break;
        }
    }

    public void Unequip(TrinketSlot slot)
    {
        switch(slot)
        {
            case TrinketSlot.Left:
                if(LeftTrinket != null)
                {
                    for (int i = 0; i < LeftTrinket.Buffs.Count; i++)
                        RemoveSourceBuff(LeftTrinket.Buffs[i], BuffSourceType.Trinket);
                    LeftTrinket = null;
                }
                break;
            case TrinketSlot.Right:
                if(RightTrinket != null)
                {
                    for (int i = 0; i < RightTrinket.Buffs.Count; i++)
                        RemoveSourceBuff(RightTrinket.Buffs[i], BuffSourceType.Trinket);
                    RightTrinket = null;
                }
                break;
        }
    }

    public override int Heal(float healAmount, bool includeModifier)
    {
        RevertDeathsDoor();

        return base.Heal(healAmount, includeModifier);
    }

    public void TownReset()
    {
        foreach (var buff in BuffInfo.FindAll(info => !(info.SourceType == BuffSourceType.Quirk ||
             info.SourceType == BuffSourceType.Trinket || info.SourceType == BuffSourceType.Trait)))
        {
            RemoveBuff(buff);
        }
        foreach (var statusEntry in StatusEffects)
            statusEntry.Value.ResetStatus();

        Status = HeroStatus.Available;
        Heal(MaxHealth, false);

        if (Trait != null && Trait.Type == OverstressType.Virtue)
            RevertTrait();
    }

    public void UpdateResolve()
    {
        base.UpdateResolve(Resolve.Level, HeroClass);
    }

    public void UpdateSaveData(SaveHeroData saveHeroData)
    {
        saveHeroData.Status = Status;
        saveHeroData.InActivity = InActivity;
        saveHeroData.Trait = Trait == null ? "" : Trait.Id;
        saveHeroData.MissingDuration = MissingDuration;

        saveHeroData.RosterId = RosterId;
        saveHeroData.Name = HeroName;
        saveHeroData.HeroClass = HeroClass.StringId;
        saveHeroData.ResolveLevel = Resolve.Level;
        saveHeroData.ResolveXP = Resolve.CurrentXP;
        saveHeroData.CurrentHp = CurrentHealth;
        saveHeroData.StressLevel = Stress.CurrentValue;
        saveHeroData.WeaponLevel = Weapon.UpgradeLevel;
        saveHeroData.ArmorLevel = Armor.UpgradeLevel;
        saveHeroData.LeftTrinketId = LeftTrinket != null ? LeftTrinket.Id : "";
        saveHeroData.RightTrinketId = RightTrinket != null ? RightTrinket.Id : "";

        saveHeroData.Quirks = quirkData;
        saveHeroData.Buffs = BuffInfo;

        saveHeroData.SelectedCombatSkillIndexes.Clear();
        saveHeroData.SelectedCampingSkillIndexes.Clear();
        for(int i = 0; i < CurrentCombatSkills.Length; i++)
        {
            if (CurrentCombatSkills[i] != null && SelectedCombatSkills.Contains(CurrentCombatSkills[i]))
                saveHeroData.SelectedCombatSkillIndexes.Add(i);
        }
        for (int i = 0; i < CurrentCampingSkills.Length; i++)
        {
            if (CurrentCampingSkills[i] != null && SelectedCampingSkills.Contains(CurrentCampingSkills[i]))
                saveHeroData.SelectedCampingSkillIndexes.Add(i);
        }
    }

    public override void UpdateSaveData(FormationUnitSaveData saveUnitData)
    {
        saveUnitData.IsHero = true;
        saveUnitData.RosterId = RosterId;
        saveUnitData.Class = Class;
        saveUnitData.Name = Name;
        saveUnitData.CurrentHp = CurrentHealth;
        saveUnitData.Buffs = BuffInfo;
        saveUnitData.Statuses = StatusEffects;
    }
}