using UnityEngine;
using UnityEngine.UI;

public enum PopupMovementType
{
    Top,
    Bottom 
}
public enum PopupMessageType
{
    Miss, Dodge, ZeroDamage,
    Damage, CritDamage, Heal,
    CritHeal, Pass, Tagged,
    Cured, Bleed, Poison,
    Stunned, BleedResist,
    PoisonResist, StunResist,
    MoveResist, DebuffResist,
    Buff, Debuff, Unstun,
    Untagged, DiseaseResist,
    Stress, StressHeal, Disease,
    PositiveQuirk, NegativeQuirk,
    QuirkRemoved, DeathsDoor,
    DeathBlow, HeartAttack,
    Guard, Riposte, DiseaseCured,
    RetreatFailed
}

public class PopupMessage : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Image popupIcon;
    [SerializeField]
    private Text skillMessage;
    [SerializeField]
    private Outline outlineEffect;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float smoothTime;
    [SerializeField]
    private PopupMovementType movementType;

    public RectTransform RectTransform { get { return rectTransform; } }
    public Text SkillMessage { get { return skillMessage; } }
    
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset = Vector3.zero;
    private bool followBone;
    private Spine.Bone targetBone;
    private FormationUnit targetUnit;

    private void Awake()
    {
        RectTransform.localPosition += offset;

        if(RectTransform.anchoredPosition.y + speed * smoothTime > -RectTransform.sizeDelta.y)
            RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x,
                -RectTransform.sizeDelta.y - speed * smoothTime);

        if (movementType == PopupMovementType.Top)
            targetPosition = RectTransform.localPosition + new Vector3(0, speed * smoothTime, 0);
        else
            targetPosition = RectTransform.localPosition - new Vector3(0, speed * smoothTime, 0);

        Destroy(gameObject, smoothTime);
    }

    private void Update()
    {
        RectTransform.localPosition = Vector3.SmoothDamp(RectTransform.localPosition, targetPosition, ref velocity, smoothTime);

        if (followBone)
        {
            if (targetBone != null && targetUnit != null)
            {
                Vector3 screenPosition = RaidSceneManager.DungeonPositionToScreen
                    (targetUnit.RectTransform.TransformPoint(targetBone.WorldX, targetBone.WorldY, 0));
                RectTransform.position = new Vector3(screenPosition.x, RectTransform.position.y, RectTransform.position.z);
            }
        }
    }

    public void SetOffset(Vector3 messageOffset)
    {
        offset = messageOffset;
    }

    public void SetIcon(string iconName)
    {
        popupIcon.enabled = true;
        popupIcon.sprite = DarkestDungeonManager.Data.Sprites[iconName];
    }

    public void SetMessage(string message)
    {
        SkillMessage.text = message;
    }

    public void SetColor(Color main, Color outline)
    {
        SkillMessage.color = main;
        outlineEffect.effectColor = outline;
    }

    public void SetRotation(Vector3 rotation)
    {
        RectTransform.localEulerAngles = rotation;
    }

    public void FollowXBone(Spine.Bone bone, FormationUnit unit)
    {
        followBone = true;
        targetBone = bone;
        targetUnit = unit;
    }
}
