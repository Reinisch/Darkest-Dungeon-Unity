using UnityEngine;

public enum PropType { Door, Curio, Obstacle, Trap }

public class RaidProp : MonoBehaviour
{
    public bool HasTarget { get; set; }
    public Vector3 StartingPosition { get; set; }
    public Vector3 TargetPosition { get; set; }
    public float TargetSmoothTime { get; set; }

    public IRaidArea AreaView { get; set; }
    public RectTransform RectTransform { get; set; }
    public Animator Animator { get; set; }
    public PropType PropType { get; set; }

    private Vector3 velocity = Vector3.zero;
    private bool hasRectTarget;
    private RectTransform rectTarget;

    private void Update()
    {
        if (hasRectTarget)
            RectTransform.position = Vector3.SmoothDamp(RectTransform.position, rectTarget.position, ref velocity, TargetSmoothTime);
        else if (HasTarget)
        {
            RectTransform.position = Vector3.SmoothDamp(RectTransform.position, TargetPosition, ref velocity, TargetSmoothTime);
            if (RectTransform.position == TargetPosition)
                HasTarget = false;
        }
    }

    public void Relocate(Vector3 target, float smoothTime)
    {
        StartingPosition = RectTransform.position;
        TargetSmoothTime = smoothTime;
        TargetPosition = target;
        HasTarget = true;
    }

    public void SetRectTarget(RectTransform target, float time)
    {
        HasTarget = false;
        hasRectTarget = true;
        rectTarget = target;
        TargetSmoothTime = time;
    }

    public void Return(float smoothTime)
    {
        TargetSmoothTime = smoothTime;
        TargetPosition = StartingPosition;
        HasTarget = true;
        hasRectTarget = false;
    }

    public virtual void Initialize(IRaidArea areaView)
    {
        AreaView = areaView;
        HasTarget = false;
        TargetSmoothTime = 0.1f;
        RectTransform = GetComponent<RectTransform>();
        Animator = GetComponent<Animator>();
    }

    public virtual void Activate()
    {
    }

    public virtual void SetSortingOrder(int order)
    {
    }
}