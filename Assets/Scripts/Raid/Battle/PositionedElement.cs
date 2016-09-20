using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PositionedElement : MonoBehaviour
{
    const int standardUnitWidth = 170;

    public LayoutElement element;
    public RectTransform rectTransform;

    public void SetUnit(FormationUnit unit, float time, bool isSingle = false)
    {
        if(isSingle)
            element.preferredWidth = standardUnitWidth;
        else
            element.preferredWidth = standardUnitWidth * unit.Size;
        unit.SetRectTarget(rectTransform, time);
        gameObject.SetActive(true);
    }
    public void SetTrap(RaidTrap trap, float time)
    {
        element.preferredWidth = standardUnitWidth;
        trap.SetRectTarget(rectTransform, time);
        gameObject.SetActive(true);
    }
    public void Reset()
    {
        gameObject.SetActive(false);
    }
}
