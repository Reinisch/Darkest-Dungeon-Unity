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
    public RectTransform rectTransform;
    public Image popupIcon;
    public Text skillMessage;
    public Outline outlineEffect;

    public float speed;
    public float smoothTime;
    public PopupMovementType movementType;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset = Vector3.zero;
    private bool followBone;
    private Spine.Bone targetBone;
    private FormationUnit targetUnit;

    public virtual void SetOffset(Vector3 messageOffset)
    {
        offset = messageOffset;
    }
    public virtual void SetIcon(string iconName)
    {
        popupIcon.enabled = true;
        popupIcon.sprite = DarkestDungeonManager.Data.Sprites[iconName];
    }
    public virtual void SetMessage(string message)
    {
        skillMessage.text = message;
    }
    public virtual void SetColor(Color main, Color outline)
    {
        skillMessage.color = main;
        outlineEffect.effectColor = outline;
    }
    public void SetRotation(Vector3 rotation)
    {
        rectTransform.localEulerAngles = rotation;
    }

    public void FollowXBone(Spine.Bone bone, FormationUnit unit)
    {
        followBone = true;
        targetBone = bone;
        targetUnit = unit;
    }

    void Awake()
    {
        rectTransform.localPosition += offset;

        if(rectTransform.anchoredPosition.y + speed * smoothTime > -rectTransform.sizeDelta.y)
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x,
                -rectTransform.sizeDelta.y - speed * smoothTime);

        if (movementType == PopupMovementType.Top)
            targetPosition = rectTransform.localPosition + new Vector3(0, speed * smoothTime, 0);
        else
            targetPosition = rectTransform.localPosition - new Vector3(0, speed * smoothTime, 0);

        Destroy(gameObject, smoothTime);
    }

    void Update()
    {
        rectTransform.localPosition = Vector3.SmoothDamp(rectTransform.localPosition, targetPosition, ref velocity, smoothTime);

        if (followBone)
        {
            if (targetBone != null && targetUnit != null)
            {
                Vector3 screenPosition = RaidSceneManager.DungeonPositionToScreen
                    (targetUnit.RectTransform.TransformPoint(targetBone.WorldX, targetBone.WorldY, 0));
                rectTransform.position = new Vector3(screenPosition.x, rectTransform.position.y, rectTransform.position.z);
            }
        }
    }
}
