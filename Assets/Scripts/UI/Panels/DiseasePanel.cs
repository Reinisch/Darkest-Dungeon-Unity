using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiseasePanel : MonoBehaviour
{
    public List<QuirkSlot> diseaseQuirkSlots;

    public void UpdateDiseasePanel(Hero hero)
    {
        int diseases = 0;
        foreach (var diseaseQuirk in hero.Diseases)
            diseaseQuirkSlots[diseases++].UpdateQuirk(diseaseQuirk);

        for (int i = diseases; i < diseaseQuirkSlots.Count; i++)
            diseaseQuirkSlots[i].ResetSlot();
    }
}
