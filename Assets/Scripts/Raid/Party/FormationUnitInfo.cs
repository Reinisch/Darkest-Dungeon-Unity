using System.Collections.Generic;
using System.IO;

public class FormationUnitInfo
{
    public string LastCombatSkillUsed { get; set; }
    public int LastCombatSkillTarget { get; set; }

    public int RoundsAlive { get; set; }
    public bool MarkedForDeath { get; set; }
    public bool IsSurprised { get; set; }
    public bool IsImmobilized { get; set; }
    public bool IsDead { get; set; }
    public bool CheckLoot { get; set; }
    public bool OneShotted { get; set; }

    public int TotalInitiatives { get; set; }
    public int CurrentInitiative { get; set; }
    public int CombatId { get; set; }

    public List<string> SkillsUsedInBattle { get; set; }
    public List<string> SkillsUsedThisTurn { get; set; }
    public List<SkillCooldown> SkillCooldowns { get; set; }

    public List<int> BlockedMoveUnitIds { get; set; }
    public List<int> BlockedHealUnitIds { get; set; }
    public List<int> BlockedBuffUnitIds { get; set; }
    public List<string> BlockedItems { get; set; }

    public FormationUnitInfo()
    {
        SkillCooldowns = new List<SkillCooldown>();
        SkillsUsedThisTurn = new List<string>();
        SkillsUsedInBattle = new List<string>();

        BlockedMoveUnitIds = new List<int>();
        BlockedHealUnitIds = new List<int>();
        BlockedBuffUnitIds = new List<int>();
        BlockedItems = new List<string>();
    }

    public void PrepareForBattle(int id, Monster monster, bool checkLoot)
    {
        RoundsAlive = 0;
        LastCombatSkillTarget = -1;
        TotalInitiatives = monster.Initiative.NumberOfTurns;
        CurrentInitiative = 0;
        CombatId = id;
        IsImmobilized = false;
        IsDead = false;
        MarkedForDeath = false;
        IsSurprised = false;
        OneShotted = false;
        CheckLoot = checkLoot;

        SkillsUsedInBattle.Clear();
        SkillsUsedThisTurn.Clear();
        SkillCooldowns.Clear();

        BlockedMoveUnitIds.Clear();
        BlockedHealUnitIds.Clear();
        BlockedBuffUnitIds.Clear();
        BlockedItems.Clear();
    }

    public void PrepareForBattle(int id)
    {
        RoundsAlive = 0;
        LastCombatSkillTarget = -1;
        TotalInitiatives = 1;
        CurrentInitiative = 0;
        CombatId = id;
        IsImmobilized = false;
        IsDead = false;
        MarkedForDeath = false;
        IsSurprised = false;
        CheckLoot = true;
        OneShotted = false;

        SkillsUsedInBattle.Clear();
        SkillsUsedThisTurn.Clear();
        SkillCooldowns.Clear();

        BlockedMoveUnitIds.Clear();
        BlockedHealUnitIds.Clear();
        BlockedBuffUnitIds.Clear();
        BlockedItems.Clear();
    }

    public void UpdateNextTurn()
    {
        CurrentInitiative++;
        for (int i = SkillCooldowns.Count - 1; i >= 0; i--)
            if (SkillCooldowns[i].ReduceCooldown())
                SkillCooldowns.RemoveAt(i);
    }

    public void UpdateNextRound()
    {
        RoundsAlive++;
        SkillsUsedThisTurn.Clear();
        CurrentInitiative = 0;

        BlockedMoveUnitIds.Clear();
        BlockedHealUnitIds.Clear();
        BlockedBuffUnitIds.Clear();
        BlockedItems.Clear();
    }

    public void WriteCombatInfoData(BinaryWriter bw)
    {
        bw.Write(LastCombatSkillUsed == null ? "" : LastCombatSkillUsed);
        bw.Write(LastCombatSkillTarget);

        bw.Write(RoundsAlive);
        bw.Write(MarkedForDeath);
        bw.Write(IsSurprised);
        bw.Write(IsImmobilized);
        bw.Write(IsDead);
        bw.Write(CheckLoot);

        bw.Write(TotalInitiatives);
        bw.Write(CurrentInitiative);
        bw.Write(CombatId);

        bw.Write(SkillsUsedInBattle.Count);
        for (int i = 0; i < SkillsUsedInBattle.Count; i++)
            bw.Write(SkillsUsedInBattle[i]);

        bw.Write(SkillsUsedThisTurn.Count);
        for (int i = 0; i < SkillsUsedThisTurn.Count; i++)
            bw.Write(SkillsUsedThisTurn[i]);

        bw.Write(SkillCooldowns.Count);
        for (int i = 0; i < SkillCooldowns.Count; i++)
        {
            bw.Write(SkillCooldowns[i].SkillId);
            bw.Write(SkillCooldowns[i].Amount);
        }

        bw.Write(BlockedMoveUnitIds.Count);
        for (int i = 0; i < BlockedMoveUnitIds.Count; i++)
            bw.Write(BlockedMoveUnitIds[i]);

        bw.Write(BlockedHealUnitIds.Count);
        for (int i = 0; i < BlockedHealUnitIds.Count; i++)
            bw.Write(BlockedHealUnitIds[i]);

        bw.Write(BlockedBuffUnitIds.Count);
        for (int i = 0; i < BlockedBuffUnitIds.Count; i++)
            bw.Write(BlockedBuffUnitIds[i]);

        bw.Write(BlockedItems.Count);
        for (int i = 0; i < BlockedItems.Count; i++)
            bw.Write(BlockedItems[i]);
    }

    public void ReadCombatInfoData(BinaryReader br)
    {
        LastCombatSkillUsed = br.ReadString();
        LastCombatSkillTarget = br.ReadInt32();

        RoundsAlive = br.ReadInt32();
        MarkedForDeath = br.ReadBoolean();
        IsSurprised = br.ReadBoolean();
        IsImmobilized = br.ReadBoolean();
        IsDead = br.ReadBoolean();
        CheckLoot = br.ReadBoolean();

        TotalInitiatives = br.ReadInt32();
        CurrentInitiative = br.ReadInt32();
        CombatId = br.ReadInt32();

        int skillUsedInBattleCount = br.ReadInt32();
        SkillsUsedInBattle.Clear();
        for (int i = 0; i < skillUsedInBattleCount; i++)
            SkillsUsedInBattle.Add(br.ReadString());

        int skillUsedThisTurnCount = br.ReadInt32();
        SkillsUsedThisTurn.Clear();
        for (int i = 0; i < skillUsedThisTurnCount; i++)
            SkillsUsedThisTurn.Add(br.ReadString());

        int skillCooldownCount = br.ReadInt32();
        SkillCooldowns.Clear();
        for (int i = 0; i < skillCooldownCount; i++)
            SkillCooldowns.Add(new SkillCooldown(br.ReadString(), br.ReadInt32()));

        int blockedMoveUnitCount = br.ReadInt32();
        BlockedMoveUnitIds.Clear();
        for (int i = 0; i < blockedMoveUnitCount; i++)
            BlockedMoveUnitIds.Add(br.ReadInt32());

        int blockedHealUnitCount = br.ReadInt32();
        BlockedHealUnitIds.Clear();
        for (int i = 0; i < blockedHealUnitCount; i++)
            BlockedHealUnitIds.Add(br.ReadInt32());

        int blockedBuffUnitCount = br.ReadInt32();
        BlockedBuffUnitIds.Clear();
        for (int i = 0; i < blockedBuffUnitCount; i++)
            BlockedBuffUnitIds.Add(br.ReadInt32());

        int blockedItemCount = br.ReadInt32();
        BlockedItems.Clear();
        for (int i = 0; i < blockedItemCount; i++)
            BlockedItems.Add(br.ReadString());
    }
}