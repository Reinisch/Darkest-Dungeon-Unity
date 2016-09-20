using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RaidInventoryPanel : MonoBehaviour
{
    public PartyInventory partyInventory;

    public Button switchButton;

    public RectTransform RectTransform { get; set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}
