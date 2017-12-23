using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void UnitOverlayEvent(FormationOverlaySlot slot);

public class FormationOverlaySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image selectorOverlay;
    [SerializeField]
    private HealthBar healthBar;
    [SerializeField]
    private StressOverlayPanel stressBar;
    [SerializeField]
    private TrayPanel trayBar;
    [SerializeField]
    private Image selectorPlus;
    [SerializeField]
    private PopupDialog dialogPopup;
    [SerializeField]
    private RectTransform leftEdge;
    [SerializeField]
    private RectTransform rightEdge;

    public FormationUnit TargetUnit { get; private set; }
    public FormationOverlay Overlay { private get; set; }
    public RectTransform RectTransform { get; private set; }
    public Animator SlotAnimator { get; private set; }
    private Animator SelectorAnimator { get; set; }
    private Character Character { get { return TargetUnit.Character; } }

    public bool IsSelectionLocked { get; private set; }
    public bool IsHovered { get; private set; }
    public bool IsDoingDialog { get { return dialogPopup.IsActive; } }

    public event UnitOverlayEvent HeroSelected;
    public event UnitOverlayEvent SkillTargetSelected;

    private int baseWidth = 140;

#if UNITY_ANDROID || UNITY_IOS
    float doubleTapTimer = 0f;
    float doubleTapTime = 0.2f;

    private void Update()
    {
        if (doubleTapTimer > 0)
            doubleTapTimer -= Time.deltaTime;
    }
#endif

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        SlotAnimator = GetComponent<Animator>();
        SelectorAnimator = RectTransform.Find("SelectionFrame").GetComponent<Animator>();
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        Vector3 screenPoint = RaidSceneManager.DungeonPositionToScreen(TargetUnit.RectTransform.position);

        RectTransform.position = new Vector3(screenPoint.x, screenPoint.y, RectTransform.position.z);
    }

    public void StartDialog(string dialogText, bool skipable = true)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/ui/shared/text_popup");
        dialogPopup.SetCurrentDialog(dialogText, skipable);
    }

    public void LockOnUnit(FormationUnit unit)
    {
        TargetUnit = unit;
        healthBar.SetSize(unit.Character);
        healthBar.UpdateHealth(unit.Character);
        RectTransform.sizeDelta = new Vector2(baseWidth * unit.Size, RectTransform.sizeDelta.y);
        selectorPlus.gameObject.SetActive(false);
        if (unit.Character.IsMonster)
        {
            if (unit.Character.SharedHealth != null)
            {
                RaidSceneManager.BattleGround.SharedHealth.UpdateOverlay();
                healthBar.gameObject.SetActive(false);
            }
            else
            {
                healthBar.gameObject.SetActive(true);
            }

            stressBar.gameObject.SetActive(false);
        }

        if (unit.Character.HealthbarModifier != null && unit.Character.HealthbarModifier.Type == "corpse")
            healthBar.HealthImage.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            healthBar.HealthImage.material = healthBar.HealthImage.defaultMaterial;

        trayBar.UpdatePanel(TargetUnit);
        unit.OverlaySlot = this;
        gameObject.SetActive(true);
    }

    public void UpdateUnit()
    {
        healthBar.SetSize(TargetUnit.Character);

        UpdateOverlay();

        if (TargetUnit.Character.HealthbarModifier != null && TargetUnit.Character.HealthbarModifier.Type == "corpse")
            healthBar.HealthImage.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            healthBar.HealthImage.material = healthBar.HealthImage.defaultMaterial;
    }

    public void UpdateOverlay()
    {
        healthBar.UpdateHealth(TargetUnit.Character);
        trayBar.UpdatePanel(TargetUnit);
        selectorPlus.gameObject.SetActive(false);
        if (TargetUnit.Character.IsMonster)
        {
            if (TargetUnit.Character.SharedHealth != null)
            {
                healthBar.gameObject.SetActive(false);
                RaidSceneManager.BattleGround.SharedHealth.UpdateOverlay();
            }
            else
                healthBar.gameObject.SetActive(true);

            stressBar.gameObject.SetActive(false);
        }
        else
            stressBar.UpdateStress(TargetUnit.Character.Stress.ValueRatio);
    }

    public void ClearTarget()
    {
        TargetUnit = null;
        trayBar.ResetPanel();
        selectorPlus.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Relocate()
    {
        if (isActiveAndEnabled)
            RectTransform.position = TargetUnit.RectTransform.position;
    }

    #region Slot states

    public void SetFriendly()
    {
        selectorOverlay.sprite = Overlay.FriendSizeSprites[Character.Size - 1];
        selectorOverlay.SetNativeSize();
        selectorPlus.gameObject.SetActive(false);
    }

    public void SetMovable()
    {
        selectorOverlay.sprite = Overlay.MoveSprite;
        selectorOverlay.SetNativeSize();
        selectorPlus.gameObject.SetActive(false);
    }

    public void SetEnemy(bool combined = false)
    {
        selectorOverlay.sprite = Overlay.EnemySizeSprites[Character.Size - 1];
        selectorOverlay.SetNativeSize();
        if (combined)
        {
            var unitIndex = TargetUnit.Party.Units.IndexOf(TargetUnit);
            var targetIndex = TargetUnit.Team == Team.Heroes ? unitIndex - 1 : unitIndex + 1;
            if (targetIndex >= 0 && targetIndex < TargetUnit.Party.Units.Count)
            {
                var neighbour = TargetUnit.Party.Units[targetIndex];
                var positionX = (rightEdge.position.x + neighbour.OverlaySlot.leftEdge.position.x)/2;
                selectorPlus.rectTransform.position = new Vector3(positionX,
                    selectorPlus.rectTransform.position.y, selectorPlus.rectTransform.position.z);
                selectorPlus.gameObject.SetActive(true);
            }
            else
                selectorPlus.gameObject.SetActive(false);
        }
        else
            selectorPlus.gameObject.SetActive(false);
    }

    public void SetSelected()
    {
        selectorOverlay.sprite = Overlay.SelectionSizeSprites[Character.Size - 1];
        selectorOverlay.SetNativeSize();
        selectorPlus.gameObject.SetActive(false);
    }

    public void SetActive()
    {
        SlotAnimator.SetBool("Selected", true);
        SelectorAnimator.SetTrigger("Selected");
    }

    public void SetStatic()
    {
        SlotAnimator.SetBool("Selected", true);
        SelectorAnimator.SetTrigger("Available");
    }

    public void SetDeselected()
    {
        SlotAnimator.SetBool("Selected", false);
        SelectorAnimator.SetTrigger("Deselected");
        selectorPlus.gameObject.SetActive(false);
    }

    #endregion

    public void Show()
    {
        if(isActiveAndEnabled)
            SlotAnimator.SetBool("Hidden", false);
    }

    public void Hide()
    {
        if (isActiveAndEnabled)
            SlotAnimator.SetBool("Hidden", true);
    }

    public void LockSelection()
    {
        IsSelectionLocked = true;
    }

    public void UnlockSelection()
    {
        IsSelectionLocked = false;
    }

    public void UnitSelected()
    {
        if (HeroSelected != null)
            HeroSelected(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (doubleTapTimer > 0)
        {
            if (TargetUnit != null && TargetUnit.Character.IsMonster == false)
                RaidSceneManager.Instanse.HeroCharacterWindowOpened(this);
        }
        else
            doubleTapTimer = doubleTapTime;
#else
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (TargetUnit != null && TargetUnit.Character.IsMonster == false)
                RaidSceneManager.Instanse.HeroCharacterWindowOpened(this);
        }
#endif

        if (TargetUnit.IsTargetable)
        {
            if (SkillTargetSelected != null)
                SkillTargetSelected(this);

            return;
        }

        if(!IsSelectionLocked)
            UnitSelected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
        if (TargetUnit == null)
            return;

        if (TargetUnit != null && RaidSceneManager.RaidPanel.SelectedUnit.Team != TargetUnit.Team)
        {
            if (TargetUnit.IsTargetable)
                RaidSceneManager.RaidEvents.MonsterTooltip.Show(TargetUnit);
            else
            {
                RaidSceneManager.RaidEvents.MonsterTooltip.Show(TargetUnit);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
        RaidSceneManager.RaidEvents.MonsterTooltip.Hide();
    }
}