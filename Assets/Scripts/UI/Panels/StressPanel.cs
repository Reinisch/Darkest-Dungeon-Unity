using UnityEngine;
using System.Collections.Generic;

public class StressPanel : MonoBehaviour
{
    const int StressPipCount = 10;

    public List<StressPip> stressPips;

    public void UpdateStress(float stressPercentage)
    {
        int stressedPips = Mathf.RoundToInt(stressPercentage * StressPipCount);
        for (int i = 0; i < stressedPips; i++)
            stressPips[i].StressController.SetBool("Stressed", true);
        for (int i = stressedPips; i < StressPipCount; i++)
            stressPips[i].StressController.SetBool("Stressed", false);
    }
}
