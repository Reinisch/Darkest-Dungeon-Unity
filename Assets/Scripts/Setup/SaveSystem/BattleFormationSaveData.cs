using System.IO;
using System.Collections.Generic;

public class BattleFormationSaveData
{
    public readonly List<FormationUnitSaveData> UnitData;

    public BattleFormationSaveData()
    {
        UnitData = new List<FormationUnitSaveData>();
    }

    public void UpdateFormation(BattleFormation formation)
    {
        UnitData.Clear();
        foreach (FormationUnit unit in formation.Party.Units)
        {
            var newFormationUnitData = new FormationUnitSaveData();
            newFormationUnitData.UpdateFromUnit(unit);
            UnitData.Add(newFormationUnitData);
        }
    }

    public void WriteFormationData(BinaryWriter bw)
    {
        bw.Write(UnitData.Count);
        foreach (FormationUnitSaveData unit in UnitData)
            unit.WriteFormationUnitData(bw);
    }

    public void ReadFormationData(BinaryReader br)
    {
        int unitDataCount = br.ReadInt32();
        UnitData.Clear();
        for (int i = 0; i < unitDataCount; i++)
        {
            var newFormationUnitData = new FormationUnitSaveData();
            newFormationUnitData.ReadFormationUnitData(br);
            UnitData.Add(newFormationUnitData);
        }
    }
}