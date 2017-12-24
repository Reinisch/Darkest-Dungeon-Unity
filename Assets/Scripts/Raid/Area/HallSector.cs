using UnityEngine.Assertions;

public class HallSector : Area
{
    public Hallway Hallway { get; set; }

    public HallSector()
    {
        Knowledge = Knowledge.Hidden;
        TextureId = "0";
        MashId = 0;
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway) : this()
    {
        Id = id;
        Hallway = parentHallway;

        GridX = gridX;
        GridY = gridY;
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Knowledge knowledge,
        AreaType areaType, string textureId) : this(id, gridX, gridY, parentHallway)
    {
        Knowledge = knowledge;
        Type = areaType;
        TextureId = textureId;
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Knowledge knowledge, AreaType areaType, 
        string textureId, string propId) : this(id, gridX, gridY, parentHallway, knowledge, areaType, textureId)
    {
        switch (areaType)
        {
            case AreaType.Curio:
            case AreaType.Tresure:
                Prop = DarkestDungeonManager.Data.Curios[propId];
                break;
            case AreaType.Trap:
                Prop = DarkestDungeonManager.Data.Traps[propId];
                break;
            case AreaType.Obstacle:
                Prop = DarkestDungeonManager.Data.Obstacles[propId];
                break;
            default:
                Assert.IsTrue(false, "Trying to initialize prop of invalid area type! AreaType: " + areaType);
                break;
        }
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Knowledge knowledge, AreaType areaType, string textureId, 
        string dungeon, string encounter, int mashId, int index) : this(id, gridX, gridY, parentHallway, knowledge, areaType, textureId)
    {
        BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData[dungeon].BattleMashes.
            Find(mash => mash.MashId == mashId).NamedEncounters[encounter][index].MonsterSet);
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Door door) : this()
    {
        Id = id;
        Hallway = parentHallway;

        GridX = gridX;
        GridY = gridY;

        Prop = door;
        Type = AreaType.Door;
    }

    public override void Scout()
    {
        if (Knowledge == Knowledge.Hidden)
        {
            Knowledge = Knowledge.Scouted;

            if (Type != AreaType.Door)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                RaidSceneManager.MapPanel.UpdateArea(this);

                if (Type != AreaType.Trap || RaidSceneManager.SceneState != DungeonSceneState.Hall)
                    return;

                if (RaidSceneManager.HallwayView.Hallway != Hallway)
                    return;

                var raidSector = RaidSceneManager.HallwayView.RaidHallway.HallSectors.Find(trapSector => trapSector.HallSector == this);
                ((RaidTrap)raidSector.Prop).SkeletonAnimation.MeshRenderer.enabled = true;
            }
        }
    }
}
