using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class RaidPartyController : MonoBehaviour
{
    const float movementSpeed = 40f;

    public FormationParty heroParty;
    public Rigidbody2D RigidBody { get; set; }
    public BoxCollider2D Activator { get; set; }
    public RaidHallwayPassage Passage { get; set; }
    public RectTransform RectTransform { get; set; }

    public bool MovementAllowed { get; set; }
    public bool ForwardMovementAllowed { get; set; }

    public Direction PointerMovementDirection { get; set; }

    bool moving = false;

    void CheckSmoothStop()
    {
        foreach (var unit in RaidSceneManager.HeroParty.Units)
        {
            if (unit.IsWalking())
            {
                var trackEntry = unit.SkeletonAnimations[3].state.GetCurrent(0);

                if (trackEntry == null)
                {
                    if(unit.SkeletonAnimations[2].state != null)
                    {
                        if(unit.SkeletonAnimations[2].state.GetCurrent(0) != null)
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

    void Awake()
    {
        PointerMovementDirection = Direction.Bot;
        MovementAllowed = true;
        ForwardMovementAllowed = true;
        RigidBody = GetComponent<Rigidbody2D>();
        Activator = GetComponent<BoxCollider2D>();
        RectTransform = GetComponent<RectTransform>();
    }
    void Update()
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

            if (direction < 0 && heroParty.LastUnit != null && Passage.leftWall.position.x >= heroParty.LastUnit.RectTransform.position.x)
                advancment = 0;
            else if (direction > 0 && Passage.rightWall.position.x <= RectTransform.position.x)
                advancment = 0;
            else if (!(direction > 0 && !ForwardMovementAllowed))
            {
                advancment = Mathf.Abs((float)direction) > 0.1f ? direction * movementSpeed * delta : 0;
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

    public void Stop()
    {
        moving = false;
    }
    public void TranseferToPassage(RaidHallwayPassage passage)
    {
        RectTransform.localPosition = Vector3.zero;
        Passage = passage;
        Stop();
    }
}