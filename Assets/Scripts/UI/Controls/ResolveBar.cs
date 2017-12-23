using UnityEngine;
using UnityEngine.UI;

public class ResolveBar : MonoBehaviour
{
    [SerializeField]
    private Text levelText;
    [SerializeField]
    private Slider resolveXpBarSlider;
    [SerializeField]
    private Image resolveIcon;

    public void UpdateResolve(Hero hero)
    {
        levelText.text = hero.Resolve.Level.ToString();
        resolveXpBarSlider.value = hero.Resolve.Ratio;
        resolveIcon.sprite = hero.Resolve.Level > 0 ?
            DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background_lvl" + hero.Resolve.Level] :
            DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background"];
    }

    public void UpdateEmpty()
    {
        levelText.text = "";
        resolveXpBarSlider.value = 0;
        resolveIcon.sprite = DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background"];
    }
}
