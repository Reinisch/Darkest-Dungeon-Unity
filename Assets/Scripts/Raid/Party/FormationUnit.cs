using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FormationUnit : MonoBehaviour
{
    public FormationParty Party { get; set; }
    public BattleFormation Formation { get; set; }
    public FormationOverlaySlot OverlaySlot { get; set; }
    public FormationRanksSlot RankSlot { get; set; }
    public AnimatedEffect CurrentHalo { get; private set; }
    public RectTransform RectTransform { get; private set; }

    public FormationUnitInfo CombatInfo { get; private set; }
    public List<EffectEvent> EventQueue { get; private set; }
    public Character Character { get; set; }

    public Team Team { get; set; }
    public int Rank { get; set; }
    public string OriginalClass { get; private set; }

    public bool IsCorpse { get { return Character.IsMonster && Character.MonsterTypes.Contains(MonsterType.Corpse); } }
    public bool IsTargetable { get { return isLegitTarget; } }
    public int Size { get { return Character.Size; } }

    public RectTransform TargetRect
    {
        get { return targetRect; }
    }

    public SkeletonAnimation CurrentState
    {
        get { return unitAnimator.SkeletonAnimations.Find(effect => effect.isActiveAndEnabled); }
    }

    public List<SkeletonAnimation> SkeletonAnimations
    {
        get { return unitAnimator.SkeletonAnimations; }
    }

    private readonly List<EffectEvent> effectStackTemp = new List<EffectEvent>();
    private static readonly List<int> DeathDoorStateIndexes = new List<int> { 0, 2, 3 };

    private float smoothTime = 0.1f;
    private int instantRelocateCount;
    private bool isLegitTarget;
    private bool hasTarget;
    private bool hasRectTarget;
    private UnitAnimator unitAnimator;
    private RectTransform targetRect;
    private RectTransform campRect;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition = Vector3.zero;

    private bool deathDoorRedUp;
    private bool colorReset;
    private float deathDoorColorOverride = 1f;

    private void Update()
    {
        #region Death door color change

        if (Character.IsMonster == false)
        {
            if (Character.AtDeathsDoor)
            {
                colorReset = false;

                if (CurrentState != null && DeathDoorStateIndexes.Contains(SkeletonAnimations.IndexOf(CurrentState)))
                {
                    if (deathDoorRedUp)
                    {
                        if (deathDoorColorOverride < 1f)
                        {
                            deathDoorColorOverride = Mathf.Clamp(deathDoorColorOverride + 0.8f * Time.deltaTime, 0, 1);

                            for (int i = 0; i < DeathDoorStateIndexes.Count; i++)
                            {
                                if (SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton == null)
                                    continue;
                                SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.R = 1f;
                                SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.G = deathDoorColorOverride;
                                SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.B = deathDoorColorOverride;
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

                            for (int i = 0; i < DeathDoorStateIndexes.Count; i++)
                            {
                                if (SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton == null)
                                    continue;
                                SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.R = 1f;
                                SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.G = deathDoorColorOverride;
                                SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.B = deathDoorColorOverride;
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

                for (int i = 0; i < DeathDoorStateIndexes.Count; i++)
                {
                    if (SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton == null)
                        continue;
                    SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.R = 1f;
                    SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.G = 1f;
                    SkeletonAnimations[DeathDoorStateIndexes[i]].Skeleton.B = 1f;
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

    #region Animation Helpers

    public void SetLayer(int layer)
    {
#if !(UNITY_ANDROID || UNITY_IOS)
        for (int i = 0; i < unitAnimator.SkeletonAnimations.Count; i++)
            unitAnimator.SkeletonAnimations[i].gameObject.layer = layer;
#endif
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

    #endregion

    #region Animated Effects

    public void SetHalo(string haloAnimation)
    {
        if (Character.DisplayModifier != null && Character.DisplayModifier.DisableHalos)
            return;

        if (CurrentHalo == null)
        {
            GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/halo");
            if (animationObject != null)
            {
                AnimatedEffect effectAnimation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                effectAnimation.SkeletonAnimation.Reset();
                effectAnimation.BindToTarget(this, CurrentState, "fxhead");
                effectAnimation.IsLooping = true;
                CurrentHalo = effectAnimation;
            }
            else
                return;
        }

        CurrentHalo.gameObject.SetActive(true);
        CurrentHalo.BindToTarget(this, CurrentState, "fxhead");
        CurrentHalo.SkeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
        CurrentHalo.gameObject.layer = CurrentState.gameObject.layer;

        switch (haloAnimation)
        {
            case "stunned":
                CurrentHalo.SkeletonAnimation.state.SetAnimation(0, haloAnimation, false);
                CurrentHalo.SkeletonAnimation.state.AddAnimation(0, "stun_loop", true, 1.7f);
                break;
            case "surprised":
                CurrentHalo.SkeletonAnimation.state.SetAnimation(0, "surprised_hero", false);
                CurrentHalo.SkeletonAnimation.state.AddAnimation(0, "surprise_loop", true, 1.7f);
                break;
            default:
                CurrentHalo.SkeletonAnimation.state.SetAnimation(0, haloAnimation, false);
                if (Character.GetStatusEffect(StatusType.Stun).IsApplied)
                    CurrentHalo.SkeletonAnimation.state.AddAnimation(0, "stun_loop", true, 1.7f);
                else if (CombatInfo.IsSurprised)
                    CurrentHalo.SkeletonAnimation.state.AddAnimation(0, "surprise_loop", true, 1.7f);
                break;
        }
    }

    public void ResetHalo()
    {
        if (CurrentHalo != null)
        {
            CurrentHalo.SkeletonAnimation.state.ClearTracks();
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
            if (activate)
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
        SetTargetEffect(performer, artInfo.TargetChestFx, "fxchest");
        SetTargetEffect(performer, artInfo.TargetFx, "fxskill");
        SetTargetEffect(performer, artInfo.TargetHeadFx, "fxhead");
    }

    public void SetCaptureEffect(FormationUnit performer)
    {
        GameObject animationObject = null;
        var exclusiveAnimation = performer.unitAnimator.Effects.Find(effect => effect.name == "capture_target");

        if (exclusiveAnimation != null)
            animationObject = exclusiveAnimation.gameObject;

        if (animationObject != null)
        {
            AnimatedEffect effect = Instantiate(animationObject).GetComponent<AnimatedEffect>();
            if (effect != null)
            {
                effect.IsLooping = true;
                effect.SkeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                effect.BindToTarget(this, CurrentState, "fxskill");
                effect.gameObject.layer = CurrentState.gameObject.layer;
                effect.SkeletonAnimation.Reset();
                effect.SkeletonAnimation.state.SetAnimation(0, "capture_target_fade_in", false);
                effect.SkeletonAnimation.state.AddAnimation(0, "capture_target_loop", true, 0.667f);
            }
        }
    }

    public void SetTargetEffect(FormationUnit performer, string effectId, string fxTarget)
    {
        if (effectId != null)
        {
            var exclusiveAnimation = performer.unitAnimator.Effects.Find(effect => effect.name == effectId);
            var animationObject = exclusiveAnimation != null ? exclusiveAnimation.gameObject :
                Resources.Load<GameObject>("Prefabs/Effects/" + effectId);

            if (animationObject != null)
            {
                AnimatedEffect effect = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (effect != null)
                {
                    effect.SkeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                    effect.BindToTarget(this, CurrentState, fxTarget);
                    effect.gameObject.layer = CurrentState.gameObject.layer;
                }
            }
        }
    }

    public void SetTargetItemEffect(string effectId)
    {
        if (effectId != null)
        {
            GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/" + effectId);

            if (animationObject != null)
            {
                AnimatedEffect effect = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (effect != null)
                {
                    effect.SkeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                    effect.BindToTargetUnit(this, CurrentState, "fxchest");
                    effect.gameObject.layer = CurrentState.gameObject.layer;
                }
            }
        }
    }

    public void SetTargetEffect(FormationUnit performer, string effectId, string fxTarget, string subEffect)
    {
        if (effectId == null)
            return;

        var exclusiveAnimation = performer.unitAnimator.Effects.Find(effect => effect.name == effectId);
        var animationObject = exclusiveAnimation != null ? exclusiveAnimation.gameObject :
            Resources.Load<GameObject>("Prefabs/Effects/" + effectId);

        if (animationObject != null)
        {
            AnimatedEffect effect = Instantiate(animationObject).GetComponent<AnimatedEffect>();
            if (effect != null)
            {
                effect.SkeletonAnimation.MeshRenderer.sortingOrder = CurrentState.MeshRenderer.sortingOrder + 1;
                effect.SkeletonAnimation.Reset();
                effect.BindToTarget(this, CurrentState, fxTarget);
                effect.SkeletonAnimation.state.SetAnimation(0, subEffect, false);
                effect.gameObject.layer = CurrentState.gameObject.layer;
            }
        }
    }

    public void RemoveCaptureEffect()
    {
        foreach (var effect in RectTransform.GetComponentsInChildren<AnimatedEffect>())
            if (effect.gameObject.name.Contains("capture_target"))
            {
                effect.SkeletonAnimation.state.SetAnimation(0, "capture_target_fade_out", false);
                Destroy(effect.gameObject, 0.667f);
            }
    }

    #endregion

    #region Overlay Helpers

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

    #endregion

    #region Movement

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

    #endregion
}