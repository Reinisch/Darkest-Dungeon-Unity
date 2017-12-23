using UnityEngine;

public class RaidInterface : MonoBehaviour
{
    public Camera OverlayCamera { get; private set; }
    public RectTransform OverlayRect { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }
    public RaidPanel RaidPanel { get; private set; }
    public RaidQuestPanel QuestPanel { get; private set; }
    
    private void Awake()
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
        DarkestDungeonManager.MainMenu.UICanvasGroup = RaidSceneManager.RaidEvents.RaidUICanvasGroup;
    }
}
