using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeWindow : MonoBehaviour
{
    [SerializeField]
    private Text upgradedValue;
    [SerializeField]
    private List<BuildingUpgradeTreeSlot> upgradeTrees;

    public Text UpgradedValue { get { return upgradedValue; } }
    public List<BuildingUpgradeTreeSlot> UpgradeTrees { get { return upgradeTrees; } }
}
