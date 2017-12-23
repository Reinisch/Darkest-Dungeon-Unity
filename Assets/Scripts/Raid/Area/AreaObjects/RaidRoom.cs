using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaidRoom : MonoBehaviour, IRaidArea
{
    [SerializeField]
    private Image roomWall;

    public RaidProp Prop { get; set; }
    public RectTransform RectTransform { get; private set; }
    public Area Area { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void LoadRoom(DungeonRoom room, bool savedBattle)
    {
        Area = room;

        switch (Area.Type)
        {
            case AreaType.Curio:
                if (Prop != null)
                    Destroy(Prop.gameObject);

                RaidCurio questCurio;
                GameObject questCurioObject = Resources.Load("Prefabs/Props/SpineCurios/" + Area.Prop.StringId) as GameObject;

                if (questCurioObject == null)
                {
                    Debug.LogError("Curio: " + Area.Prop.StringId + " not found.");
                    questCurio = Instantiate(Resources.Load("Prefabs/Props/SpineCurios/_template")
                        as GameObject).GetComponent<RaidCurio>();
                }
                else
                    questCurio = Instantiate(questCurioObject).GetComponent<RaidCurio>();

                questCurio.Initialize(this);
                Prop = questCurio;
                Prop.RectTransform.SetParent(RectTransform, false);
                if (Area.Knowledge == Knowledge.Completed)
                    questCurio.Activate();
                break;
            case AreaType.BattleTresure:
            case AreaType.BattleCurio:
                if (Prop != null)
                    Destroy(Prop.gameObject);

                RaidCurio curio;
                GameObject curioObject = Resources.Load("Prefabs/Props/SpineCurios/" + Area.Prop.StringId) as GameObject;

                if (curioObject == null)
                {
                    Debug.LogError("Curio: " + Area.Prop.StringId + " not found.");
                    curio = Instantiate(Resources.Load("Prefabs/Props/SpineCurios/_template")
                        as GameObject).GetComponent<RaidCurio>();
                }
                else
                    curio = Instantiate(curioObject).GetComponent<RaidCurio>();

                curio.Initialize(this);
                Prop = curio;
                Prop.RectTransform.SetParent(RectTransform, false);
                if (Area.Knowledge == Knowledge.Completed)
                    curio.Activate();
                break;
            case AreaType.Battle:
            case AreaType.Boss:
                if (Prop != null)
                    Destroy(Prop.gameObject);

                if(Area.Prop != null && Area.Prop.Type == AreaType.Curio)
                {
                    RaidCurio questBossCurio;
                    GameObject questBossObject = Resources.Load("Prefabs/Props/SpineCurios/" + Area.Prop.StringId) as GameObject;

                    if (questBossObject == null)
                    {
                        Debug.LogError("Curio: " + Area.Prop.StringId + " not found.");
                        questBossCurio = Instantiate(Resources.Load("Prefabs/Props/SpineCurios/_template")
                            as GameObject).GetComponent<RaidCurio>();
                    }
                    else
                        questBossCurio = Instantiate(questBossObject).GetComponent<RaidCurio>();

                    questBossCurio.Initialize(this);
                    Prop = questBossCurio;
                    Prop.RectTransform.SetParent(RectTransform, false);
                    if (Area.Knowledge == Knowledge.Completed)
                    {
                        questBossCurio.Activate();
                        if(Area.Prop.StringId == "beacon" || Area.Prop.StringId == "teleporter")
                            questBossCurio.SkeletonAnimation.state.SetAnimation(0, "disturbed", true);
                    }
                }
                break;
            case AreaType.Entrance:
                if (Prop != null)
                    Destroy(Prop.gameObject);
                break;
            default:
                if (Prop != null)
                    Destroy(Prop.gameObject);
                break;
        }
        if (Area.BattleEncounter != null && !Area.BattleEncounter.Cleared && !savedBattle)
                RaidSceneManager.Instanse.EncounterMonsters(this);
        UpdateEnviroment();
    }

    public void UpdateEnviroment()
    {
        roomWall.sprite = DarkestDungeonManager.Data.DungeonSprites[RaidSceneManager.Raid.Quest.Dungeon + ".room_wall." + Area.TextureId];
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
        if (Prop != null && Prop.PropType == PropType.Curio && !((RaidCurio)Prop).Investigated)
            RaidSceneManager.Instanse.ActivateCurio(this);
    }
}