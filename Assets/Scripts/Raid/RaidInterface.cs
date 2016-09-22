using UnityEngine;

public class RaidInterface : MonoBehaviour
{
    public Camera OverlayCamera { get; set; }
    public RectTransform OverlayRect { get; set; }
    public CanvasGroup CanvasGroup { get; set; }
    public RaidPanel RaidPanel { get; set; }
    public RaidQuestPanel QuestPanel { get; set; }
    
    void Awake()
    {
        OverlayRect = GetComponent<RectTransform>();
        CanvasGroup = GetComponent<CanvasGroup>();
        OverlayCamera = OverlayRect.parent.GetComponentInChildren<Camera>();
        RaidPanel = GetComponentInChildren<RaidPanel>();
        QuestPanel = GetComponentInChildren<RaidQuestPanel>();
    }

    public void UpdateRaidScene()
    {
        DarkestDungeonManager.Instanse.UpdateSceneOverlay(OverlayCamera);
        DarkestDungeonManager.Instanse.mainMenu.uiCanvasGroup = RaidSceneManager.RaidEvents.raidUiCanvasGroup;
    }
}
