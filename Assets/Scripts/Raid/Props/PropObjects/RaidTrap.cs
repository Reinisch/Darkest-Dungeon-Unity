using UnityEngine;
using UnityEngine.EventSystems;

public class RaidTrap : RaidProp, IPointerEnterHandler, IPointerExitHandler
{
    public SkeletonAnimation SkeletonAnimation { get; set; }
    public bool Activated { get; set; }

    public override void Initialize(IRaidArea areaView)
    {
        AreaView = areaView;
        HasTarget = false;
        TargetSmoothTime = 0.1f;
        RectTransform = GetComponent<RectTransform>();
        SkeletonAnimation = GetComponent<SkeletonAnimation>();

        PropType = PropType.Trap;
        Activated = false;
    }

    public override void Activate()
    {
        Activated = true;
    }

    public override void SetSortingOrder(int order)
    {
        SkeletonAnimation.MeshRenderer.sortingOrder = order;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Activated)
            RaidSceneManager.Instanse.ActivateTrap(AreaView as RaidHallSector, false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Activated)
        {
            SkeletonAnimation.state.ClearTracks();
            SkeletonAnimation.state.SetAnimation(0, "active", false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Activated)
        {
            SkeletonAnimation.state.ClearTracks();
            SkeletonAnimation.state.SetAnimation(0, "idle", false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Activated)
            AreaView.OnPointerClick(eventData);
    }
}