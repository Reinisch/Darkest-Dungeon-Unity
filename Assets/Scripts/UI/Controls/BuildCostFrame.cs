using UnityEngine;
using UnityEngine.UI;

public class BuildCostFrame : MonoBehaviour
{
    [SerializeField]
    private Image heirloomOneIcon;
    [SerializeField]
    private Text heirloomOneAmount;
    [SerializeField]
    private Image heirloomTwoIcon;
    [SerializeField]
    private Text heirloomTwoAmount;

    public Image HeirloomOneIcon { get { return heirloomOneIcon; } }
    public Text HeirloomOneAmount { get { return heirloomOneAmount; } }
    public Image HeirloomTwoIcon { get { return heirloomTwoIcon; } }
    public Text HeirloomTwoAmount { get { return heirloomTwoAmount; } }
}
