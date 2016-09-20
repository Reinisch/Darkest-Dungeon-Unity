using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResolveBar : MonoBehaviour
{
    public Text levelText;
    public Slider resolveXpBarSlider;
    public Image resolveIcon;

    public void UpdateResolve(Hero hero)
    {
        levelText.text = hero.Resolve.Level.ToString();
        resolveXpBarSlider.value = hero.Resolve.Ratio;
        if(hero.Resolve.Level > 0)
            resolveIcon.sprite = DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background_lvl" + hero.Resolve.Level.ToString()];
        else
            resolveIcon.sprite = DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background"];
    }
    public void UpdateEmpty()
    {
        levelText.text = "";
        resolveXpBarSlider.value = 0;
        resolveIcon.sprite = DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background"];
    }
}
