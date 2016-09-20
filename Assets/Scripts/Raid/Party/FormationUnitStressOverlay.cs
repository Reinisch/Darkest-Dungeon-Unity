using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FormationUnitStressOverlay : MonoBehaviour
{
    const int StressPipCount = 10;

    List<StressPip> stressPips;

    void Awake()
    {
        stressPips = new List<StressPip>();

        foreach (var slot in gameObject.GetComponentsInChildren<RectTransform>())
            stressPips.Add(slot.GetComponentInChildren<StressPip>());
    }

    public void UpdateStress(float stressPercentage)
    {
        int stressedPips = Mathf.RoundToInt(stressPercentage * StressPipCount);
        for (int i = 0; i < stressedPips; i++)
            stressPips[i].StressController.SetBool("Stressed", true);
        for (int i = stressedPips; i < StressPipCount; i++)
            stressPips[i].StressController.SetBool("Stressed", false);
    }
}
