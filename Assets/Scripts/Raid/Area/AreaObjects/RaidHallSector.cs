using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public interface IRaidArea : IPointerClickHandler
{
    RaidProp Prop { get; set; }
    RectTransform RectTransform { get; }
    Area Area { get; }

    void UpdateEnviroment();
    void CompleteArea();
}

public class RaidHallSector : MonoBehaviour, IRaidArea
{
    bool isPartyInside = false;

    public Image hallWall;
    public Image hallFloor;

    public BoxCollider2D activator;

    public RaidProp Prop { get; set; }
    public RectTransform RectTransform { get; set; }
    public HallSector HallSector
    {
        get
        {
            return Area as HallSector;
        }
    }
    public Area Area
    {
        get;
        private set;
    }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void LoadSector(HallSector hallSector)
    {
        Area = hallSector;
        switch(Area.Type)
        {
            case AreaType.Door:
                if(Prop == null)
                {
                    GameObject doorObject = Resources.Load("Prefabs/Props/SpineDoors/" +
                        RaidSceneManager.Raid.Quest.Dungeon + "/Door") as GameObject;
                    RaidDoor door = Instantiate(doorObject).GetComponent<RaidDoor>();
                    door.Initialize(this);
                    Prop = door;
                    Prop.RectTransform.SetParent(hallWall.rectTransform, false);
                }
                else
                {
                    (Prop as RaidDoor).Close();
                }
                break;
            case AreaType.Curio:
                if (Prop != null)
                    Destroy(Prop.gameObject);

                RaidCurio curio;
                GameObject curioObject = Resources.Load("Prefabs/Props/SpineCurios/"
                    + (Area.Prop as Curio).StringId) as GameObject;

                if (curioObject == null)
                {
                    Debug.LogError("Curio: " + (Area.Prop as Curio).StringId + " not found.");
                    curio = Instantiate(Resources.Load("Prefabs/Props/SpineCurios/_template")
                        as GameObject).GetComponent<RaidCurio>();
                }
                else
                    curio = Instantiate(curioObject).GetComponent<RaidCurio>();

                curio.Initialize(this);
                Prop = curio;
                Prop.RectTransform.SetParent(hallWall.rectTransform, false);
                if (Area.Knowledge == Knowledge.Completed)
                    curio.Activate();
                break;
            case AreaType.Obstacle:
                if (Prop != null)
                    Destroy(Prop.gameObject);

                if (Area.Knowledge == Knowledge.Completed)
                    break;

                RaidObstacle obstacle;
                GameObject obstacleObject = Resources.Load("Prefabs/Props/SpineObstacles/"
                    + (Area.Prop as Obstacle).StringId) as GameObject;

                if (obstacleObject == null)
                {
                    Debug.LogError("Obstacle: " + (Area.Prop as Obstacle).StringId + " not found.");
                    obstacle = Instantiate(Resources.Load("Prefabs/Props/SpineObstacles/_template") 
                        as GameObject).GetComponent<RaidObstacle>();
                }
                else
                    obstacle = Instantiate(obstacleObject).GetComponent<RaidObstacle>();

                obstacle.Initialize(this);
                Prop = obstacle;
                Prop.RectTransform.SetParent(hallWall.rectTransform, false);
                Prop.RectTransform.SetAsLastSibling();
               
                break;
            case AreaType.Trap:
                if (Prop != null)
                    Destroy(Prop.gameObject);

                if (Area.Knowledge == Knowledge.Completed)
                    break;

                RaidTrap trap;
                GameObject trapObject = Resources.Load("Prefabs/Props/SpineTraps/"
                    + (Area.Prop as Trap).StringId) as GameObject;

                if (trapObject == null)
                {
                    Debug.LogError("Trap: " + (Area.Prop as Trap).StringId + " not found.");
                    trap = Instantiate(Resources.Load("Prefabs/Props/SpineTraps/_template")
                        as GameObject).GetComponent<RaidTrap>();
                }
                else
                    trap = Instantiate(trapObject).GetComponent<RaidTrap>();

                trap.Initialize(this);
                Prop = trap;
                Prop.RectTransform.SetParent(hallWall.rectTransform, false);
                Prop.RectTransform.SetAsLastSibling();

                if (Area.Knowledge == Knowledge.Hidden || Area.Knowledge == Knowledge.Visited)
                    trap.SkeletonAnimation.MeshRenderer.enabled = false;
                break;
            default:
                if (Prop != null)
                    Destroy(Prop.gameObject);
                break;
        }

        UpdateEnviroment();
    }
    public void LeaveSector()
    {
        isPartyInside = false;
    }
    public void UpdateEnviroment()
    {
        var sprites = DarkestDungeonManager.Data.Sprites;
        if(Area.Type == AreaType.Door)
        {
            hallWall.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor_door.basic_0"];
            hallFloor.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor_door.basic_1"];
        }
        else
        {
            hallWall.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor_wall." + Area.TextureId + "_0"];
            hallFloor.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor_wall." + Area.TextureId + "_1"];
        }
    }
    public void UpdateBorder()
    {
        var sprites = DarkestDungeonManager.Data.Sprites;
        hallWall.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor_wall.1_0"];
        hallFloor.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor_wall.1_1"];
    }
    public void SetInside(bool inside)
    {
        isPartyInside = inside;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPartyInside)
            return;
        isPartyInside = true;
        if (Area != null)
        {
            if (Area.Knowledge == Knowledge.Hidden)
                RaidSceneManager.TorchMeter.DecreaseTorch(6);
            else
                RaidSceneManager.TorchMeter.DecreaseTorch(1);

            RaidSceneManager.Raid.EnteredSector(Area as HallSector);

            RaidSceneManager.Raid.CurrentLocation = Area;
            RaidSceneManager.HallwayView.CurrentSector = this;

            if (Area.Type == AreaType.Battle)
            {
                #region Battle Checks
                RaidSceneManager.MapPanel.SetCurrentIndicator(Area as HallSector);
                if (!Area.BattleEncounter.Cleared)
                {
                    if(RaidSceneManager.Raid.Dungeon.SharedMash != null)
                    {
                        if (RaidSceneManager.TorchMeter.CurrentRange.RangeType == TorchRangeType.Out)
                        {
                            if (!RaidSceneManager.Raid.Dungeon.SharedMashExecutionIds.Contains(0))
                            {
                                RaidSceneManager.Raid.Dungeon.SharedMashExecutionIds.Add(0);
                                if (RandomSolver.CheckSuccess((float)RaidSceneManager.Raid.Dungeon.
                                    SharedMash.HallEncounters[0].Chance / 100))
                                    Area.BattleEncounter = new BattleEncounter(RaidSceneManager.Raid.Dungeon.
                                        SharedMash.HallEncounters[0].MonsterSet);
                            }
                        }
                        
                        if(RaidSceneManager.Inventory.PercentageFull >= 0.65f)
                        {
                            if (!RaidSceneManager.Raid.Dungeon.SharedMashExecutionIds.Contains(1))
                            {
                                RaidSceneManager.Raid.Dungeon.SharedMashExecutionIds.Add(1);
                                if (RandomSolver.CheckSuccess((float)RaidSceneManager.Raid.Dungeon.
                                    SharedMash.HallEncounters[1].Chance / 100))
                                    Area.BattleEncounter = new BattleEncounter(RaidSceneManager.Raid.Dungeon.
                                        SharedMash.HallEncounters[1].MonsterSet);
                            }
                        }
                    }
                    
                    RaidSceneManager.Instanse.EncounterMonsters(this);
                }
                #endregion
            }
            else if (Area.Type == AreaType.Door)
            {
                Area.Knowledge = Knowledge.Completed;
                return;
            }

            #region Force Curio Tag
            if (Area.Type == AreaType.Curio && (Area.Knowledge == Knowledge.Hidden || Area.Knowledge == Knowledge.Scouted))
            {
                Curio curio = Area.Prop as Curio;
                if(curio.IsQuestCurio == false)
                {
                    foreach(var triggeredHero in RaidSceneManager.Formations.heroes.party.Units)
                    {
                        if(triggeredHero.Character.Trait != null)
                        {
                            if(triggeredHero.Character.Trait.CurioTag == "All" ||
                                curio.Tags.Contains(triggeredHero.Character.Trait.CurioTag))
                            {
                                if(RandomSolver.CheckSuccess(triggeredHero.Character.Trait.TagChance))
                                {
                                    triggeredHero.OverlaySlot.UnitSelected();
                                    if (!(Prop as RaidCurio).Investigated)
                                        RaidSceneManager.Instanse.ActivateCurio(this, triggeredHero.Character.Trait);
                                    break;
                                }
                            }
                        }
                        var hero = triggeredHero.Character as Hero;
                        foreach(var triggerQuirk in hero.Quirks)
                        {
                            if(triggerQuirk.Quirk.CurioTag == "All" || curio.Tags.Contains(triggerQuirk.Quirk.CurioTag))
                            {
                                if (RandomSolver.CheckSuccess(triggerQuirk.Quirk.CurioTagChance))
                                {
                                    triggeredHero.OverlaySlot.UnitSelected();
                                    if (!(Prop as RaidCurio).Investigated)
                                        RaidSceneManager.Instanse.ActivateCurio(this, triggerQuirk.Quirk);
                                    break;
                                }
                            }
                        }
                    }
                    
                }

            }
            #endregion

            if (Area.Knowledge != Knowledge.Completed)
            {
                if (Area.Type == AreaType.Hunger)
                {
                    Area.Knowledge = Knowledge.Completed;

                    if (RaidSceneManager.Raid.HungerCooldown <= 0)
                        RaidSceneManager.Instanse.ActivateHunger(this);

                    RaidSceneManager.Raid.ResetHungerCooldown();
                }
                else if (Area.Type == AreaType.Empty)
                {
                    Area.Knowledge = Knowledge.Completed;
                    RaidSceneManager.Raid.HungerCooldown--;
                }
                else if (Area.Knowledge == Knowledge.Hidden || Area.Knowledge == Knowledge.Scouted)
                {
                    Area.Knowledge = Knowledge.Visited;
                    RaidSceneManager.Raid.HungerCooldown--;
                }
            }

            RaidSceneManager.MapPanel.SetCurrentIndicator(Area as HallSector);
            RaidSceneManager.MapPanel.OnHallSectorEnter(Area as HallSector);
            RaidSceneManager.MapPanel.FocusTarget();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        isPartyInside = false;

        if (Area != null && Area.Type != AreaType.Door)
        {

            if (Area.Knowledge != Knowledge.Completed)
            {
                if(Area.Type == AreaType.Empty)
                    Area.Knowledge = Knowledge.Completed;

                RaidSceneManager.MapPanel.OnHallSectorEnter(Area as HallSector);
            }
        }
    }

    public void CompleteArea()
    {
        if (Prop != null)
        {
            Prop.Activate();
            if (Prop is RaidCurio)
                RaidSceneManager.Raid.InvestigatedCurios.Add(Area.Prop.StringId);
        }

        Area.Knowledge = Knowledge.Completed;
        RaidSceneManager.MapPanel.UpdateArea(Area);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPartyInside)
        {
            if (Area.Type == AreaType.Door)
            {
                RaidSceneManager.Instanse.ActivateDoor(this);
                isPartyInside = false;
            }
            else if (Area.Type == AreaType.Curio)
            {
                if (!(Prop as RaidCurio).Investigated)
                    RaidSceneManager.Instanse.ActivateCurio(this);
            }
            else if (Area.Type == AreaType.Trap && (Area.Knowledge != Knowledge.Hidden))
            {
                if (Prop != null && !(Prop as RaidTrap).Activated)
                    RaidSceneManager.Instanse.ActivateTrap(this, true);
            }
        }
    }
}