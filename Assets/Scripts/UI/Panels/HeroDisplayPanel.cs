using UnityEngine;
using System.Collections.Generic;

public class HeroDisplayPanel : MonoBehaviour
{
    [SerializeField]
    private List<SkeletonAnimation> heroes;

    public Light Light { get; private set; }

    private SkeletonAnimation currentDisplay;

    private void Awake()
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
