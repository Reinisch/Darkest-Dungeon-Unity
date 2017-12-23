using UnityEngine;
using System.Collections.Generic;

public class BuildingUpgradeTreeSlot : MonoBehaviour
{
    [SerializeField]
    private string treeId;
    [SerializeField]
    private RectTransform connector;
    [SerializeField]
    private List<BuildingUpgradeSlot> upgrades;

    public string TreeId { get { return treeId; } }
    public List<BuildingUpgradeSlot> Upgrades { get { return upgrades; } }

    private const int ConnectorWidthPerSlot = 75;

    public void UpdateConnector(int lastPurchasedIndex)
    {
        if (lastPurchasedIndex == -1)
            connector.sizeDelta = new Vector2(0, connector.sizeDelta.y);
        else
        {
            if (lastPurchasedIndex >= Upgrades.Count)
                lastPurchasedIndex = Upgrades.Count - 1;

            connector.sizeDelta = new Vector2(ConnectorWidthPerSlot*(lastPurchasedIndex + 1), connector.sizeDelta.y);
        }
    }
}
