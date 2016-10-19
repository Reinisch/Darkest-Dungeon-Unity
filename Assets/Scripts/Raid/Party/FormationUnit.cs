using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public delegate void FormationUnitEvent(FormationUnit unit);

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
    public bool OneShotted { get; set; } // Not saved !

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

public class FormationUnit : MonoBehaviour
{
    public FormationParty Party
    {
        get;
        set;
    }
    public BattleFormation Formation
    {
        get;
        set;
    }
    public FormationOverlaySlot OverlaySlot
    {
        get; 
        set;
    }
    public FormationRanksSlot RankSlot
    {
        get; 
        set; 
    }
    public RectTransform TargetRect
    {
        get
        {
            return targetRect;
        }
    }

    public List<SkeletonAnimation> SkeletonAnimations
    {
        get
        {
            return unitAnimator.SkeletonAnimations;
        }
    }
    public SkeletonAnimation CurrentState
    {
        get
        {
            return unitAnimator.SkeletonAnimations.Find(animation => animation.isActiveAndEnabled);
        }
    }
    public AnimatedEffect CurrentHalo
    {
        get;
        private set;
    }
    public RectTransform RectTransform
    {
        get;
        private set;
    }

    public Character Character
    {
        get;
        set;
    }
    public FormationUnitInfo CombatInfo
    {
        get;
        private set;
    }
    public List<EffectEvent> EventQueue
    {
        get; 
        private set; 
    }

    public bool IsCorpse
    {
        get
        {
            return Character.IsMonster && Character.MonsterTypes.Contains(MonsterType.Corpse);
        }
    }
    public string OriginalClass
    {
        get;
        set;
    }
    public bool IsTargetable
    {
        get
        {
            return isLegitTarget;
        }
    }
    public int Size
    {
        get
        {
            return Character.Size;
        }
    }
    public int Rank
    {
        get;
        set;
    }
    public Team Team
    {
        get;
        set;
    }

    private float smoothTime = 0.1f;
    private int instantRelocateCount = 0;
    private bool isLegitTarget = false;
    private bool hasTarget = false;
    private bool hasRectTarget = false;
    private UnitAnimator unitAnimator;
    private RectTransform targetRect;
    private RectTransform campRect;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition = Vector3.zero;
    private List<EffectEvent> effectStackTemp = new List<EffectEvent>();

    private static List<int> deathDoorStateIndexes = new List<int>() { 0, 2, 3 };

    bool deathDoorRedUp = false;
    bool colorReset = false;
    float deathDoorColorOverride = 1f;

    void Update()
    {
        #region Death door color change
        if (Character.IsMonster == false)
        {
            if (Character.AtDeathsDoor)
            {
                colorReset = false;

                if (CurrentState != null && deathDoorStateIndexes.Contains(SkeletonAnimations.IndexOf(CurrentState)))
                {
                    if (deathDoorRedUp)
                    {
                        if (deathDoorColorOverride < 1f)
                        {
                            deathDoorColorOverride = Mathf.Clamp(deathDoorColorOverride + 0.8f * Time.deltaTime, 0, 1);

                            for (int i = 0; i < deathDoorStateIndexes.Count; i++)
                            {
                                if (SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton == null)
                                    continue;
                                SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.R = 1f;
                                SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.G = deathDoorColorOverride;
                                SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.B = deathDoorColorOverride;
                            }
                        }
                        else
                            deathDoorRedUp = false;
                    }
                    else
                    {
                        if (deathDoorColorOverride > 0.4f)
                        {
                            deathDoorColorOverride = Mathf.Clamp(deathDoorColorOverride - 0.8f * Time.deltaTime, 0, 1);

                            for (int i = 0; i < deathDoorStateIndexes.Count; i++)
                            {
                                if (SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton == null)
                                    continue;
                                SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.R = 1f;
                                SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.G = deathDoorColorOverride;
                                SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.B = deathDoorColorOverride;
                            }
                        }
                        else
                            deathDoorRedUp = true;
                    }
                }
            }
            else if (colorReset == false)
            {
                deathDoorColorOverride = 1f;

                for (int i = 0; i < deathDoorStateIndexes.Count; i++)
                {
                    if (SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton == null)
                        continue;
                    SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.R = 1f;
                    SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.G = 1f;
                    SkeletonAnimations[deathDoorStateIndexes[i]].Skeleton.B = 1f;
                }
                colorReset = true;
            }
        }
        #endregion

        #region Relocation
        if(instantRelocateCount > 0)
        {
            instantRelocateCount--;
            if (hasRectTarget)
                RectTransform.position = targetRect.position;
            else if (hasTarget)
                RectTransform.position = targetPosition;
            else
            {
                if (RankSlot != null)
                    RectTransform.position = RankSlot.RectTransform.position;
            }
            return;
        }
        if (hasRectTarget)
            RectTransform.position = Vector3.SmoothDamp(RectTransform.position, targetRect.position, ref velocity, smoothTime);
        else if (hasTarget)
            RectTransform.position = Vector3.SmoothDamp(RectTransform.position, targetPosition, ref velocity, smoothTime);
        else
        {
            if (RankSlot != null)
                RectTransform.position = Vector3.SmoothDamp(RectTransform.position, RankSlot.RectTransform.position, ref velocity, 0.1f);
        }
        #endregion
    }

    public void Initialize(Character character, FormationUnitSaveData saveData)
    {
        EventQueue = new List<EffectEvent>();
        CombatInfo = saveData.CombatInfo;
        RectTransform = GetComponent<RectTransform>();
        unitAnimator = GetComponentInChildren<UnitAnimator>();
        Character = character;
        if(Character.IsMonster == false)
            Character.LoadStatusEffects(saveData.Statuses);
        Rank = saveData.Rank;
        Team = saveData.Team;
        OriginalClass = saveData.OriginalClass;

        Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(this));
    }
    public void Initialize(Character character, int rank, Team team)
    {
        EventQueue = new List<EffectEvent>();
        CombatInfo = new FormationUnitInfo();
        RectTransform = GetComponent<RectTransform>();
        unitAnimator = GetComponentInChildren<UnitAnimator>();
        Character = character;
        Rank = rank;
        Team = team;
        OriginalClass = character.Class;

        Character.ApplyAllBuffRules(RaidSceneManager.Rules.GetIdleUnitRules(this));
    }
    public void Replace(Character character)
    {
        Character = character;
    }
    public void StackEvents()
    {
        for (int i = EventQueue.Count - 1; i >= 0; i--)
        {
            if (EventQueue[i].SubEffect.Fusable)
            {
                var coreEffect = effectStackTemp.Find(item => item.SubEffect.Type == EventQueue[i].SubEffect.Type);
                if (coreEffect == null)
                {
                    effectStackTemp.Add(EventQueue[i]);
                    EventQueue[i].FuseSelf();
                }
                else
                {
                    coreEffect.Fuse(EventQueue[i]);
                    EventQueue.RemoveAt(i);
                }
            }
        }
        effectStackTemp.Clear();
    }
    
    public void SwapUnits(FormationUnit swapper, FormationUnit target)
    {
        if (swapper.RankSlot.Ranks != target.RankSlot.Ranks || swapper.Party != target.Party)
            return;

        swapper.Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });

        swapper.Party.Units[swapper.Party.Units.IndexOf(swapper)] = target;
        swapper.Party.Units[target.Party.Units.IndexOf(target)] = swapper;

        for(int i = 0; i < swapper.Party.Units.Count; i++)
        {
            if (i == 0)
                swapper.Party.Units[i].RankSlot.Relocate(1);
            else
                swapper.Party.Units[i].RankSlot.Relocate(swapper.Party.Units[i - 1].Rank + swapper.Party.Units[i - 1].Size - 1);
        }
    }
    public void Pull(int strength, bool changeUnitOrder = true)
    {
        if (CombatInfo.IsImmobilized)
            return;
        int pulled = 0;
        foreach(var frontUnit in Party.Units.FindAll(unit => unit.Rank < Rank).OrderByDescending(unit => unit.Rank))
        {
            if (frontUnit.CombatInfo.IsImmobilized)
                break;
            int frontUnitTargetRank = frontUnit.Rank + Size;
            int pulledTargetRank = frontUnit.Rank;

            RankSlot.Relocate(pulledTargetRank, changeUnitOrder);
            frontUnit.RankSlot.Relocate(frontUnitTargetRank);

            pulled += frontUnit.Size;

            if (pulled >= strength)
                break;
        }
        Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
    }
    public void Push(int strength, bool changeUnitOrder = true)
    {
        if (CombatInfo.IsImmobilized)
            return;
        int pushed = 0;
        foreach (var backUnit in Party.Units.FindAll(unit => unit.Rank > Rank).OrderBy(unit => unit.Rank))
        {
            if (backUnit.CombatInfo.IsImmobilized)
                break;
            int backUnitTargetRank = Rank;
            int pushedTargetRank = Rank + backUnit.Size;

            RankSlot.Relocate(pushedTargetRank, changeUnitOrder);
            backUnit.RankSlot.Relocate(backUnitTargetRank);

            pushed += backUnit.Size;

            if (pushed >= strength)
                break;
        }
        Party.Units.Sort((x, y) => { if (x.Rank == y.Rank) return 0; if (x.Rank > y.Rank) return 1; else return -1; });
    }

    public void SetLayer(int layer)
    {
        for (int i = 0; i < unitAnimator.SkeletonAnimations.Count; i++)
            unitAnimator.SkeletonAnimations[i].gameObject.layer = layer;
    }
    public void SetSortingOrder(int order)
    {
        unitAnimator.SetSortingOrder(order);
    }
    public void ResetAnimations()
    {
        for (int j = 0; j < unitAnimator.SkeletonAnimations.Count; j++)
            unitAnimator.SkeletonAnimations[j].Reset();
    }
    public void SetCombatAnimation(bool active)
    {
        if (Character.Mode == null)
            unitAnimator.Animator.SetBool("InCombat", active);
        else
            unitAnimator.Animator.SetBool("InCombat_" + Character.Mode.Id, active);
    }
    public void SetCampingAnimation(bool active)
    {
        unitAnimator.Animator.SetBool("IsCamping", active);
    }
    public void SetCorpseKillAnimation(bool active)
    {
        unitAnimator.Animator.SetBool("corpse_death", active);
    }
    public void SetDeathAnimation(bool active)
    {
        if (Character.Mode == null)
            unitAnimator.Animator.SetBool("death", active);
        else
            unitAnimator.Animator.SetBool("death_" + Character.Mode.Id, active);
    }
    public void SetReleaseAnimation(bool active)
    {
        unitAnimator.Animator.SetBool("release", active);
    }
    public void SetDefendAnimation(bool active)
    {
        if (Character.Mode == null)
            unitAnimator.Animator.SetBool("defend", active);
        else
            unitAnimator.Animator.SetBool("defend_" + Character.Mode.Id, active);
    }
    public void SetCorpseAnimation(bool active)
    {
        unitAnimator.Animator.SetBool("corpse", active);
    }
    public void SetInstantCorpseAnimation()
    {
        SetDeathAnimation(true);
        SetCorpseAnimation(true);
        unitAnimator.Animator.Play("corpse_dead");
    }
    public void SetInvestigateAnimation(bool active)
    {
        unitAnimator.Animator.SetBool("investigate", active);
    }
    public void SetHeroic(bool active)
    {
        if (Character.Mode == null)
            unitAnimator.Animator.SetBool("heroic", active);
        else
            unitAnimator.Animator.SetBool("heroic_" + Character.Mode.Id, active);
    }
    public void SetAfflicted(bool active)
    {
        unitAnimator.Animator.SetBool("afflicted", active);
    }
    public void SetWalking(bool active)
    {
        unitAnimator.Animator.SetBool("IsWalking", active);
    }
    public void SetSurprised(bool active)
    {
        CombatInfo.IsSurprised = active;
        if (active)
            SetHalo("surprised");
        else if (Character[StatusType.Stun].IsApplied)
            SetHalo("stunned");
        else
            ResetHalo();
    }
    public bool IsWalking()
    {
        return unitAnimator.Animator.GetBool("IsWalking");
    }

    public void SetFriendlyPerformerStatus(bool legitTarget)
    {
        OverlaySlot.SetFriendly();
        OverlaySlot.SetActive();
        isLegitTarget = legitTarget;
    }
    public void SetMoveTargetStatus(bool legitTarget)
    {
        OverlaySlot.SetMovable();
        OverlaySlot.SetActive();
        isLegitTarget = legitTarget;
    }
    public void SetFriendlyTargetStatus(bool legitTarget)
    {
        OverlaySlot.SetFriendly();
        OverlaySlot.SetStatic();
        isLegitTarget = legitTarget;
    }
    public void SetEnemyTargetStatus(bool legitTarget, bool isCombined)
    {
        OverlaySlot.SetEnemy(isCombined);
        OverlaySlot.SetStatic();
        isLegitTarget = legitTarget;
    }
    public void SetPerformerStatus()
    {
        OverlaySlot.SetSelected();
        OverlaySlot.SetActive();
        isLegitTarget = false;
    }
    public void SetDeactivatedStatus()
    {
        OverlaySlot.SetDeselected();
        isLegitTarget = false;
    }

    public void SetTarget(Vector3 target, float time)
    {
        hasTarget = true;
        targetPosition = target;
        smoothTime = time;
    }
    public void SetRectTarget(RectTransform target, float time, bool camping = false)
    {
        hasRectTarget = true;
        targetRect = target;
        smoothTime = time;
        if (camping)
            campRect = target;
    }
    public void DeleteTarget(float time, bool removeCamping = false)
    {
        if (campRect != null)
        {
            if (removeCamping)
            {
                campRect = null;
            }
            else
            {
                targetRect = campRect;
                return;
            }
        }
        hasTarget = false;
        hasRectTarget = false;
        targetPosition = Vector3.zero;
        smoothTime = time;
        

    }

    public void SetHalo(string haloAnimation)
    {
        if (Character.DisplayModifier != null && Character.DisplayModifier.DisableHalos)
            return;

        if (CurrentHalo == null)
        {
            GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/halo");
            if (animationObject != null)
            {
                AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                animation.skeletonAnimation.Reset();
                animation.BindToTarget(this, CurrentState, "fxhead");
                animation.IsLooping = true;
                CurrentHalo = animation;
            }
            else
                return;
        }

        CurrentHalo.gameObject.SetActive(true);
        CurrentHalo.BindToTarget(this, CurrentState, "fxhead");
        CurrentHalo.skeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
        CurrentHalo.gameObject.layer = CurrentState.gameObject.layer;

        switch (haloAnimation)
        {
            case "stunned":
                CurrentHalo.skeletonAnimation.state.SetAnimation(0, haloAnimation, false);
                CurrentHalo.skeletonAnimation.state.AddAnimation(0, "stun_loop", true, 1.7f);
                break;
            case "surprised":
                CurrentHalo.skeletonAnimation.state.SetAnimation(0, "surprised_hero", false);
                CurrentHalo.skeletonAnimation.state.AddAnimation(0, "surprise_loop", true, 1.7f);
                break;
            default:
                CurrentHalo.skeletonAnimation.state.SetAnimation(0, haloAnimation, false);
                if (Character.GetStatusEffect(StatusType.Stun).IsApplied)
                    CurrentHalo.skeletonAnimation.state.AddAnimation(0, "stun_loop", true, 1.7f);
                else if (CombatInfo.IsSurprised)
                    CurrentHalo.skeletonAnimation.state.AddAnimation(0, "surprise_loop", true, 1.7f);
                break;
        }
    }
    public void ResetHalo()
    {
        if (CurrentHalo != null)
        {
            CurrentHalo.skeletonAnimation.state.ClearTracks();
            CurrentHalo.gameObject.SetActive(false);
        }
    }

    public void SetPerformerSkillAnimation(SkillArtInfo skillInfo, bool activate)
    {
        SkeletonAnimation skillAnimation = unitAnimator.Effects.Find(item => item.name == skillInfo.ArtId);
        if (skillAnimation != null)
        {
            skillAnimation.gameObject.layer = CurrentState.gameObject.layer;
            skillAnimation.gameObject.SetActive(activate);
            skillAnimation.Reset();
        }
        unitAnimator.Animator.SetBool(skillInfo.AnimationId, activate);
    }
    public void SetPerformerSkillAnimation(string skillArtId, bool activate)
    {
        SkeletonAnimation skillAnimation = unitAnimator.Effects.Find(item => item.name == skillArtId);
        if (skillAnimation != null)
        {
            skillAnimation.gameObject.layer = CurrentState.gameObject.layer;
            skillAnimation.gameObject.SetActive(activate);
            skillAnimation.Reset();
        }
        unitAnimator.Animator.SetBool(skillArtId, activate);
    }
    public void SetPerformerSkillAnimationOverriden(SkillArtInfo skillInfo, string mode, bool activate)
    {
        SkeletonAnimation skillAnimation = unitAnimator.Effects.Find(item => item.name == skillInfo.ArtId);
        if (skillAnimation != null)
        {
            skillAnimation.gameObject.layer = CurrentState.gameObject.layer;
            skillAnimation.gameObject.SetActive(activate);
            if(activate)
            {
                skillAnimation.Reset();
                skillAnimation.state.SetAnimation(0, skillInfo.ArtId + "_" + mode, false).Time = 0;
                skillAnimation.Update(0);
                skillAnimation.AnimationName = skillInfo.ArtId + "_" + mode;
            }
        }
        unitAnimator.Animator.SetBool(skillInfo.AnimationId + "_" + mode, activate);
    }
    public void SetTargetSkillEffect(SkillArtInfo artInfo, FormationUnit performer)
    {
        SetTargetEffect(performer, artInfo.TargetChestFX, "fxchest");
        SetTargetEffect(performer, artInfo.TargetFX, "fxskill");
        SetTargetEffect(performer, artInfo.TargetHeadFX, "fxhead");
    }
    public void SetCaptureEffect(FormationUnit performer)
    {
        GameObject animationObject = null;
        var exclusiveAnimation = performer.unitAnimator.Effects.Find(animation => animation.name == "capture_target");

        if (exclusiveAnimation != null)
            animationObject = exclusiveAnimation.gameObject;

        if (animationObject != null)
        {
            AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
            if (animation != null)
            {
                animation.IsLooping = true;
                animation.skeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;  
                animation.BindToTarget(this, CurrentState, "fxskill");
                animation.gameObject.layer = CurrentState.gameObject.layer;
                animation.skeletonAnimation.Reset();
                animation.skeletonAnimation.state.SetAnimation(0, "capture_target_fade_in", false);
                animation.skeletonAnimation.state.AddAnimation(0, "capture_target_loop", true, 0.667f);
            }
        }
    }
    public void SetTargetEffect(FormationUnit performer, string effect, string fxTarget)
    {
        if (effect != null)
        {
            GameObject animationObject = null;
            var exclusiveAnimation = performer.unitAnimator.Effects.Find(animation => animation.name == effect);

            if (exclusiveAnimation != null)
                animationObject = exclusiveAnimation.gameObject;
            else
                animationObject = Resources.Load<GameObject>("Prefabs/Effects/" + effect);

            if (animationObject != null)
            {
                AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (animation != null)
                {
                    animation.skeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                    animation.BindToTarget(this, CurrentState, fxTarget);
                    animation.gameObject.layer = CurrentState.gameObject.layer;
                }
            }
        }
    }
    public void SetTargetItemEffect(string effect)
    {
        if (effect != null)
        {
            GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/" + effect);

            if (animationObject != null)
            {
                AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (animation != null)
                {
                    animation.skeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                    animation.BindToTargetUnit(this, CurrentState, "fxchest");
                    animation.gameObject.layer = CurrentState.gameObject.layer;
                }
            }
        }
    }

    public void SetTargetEffect(FormationUnit performer, string effect, string fxTarget, string subEffect)
    {
        if (effect != null)
        {
            GameObject animationObject = null;
            var exclusiveAnimation = performer.unitAnimator.Effects.Find(animation => animation.name == effect);

            if (exclusiveAnimation != null)
                animationObject = exclusiveAnimation.gameObject;
            else
                animationObject = Resources.Load<GameObject>("Prefabs/Effects/" + effect);

            if (animationObject != null)
            {
                AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (animation != null)
                {
                    animation.skeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                    animation.skeletonAnimation.Reset();
                    animation.BindToTarget(this, CurrentState, fxTarget);
                    animation.skeletonAnimation.state.SetAnimation(0, subEffect, false);
                    animation.gameObject.layer = CurrentState.gameObject.layer;
                }
            }
        }
    }
    public void RemoveCaptureEffect()
    {
        foreach (var animation in RectTransform.GetComponentsInChildren<AnimatedEffect>())
            if (animation.gameObject.name.Contains("capture_target"))
            {
                animation.skeletonAnimation.state.SetAnimation(0, "capture_target_fade_out", false);
                Destroy(animation.gameObject, 0.667f);
            }
    }

    public void SetMoveSmoothTime(float moveTime)
    {
        smoothTime = moveTime;
        instantRelocateCount = 0;
    }
    public void InstantRelocation()
    {
        instantRelocateCount = 2;
        if (hasTarget)
            RectTransform.position = targetPosition;
        else
            RectTransform.position = RankSlot.RectTransform.position;
    }
    public void InstantFlip()
    {
        RectTransform.localScale = new Vector3(-RectTransform.localScale.x, RectTransform.localScale.y, RectTransform.localScale.z);
    }
}