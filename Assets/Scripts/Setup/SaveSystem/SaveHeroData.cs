using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveHeroData
{
    public HeroStatus status;
    public string inActivity;
    public int missingDuration;

    public int rosterId;
    public string name;
    public string heroClass;
    public string trait;

    public int resolveLevel;
    public int resolveXP;

    public float currentHp;
    public float stressLevel;

    public int weaponLevel;
    public int armorLevel;

    public string leftTrinketId;
    public string rightTrinketId;

    public List<QuirkInfo> quirks;
    public List<BuffInfo> buffs;

    public List<int> selectedCombatSkillIndexes;
    public List<int> selectedCampingSkillIndexes;

    public SaveHeroData()
    {
        status = HeroStatus.Available;
        inActivity = "";
        trait = "";

        quirks = new List<QuirkInfo>();
        buffs = new List<BuffInfo>();

        selectedCombatSkillIndexes = new List<int>();
        selectedCampingSkillIndexes = new List<int>();

        currentHp = 1000;
    }
}
