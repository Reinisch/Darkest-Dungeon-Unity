using UnityEngine;
using System.Collections.Generic;

public class EquipmentUpgradeTreeSlot : MonoBehaviour
{
    [SerializeField]
    private RectTransform connector;
    [SerializeField]
    private EquipmentSlot currentEquipment;
    [SerializeField]
    private List<EquipmentUpgradeSlot> upgrades;

    public EquipmentSlot CurrentEquipment { get { return currentEquipment; } set { currentEquipment = value; } }
    public List<EquipmentUpgradeSlot> Upgrades { get { return upgrades; } set { upgrades = value; } }

    private const int ConnectorWidthPerSlot = 75;

    public void UpdateConnector(int lastPurchasedIndex)
    {
        if (lastPurchasedIndex == -1)
            connector.sizeDelta = new Vector2(0, connector.sizeDelta.y);
        else
        {
            if (lastPurchasedIndex >= Upgrades.Count)
                lastPurchasedIndex = Upgrades.Count - 1;

            connector.sizeDelta = new Vector2(ConnectorWidthPerSlot * (lastPurchasedIndex + 1), connector.sizeDelta.y);
        }
    }
}
