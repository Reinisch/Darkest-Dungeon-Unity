using UnityEngine;
using System.Collections.Generic;

public class UnitAnimator : MonoBehaviour
{
    public Animator Animator;

    public List<SkeletonAnimation> SkeletonAnimations;
    public List<SkeletonAnimation> Effects;

    private void Awake()
    {
        if(Effects != null)
        foreach (var skelAnim in Effects)
            skelAnim.MeshRenderer.sortingOrder = 9;

        foreach (var skelAnim in SkeletonAnimations)
                skelAnim.MeshRenderer.sortingOrder = 8;
    }

    public void SetSortingOrder(int order)
    {
        foreach (var skelAnim in SkeletonAnimations)
                skelAnim.MeshRenderer.sortingOrder = order;

        if (Effects != null)
            foreach (var effectAnim in Effects)
                effectAnim.MeshRenderer.sortingOrder = order + 1;
    }

    public void Reset(int animatorIndex)
    {
        if (SkeletonAnimations[animatorIndex].Skeleton == null)
            SkeletonAnimations[animatorIndex].Reset();
        else
        {
            if (SkeletonAnimations[animatorIndex].state.GetCurrent(0) != null)
            {
                SkeletonAnimations[animatorIndex].Skeleton.SetToSetupPose();
                SkeletonAnimations[animatorIndex].AnimationName = SkeletonAnimations[animatorIndex].name;
                SkeletonAnimations[animatorIndex].state.GetCurrent(0).Time = 0;
                SkeletonAnimations[animatorIndex].state.SetAnimation(0, SkeletonAnimations[animatorIndex].name, false).Time = 0;
                SkeletonAnimations[animatorIndex].Update(0);
            }
            else
            {
                SkeletonAnimations[animatorIndex].Skeleton.SetToSetupPose();
                SkeletonAnimations[animatorIndex].AnimationName = SkeletonAnimations[animatorIndex].name;
                SkeletonAnimations[animatorIndex].state.SetAnimation(0, SkeletonAnimations[animatorIndex].name, false);
            }
        }
    }

    public void ResetLoop(int animatorIndex)
    {
        if (SkeletonAnimations[animatorIndex].Skeleton == null)
            SkeletonAnimations[animatorIndex].Reset();
        else
        {
            SkeletonAnimations[animatorIndex].state.SetAnimation(0, SkeletonAnimations[animatorIndex].AnimationName, true);
        }
    }

    public void CorpseDeath()
    {
        if (SkeletonAnimations[2].Skeleton == null)
            SkeletonAnimations[2].Reset();

        SkeletonAnimations[2].state.SetAnimation(0, "death", false);
    }

    public void Death()
    {
        if (SkeletonAnimations[1].Skeleton == null)
            SkeletonAnimations[1].Reset();

        SkeletonAnimations[1].AnimationName = "death";
        SkeletonAnimations[1].state.SetAnimation(0, "death", false).Time = 0;
    }

    public void TransformHuman()
    {
        if (SkeletonAnimations[14].Skeleton == null)
            SkeletonAnimations[14].Reset();
        SkeletonAnimations[14].state.SetAnimation(0, "attack_transform_human", false).Time = 0;
        SkeletonAnimations[14].Update(0);
        SkeletonAnimations[14].AnimationName = "attack_transform_human";
    }

    public void TransformBeast()
    {
        if (SkeletonAnimations[14].Skeleton == null)
            SkeletonAnimations[14].Reset();
        SkeletonAnimations[14].state.SetAnimation(0, "attack_transform_beast", false).Time = 0;
        SkeletonAnimations[14].Update(0);
        SkeletonAnimations[14].AnimationName = "attack_transform_beast";
    }

    public void CombatHuman()
    {
        if (SkeletonAnimations[0].Skeleton == null)
            SkeletonAnimations[0].Reset();

        if (SkeletonAnimations[0].AnimationName != "combat_human")
        {
            SkeletonAnimations[0].state.SetAnimation(0, "combat_human", false).Time = 0;
            SkeletonAnimations[0].Update(0);
        }

        SkeletonAnimations[0].state.SetAnimation(0, "combat_human", true).Time = 0;
    }

    public void CombatBeast()
    {
        if (SkeletonAnimations[0].Skeleton == null)
            SkeletonAnimations[0].Reset();

        if (SkeletonAnimations[0].AnimationName != "combat_beast")
        {
            SkeletonAnimations[0].state.SetAnimation(0, "combat_beast", false).Time = 0;
            SkeletonAnimations[0].Update(0);
        }

        SkeletonAnimations[0].state.SetAnimation(0, "combat_beast", true).Time = 0;
    }

    public void HeroicBeast()
    {
        if (SkeletonAnimations[6].skeleton == null)
            SkeletonAnimations[6].Reset();

        SkeletonAnimations[6].state.SetAnimation(0, "heroic_beast", false).Time = 0;
        SkeletonAnimations[6].Update(0);
        SkeletonAnimations[6].AnimationName = "heroic_beast";
    }

    public void HeroicHuman()
    {
        if(SkeletonAnimations[6].skeleton == null)
            SkeletonAnimations[6].Reset();

        SkeletonAnimations[6].state.SetAnimation(0, "heroic_human", false).Time = 0;
        SkeletonAnimations[6].Update(0);
        SkeletonAnimations[6].AnimationName = "heroic_human";
    }

    public void DefendHuman()
    {
        if (SkeletonAnimations[1].Skeleton == null)
            SkeletonAnimations[1].Reset();
        SkeletonAnimations[1].state.SetAnimation(0, "defend_human", false).Time = 0;
        SkeletonAnimations[1].Update(0);
        SkeletonAnimations[1].AnimationName = "defend_human";
    }

    public void DefendBeast()
    {
        if (SkeletonAnimations[1].Skeleton == null)
            SkeletonAnimations[1].Reset();
        SkeletonAnimations[1].state.SetAnimation(0, "defend_beast", false).Time = 0;
        SkeletonAnimations[1].Update(0);
        SkeletonAnimations[1].AnimationName = "defend_beast";
    }

    public void DeathHuman()
    {
        if (SkeletonAnimations[1].Skeleton == null)
            SkeletonAnimations[1].Reset();
        SkeletonAnimations[1].state.SetAnimation(0, "death_human", false).Time = 0;
        SkeletonAnimations[1].Update(0);
        SkeletonAnimations[1].AnimationName = "death_human";
    }

    public void DeathBeast()
    {
        if (SkeletonAnimations[1].Skeleton == null)
            SkeletonAnimations[1].Reset();
        SkeletonAnimations[1].state.SetAnimation(0, "death_beast", false).Time = 0;
        SkeletonAnimations[1].Update(0);
        SkeletonAnimations[1].AnimationName = "death_beast";
    }

    public void DeathIndexed(int index)
    {
        if (SkeletonAnimations[index].Skeleton == null)
            SkeletonAnimations[index].Reset();

        SkeletonAnimations[index].AnimationName = "death";
        SkeletonAnimations[index].state.SetAnimation(0, "death", false).Time = 0;
    }
}