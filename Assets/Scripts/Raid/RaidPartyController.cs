using UnityEngine;

public class RaidPartyController : MonoBehaviour
{
    [SerializeField]
    private FormationParty heroParty;

    public Direction PointerMovementDirection { private get; set; }
    public bool MovementAllowed { get; set; }
    public bool ForwardMovementAllowed { get; set; }

    private RaidHallwayPassage Passage { get; set; }
    private RectTransform RectTransform { get; set; }

    private const float MovementSpeed = 40f;
    private bool moving;

    private void Awake()
    {
        PointerMovementDirection = Direction.Bot;
        MovementAllowed = true;
        ForwardMovementAllowed = true;
        RectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (moving == false)
            CheckSmoothStop();

        if (MovementAllowed && !DarkestDungeonManager.GamePaused && !RaidSceneManager.AnyWindowOpened)
        {
            double delta = Mathf.Clamp(Time.deltaTime, 0, 0.1f);
            double direction = PointerMovementDirection == Direction.Bot ? Input.GetAxis("Horizontal") :
                PointerMovementDirection == Direction.Left ? -1 :
                PointerMovementDirection == Direction.Right ? 1 : 0;
            double advancment = 0;

            if (direction < 0 && heroParty.LastUnit != null && Passage.LeftWall.position.x >= heroParty.LastUnit.RectTransform.position.x)
                advancment = 0;
            else if (direction > 0 && Passage.RightWall.position.x <= RectTransform.position.x)
                advancment = 0;
            else if (!(direction > 0 && !ForwardMovementAllowed))
            {
                advancment = Mathf.Abs((float)direction) > 0.1f ? direction * MovementSpeed * delta : 0;
                if (direction < 0)
                    advancment /= 2;
                RectTransform.position += new Vector3((float)advancment, 0, 0);
            }

            if (advancment > 0 && RaidSceneManager.Rules.IsWalkingBack)
                RaidSceneManager.Rules.SetWalkingBack(false);
            else if (advancment < 0 && !RaidSceneManager.Rules.IsWalkingBack)
                RaidSceneManager.Rules.SetWalkingBack(true);

            if(advancment != 0)
            {
                for (int i = 0; i < RaidSceneManager.BattleGround.HeroParty.Units.Count; i++)
                {
                    var trackEntry = RaidSceneManager.BattleGround.HeroParty.Units[i].SkeletonAnimations[3].state.GetCurrent(0);
                    if (trackEntry != null) trackEntry.Loop = true;

                    if (RaidSceneManager.BattleGround.HeroParty.Units[i].SkeletonAnimations[3].timeScale < 1f)
                        RaidSceneManager.BattleGround.HeroParty.Units[i].SkeletonAnimations[3].timeScale += Time.deltaTime * 3f;
                }
            }

            if (moving && advancment == 0)
                moving = false;
            else if (!moving && advancment != 0)
            {
                for (int i = 0; i < RaidSceneManager.BattleGround.HeroParty.Units.Count; i++)
                {
                    if (!RaidSceneManager.BattleGround.HeroParty.Units[i].IsWalking())
                    {
                        RaidSceneManager.BattleGround.HeroParty.Units[i].SkeletonAnimations[3].Reset();
                        RaidSceneManager.BattleGround.HeroParty.Units[i].SetWalking(true);
                    }
                    RaidSceneManager.BattleGround.HeroParty.Units[i].SkeletonAnimations[3].timeScale = 0.75f;
                }
                moving = true;
            }
        }
        else if (moving)
        {
            moving = false;

            if (RaidSceneManager.Rules.IsWalkingBack)
                RaidSceneManager.Rules.SetWalkingBack(false);
        }
    }

    public void TranseferToPassage(RaidHallwayPassage passage)
    {
        RectTransform.localPosition = Vector3.zero;
        Passage = passage;
        moving = false;
    }

    private void CheckSmoothStop()
    {
        foreach (var unit in RaidSceneManager.HeroParty.Units)
        {
            if (unit.IsWalking())
            {
                var trackEntry = unit.SkeletonAnimations[3].state.GetCurrent(0);

                if (trackEntry == null)
                {
                    if (unit.SkeletonAnimations[2].state != null)
                    {
                        if (unit.SkeletonAnimations[2].state.GetCurrent(0) != null)
                        {
                            unit.SkeletonAnimations[2].state.GetCurrent(0).Time = 0;
                            unit.SkeletonAnimations[2].state.Update(0);
                        }
                    }
                    unit.SetWalking(false);
                }
                else
                {
                    if (trackEntry.Loop)
                    {
                        trackEntry.Loop = false;
                        trackEntry.LastTime = trackEntry.LastTime % trackEntry.EndTime;
                        trackEntry.Time = trackEntry.Time % trackEntry.EndTime;
                    }

                    if (unit.SkeletonAnimations[3].timeScale < 4f)
                        unit.SkeletonAnimations[3].timeScale += Time.deltaTime * 6f;
                }
            }
        }
    }
}