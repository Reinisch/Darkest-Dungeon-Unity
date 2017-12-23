using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterWindow : MonoBehaviour
{
    [SerializeField]
    private Text heroName;
    [SerializeField]
    private Text heroClassName;
    [SerializeField]
    private Image heroGuildHeader;

    [SerializeField]
    private StressPanel stressPanel;
    [SerializeField]
    private ResolveBar resolveBar;
    [SerializeField]
    private QuirksPanel quirksPanel;
    [SerializeField]
    private DiseasePanel diseasePanel;
    [SerializeField]
    private CharStatsPanel charStatsPanel;
    [SerializeField]
    private ResistancesPanel resistancesPanel;
    [SerializeField]
    private CharEquipmentPanel charEquipmentPanel;
    [SerializeField]
    private CharCombatSkillPanel charCombatSkillPanel;
    [SerializeField]
    private CharCampingSkillPanel charCampingSkillPanel;
    [SerializeField]
    private HeroDisplayPanel displayPanel;
    [SerializeField]
    private Text weaponLevel;
    [SerializeField]
    private Text armorLevel;

    public Hero CurrentHero { get; private set; }
    public bool IsOpened { get { return gameObject.activeSelf; } }

    private bool interactable;

    public event Action EventWindowClosed;
    public event Action EventNextButtonClicked;
    public event Action EventPreviousButtonClick;

    private void Awake()
    {
        charEquipmentPanel.EventPanelChanged += UpdateAttributes;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            PreviousButtonClicked();
        else if (Input.GetKeyDown(KeyCode.E))
            NextButtonClicked();
    }

    public void UpdateAttributes()
    {
        charStatsPanel.UpdateStats(CurrentHero);
        quirksPanel.UpdateQuirksPanel(CurrentHero);
        diseasePanel.UpdateDiseasePanel(CurrentHero);
        resistancesPanel.UpdateResistances(CurrentHero);

        stressPanel.UpdateStress(CurrentHero.GetPairedAttribute(AttributeType.Stress).ValueRatio);
    }

    public void UpdateClassInfo(bool isInteractable)
    {
        heroName.text = CurrentHero.HeroName;
        heroClassName.text = LocalizationManager.GetString("hero_class_name_" + CurrentHero.ClassStringId);
        heroGuildHeader.sprite = DarkestDungeonManager.HeroSprites[CurrentHero.ClassStringId].Header;
        
        weaponLevel.text = CurrentHero.WeaponLevel.ToString();
        armorLevel.text = CurrentHero.ArmorLevel.ToString();

        resolveBar.UpdateResolve(CurrentHero);

        charCombatSkillPanel.UpdateCombatSkillPanel(CurrentHero, isInteractable);
        charCampingSkillPanel.UpdateCampingSkillPanel(CurrentHero, isInteractable);
        displayPanel.UpdateDisplay(CurrentHero);
    }

    public void UpdateTrinkets(bool isInteractable)
    {
        if (isInteractable)
            charEquipmentPanel.SetActive();
        else
            charEquipmentPanel.SetDisabled();

        charEquipmentPanel.UpdateEquipmentPanel(CurrentHero, isInteractable);
    }

    public void UpdateCharacterInfo()
    {
        if (CurrentHero == null)
            return;

        UpdateCharacterInfo(CurrentHero, interactable, true);
    }

    public void UpdateCharacterInfo(Hero updateHero, bool interactionAllowed, bool forced = false)
    {
        interactable = interactionAllowed;
        if (CurrentHero == null || CurrentHero.RosterId != updateHero.RosterId || forced)
        {
            CurrentHero = updateHero;
            UpdateClassInfo(interactable);
            UpdateAttributes();
            UpdateTrinkets(interactable);
        }
    }

    public void UpdateRaidCharacterInfo(Hero updateHero, bool interactionAllowed)
    {
        displayPanel.Light.enabled = RaidSceneManager.SceneState == DungeonSceneState.Hall;

        interactable = interactionAllowed;
        if (CurrentHero == null || CurrentHero.RosterId != updateHero.RosterId)
        {
            CurrentHero = updateHero;
            UpdateClassInfo(interactable);
            UpdateAttributes();
            UpdateTrinkets(false);
        }
    }

    public void DismissButtonClicked()
    {
        if (CurrentHero == null || EstateSceneManager.Instanse == null)
            return;

        if (CurrentHero.Status != HeroStatus.Available)
            return;

        if (DarkestDungeonManager.Campaign.Heroes.FindAll(hero =>
            hero.Class != "abomination" && hero.Status != HeroStatus.Missing).Count < 5)
            return;

        DarkestDungeonManager.Campaign.DismissHero(CurrentHero);
        EstateSceneManager.Instanse.RosterPanel.DestroySlot(CurrentHero);
        DarkestSoundManager.ExecuteNarration("dismiss_hero", NarrationPlace.Town);
        WindowClosed();
    }

    public void NextButtonClicked()
    {
        if (EventNextButtonClicked != null)
            EventNextButtonClicked();
    }

    public void PreviousButtonClicked()
    {
        if (EventPreviousButtonClick != null)
            EventPreviousButtonClick();
    }

    public void WindowOpened()
    {
        gameObject.SetActive(true);
    }

    public void WindowClosed()
    {
        if (EventWindowClosed != null)
            EventWindowClosed();

        gameObject.SetActive(false);
        CurrentHero = null;
    }
}