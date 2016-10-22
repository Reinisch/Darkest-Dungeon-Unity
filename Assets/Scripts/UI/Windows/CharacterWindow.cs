using UnityEngine;
using UnityEngine.UI;

public delegate void WindowEvent();

public class CharacterWindow : MonoBehaviour
{
    public Text heroName;
    public Text heroClassName;

    public Button renameButton;
    public Button dismissButton;

    public Image heroGuildHeader;

    public StressPanel stressPanel;
    public ResolveBar resolveBar;
    public QuirksPanel quirksPanel;
    public DiseasePanel diseasePanel;
    public CharStatsPanel charStatsPanel;
    public ResistancesPanel resistancesPanel;
    public CharEquipmentPanel charEquipmentPanel;
    public CharCombatSkillPanel charCombatSkillPanel;
    public CharCampingSkillPanel charCampingSkillPanel;
    public HeroDisplayPanel displayPanel;

    public Text weaponLevel;
    public Text armorLevel;

    public Hero CurrentHero { get; set; }

    bool interactable;

    public event WindowEvent onWindowClose;
    public event WindowEvent onNextButtonClick;
    public event WindowEvent onPreviousButtonClick;

    public bool IsOpened
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    void Awake()
    {
        charEquipmentPanel.onPanelChanged += UpdateAttributes;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            PreviousButtonClicked();
        else if (Input.GetKeyDown(KeyCode.E))
            NextButtonClicked();
    }

    void UpdateAttributes()
    {
        charStatsPanel.UpdateStats(CurrentHero);
        quirksPanel.UpdateQuirksPanel(CurrentHero);
        diseasePanel.UpdateDiseasePanel(CurrentHero);
        resistancesPanel.UpdateResistances(CurrentHero);

        stressPanel.UpdateStress(CurrentHero.GetPairedAttribute(AttributeType.Stress).ValueRatio);
    }
    void UpdateClassInfo(bool interactable)
    {
        heroName.text = CurrentHero.HeroName;
        heroClassName.text = LocalizationManager.GetString("hero_class_name_" + CurrentHero.ClassStringId);
        heroGuildHeader.sprite = DarkestDungeonManager.HeroSprites[CurrentHero.ClassStringId].Header;
        
        weaponLevel.text = CurrentHero.WeaponLevel.ToString();
        armorLevel.text = CurrentHero.ArmorLevel.ToString();

        resolveBar.UpdateResolve(CurrentHero);

        charCombatSkillPanel.UpdateCombatSkillPanel(CurrentHero, interactable);
        charCampingSkillPanel.UpdateCampingSkillPanel(CurrentHero, interactable);
        displayPanel.UpdateDisplay(CurrentHero);
    }
    void UpdateTrinkets(bool interactable)
    {
        if (interactable)
            charEquipmentPanel.SetActive();
        else
            charEquipmentPanel.SetDisabled();

        charEquipmentPanel.UpdateEquipmentPanel(CurrentHero, interactable);
    }

    public void UpdateCharacterInfo(Hero updateHero, bool interactionAllowed)
    {
        interactable = interactionAllowed;
        if (CurrentHero == null || CurrentHero.RosterId != updateHero.RosterId)
        {
            CurrentHero = updateHero;
            UpdateClassInfo(interactable);
            UpdateAttributes();
            UpdateTrinkets(interactable);
        }
    }
    public void UpdateRaidCharacterInfo(Hero updateHero, bool interactionAllowed)
    {
        if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
            displayPanel.Light.enabled = true;
        else
            displayPanel.Light.enabled = false;

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
        EstateSceneManager.Instanse.rosterPanel.DestroySlot(CurrentHero);
        DarkestSoundManager.ExecuteNarration("dismiss_hero", NarrationPlace.Town);
        WindowClosed();
    }
    public void NextButtonClicked()
    {
        if (onNextButtonClick != null)
            onNextButtonClick();
    }
    public void PreviousButtonClicked()
    {
        if (onPreviousButtonClick != null)
            onPreviousButtonClick();
    }
    public void WindowOpened()
    {
        gameObject.SetActive(true);
    }
    public void WindowClosed()
    {
        if (onWindowClose != null)
            onWindowClose();

        gameObject.SetActive(false);
        CurrentHero = null;
    }
}