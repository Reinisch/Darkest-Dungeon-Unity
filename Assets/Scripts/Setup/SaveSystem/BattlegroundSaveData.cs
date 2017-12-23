using System.Collections.Generic;
using System.IO;

public class BattleGroundSaveData
{
    public readonly BattleFormationSaveData MonsterFormation = new BattleFormationSaveData();

    public int RoundNumber { get; private set; }
    public RoundStatus RoundStatus { get; private set; }
    public TurnType TurnType { get; private set; }
    public TurnStatus TurnStatus { get; private set; }
    public HeroTurnAction HeroAction { get; private set; }
    public BattleStatus BattleStatus { get; private set; }
    public SurpriseStatus SurpriseStatus { get; private set; }

    public int StallingRoundNumber { get; private set; }
    public int SelectedUnitId { get; private set; }
    public int SelectedTargetId { get; private set; }
    public string LastSkillUsed { get; private set; }

    public List<int> OrderedUnitsCombatIds { get; private set; } 
    public List<int> CombatIds { get; private set; }
    public List<int> Companions { get; private set; }
    public List<int> Captures { get; private set; }
    public List<int> Controls { get; private set; }
    public List<string> LastDamaged { get; private set; }
    public List<LootDefinition> BattleLoot { get; private set; }

    public List<FormationUnitSaveData> RemovedUnits { get; private set; }

    public BattleGroundSaveData()
    {
        OrderedUnitsCombatIds = new List<int>();
        CombatIds = new List<int>();
        Companions = new List<int>();
        Captures = new List<int>();
        Controls = new List<int>();
        BattleLoot = new List<LootDefinition>();
        LastDamaged = new List<string>();
        RemovedUnits = new List<FormationUnitSaveData>();
        LastSkillUsed = "";
    }

    public void UpdateFromBattleGround(BattleGround battleGround)
    {
        RoundNumber = battleGround.Round.RoundNumber;
        RoundStatus = battleGround.RoundStatus;
        TurnType = battleGround.Round.TurnType;
        TurnStatus = battleGround.TurnStatus;

        HeroAction = battleGround.Round.HeroAction;
        SelectedUnitId = battleGround.Round.SelectedUnit == null ? -1 : battleGround.Round.SelectedUnit.CombatInfo.CombatId;
        SelectedTargetId = battleGround.Round.SelectedTarget == null ? -1 : battleGround.Round.SelectedTarget.CombatInfo.CombatId;

        BattleStatus = battleGround.BattleStatus;
        SurpriseStatus = battleGround.SurpriseStatus;

        StallingRoundNumber = battleGround.StallingRoundNumber;

        OrderedUnitsCombatIds.Clear();
        for (int i = 0; i < battleGround.Round.OrderedUnits.Count; i++)
            OrderedUnitsCombatIds.Add(battleGround.Round.OrderedUnits[i].CombatInfo.CombatId);

        CombatIds.Clear();
        CombatIds.AddRange(battleGround.CombatIds);
        Companions.Clear();
        for (int i = 0; i < battleGround.Companions.Count; i++)
            Companions.Add(battleGround.Companions[i].GetHashCode());

        Captures.Clear();
        RemovedUnits.Clear();
        for (int i = 0; i < battleGround.Captures.Count; i++)
        {
            Captures.Add(battleGround.Captures[i].GetHashCode());
            if (battleGround.Captures[i].RemoveFromParty)
            {
                var newRemovedUnitData = new FormationUnitSaveData();
                newRemovedUnitData.UpdateFromUnit(battleGround.Captures[i].PrisonerUnit);
                RemovedUnits.Add(newRemovedUnitData);
            }
        }

        Controls.Clear();
        for (int i = 0; i < battleGround.Controls.Count; i++)
            Controls.Add(battleGround.Controls[i].GetHashCode());

        BattleLoot = battleGround.BattleLoot;
        LastDamaged = battleGround.LastDamaged;
        LastSkillUsed = battleGround.LastSkillUsed == null ? "" : battleGround.LastSkillUsed;

        MonsterFormation.UpdateFormation(battleGround.MonsterFormation);
    }

    public void WriteBattlegroundData(BinaryWriter bw)
    {
        bw.Write(RoundNumber);

        bw.Write((int)RoundStatus);
        bw.Write((int)TurnType);
        bw.Write((int)TurnStatus);
        bw.Write((int)HeroAction);
        bw.Write((int)BattleStatus);
        bw.Write((int)SurpriseStatus);

        bw.Write(StallingRoundNumber);
        bw.Write(SelectedUnitId);
        bw.Write(SelectedTargetId);
        bw.Write(LastSkillUsed == null ? "" : LastSkillUsed);

        bw.Write(OrderedUnitsCombatIds.Count);
        for (int i = 0; i < OrderedUnitsCombatIds.Count; i++)
            bw.Write(OrderedUnitsCombatIds[i]);

        bw.Write(CombatIds.Count);
        for (int i = 0; i < CombatIds.Count; i++)
            bw.Write(CombatIds[i]);

        bw.Write(Companions.Count);
        for (int i = 0; i < Companions.Count; i++)
            bw.Write(Companions[i].GetHashCode());

        bw.Write(Captures.Count);
        for (int i = 0; i < Captures.Count; i++)
            bw.Write(Captures[i].GetHashCode());

        bw.Write(Controls.Count);
        for (int i = 0; i < Controls.Count; i++)
            bw.Write(Controls[i].GetHashCode());

        bw.Write(LastDamaged.Count);
        for (int i = 0; i < LastDamaged.Count; i++)
            bw.Write(LastDamaged[i]);

        bw.Write(BattleLoot.Count);
        for (int i = 0; i < BattleLoot.Count; i++)
        {
            bw.Write(BattleLoot[i].Code);
            bw.Write(BattleLoot[i].Count);
        }

        bw.Write(RemovedUnits.Count);
        for (int i = 0; i < RemovedUnits.Count; i++)
            RemovedUnits[i].WriteFormationUnitData(bw);

        MonsterFormation.WriteFormationData(bw);
    }

    public void ReadBattlegroundData(BinaryReader br)
    {
        RoundNumber = br.ReadInt32();

        RoundStatus = (RoundStatus)br.ReadInt32();
        TurnType = (TurnType)br.ReadInt32();
        TurnStatus = (TurnStatus)br.ReadInt32();
        HeroAction = (HeroTurnAction)br.ReadInt32();
        BattleStatus = (BattleStatus)br.ReadInt32();
        SurpriseStatus = (SurpriseStatus)br.ReadInt32();

        StallingRoundNumber = br.ReadInt32();
        SelectedUnitId = br.ReadInt32();
        SelectedTargetId = br.ReadInt32();
        LastSkillUsed = br.ReadString();

        int orderedUnitsCount = br.ReadInt32();
        OrderedUnitsCombatIds.Clear();
        for (int i = 0; i < orderedUnitsCount; i++)
            OrderedUnitsCombatIds.Add(br.ReadInt32());

        int combatIdsCount = br.ReadInt32();
        CombatIds.Clear();
        for (int i = 0; i < combatIdsCount; i++)
            CombatIds.Add(br.ReadInt32());

        int companionsCount = br.ReadInt32();
        Companions.Clear();
        for (int i = 0; i < companionsCount; i++)
            Companions.Add(br.ReadInt32());

        int capturesCount = br.ReadInt32();
        Captures.Clear();
        for (int i = 0; i < capturesCount; i++)
            Captures.Add(br.ReadInt32());

        int controlsCount = br.ReadInt32();
        Controls.Clear();
        for (int i = 0; i < controlsCount; i++)
            Controls.Add(br.ReadInt32());

        int lastDamagedCount = br.ReadInt32();
        LastDamaged.Clear();
        for (int i = 0; i < lastDamagedCount; i++)
            LastDamaged.Add(br.ReadString());

        int battleLootCount = br.ReadInt32();
        BattleLoot.Clear();
        for (int i = 0; i < battleLootCount; i++)
        {
            var newBattleLoot = new LootDefinition()
            {
                Code = br.ReadString(),
                Count = br.ReadInt32(),
            };
            BattleLoot.Add(newBattleLoot);
        }

        int removedUnitsCount = br.ReadInt32();
        RemovedUnits.Clear();
        for (int i = 0; i < removedUnitsCount; i++)
        {
            var newFormationUnitData = new FormationUnitSaveData();
            newFormationUnitData.ReadFormationUnitData(br);
            RemovedUnits.Add(newFormationUnitData);
        }

        MonsterFormation.ReadFormationData(br);
    }
}