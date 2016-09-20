using UnityEngine;
using System.Collections;

public class RaidDoor : RaidProp
{
    public SkeletonAnimation SkeletonAnimation { get; set; }

    public override void Initialize(IRaidArea areaView)
    {
        AreaView = areaView;
        HasTarget = false;
        TargetSmoothTime = 0.1f;
        RectTransform = GetComponent<RectTransform>();
        SkeletonAnimation = GetComponent<SkeletonAnimation>();
        PropType = PropType.Door;
    }
    public override void Activate()
    {
        base.Activate();
    }
    public override void SetSortingOrder(int order)
    {

    }

    public void Open()
    {
        SkeletonAnimation.state.ClearTracks();
        SkeletonAnimation.state.SetAnimation(0, "open", false);
    }
    public void Close()
    {
        SkeletonAnimation.state.ClearTracks();
        SkeletonAnimation.state.SetAnimation(0, "closed", true);
    }
}
