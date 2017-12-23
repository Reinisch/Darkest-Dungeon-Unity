using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PositionSet : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private HorizontalLayoutGroup layoutGroup;
    [SerializeField]
    private List<PositionedElement> elements;

    private Vector2 smoothTarget = Vector2.zero;
    private Vector2 targetValues = Vector2.zero;
    private Vector2 velocity = Vector2.zero;
    private float smoothTime;

    private void Update()
    {
        smoothTarget.Set(layoutGroup.spacing, rectTransform.anchoredPosition.x);
        var target = Vector2.SmoothDamp(smoothTarget, targetValues, ref velocity, smoothTime, float.MaxValue, Time.deltaTime);

        layoutGroup.spacing = target.x;

        rectTransform.anchoredPosition = new Vector2(target.y, rectTransform.anchoredPosition.y);
    }

    public void SetUnitTarget(FormationUnit unit, float time, Vector2 offset)
    {
        gameObject.SetActive(true);

        targetValues = Vector3.zero;
        layoutGroup.spacing = 0;
        smoothTime = 0;

        rectTransform.anchoredPosition = offset;
        targetValues = new Vector2(0, rectTransform.anchoredPosition.x);

        elements[0].SetUnit(unit, time, true);

        for (int i = 1; i < elements.Count; i++)
            elements[i].Reset();
    }

    public void SetUnitTargets(List<FormationUnit> units, float time, Vector2 offset)
    {
        gameObject.SetActive(true);

        targetValues = Vector3.zero;
        layoutGroup.spacing = 0;
        smoothTime = 0;

        rectTransform.anchoredPosition = offset;
        targetValues = new Vector2(0, rectTransform.anchoredPosition.x);

        int positionedUnits = Mathf.Min(units.Count, elements.Count);
        for(int i = 0; i < positionedUnits; i++)
            elements[i].SetUnit(units[i], time);

        for (int i = positionedUnits; i < elements.Count; i++)
            elements[i].Reset();
    }

    public void SetUnitTargets(FormationUnit unit, float time, Vector2 offset)
    {
        gameObject.SetActive(true);

        targetValues = Vector3.zero;
        layoutGroup.spacing = 0;
        smoothTime = 0;

        rectTransform.anchoredPosition = offset;
        targetValues = new Vector2(0, rectTransform.anchoredPosition.x);

        elements[0].SetUnit(unit, time, true);
        for (int i = 1; i < elements.Count; i++)
            elements[i].Reset();
    }

    public void SetTrap(RaidTrap trap, float time, Vector2 offset)
    {
        gameObject.SetActive(true);

        targetValues = Vector3.zero;
        layoutGroup.spacing = 0;
        smoothTime = 0;

        rectTransform.anchoredPosition = offset;

        int positionedUnits = 1;
        elements[0].SetTrap(trap, time);

        for (int i = positionedUnits; i < elements.Count; i++)
            elements[i].Reset();
    }

    public void SetSpacing(float spacing, float time)
    {
        targetValues = new Vector2(spacing, rectTransform.anchoredPosition.x);
        smoothTime = time;
    }

    public void SetSliding(float sliding, float time)
    {
        targetValues = new Vector2(0, rectTransform.anchoredPosition.x + sliding);
        smoothTime = time;
    }

    public void Reset()
    {
        gameObject.SetActive(false);
    }
}