using UnityEngine;
using System.Collections.Generic;

public class SkillUpgradeTreeSlot : MonoBehaviour
{
    private const int ConnectorWidthPerSlot = 75;

    [SerializeField]
    private RectTransform connector;
    [SerializeField]
    private SkillPurchaseSlot currentSkill;
    [SerializeField]
    private List<SkillUpgradeSlot> upgrades;

    public SkillPurchaseSlot CurrentSkill { get { return currentSkill; } }
    public List<SkillUpgradeSlot> Upgrades { get { return upgrades; } }

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
