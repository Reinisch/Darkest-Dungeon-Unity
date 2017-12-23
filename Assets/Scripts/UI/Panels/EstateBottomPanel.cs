using System;
using UnityEngine;
using UnityEngine.UI;

public class EstateBottomPanel : MonoBehaviour
{
    [SerializeField]
    private Button activityLogButton;
    [SerializeField]
    private Button realmInventoryButton;
    [SerializeField]
    private Button settingsButton;
    [SerializeField]
    private Button glossaryButton;
    [SerializeField]
    private Button townEventButton;

    [SerializeField]
    private Button prepareEmbarkButton;
    [SerializeField]
    private Button provisionButton;
    [SerializeField]
    private Button finalEmbarkButton;

    public Button ActivityLogButton { get { return activityLogButton; } }
    public Button RealmInventoryButton { get { return realmInventoryButton; } }
    public Button TownEventButton { get { return townEventButton; } }
    public Button ProvisionButton { get { return provisionButton; } }

    public SkeletonAnimation RealmInventoryAnimator { get; private set; }
    public SkeletonAnimation SettingsAnimator { get; private set; }
    public SkeletonAnimation ActivityLogAnimator { get; private set; }
    public SkeletonAnimation GlossaryAnimator { get; private set; }
    public SkeletonAnimation TownEventAnimator { get; private set; }

    public event Action EventActivityIconClick;
    public event Action EventRealmInventoryIconClick;
    public event Action EventMainMenuIconClick;
    public event Action EventGlossaryIconClick;
    public event Action EventTownEventIconClick;

    public event Action EventPrepareEmbarkButtonClick;
    public event Action EventProvisionButtonClick;
    public event Action EventFinalEmbarkButtonClick;

    private void Awake()
    {
        RealmInventoryAnimator = RealmInventoryButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        SettingsAnimator = settingsButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        ActivityLogAnimator = ActivityLogButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        GlossaryAnimator = glossaryButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        TownEventAnimator = TownEventButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
    }

    public void EnableTransitions()
    {
        prepareEmbarkButton.interactable = true;
        ProvisionButton.interactable = true;
        finalEmbarkButton.interactable = true;
    }

    public void DisableTransitions()
    {
        prepareEmbarkButton.interactable = false;
        ProvisionButton.interactable = false;
        finalEmbarkButton.interactable = false;
    }

    public void ActivityLogClicked()
    {
        if (EventActivityIconClick != null)
            EventActivityIconClick();
    }

    public void RealmInventoryClicked()
    {
        if (EventRealmInventoryIconClick != null)
            EventRealmInventoryIconClick();
    }

    public void MainMenuClicked()
    {
        if (EventMainMenuIconClick != null)
            EventMainMenuIconClick();
    }

    public void GlossaryClicked()
    {
        if (EventGlossaryIconClick != null)
            EventGlossaryIconClick();
    }

    public void TownEventClicked()
    {
        if (EventTownEventIconClick != null)
            EventTownEventIconClick();
    }

    public void EmbarkPreparationClicked()
    {
        if (EventPrepareEmbarkButtonClick != null)
            EventPrepareEmbarkButtonClick();
    }

    public void ProvisionClicked()
    {
        if (EventProvisionButtonClick != null)
            EventProvisionButtonClick();
    }

    public void FinalEmbarkClicked()
    {
        if (EventFinalEmbarkButtonClick != null)
            EventFinalEmbarkButtonClick();
    }
}
