using UnityEngine;
using UnityEngine.UI;

public class PositionedElement : MonoBehaviour
{
    [SerializeField]
    private LayoutElement element;
    [SerializeField]
    private RectTransform rectTransform;

    private const int StandardUnitWidth = 170;

    public void SetUnit(FormationUnit unit, float time, bool isSingle = false)
    {
        if(isSingle)
            element.preferredWidth = StandardUnitWidth;
        else
            element.preferredWidth = StandardUnitWidth * unit.Size;
        unit.SetRectTarget(rectTransform, time);
        gameObject.SetActive(true);
    }

    public void SetTrap(RaidTrap trap, float time)
    {
        element.preferredWidth = StandardUnitWidth;
        trap.SetRectTarget(rectTransform, time);
        gameObject.SetActive(true);
    }

    public void Reset()
    {
        gameObject.SetActive(false);
    }
}