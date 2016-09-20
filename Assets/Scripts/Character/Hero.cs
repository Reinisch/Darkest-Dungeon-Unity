using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public enum HeroStatus { Available, Tavern, Sanitarium, Abbey, RaidParty, Missing }

public class Hero : Character
{
    public HeroStatus Status { get; set; }
    public string InActivity { get; set; }
    public int MissingDuration { get; set; }

    public int RosterId { get; set; }
    public string HeroName { get; set; }
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
            if (deathResist != null)
                return Mathf.Clamp(deathResist.ModifiedValue, 0.5f, 0.87f);
            else
                return 0.5f;
        }
    }

    public override List<SkillArtInfo> SkillArtInfo
    {
        get
        {
            return HeroClass.SkillArtInfo;
        }
    }
    public override CombatSkill RiposteSkill
    {
        get
        {
            return HeroClass.RiposteSkill;
        }
    }

    public override bool AtDeathsDoor
    {
        get
        {
            return GetStatusEffect(StatusType.DeathsDoor).IsApplied;
        }
    }
    public override bool IsStressed
    {
        get
        {
            return Stress.CurrentValue >= 50;
        }
    }
    public override bool IsOverstressed
    {
        get
        {
            return Stress.CurrentValue >= 100;
        }
    }
    public override bool IsVirtued
    {
        get
        {
            return Trait != null && Trait.Type == OverstressType.Virtue;
        }
    }
    public override bool IsAfflicted
    {
        get
        {
            return Trait != null && Trait.Type == OverstressType.Affliction;
        }
    }

    public override int RenderRankOverride
    {
        get
        {
            return HeroClass.RenderingRankOverride;
        }
    }
    public override bool IsMonster
    {
        get
        {
            return false;
        }
    }
    public override string Name
    {
        get
        {
            return HeroName;
        }
    }
    public override string Class
    {
        get
        {
            return ClassStringId;
        }
    }
    public override int Size
    {
        get
        {
            return base.Size;
        }
    }
    public override bool InMode
    {
        get
        {
            return CurrentMode != null;
        }
    }

    public override CharacterMode Mode
    {
        get
        {
            return CurrentMode;
        }
    }
    public override CommonEffects CommonEffects
    {
        get
        {
            return HeroClass.CommonEffects;
        }
    }

    public CombatSkill[] CurrentCombatSkills { get; set; }
    public CampingSkill[] CurrentCampingSkills { get; set; }
    public List<CombatSkill> SelectedCombatSkills { get; set; }
    public List<CampingSkill> SelectedCampingSkills { get; set; }

    public ReadOnlyCollection<QuirkInfo> Quirks
    {
        get
        {
            return quirkData.AsReadOnly();
        }
    }
    public ReadOnlyCollection<QuirkInfo> Diseases
    {
        get
        {
            return quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease).AsReadOnly();
        }
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
        get
        {
            return quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive).AsReadOnly();
        }
    }
    public ReadOnlyCollection<QuirkInfo> LockedPositiveQuirks
    {
        get
        {
            return quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive && quirkInfo.IsLocked).AsReadOnly();
        }
    }

    private List<QuirkInfo> quirkData;

    void ApplyQuirk(Quirk quirk)
    {
        for (int i = 0; i < quirk.Buffs.Count; i++)
            AddBuff(new BuffInfo(quirk.Buffs[i], BuffDurationType.Permanent, BuffSourceType.Quirk));
    }
    void RevertQuirk(Quirk quirk)
    {
        for (int i = 0; i < quirk.Buffs.Count; i++)
            RemoveSourceBuff(quirk.Buffs[i], BuffSourceType.Quirk);
    }
    
    public void ApplyDeathDoor()
    {
        var deathDoorStatus = GetStatusEffect(StatusType.DeathsDoor) as DeathsDoorStatusEffect;
        if (deathDoorStatus.IsApplied)
            return;
        else
            deathDoorStatus.AtDeathsDoor = true;

        RevertMortality();

        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["deathsdoorACCDebuff"], BuffDurationType.Permanent, BuffSourceType.DeathsDoor));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["deathsdoorDMGLowDebuff"], BuffDurationType.Permanent, BuffSourceType.DeathsDoor));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["deathsdoorDMGHighDebuff"], BuffDurationType.Permanent, BuffSourceType.DeathsDoor));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["deathsdoorSPDDebuff"], BuffDurationType.Permanent, BuffSourceType.DeathsDoor));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["deathsdoorSRDebuff"], BuffDurationType.Permanent, BuffSourceType.DeathsDoor));
    }
    public void RevertDeathsDoor()
    {
        if (GetStatusEffect(StatusType.DeathsDoor).IsApplied)
        {
            GetStatusEffect(StatusType.DeathsDoor).ResetStatus();
            foreach (var removedBuff in buffInfo.FindAll((buffEntry => buffEntry.SourceType == BuffSourceType.DeathsDoor)))
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

        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["mortalityACCDebuff"], BuffDurationType.Camp, BuffSourceType.Mortality));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["mortalityDMGLowDebuff"], BuffDurationType.Camp, BuffSourceType.Mortality));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["mortalityDMGHighDebuff"], BuffDurationType.Camp, BuffSourceType.Mortality));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["mortalitySPDDebuff"], BuffDurationType.Camp, BuffSourceType.Mortality));
        AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs["mortalitySRDebuff"], BuffDurationType.Camp, BuffSourceType.Mortality));
    }
    public void RevertMortality()
    {
        if (GetStatusEffect(StatusType.DeathRecovery).IsApplied)
        {
            GetStatusEffect(StatusType.DeathRecovery).ResetStatus();
            foreach (var removedBuff in buffInfo.FindAll((buffEntry => buffEntry.SourceType == BuffSourceType.Mortality)))
                RemoveBuff(removedBuff);
        }
    }
    public void TownReset()
    {
        foreach(var buff in buffInfo.FindAll(info => !(info.SourceType == BuffSourceType.Quirk || 
            info.SourceType == BuffSourceType.Trinket|| info.SourceType == BuffSourceType.Trait)))
        {
            RemoveBuff(buff);
        }
        foreach (var statusEntry in statusEffects)
            statusEntry.Value.ResetStatus();

        Status = HeroStatus.Available;
        Health.ValueRatio = 1;

        if (Trait != null && Trait.Type == OverstressType.Virtue)
            RevertTrait();
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
        foreach (var removedBuff in buffInfo.FindAll((buffEntry => buffEntry.SourceType == BuffSourceType.Trait)))
            RemoveBuff(removedBuff);
    }

    public bool AddQuirk(Quirk newQuirk)
    {
        if (quirkData.Find(item => item.Quirk == newQuirk) != null)
            return false;
        for (int i = 0; i < quirkData.Count; i++)
        {
            if (quirkData[i].Quirk.IncompatibleQuirks.Contains(newQuirk.Id))
                return false;
        }
        if (newQuirk.IsDisease)
        {
            var diseases = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease);
            if (diseases.Count < 3)
            {
                quirkData.Add(new QuirkInfo(newQuirk, false, 1, true));
                ApplyQuirk(newQuirk);
            }
            else
            {
                int replaceIndex = Random.Range(0, diseases.Count);
                RevertQuirk(diseases[replaceIndex].Quirk);
                diseases[replaceIndex].ReplaceBy(newQuirk);
                ApplyQuirk(newQuirk);
            }
        }
        else
        {
            var quirkGroup = newQuirk.IsPositive ? quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive) :
                quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease);

            if (quirkGroup.Count < 5)
            {
                quirkData.Add(new QuirkInfo(newQuirk, false, 1, true));
                ApplyQuirk(newQuirk);
            }
            else
            {
                var replacements = quirkGroup.FindAll(quirkInfo => !quirkInfo.IsLocked);
                if (replacements.Count > 0)
                {
                    int replaceIndex = Random.Range(0, replacements.Count);
                    RevertQuirk(replacements[replaceIndex].Quirk);
                    replacements[replaceIndex].ReplaceBy(newQuirk);
                    ApplyQuirk(newQuirk);
                }
            }
        }
        return true;
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
        var quirkGroup = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsPositive);
        var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsPositive &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);

        var addedQuirk = availableQuirks[Random.Range(0, availableQuirks.Count)];
        if (quirkGroup.Count < 5)
        {
            quirkData.Add(new QuirkInfo(addedQuirk, false, 1, true));
            ApplyQuirk(addedQuirk);
        }
        else
        {
            var replacements = quirkGroup.FindAll(quirkInfo => !quirkInfo.IsLocked);
            if (replacements.Count > 0)
            {
                int replaceIndex = Random.Range(0, replacements.Count);
                RevertQuirk(replacements[replaceIndex].Quirk);
                replacements[replaceIndex].ReplaceBy(addedQuirk);
                ApplyQuirk(addedQuirk);
            }
        }
        return addedQuirk;
    }
    public Quirk AddNegativeQuirk()
    {
        var quirkGroup = quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease);
        var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => !newQuirk.IsPositive && !newQuirk.IsDisease &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);

        var addedQuirk = availableQuirks[Random.Range(0, availableQuirks.Count)];
        if (quirkGroup.Count < 5)
        {
            quirkData.Add(new QuirkInfo(addedQuirk, false, 1, true));
            ApplyQuirk(addedQuirk);
        }
        else
        {
            var replacements = quirkGroup.FindAll(quirkInfo => !quirkInfo.IsLocked);
            if (replacements.Count > 0)
            {
                int replaceIndex = Random.Range(0, replacements.Count);
                RevertQuirk(replacements[replaceIndex].Quirk);
                replacements[replaceIndex].ReplaceBy(addedQuirk);
                ApplyQuirk(addedQuirk);
            }
        }
        return addedQuirk;
    }
    public Quirk AddRandomDisease()
    {
        var availableDiseases = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsDisease &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);

        var rolledDisease = availableDiseases[Random.Range(0, availableDiseases.Count)];

        var diseases = quirkData.FindAll(quirkInfo => quirkInfo.Quirk.IsDisease);
        if (diseases.Count < 3)
        {
            quirkData.Add(new QuirkInfo(rolledDisease, false, 1, true));
            ApplyQuirk(rolledDisease);
        }
        else
        {
            int replaceIndex = Random.Range(0, diseases.Count);
            RevertQuirk(diseases[replaceIndex].Quirk);
            diseases[replaceIndex].ReplaceBy(rolledDisease);
            ApplyQuirk(rolledDisease);
        }
        return rolledDisease;
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
            var removedQuirk = positiveQuirks[Random.Range(0, positiveQuirks.Count)];
            RevertQuirk(removedQuirk.Quirk);
            quirkData.Remove(removedQuirk);
            return removedQuirk.Quirk;
        }
        return null;
    }
    public Quirk RemoveNegativeQuirk()
    {
        var negativeQuirks = quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease && !quirkInfo.IsLocked);
        if (negativeQuirks.Count == 0)
            negativeQuirks = quirkData.FindAll(quirkInfo => !quirkInfo.Quirk.IsPositive && !quirkInfo.Quirk.IsDisease);

        if (negativeQuirks.Count > 0)
        {
            var removedQuirk = negativeQuirks[Random.Range(0, negativeQuirks.Count)];
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
            var removedDisease = diseases[Random.Range(0, diseases.Count)];
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

    public Hero(int rosterId, string classId, string generatedName)
        :base(DarkestDungeonManager.Data.HeroClasses[classId])
    {
        RosterId = rosterId;
        HeroName = generatedName;
        ClassStringId = classId;
        Resolve = new Resolve(0, 0);
        HeroClass = DarkestDungeonManager.Data.HeroClasses[classId];
        ClassIndexId = HeroClass.IndexId;
        AddPairedAttribute(AttributeType.Stress, new PairedAttribute(10, 200, true, AttributeCategory.CombatStat));

        #region Equipment Generation
        Equipment weapon = HeroClass.Weapons.Find(wep => wep.UpgradeLevel == 1);
        Equip(weapon, HeroEquipmentSlot.Weapon);
        Equipment armor = HeroClass.Armors.Find(arm => arm.UpgradeLevel == 1);
        Equip(armor, HeroEquipmentSlot.Armor);
        #endregion

        #region Quirk Generation
        quirkData = new List<QuirkInfo>();
        int positiveQuirkNumber = Random.Range(HeroClass.Generation.NumberOfPositiveQuirksMin, HeroClass.Generation.NumberOfPositiveQuirksMax + 1);
        for (int i = 0; i < positiveQuirkNumber ; i++ )
        {
            var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsPositive &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);
            var availableQuirk = availableQuirks[Random.Range(0, availableQuirks.Count)];
            quirkData.Add(new QuirkInfo(availableQuirk, false, 1, false));
            ApplyQuirk(availableQuirk);
        }
        int negativeQuirkNumber = Random.Range(HeroClass.Generation.NumberOfNegativeQuirksMin, HeroClass.Generation.NumberOfNegativeQuirksMax + 1);
        for (int i = 0; i < negativeQuirkNumber; i++)
        {
            var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => !newQuirk.IsPositive &&
                !newQuirk.IsDisease && quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);
            var availableQuirk = availableQuirks[Random.Range(0, availableQuirks.Count)];
            quirkData.Add(new QuirkInfo(availableQuirk, false, 1, false));
            ApplyQuirk(availableQuirk);
        }
        #endregion

        #region Combat Generation
        var availableSkills = new List<CombatSkill>(HeroClass.CombatSkills);
        int skillsRequired = Mathf.Clamp(HeroClass.Generation.NumberOfRandomCombatSkills, 0, HeroClass.CombatSkills.Count);
        CurrentCombatSkills = new CombatSkill[HeroClass.CombatSkills.Count];

        foreach(var guaranteedSkill in availableSkills.FindAll(skill => skill.IsGenerationGuaranteed))
        {
            CurrentCombatSkills[HeroClass.CombatSkills.IndexOf(guaranteedSkill)] = guaranteedSkill;
            availableSkills.Remove(guaranteedSkill);
            skillsRequired--;
        }

        for(int i = skillsRequired; i > 0; i--)
        {
            int generatedIndex = Random.Range(0, availableSkills.Count);
            CurrentCombatSkills[HeroClass.CombatSkills.IndexOf(availableSkills[generatedIndex])] = availableSkills[generatedIndex];
            availableSkills.RemoveAt(generatedIndex);
        }

        SelectedCombatSkills = new List<CombatSkill>();
        var selectionList = new List<CombatSkill>(CurrentCombatSkills);
        selectionList.RemoveAll(skill => skill == null);
        int selectedSkills = Mathf.Clamp(HeroClass.NumberOfSelectedCombatSkills, 0, selectionList.Count);
        for (int i = 0; i < selectedSkills; i++)
        {
            int selectedItem = Random.Range(0, selectionList.Count);
            SelectedCombatSkills.Add(selectionList[selectedItem]);
            selectionList.RemoveAt(selectedItem);
        }
        #endregion

        #region Camping Generation
        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];

        var availableGeneralSkills = HeroClass.CampingSkills.FindAll(skill => skill.Classes.Count > 4);
        int generalSkillsRequired = HeroClass.Generation.NumberOfSharedCampingSkills;
        foreach(var skill in availableGeneralSkills.OrderBy(x => Random.value)
            .Take(Mathf.Min(generalSkillsRequired, availableGeneralSkills.Count)))
        {
            int skillIndex = HeroClass.CampingSkills.IndexOf(skill);
            CurrentCampingSkills[skillIndex] = skill;
        }
        var availableSpecificSkills = HeroClass.CampingSkills.FindAll(skill => skill.Classes.Count <= 4);
        int specificSkillsRequired = HeroClass.Generation.NumberOfSpecificCampingSkills;

        foreach (var skill in availableSpecificSkills.OrderBy(x => Random.value)
            .Take(Mathf.Min(specificSkillsRequired, availableSpecificSkills.Count)))
        {
            int skillIndex = HeroClass.CampingSkills.IndexOf(skill);
            CurrentCampingSkills[skillIndex] = skill;
        }

        var availableGeneratedSkills = new List<CampingSkill>(CurrentCampingSkills);
        availableGeneratedSkills.RemoveAll(skill => skill == null);

        SelectedCampingSkills = availableGeneratedSkills.OrderBy(x => Random.value)
            .Take(Mathf.Min(4, availableGeneratedSkills.Count)).ToList();
        #endregion
    }
    public Hero(int rosterId, string classId, string generatedName, RecruitUpgrade expUpgrade)
        : base(DarkestDungeonManager.Data.HeroClasses[classId], expUpgrade.Level)
    {
        RosterId = rosterId;
        HeroName = generatedName;
        ClassStringId = classId;
        Resolve = new Resolve(expUpgrade.Level, 0);
        HeroClass = DarkestDungeonManager.Data.HeroClasses[classId];
        ClassIndexId = HeroClass.IndexId;
        AddPairedAttribute(AttributeType.Stress, new PairedAttribute(10, 200, true, AttributeCategory.CombatStat));

        #region Equipment Generation
        var weaponTree = DarkestDungeonManager.Data.UpgradeTrees[classId + ".weapon"];
        int equipLevel = weaponTree.Upgrades.FindAll(upgrade => upgrade is HeroUpgrade && (upgrade as HeroUpgrade).PrerequisiteResolveLevel <= expUpgrade.Level).Count + 1;
        Equipment weapon = HeroClass.Weapons.Find(wep => wep.UpgradeLevel == equipLevel);
        Equip(weapon, HeroEquipmentSlot.Weapon);
        Equipment armor = HeroClass.Armors.Find(arm => arm.UpgradeLevel == equipLevel);
        Equip(armor, HeroEquipmentSlot.Armor);
        #endregion

        #region Quirk Generation
        quirkData = new List<QuirkInfo>();
        int positiveQuirkNumber = Random.Range(HeroClass.Generation.NumberOfPositiveQuirksMin, HeroClass.Generation.NumberOfPositiveQuirksMax + expUpgrade.ExtraPositiveQuirks + 1);
        for (int i = 0; i < positiveQuirkNumber; i++)
        {
            var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => newQuirk.IsPositive &&
                quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);
            var availableQuirk = availableQuirks[Random.Range(0, availableQuirks.Count)];
            quirkData.Add(new QuirkInfo(availableQuirk, false, 1, false));
            ApplyQuirk(availableQuirk);
        }
        int negativeQuirkNumber = Random.Range(HeroClass.Generation.NumberOfNegativeQuirksMin, HeroClass.Generation.NumberOfNegativeQuirksMax + expUpgrade.ExtraNegativeQuirks + 1);
        for (int i = 0; i < negativeQuirkNumber; i++)
        {
            var availableQuirks = DarkestDungeonManager.Data.Quirks.Values.ToList().FindAll(newQuirk => !newQuirk.IsPositive &&
                !newQuirk.IsDisease && quirkData.TrueForAll(quirkInfo => !quirkInfo.Quirk.IncompatibleQuirks.Contains(newQuirk.Id)) &&
                quirkData.Find(quirkInfo => quirkInfo.Quirk == newQuirk) == null);
            var availableQuirk = availableQuirks[Random.Range(0, availableQuirks.Count)];
            quirkData.Add(new QuirkInfo(availableQuirk, false, 1, false));
            ApplyQuirk(availableQuirk);
        }
        #endregion

        #region Combat Generation
        var availableSkills = new List<CombatSkill>(HeroClass.CombatSkills);
        int skillsRequired = Mathf.Clamp(HeroClass.Generation.NumberOfRandomCombatSkills + expUpgrade.ExtraCombatSkills, 0, HeroClass.CombatSkills.Count);
        CurrentCombatSkills = new CombatSkill[HeroClass.CombatSkills.Count];

        foreach (var guaranteedSkill in availableSkills.FindAll(skill => skill.IsGenerationGuaranteed))
        {
            CurrentCombatSkills[HeroClass.CombatSkills.IndexOf(guaranteedSkill)] = guaranteedSkill;
            availableSkills.Remove(guaranteedSkill);
            skillsRequired--;
        }

        for (int i = skillsRequired; i > 0; i--)
        {
            int generatedIndex = Random.Range(0, availableSkills.Count);
            CurrentCombatSkills[HeroClass.CombatSkills.IndexOf(availableSkills[generatedIndex])] = availableSkills[generatedIndex];
            availableSkills.RemoveAt(generatedIndex);
        }

        SelectedCombatSkills = new List<CombatSkill>();
        var selectionList = new List<CombatSkill>(CurrentCombatSkills);
        selectionList.RemoveAll(skill => skill == null);
        int selectedSkills = Mathf.Clamp(HeroClass.NumberOfSelectedCombatSkills, 0, selectionList.Count);
        for (int i = 0; i < selectedSkills; i++)
        {
            int selectedItem = Random.Range(0, selectionList.Count);
            SelectedCombatSkills.Add(selectionList[selectedItem]);
            selectionList.RemoveAt(selectedItem);
        }
        #endregion

        #region Camping Generation
        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];

        var availableGeneralSkills = HeroClass.CampingSkills.FindAll(skill => skill.Classes.Count > 4);
        int generalSkillsRequired = HeroClass.Generation.NumberOfSharedCampingSkills;
        foreach (var skill in availableGeneralSkills.OrderBy(x => Random.value)
            .Take(Mathf.Min(generalSkillsRequired, availableGeneralSkills.Count)))
        {
            int skillIndex = HeroClass.CampingSkills.IndexOf(skill);
            CurrentCampingSkills[skillIndex] = skill;
        }
        var availableSpecificSkills = HeroClass.CampingSkills.FindAll(skill => skill.Classes.Count <= 4);
        int specificSkillsRequired = HeroClass.Generation.NumberOfSpecificCampingSkills + expUpgrade.ExtraCampingSkills;

        foreach (var skill in availableSpecificSkills.OrderBy(x => Random.value)
            .Take(Mathf.Min(specificSkillsRequired, availableSpecificSkills.Count)))
        {
            int skillIndex = HeroClass.CampingSkills.IndexOf(skill);
            CurrentCampingSkills[skillIndex] = skill;
        }

        var availableGeneratedSkills = new List<CampingSkill>(CurrentCampingSkills);
        availableGeneratedSkills.RemoveAll(skill => skill == null);

        SelectedCampingSkills = availableGeneratedSkills.OrderBy(x => Random.value)
            .Take(Mathf.Min(4, availableGeneratedSkills.Count)).ToList();
        #endregion
    }
    public Hero(Estate estate, SaveHeroData saveHeroData):base(saveHeroData)
    {
        var database = DarkestDungeonManager.Data;

        Status = saveHeroData.status;
        InActivity = saveHeroData.inActivity;
        MissingDuration = saveHeroData.missingDuration;

        RosterId = saveHeroData.rosterId;
        HeroName = saveHeroData.name;
        ClassStringId = saveHeroData.heroClass;

        if (!estate.PickRosterId(RosterId))
            Debug.LogError("Missing id " + RosterId.ToString() + " in estate from hero " + HeroName);

        Resolve = new Resolve(saveHeroData.resolveLevel, saveHeroData.resolveXP);

        AddPairedAttribute(AttributeType.Stress, new PairedAttribute(saveHeroData.stressLevel, 200, true, AttributeCategory.CombatStat));

        HeroClass = database.HeroClasses[ClassStringId];
        ClassIndexId = HeroClass.IndexId;
        Equipment weapon = HeroClass.Weapons.Find(wep => wep.UpgradeLevel == estate.GetUpgradedWeaponLevel(RosterId, HeroClass.StringId));
        Equip(weapon, HeroEquipmentSlot.Weapon);
        Equipment armor = HeroClass.Armors.Find(arm => arm.UpgradeLevel == estate.GetUpgradedArmorLevel(RosterId, HeroClass.StringId));
        Equip(armor, HeroEquipmentSlot.Armor);
        if(saveHeroData.leftTrinketId != "")
        {
            Trinket trinket = (Trinket)database.Items["trinket"][saveHeroData.leftTrinketId];
            Equip(trinket, TrinketSlot.Left);
        }
        if (saveHeroData.rightTrinketId != "")
        {
            Trinket trinket = (Trinket)database.Items["trinket"][saveHeroData.rightTrinketId];
            Equip(trinket, TrinketSlot.Right);
        }

        quirkData = new List<QuirkInfo>();
        foreach (var quirkEntry in saveHeroData.quirks)
        {
            quirkData.Add(quirkEntry);
            ApplyQuirk(quirkEntry.Quirk);
        }

        CurrentCombatSkills = new CombatSkill[7];
        SelectedCombatSkills = new List<CombatSkill>();

        for (int i = 0; i < 7; i++)
            CurrentCombatSkills[i] = HeroClass.CombatSkillVariants.Find(skill => skill.Id == HeroClass.CombatSkills[i].Id
                && skill.Level == estate.GetUpgradedSkillLevel(RosterId, HeroClass.StringId, HeroClass.CombatSkills[i].Id));

        for (int i = 0; i < saveHeroData.selectedCombatSkillIndexes.Count; i++)
            if (CurrentCombatSkills[saveHeroData.selectedCombatSkillIndexes[i]] != null)
                SelectedCombatSkills.Add(CurrentCombatSkills[saveHeroData.selectedCombatSkillIndexes[i]]);

        CurrentCampingSkills = new CampingSkill[HeroClass.CampingSkills.Count];
        SelectedCampingSkills = new List<CampingSkill>();

        for (int i = 0; i < CurrentCampingSkills.Length; i++)
            if (estate.GetUpgradedCampingStatus(RosterId, HeroClass.CampingSkills[i].Id))
                CurrentCampingSkills[i] = HeroClass.CampingSkills[i];

        for (int i = 0; i < saveHeroData.selectedCampingSkillIndexes.Count; i++)
            if (CurrentCampingSkills[saveHeroData.selectedCampingSkillIndexes[i]] != null)
                SelectedCampingSkills.Add(CurrentCampingSkills[saveHeroData.selectedCampingSkillIndexes[i]]);

        if (saveHeroData.trait != "")
        {
            var heroTrait = DarkestDungeonManager.Data.Traits.Find(trait => trait.Id == saveHeroData.trait);
            if (heroTrait != null)
                ApplyTrait(heroTrait);
        }

        Health.CurrentValue = saveHeroData.currentHp;
    }

    public void UpdateResolve()
    {
        base.UpdateResolve(Resolve.Level, HeroClass);
    }
    public void UpdateSaveData(SaveHeroData saveHeroData)
    {
        saveHeroData.status = Status;
        saveHeroData.inActivity = InActivity;
        saveHeroData.trait = Trait == null ? "" : Trait.Id;
        saveHeroData.missingDuration = MissingDuration;

        saveHeroData.rosterId = RosterId;
        saveHeroData.name = HeroName;
        saveHeroData.heroClass = HeroClass.StringId;
        saveHeroData.resolveLevel = Resolve.Level;
        saveHeroData.resolveXP = Resolve.CurrentXP;
        saveHeroData.currentHp = Health.CurrentValue;
        saveHeroData.stressLevel = Stress.CurrentValue;
        saveHeroData.weaponLevel = Weapon.UpgradeLevel;
        saveHeroData.armorLevel = Armor.UpgradeLevel;
        saveHeroData.leftTrinketId = LeftTrinket != null ? LeftTrinket.Id : "";
        saveHeroData.rightTrinketId = RightTrinket != null ? RightTrinket.Id : "";

        saveHeroData.quirks = quirkData;
        saveHeroData.buffs = buffInfo;

        saveHeroData.selectedCombatSkillIndexes.Clear();
        saveHeroData.selectedCampingSkillIndexes.Clear();
        for(int i = 0; i < CurrentCombatSkills.Length; i++)
        {
            if (CurrentCombatSkills[i] != null && SelectedCombatSkills.Contains(CurrentCombatSkills[i]))
                saveHeroData.selectedCombatSkillIndexes.Add(i);
        }
        for (int i = 0; i < CurrentCampingSkills.Length; i++)
        {
            if (CurrentCampingSkills[i] != null && SelectedCampingSkills.Contains(CurrentCampingSkills[i]))
                saveHeroData.selectedCampingSkillIndexes.Add(i);
        }
    }
    public override void UpdateSaveData(FormationUnitSaveData saveUnitData)
    {
        saveUnitData.IsHero = true;
        saveUnitData.RosterId = RosterId;
        saveUnitData.Class = Class;
        saveUnitData.Name = Name;
        saveUnitData.CurrentHp = Health.CurrentValue;
        saveUnitData.Buffs = buffInfo;
        saveUnitData.Statuses = statusEffects;
    }
}