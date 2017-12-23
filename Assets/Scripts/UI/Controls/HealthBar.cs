using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Image healthImage;

    public Image HealthImage { get { return healthImage; } }

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
