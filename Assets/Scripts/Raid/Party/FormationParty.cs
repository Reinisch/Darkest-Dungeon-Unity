using UnityEngine;
using System.Collections.Generic;

public class FormationParty : MonoBehaviour
{
    public List<FormationUnit> Units { get; private set; }
    public FormationUnit LastUnit { get { return Units.Count > 0 ? Units[Units.Count - 1] : null; } }

    public void AddUnit(FormationUnit unit, int targetRank)
    {
        if (Units.Count > 0)
        {
            Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
            for (int i = 0; i < Units.Count; i++)
            {
                if (i == Units.Count - 1)
                {
                    if (i != 0)
                        Units.Add(unit);
                    else
                        Units.Insert(0, unit);
                    break;
                }
                else if (Units[i].Rank >= targetRank)
                {
                    Units.Insert(i, unit);
                    break;
                }
            }
        }
        else
            Units.Add(unit);
        
        Units[0].Rank = 1;
        for(int i = 1; i < Units.Count; i++)
            Units[i].Rank = Units[i - 1].Rank + Units[i - 1].Size;
    }

    public void RemoveUnit(FormationUnit unit)
    {
        Units.Remove(unit);
        for(int i = 0; i < Units.Count; i++)
        {
            if (Units[i].Rank > unit.Rank)
                Units[i].Rank -= unit.Size;

            Units[i].RankSlot.UpdateSlot();
        }
    }

    public void SortRanked()
    {
        Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
    }
   
    public void CreateFormation(RaidParty party)
    {
        Units = new List<FormationUnit>();

        for (int i = 0; i < party.HeroInfo.Count; i++)
        {
            if (party.HeroInfo[i].IsAlive)
            {
                FormationUnit unit = Instantiate((Resources.Load("Prefabs/Heroes/"
                    + party.HeroInfo[i].Hero.ClassStringId) as GameObject)).GetComponent<FormationUnit>();

                unit.transform.SetParent(transform, false);
                unit.Party = this;
                unit.Initialize(party.HeroInfo[i].Hero, 4 - i, Team.Heroes);
                Units.Add(unit);
                unit.ResetAnimations();

                if (party.HeroInfo[i].Hero.HeroClass.Modes.Count > 0)
                    party.HeroInfo[i].Hero.CurrentMode = party.HeroInfo[i].Hero.HeroClass.Modes.Find(mode => mode.IsRaidDefault);
            }
        }
    }

    public void CreateFormation(RaidParty party, PhotonPlayer player)
    {
        Units = new List<FormationUnit>();

        for (int i = 0; i < party.HeroInfo.Count; i++)
        {
            if (party.HeroInfo[i].IsAlive)
            {
                FormationUnit unit = Instantiate((Resources.Load("Prefabs/Heroes/"
                    + party.HeroInfo[i].Hero.ClassStringId) as GameObject)).GetComponent<FormationUnit>();

                unit.transform.SetParent(transform, false);
                unit.Party = this;
                unit.Initialize(party.HeroInfo[i].Hero, 4 - i, player.IsMasterClient ? Team.Heroes : Team.Monsters);
                Units.Add(unit);
                unit.ResetAnimations();

                if (party.HeroInfo[i].Hero.HeroClass.Modes.Count > 0)
                    party.HeroInfo[i].Hero.CurrentMode = party.HeroInfo[i].Hero.HeroClass.Modes.Find(mode => mode.IsRaidDefault);
            }
        }
    }

    public void CreateFormation(BattleEncounter encounter)
    {
        Units = new List<FormationUnit>();
        int summonRank = 1;
        foreach(var monster in encounter.Monsters)
        {
            GameObject unitObject = Resources.Load("Prefabs/Monsters/" + monster.Class) as GameObject;

            if (unitObject == null)
                unitObject = (Resources.Load("Prefabs/Monsters/brigand_cutthroat") as GameObject);

            FormationUnit unit = Instantiate(unitObject).GetComponent<FormationUnit>();

            unit.transform.SetParent(transform, false);
            unit.Party = this;
            unit.Initialize(monster, summonRank, Team.Monsters);
            Units.Add(unit);
            summonRank += monster.Size;
            unit.ResetAnimations();
        }
    }

    public void CreateFormation(BattleFormationSaveData formationSaveData)
    {
        Units = new List<FormationUnit>();
        foreach (var unitSaveData in formationSaveData.UnitData)
        {
            if(unitSaveData.IsHero)
            {
                Hero hero = DarkestDungeonManager.Campaign.Heroes.Find(estateHero =>
                    estateHero.RosterId == unitSaveData.RosterId);
                FormationUnit unit = Instantiate(Resources.Load<GameObject>("Prefabs/Heroes/" +
                    hero.Class)).GetComponent<FormationUnit>();
                unit.transform.SetParent(transform, false);
                unit.Party = this;
                unit.Initialize(hero, unitSaveData);
                Units.Add(unit);
                unit.ResetAnimations();

                if (hero.HeroClass.Modes.Count > 0)
                {
                    var currentMode = hero.HeroClass.Modes.Find(mode => mode.Id == unitSaveData.CurrentMode);
                    if (currentMode != null)
                        hero.CurrentMode = currentMode;
                    else
                        hero.CurrentMode = hero.HeroClass.Modes.Find(mode => mode.IsRaidDefault);
                }
            }
            else
            {
                GameObject unitObject = unitSaveData.IsCorpse ?
                    Resources.Load("Prefabs/Monsters/" + unitSaveData.OriginalClass) as GameObject :
                    Resources.Load("Prefabs/Monsters/" + unitSaveData.Class) as GameObject;
                FormationUnit unit = Instantiate(unitObject).GetComponent<FormationUnit>();
                unit.transform.SetParent(transform, false);
                unit.Party = this;
                var monsterSave = new Monster(unitSaveData);
                unit.Initialize(monsterSave, unitSaveData);
                Units.Add(unit);
                unit.ResetAnimations();
                if (unitSaveData.IsCorpse)
                    unit.SetInstantCorpseAnimation();
            }
        }
    }
    
    public void DeleteFormation()
    {
        foreach (var unit in Units)
            Destroy(unit.gameObject);

        Units.Clear();
    }
}