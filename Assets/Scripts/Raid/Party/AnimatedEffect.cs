using UnityEngine;

public class AnimatedEffect : MonoBehaviour
{
    public BoneFollower follower;
    public SkeletonAnimation skeletonAnimation;
    public Transform currentTransform;

    public bool IsLooping { get; set; }

    private FormationUnit followUnit;
    private string followBoneName;

    void Start()
    {
        if(IsLooping == false)
            Destroy(gameObject, skeletonAnimation.state.GetCurrent(0).Animation.Duration);
    }

    void LateUpdate()
    {
        if (follower != null)
        {
            if(followUnit != null && (follower.SkeletonRenderer == null || 
                follower.SkeletonRenderer.gameObject.activeSelf == false))
            {
                if (followUnit != null)
                {
                    follower.SkeletonRenderer = followUnit.CurrentState;

                    follower.followBoneRotation = false;
                    follower.followZPosition = true;
                    follower.bone = followUnit.CurrentState.Skeleton.FindBone(followBoneName);
                    skeletonAnimation.MeshRenderer.sortingOrder = followUnit.CurrentState.MeshRenderer.sortingOrder + 1;
                }
            }
        }
    }

    public void BindToTargetUnit(FormationUnit unit, SkeletonAnimation animation, string boneName)
    {
        followUnit = unit;
        followBoneName = boneName;

        gameObject.SetActive(true);
        follower.SkeletonRenderer = animation;
        currentTransform.SetParent(unit.RectTransform, false);

        follower.followBoneRotation = false;
        follower.followZPosition = true;
        follower.bone = animation.Skeleton.FindBone(boneName);
        follower.DoUpdate();
        follower.enabled = false;
        Destroy(follower);

        skeletonAnimation.MeshRenderer.sortingOrder = animation.MeshRenderer.sortingOrder + 1;
    }
    public void BindToTarget(FormationUnit unit, SkeletonAnimation animation, string boneName)
    {
        followUnit = unit;
        followBoneName = boneName;

        gameObject.SetActive(true);
        follower.SkeletonRenderer = animation;

        follower.followBoneRotation = false;
        follower.followZPosition = true;
        follower.bone = animation.Skeleton.FindBone(boneName);
        skeletonAnimation.MeshRenderer.sortingOrder = animation.MeshRenderer.sortingOrder + 1;
        currentTransform.SetParent(unit.RectTransform, false);
    }
    public void BindToTarget(RectTransform rect, SkeletonAnimation animation, string boneName)
    {
        gameObject.SetActive(true);
        follower.SkeletonRenderer = animation;

        follower.followBoneRotation = false;
        follower.followZPosition = true;
        follower.bone = animation.Skeleton.FindBone(boneName);
        skeletonAnimation.MeshRenderer.sortingOrder = animation.MeshRenderer.sortingOrder + 1;
        currentTransform.SetParent(rect, false);
    }
}
