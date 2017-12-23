using UnityEngine;

public class RaidInventoryPanel : MonoBehaviour
{
    [SerializeField]
    private PartyInventory partyInventory;

    public PartyInventory PartyInventory { get { return partyInventory; } }
    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}
