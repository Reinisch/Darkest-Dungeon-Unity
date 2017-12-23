using UnityEngine;
using UnityEngine.EventSystems;

public class RaidObstacle : RaidProp
{
    public SkeletonAnimation SkeletonAnimation { get; set; }
    public bool Removed { get; set; }

    public override void Initialize(IRaidArea areaView)
    {
        AreaView = areaView;
        HasTarget = false;
        TargetSmoothTime = 0.1f;
        RectTransform = GetComponent<RectTransform>();
        SkeletonAnimation = GetComponent<SkeletonAnimation>();
        PropType = PropType.Obstacle;
        Removed = false;
    }

    public override void Activate()
    {
        Removed = true;
        SkeletonAnimation.state.SetAnimation(0, "clear", false);
        if(SkeletonAnimation.gameObject.name.StartsWith("thorny_thicket"))
            SkeletonAnimation.state.GetCurrent(0).Time = 0.2f;
    }

    public override void SetSortingOrder(int order)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Removed)
            AreaView.OnPointerClick(eventData);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!Removed)
            RaidSceneManager.Instanse.EncounterObstacle(AreaView as RaidHallSector);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!Removed)
            RaidSceneManager.Instanse.LeaveObstacle(AreaView as RaidHallSector);
    }
}