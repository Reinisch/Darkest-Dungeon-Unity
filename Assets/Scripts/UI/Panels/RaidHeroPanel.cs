using UnityEngine;
using UnityEngine.UI;

public class RaidHeroPanel : MonoBehaviour
{
    [SerializeField]
    private CharEquipmentPanel equipmentPanel;
    [SerializeField]
    private RaidCharStatsPanel charStatsPanel;
    [SerializeField]
    private Text weaponLevel;
    [SerializeField]
    private Text armorLevel;

    public CharEquipmentPanel EquipmentPanel {  get { return equipmentPanel; } }
    public RaidCharStatsPanel CharStatsPanel { get { return charStatsPanel; } }

    private Hero selectedHero;

    private void Awake()
    {
        EquipmentPanel.EventPanelChanged += UpdateAttributes;
    }

    private void UpdateAttributes()
    {
        CharStatsPanel.UpdateStats(selectedHero);
    }

    public void UpdateHero()
    {
        selectedHero = RaidSceneManager.RaidPanel.SelectedHero;

        weaponLevel.text = selectedHero.WeaponLevel.ToString();
        armorLevel.text = selectedHero.ArmorLevel.ToString();
        EquipmentPanel.UpdateEquipmentPanel(selectedHero, true);
        CharStatsPanel.UpdateStats(selectedHero);
    }
}
