using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StressOverlayPanel : MonoBehaviour
{
    [SerializeField]
    private Sprite normalPip;
    [SerializeField]
    private Sprite stressedPip;
    [SerializeField]
    private Sprite overstressedPip;
    [SerializeField]
    private List<Image> stressPips;

    public void UpdateStress(float stressPercentage)
    {
        if (stressPercentage > 0.5f)
        {
            stressPercentage = (stressPercentage - 0.5f) * 2;
            int overstressedPips = Mathf.RoundToInt(stressPercentage * stressPips.Count);

            for (int i = 0; i < overstressedPips; i++)
                stressPips[i].sprite = overstressedPip;
            for (int i = overstressedPips; i < stressPips.Count; i++)
                stressPips[i].sprite = stressedPip;
        }
        else
        {
            stressPercentage *= 2;
            int stressedPips = Mathf.RoundToInt(stressPercentage * stressPips.Count);

            for (int i = 0; i < stressedPips; i++)
                stressPips[i].sprite = stressedPip;
            for (int i = stressedPips; i < stressPips.Count; i++)
                stressPips[i].sprite = normalPip;
        }
    }
}
