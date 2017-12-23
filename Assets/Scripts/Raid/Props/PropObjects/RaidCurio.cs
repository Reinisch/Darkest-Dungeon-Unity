using UnityEngine;
using UnityEngine.EventSystems;

public class RaidCurio : RaidProp, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SkeletonAnimation SkeletonAnimation { get; set; }
    public bool Investigated { get; set; }

    public override void Initialize(IRaidArea areaView)
    {
        AreaView = areaView;
        HasTarget = false;
        TargetSmoothTime = 0.1f;
        RectTransform = GetComponent<RectTransform>();
        SkeletonAnimation = GetComponent<SkeletonAnimation>();

        PropType = PropType.Curio;
        Investigated = false;
    }

    public override void Activate()
    {
        SkeletonAnimation.state.ClearTracks();
        SkeletonAnimation.state.SetAnimation(0, "investigate", false);
        Investigated = true;
    }

    public override void SetSortingOrder(int order)
    {
        SkeletonAnimation.MeshRenderer.sortingOrder = order;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Investigated)
        {
            SkeletonAnimation.state.ClearTracks();
            SkeletonAnimation.state.SetAnimation(0, "active", true);          
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Investigated)
        {
            SkeletonAnimation.state.ClearTracks();
            SkeletonAnimation.state.SetAnimation(0, "idle", true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Investigated)
            AreaView.OnPointerClick(eventData);
    }
}
