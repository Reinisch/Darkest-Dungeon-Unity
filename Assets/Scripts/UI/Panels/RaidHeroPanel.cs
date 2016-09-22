using UnityEngine;
using UnityEngine.UI;

public class RaidHeroPanel : MonoBehaviour
{
    public CharEquipmentPanel equipmentPanel;
    public RaidCharStatsPanel charStatsPanel;

    public Text weaponLevel;
    public Text armorLevel;

    Hero SelectedHero;

    void Awake()
    {
        equipmentPanel.onPanelChanged += UpdateAttributes;
    }
    void UpdateAttributes()
    {
        charStatsPanel.UpdateStats(SelectedHero);
    }

    public void UpdateHero()
    {
        SelectedHero = RaidSceneManager.RaidPanel.SelectedHero;

        weaponLevel.text = SelectedHero.WeaponLevel.ToString();
        armorLevel.text = SelectedHero.ArmorLevel.ToString();
        equipmentPanel.UpdateEquipmentPanel(SelectedHero, true);
        charStatsPanel.UpdateStats(SelectedHero);
    }
}
