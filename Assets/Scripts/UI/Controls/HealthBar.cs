using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public RectTransform rectTransform;
    public Slider healthSlider;
    public Image healthImage;

    public void SetSize(Character character)
    {
        rectTransform.sizeDelta = new Vector2(100.2f * character.Size, 9.8f);
    }
    public void UpdateHealth(Character character)
    {
        healthSlider.value = character.GetPairedAttribute(AttributeType.HitPoints).ValueRatio;
    }
    public void UpdateHealth(PairedAttribute health)
    {
        healthSlider.value = health.ValueRatio;
    }
}
