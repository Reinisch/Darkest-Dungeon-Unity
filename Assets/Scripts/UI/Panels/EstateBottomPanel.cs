using UnityEngine;
using UnityEngine.UI;

public class EstateBottomPanel : MonoBehaviour
{
    public Button activityLogButton;
    public Button realmInventoryButton;
    public Button settingsButton;
    public Button glossaryButton;
    public Button townEventButton;

    public Button prepareEmbarkButton;
    public Button provisionButton;
    public Button finalEmbarkButton;

    public SkeletonAnimation RealmInventoryAnimator { get; set; }
    public SkeletonAnimation SettingsAnimator { get; set; }
    public SkeletonAnimation ActivityLogAnimator { get; set; }
    public SkeletonAnimation GlossaryAnimator { get; set; }
    public SkeletonAnimation TownEventAnimator { get; set; }

    public event WindowEvent onActivityIconClick;
    public event WindowEvent onRealmInventoryIconClick;
    public event WindowEvent onMainMenuIconClick;
    public event WindowEvent onGlossaryIconClick;
    public event WindowEvent onTownEventIconClick;

    public event WindowEvent onPrepareEmbarkButtonClick;
    public event WindowEvent onProvisionButtonClick;
    public event WindowEvent onFinalEmbarkButtonClick;

    void Awake()
    {
        RealmInventoryAnimator = realmInventoryButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        SettingsAnimator = settingsButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        ActivityLogAnimator = activityLogButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        GlossaryAnimator = glossaryButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
        TownEventAnimator = townEventButton.gameObject.GetComponentInChildren<SkeletonAnimation>();
    }

    public void EnableTransitions()
    {
        prepareEmbarkButton.interactable = true;
        provisionButton.interactable = true;
        finalEmbarkButton.interactable = true;
    }
    public void DisableTransitions()
    {
        prepareEmbarkButton.interactable = false;
        provisionButton.interactable = false;
        finalEmbarkButton.interactable = false;
    }

    public void ActivityLogClicked()
    {
        if (onActivityIconClick != null)
            onActivityIconClick();
    }
    public void RealmInventoryClicked()
    {
        if (onRealmInventoryIconClick != null)
            onRealmInventoryIconClick();
    }
    public void MainMenuClicked()
    {
        if (onMainMenuIconClick != null)
            onMainMenuIconClick();
    }
    public void GlossaryClicked()
    {
        if (onGlossaryIconClick != null)
            onGlossaryIconClick();
    }
    public void TownEventClicked()
    {
        if (onTownEventIconClick != null)
            onTownEventIconClick();
    }

    public void EmbarkPreparationClicked()
    {
        if (onPrepareEmbarkButtonClick != null)
            onPrepareEmbarkButtonClick();
    }

    public void ProvisionClicked()
    {
        if (onProvisionButtonClick != null)
            onProvisionButtonClick();
    }

    public void FinalEmbarkClicked()
    {
        if (onFinalEmbarkButtonClick != null)
            onFinalEmbarkButtonClick();
    }
}
