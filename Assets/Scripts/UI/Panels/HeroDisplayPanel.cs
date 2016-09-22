using UnityEngine;
using System.Collections.Generic;

public class HeroDisplayPanel : MonoBehaviour
{
    public List<SkeletonAnimation> heroes;

    SkeletonAnimation currentDisplay = null;
    public Light Light { get; set; }

    void Awake()
    {
        Light = GetComponentInChildren<Light>();
        var canvas = GetComponentInParent<Canvas>();
        for (int i = 0; i < heroes.Count; i++)
            heroes[i].MeshRenderer.sortingOrder = canvas.sortingOrder + 1;
    }

    public void UpdateDisplay(Hero hero)
    {
        if(currentDisplay != null)
            currentDisplay.gameObject.SetActive(false);

        currentDisplay = heroes.Find(displayHero => displayHero.name == hero.Class);
        currentDisplay.gameObject.SetActive(true);
    }
}
