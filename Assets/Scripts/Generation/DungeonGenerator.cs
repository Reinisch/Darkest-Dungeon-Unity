using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class DungeonGenerator
{
    public static Dungeon GenerateDungeon(Quest quest, int seed = 0)
    {
        if (seed != 0)
            Random.InitState(seed);
        
        string[] lengthes = new string[]{"", "short", "medium", "long"};

        Dungeon dungeon = new Dungeon();
        DungeonGenerationData genData = DarkestDungeonManager.Data.DungeonGenerationData.Find(item =>
            item.Dungeon == quest.Dungeon &&
            item.Length == lengthes[quest.Length] &&
            item.QuestType == quest.Type);
        DungeonEnviromentData envData = DarkestDungeonManager.Data.DungeonEnviromentData[quest.Dungeon];

        int RoomsLeft = genData.BaseRoomNumber;
        int HallsLeft = genData.BaseCorridorNumber;
        int xSize = genData.GridSizeX;
        int ySize = genData.GridSizeY;
        dungeon.GridSizeX = xSize;
        dungeon.GridSizeY = ySize;

        List<GenRoom> Areas = new List<GenRoom>();
        GenRoom[,] areaGrid = new GenRoom[xSize, ySize];

        GenerateRooms(Areas, areaGrid, RoomsLeft, xSize, ySize);
        GenRoom hub = GetHub(Areas);

        List<GenRoom> ExistingRooms = ForceBorderRooms(Areas, hub, RoomsLeft);
        List<GenHall> ExistingHalls = ForceHallConnection(Areas, hub, ExistingRooms, HallsLeft);

        dungeon.Rooms = GetFinalRooms(ExistingRooms, genData);
        dungeon.Hallways = GetFinalHallways(dungeon, ExistingHalls, genData);

        MarkEntrance(dungeon);
        switch(quest.Goal.Type)
        {
            case "kill_monster":
                if (quest.Goal.QuestData is QuestKillMonsterData)
                {
                    var bossData = quest.Goal.QuestData as QuestKillMonsterData;
                    envData.BattleMashes.Find(mash => mash.BossEncounters.Find(encounter =>
                        encounter.MonsterSet.Contains(bossData.MonsterNameIds[0])) != null);
                    var bossGenRoom = LongestPathRoom(ExistingRooms.Find(room => room.Id == dungeon.StartingRoomId), ExistingRooms);
                    var bossRoom = dungeon.Rooms[bossGenRoom.Id];
                    bossRoom.Type = AreaType.Boss;
                    var bossEncounter = envData.BattleMashes.Find(mash => mash.MashId == quest.Difficulty).
                        BossEncounters.Find(encounter => encounter.MonsterSet.Contains(bossData.MonsterNameIds[0]));
                    bossRoom.BattleEncounter = new BattleEncounter(bossEncounter.MonsterSet);
                }
                else
                    Debug.LogError("Missing boss data in dungeon!");
                break;
            case "activate":
                if (quest.Goal.QuestData is QuestActivateData)
                {
                    var activateData = quest.Goal.QuestData as QuestActivateData;
                    var lastRoom = LongestPathRoom(ExistingRooms.Find(room => room.Id == dungeon.StartingRoomId), ExistingRooms);
                    for(int i = 0; i < activateData.Amount; i++)
                    {
                        var availableRooms = ExistingRooms.FindAll(room =>
                            room.MinPath >= (float)i / activateData.Amount * lastRoom.MinPath &&
                            room.MinPath <= (float)(i + 1) / activateData.Amount * lastRoom.MinPath);
                        int randomRoom = Random.Range(0, availableRooms.Count - 1);
                        var questRoom = dungeon.Rooms[availableRooms[randomRoom].Id];
                        if (questRoom.Type == AreaType.Empty)
                        {
                            var curio = new Curio(activateData.CurioName);
                            curio.IsQuestCurio = true;

                            if(quest.Goal.StartingItems.Count > 0)
                            {
                                curio.ItemInteractions.Add(new ItemInteraction()
                                {
                                    Chance = 1,
                                    ItemId = quest.Goal.StartingItems[0].Id,
                                    ResultType = "loot",
                                    Results = new List<CurioResult>(),
                                });
                            }
                            
                            questRoom.Type = AreaType.Curio;
                            questRoom.Prop = curio;
                        }
                        else
                            i--;
                    }
                }
                break;
            case "gather":
                if (quest.Goal.QuestData is QuestGatherData)
                {
                    var gatherData = quest.Goal.QuestData as QuestGatherData;
                    var lastRoom = LongestPathRoom(ExistingRooms.Find(room => room.Id == dungeon.StartingRoomId), ExistingRooms);
                    for(int i = 0; i < gatherData.Item.Amount; i++)
                    {
                        var availableRooms = ExistingRooms.FindAll(room =>
                            room.MinPath >= (float)i / gatherData.Item.Amount * lastRoom.MinPath &&
                            room.MinPath <= (float)(i + 1) / gatherData.Item.Amount * lastRoom.MinPath);
                        int randomRoom = Random.Range(0, availableRooms.Count - 1);
                        var questRoom = dungeon.Rooms[availableRooms[randomRoom].Id];
                        if (questRoom.Type == AreaType.Empty)
                        {
                            var curio = new Curio(gatherData.CurioName);
                            curio.IsQuestCurio = true;

                            var curioInteraction = new CurioInteraction();
                            curioInteraction.Chance = 1;
                            curioInteraction.ResultType = "loot";
                            curioInteraction.Results = new List<CurioResult>();
                            curioInteraction.Results.Add(new CurioResult()
                            {
                                Chance = 1,
                                Draws = 1,
                                Item = gatherData.Item.Id,
                            });
                            curio.Results.Add(curioInteraction);
                            questRoom.Type = AreaType.Curio;
                            questRoom.Prop = curio;
                        }
                        else
                            i--;
                    }
                }
                break;
            default:
                break;
        }
        PopulateRooms(dungeon, genData);
        LoadRoomEnviroment(dungeon, envData, quest.Difficulty);
        PopulateHalls(dungeon, genData);
        LoadHallEnviroment(dungeon, envData, quest.Difficulty);

        dungeon.GridSizeX = 1 + (xSize - 1) * 7;
        dungeon.GridSizeY = 1 + (ySize - 1) * 7;
        dungeon.Name = quest.Dungeon;
        dungeon.DungeonMash = envData.BattleMashes.Find(mash => mash.MashId == quest.Difficulty);
        dungeon.SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].
            BattleMashes.Find(mash => mash.MashId == quest.Difficulty);
        return dungeon;
    }

    static void GenerateRooms(List<GenRoom> Areas, GenRoom[,] areaGrid, int RoomsLeft, int xSize, int ySize)
    {
        for (int i = 0; i < xSize; i++)
            for (int j = 0; j < ySize; j++)
            {
                areaGrid[i, j] = new GenRoom(i, j);
                areaGrid[i, j].Id = string.Format("room{0}_{1}", i + 1, j + 1);
                Areas.Add(areaGrid[i, j]);
            }

        List<GenRoom> emptyAreas = new List<GenRoom>(Areas);

        for (int i = 0; i < RoomsLeft; i++)
        {
            int index = Random.Range(0, emptyAreas.Count);
            emptyAreas[index].Exists = true;
            emptyAreas.RemoveAt(index);
        }

        for(int j = 0; j < ySize; j++)
        {
            for (int i = 1; i < xSize; i++ )
            {
                GenHall hall = new GenHall();
                hall.roomA = areaGrid[i, j];
                hall.roomB = areaGrid[i - 1, j];
                hall.Id = string.Format("hall{0}_{1}", hall.roomA.Id, hall.roomB.Id);
                areaGrid[i, j].left = hall;
                areaGrid[i - 1, j].right = hall;
            }
        }

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 1; j < ySize; j++)
            {
                GenHall hall = new GenHall();
                hall.roomA = areaGrid[i, j];
                hall.roomB = areaGrid[i, j - 1];
                hall.Id = string.Format("hall{0}_{1}", hall.roomA.Id, hall.roomB.Id);
                areaGrid[i, j].bot = hall;
                areaGrid[i, j - 1].top = hall;
            }
        }
    }
    static GenRoom GetHub(List<GenRoom> Areas)
    {
        GenRoom room = null;
        int maxConnectivity = -1;
        for(int i = 0; i < Areas.Count; i++)
        {
            if (Areas[i].Exists)
            {
                if (Areas[i].BorderingRooms > maxConnectivity)
                {
                    room = Areas[i];
                    maxConnectivity = Areas[i].BorderingRooms;
                }
            }
        }
        return room;
    }

    static GenRoom LongestPathRoom(GenRoom entrance, List<GenRoom> Areas)
    {
        AdvancePath(entrance, 0);

        int maxPath = Areas.Max(area => area.MinPath);
        var maxRooms = Areas.FindAll(area => area.MinPath == maxPath);
        if (maxRooms.Count > 0)
        {
            return maxRooms[Random.Range(0, maxRooms.Count)];
        }
        else
            return null;
    }
    static void AdvancePath(GenRoom room, int currentPath)
    {
        if (room == null || room.MinPath <= currentPath)
            return;
        else
            room.MinPath = currentPath;

        if(room.left != null && room.left.Exists)
        AdvancePath(room.LeftRoom, currentPath + 1);
        if (room.right != null && room.right.Exists)
        AdvancePath(room.RightRoom, currentPath + 1);
        if (room.top != null && room.top.Exists)
        AdvancePath(room.TopRoom, currentPath + 1);
        if (room.bot != null && room.bot.Exists)
        AdvancePath(room.BotRoom, currentPath + 1);
    }

    static List<GenRoom> FindBorderPath(GenRoom hub)
    {
        List<GenRoom> visitedAreas = new List<GenRoom>();
        AdvanceBorderPath(hub, visitedAreas);
        return visitedAreas;
    }
    static void AdvanceBorderPath(GenRoom hub, List<GenRoom> visited)
    {
        if (hub != null && hub.Exists)
        {
            visited.Add(hub);
            if (!visited.Contains(hub.LeftRoom))
                AdvanceBorderPath(hub.LeftRoom, visited);
            if (!visited.Contains(hub.RightRoom))
                AdvanceBorderPath(hub.RightRoom, visited);
            if (!visited.Contains(hub.TopRoom))
                AdvanceBorderPath(hub.TopRoom, visited);
            if (!visited.Contains(hub.BotRoom))
                AdvanceBorderPath(hub.BotRoom, visited);
        }
    }

    static List<GenRoom> FindHallWayPath(GenRoom hub)
    {
        List<GenRoom> visitedAreas = new List<GenRoom>();
        AdvanceHallwayPath(hub, visitedAreas);
        return visitedAreas;
    }
    static void AdvanceHallwayPath(GenRoom hub, List<GenRoom> visited)
    {
        if (hub != null && hub.Exists)
        {
            visited.Add(hub);
            if (!visited.Contains(hub.LeftRoom))
                AdvanceBorderPath(hub.LeftRoom, visited);
            if (!visited.Contains(hub.RightRoom))
                AdvanceBorderPath(hub.RightRoom, visited);
            if (!visited.Contains(hub.TopRoom))
                AdvanceBorderPath(hub.TopRoom, visited);
            if (!visited.Contains(hub.BotRoom))
                AdvanceBorderPath(hub.BotRoom, visited);
        }
    }

    static List<GenRoom> ForceBorderRooms(List<GenRoom> areas, GenRoom hub, int roomNumber)
    {
        List<GenRoom> visitedAreas = FindBorderPath(hub);

        while (visitedAreas.Count != roomNumber)
        {
            foreach (var area in areas)
            {
                if (area.Exists && !visitedAreas.Contains(area))
                {
                    area.Exists = false;
                    var availableRooms = areas.FindAll(item => item.Exists == false && item.BorderingRooms > 0);
                    GenRoom newRandomArea = availableRooms[Random.Range(0, availableRooms.Count)];
                    newRandomArea.Exists = true;
                    break;
                }
            }
            visitedAreas = FindBorderPath(hub);
        }
        return visitedAreas;
    }
    static List<GenHall> ForceHallConnection(List<GenRoom> areas, GenRoom hub, List<GenRoom> existingRooms, int hallNumber)
    {
        List<GenHall> existingHalls = new List<GenHall>();
        foreach(var room in existingRooms)
        {
            if(room.left != null && room.left.RoomsExist)
            {
                if(!existingHalls.Contains(room.left))
                {
                    room.left.Exists = true;
                    existingHalls.Add(room.left);
                }
            }
            if (room.right != null && room.right.RoomsExist)
            {
                if (!existingHalls.Contains(room.right))
                {
                    room.right.Exists = true;
                    existingHalls.Add(room.right);
                }
            }
            if (room.top != null && room.top.RoomsExist)
            {
                if (!existingHalls.Contains(room.top))
                {
                    room.top.Exists = true;
                    existingHalls.Add(room.top);
                }
            }
            if (room.bot != null && room.bot.RoomsExist)
            {
                if (!existingHalls.Contains(room.bot))
                {
                    room.bot.Exists = true;
                    existingHalls.Add(room.bot);
                }
            }
        }

        while(existingHalls.Count != hallNumber)
        {
            if(existingHalls.Count > hallNumber)
            {
                // if removed and path saved remove hall
            }
            else if (existingHalls.Count < hallNumber)
            {
                // move 1 connectivity room to max connectivity
            }
            break;
        }
        return existingHalls;
    }
    static Dictionary<string, DungeonRoom> GetFinalRooms(List<GenRoom> rooms, DungeonGenerationData genData)
    {
        Dictionary<string, DungeonRoom> finalAreas = new Dictionary<string, DungeonRoom>();
        foreach (var genRoom in rooms)
        {
            DungeonRoom room = new DungeonRoom(genRoom.Id, 1 + (genRoom.gridX - 1) * 7, 1 + (genRoom.gridY - 1) * 7);

            if(genRoom.left != null && genRoom.left.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.left.Id, Direction.Left));

            if (genRoom.right != null && genRoom.right.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.right.Id, Direction.Right));

            if (genRoom.top != null && genRoom.top.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.top.Id, Direction.Top));

            if (genRoom.bot != null && genRoom.bot.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.bot.Id, Direction.Bot));

            finalAreas.Add(room.Id, room);
        }

        return finalAreas;
    }
    static Dictionary<string, Hallway> GetFinalHallways(Dungeon dungeon, List<GenHall> halls, DungeonGenerationData genData)
    {
        Dictionary<string, Hallway> finalHallways = new Dictionary<string, Hallway>();
 
        foreach (var genHall in halls)
        {
            Hallway hallway = new Hallway(genHall.Id);

            hallway.RoomA = dungeon.Rooms[genHall.roomA.Id];
            hallway.RoomB = dungeon.Rooms[genHall.roomB.Id];
            int hallIncrementX = 0, hallIncrementY = 0;
            int hallGridX = hallway.RoomA.GridX, hallGridY = hallway.RoomA.GridY;

            if (hallway.RoomA.GridX < hallway.RoomB.GridX)
                hallIncrementX = 1;
            else if (hallway.RoomA.GridX > hallway.RoomB.GridX)
                hallIncrementX = -1;

            if (hallway.RoomA.GridY < hallway.RoomB.GridY)
                hallIncrementY = 1;
            else if (hallway.RoomA.GridY > hallway.RoomB.GridY)
                hallIncrementY = -1;

            hallGridX += hallIncrementX;
            hallGridY += hallIncrementY;

            hallway.Halls.Add(new HallSector(hallway.Id + "_0", hallGridX, hallGridY, hallway,
                new Door(hallway.Id, genHall.roomA.Id, Direction.Left)));

            for (int i = 1; i <= genData.Spacing; i++)
            {
                hallGridX += hallIncrementX;
                hallGridY += hallIncrementY;
                hallway.Halls.Add(new HallSector(hallway.Id + "_" + i.ToString(), hallGridX, hallGridY, hallway));
            }

            hallGridX += hallIncrementX;
            hallGridY += hallIncrementY;
            hallway.Halls.Add(new HallSector(hallway.Id + "_" + (genData.Spacing + 1).ToString(), hallGridX, hallGridY,
                hallway, new Door(hallway.Id, genHall.roomB.Id, Direction.Right)));

            finalHallways.Add(hallway.Id, hallway);
        }

        return finalHallways;
    }

    static void MarkEntrance(Dungeon dungeon)
    {
        int minConnections = 5;
        DungeonRoom entranceRoom = null;
        foreach (var room in dungeon.Rooms.Values)
        {
            if(room.Connections < minConnections)
            {
                minConnections = room.Connections;
                entranceRoom = room;
            }
            else if (room.Connections == minConnections)
            {
                if(Random.Range(0, 2) == 1)
                {
                    minConnections = room.Connections;
                    entranceRoom = room;
                }
            }
        }
        if(entranceRoom == null)
        {
            List<DungeonRoom> rooms = Enumerable.ToList(dungeon.Rooms.Values);
            entranceRoom = rooms[Random.Range(0, rooms.Count)];
        }

        entranceRoom.Type = AreaType.Entrance;
        dungeon.StartingRoomId = entranceRoom.Id;
    }
    static void PopulateRooms(Dungeon dungeon, DungeonGenerationData genData)
    {
        List<DungeonRoom> rooms = Enumerable.ToList(dungeon.Rooms.Values).FindAll(item => item.Type == AreaType.Empty);

        dungeon.TotalRoomBattles = Random.Range(genData.TotalRoomBattleMin, genData.TotalRoomBattleMax + 1);
        int currentBattles = 0;
        int maxBattles = dungeon.TotalRoomBattles;

        int guardedTresures = Random.Range(genData.RoomGuardedTresureMin, genData.RoomGuardedTresureMax + 1);
        dungeon.RoomGuardedTresure = Mathf.Clamp(guardedTresures, 0, maxBattles - currentBattles);
        currentBattles += dungeon.RoomGuardedTresure;

        for (int i = 0; i < dungeon.RoomGuardedTresure; i++)
        {
            if (rooms.Count == 0)
                return;

            int index = Random.Range(0, rooms.Count);
            rooms[index].Type = AreaType.BattleTresure;
            rooms.RemoveAt(index);
        }

        int guardedCurios = Random.Range(genData.RoomGuardedCurioMin, genData.RoomGuardedCurioMax + 1);
        dungeon.RoomGuardedCurio = Mathf.Clamp(guardedCurios, 0, maxBattles - currentBattles);
        currentBattles += dungeon.RoomGuardedCurio;

        for(int i = 0; i < dungeon.RoomGuardedCurio; i++)
        {
            if (rooms.Count == 0)
                return;

            int index = Random.Range(0, rooms.Count);
            rooms[index].Type = AreaType.BattleCurio;
            rooms.RemoveAt(index);
        }

        for (int i = currentBattles; i < dungeon.TotalRoomBattles; i++)
        {
            if (rooms.Count == 0)
                return;

            int index = Random.Range(0, rooms.Count);
            rooms[index].Type = AreaType.Battle;
            rooms.RemoveAt(index);
        }
    }
    static void PopulateHalls(Dungeon dungeon, DungeonGenerationData genData)
    {
        List<HallSector> hallSectors = new List<HallSector>();
        foreach (var hallway in dungeon.Hallways.Values)
        {
            foreach (var hallSector in hallway.Halls)
                if (hallSector.Type != AreaType.Door)
                    hallSectors.Add(hallSector);
        }

        dungeon.HallwayBattles = Random.Range(genData.HallwayBattleMin, genData.HallwayBattleMax + 1);
        for (int i = 0; i < dungeon.HallwayBattles; i++)
        {
            if (hallSectors.Count == 0)
                return;

            int index = Random.Range(0, hallSectors.Count);
            hallSectors[index].Type = AreaType.Battle;
            hallSectors.RemoveAt(index);
        }

        dungeon.HallwayTraps = Random.Range(genData.HallwayTrapMin, genData.HallwayTrapMax + 1);
        for (int i = 0; i < dungeon.HallwayTraps; i++)
        {
            if (hallSectors.Count == 0)
                return;

            int index = Random.Range(0, hallSectors.Count);
            hallSectors[index].Type = AreaType.Trap;
            hallSectors.RemoveAt(index);
        }

        dungeon.HallwayObstacles = Random.Range(genData.HallwayObstacleMin, genData.HallwayObstacleMax + 1);

        for (int i = 0; i < dungeon.HallwayObstacles; i++)
        {
            if (hallSectors.Count == 0)
                return;

            int index = Random.Range(0, hallSectors.Count);
            hallSectors[index].Type = AreaType.Obstacle;
            hallSectors.RemoveAt(index);
        }

        dungeon.HallwayCurios = Random.Range(genData.HallwayCurioMin, genData.HallwayCurioMax + 1);

        for (int i = 0; i < dungeon.HallwayCurios; i++)
        {
            if (hallSectors.Count == 0)
                return;

            int index = Random.Range(0, hallSectors.Count);
            hallSectors[index].Type = AreaType.Curio;
            hallSectors.RemoveAt(index);
        }

        dungeon.HallwayHunger = Random.Range(genData.HallwayHungerMin, genData.HallwayHungerMax + 1);

        for (int i = 0; i < dungeon.HallwayHunger; i++)
        {
            if (hallSectors.Count == 0)
                return;

            int index = Random.Range(0, hallSectors.Count);
            hallSectors[index].Type = AreaType.Hunger;
            hallSectors.RemoveAt(index);
        }
    }

    static void LoadRoomEnviroment(Dungeon dungeon, DungeonEnviromentData envData, int mashIndex)
    {
        foreach(var room in dungeon.Rooms.Values)
        {
            room.TextureId = envData.RoomVariations[Random.Range(0, envData.RoomVariations.Count)];
            switch (room.Type)
            {
                case AreaType.Battle:
                    var monsterBattleSet = RandomSolver.ChooseByRandom<DungeonBattleEncounter>(
                        envData.BattleMashes.Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;
                    room.BattleEncounter = new BattleEncounter(monsterBattleSet);
                    break;
                case AreaType.BattleCurio:
                    if(room.Prop == null)
                    {
                        string curioName = RandomSolver.ChooseByRandom<DungeonPropsEncounter>(envData.RoomTresures).PropName;
                        room.Prop = DarkestDungeonManager.Data.Curios[curioName];
                        var monsterCurioSet = RandomSolver.ChooseByRandom<DungeonBattleEncounter>(
                            envData.BattleMashes.Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;
                        room.BattleEncounter = new BattleEncounter(monsterCurioSet);
                    }
                    break;
                case AreaType.BattleTresure:
                    if(room.Prop == null)
                    {
                        string tresureName = RandomSolver.ChooseByRandom<DungeonPropsEncounter>(envData.RoomTresures).PropName;
                        room.Prop = DarkestDungeonManager.Data.Curios[tresureName];
                    }
                    var monsterTreasureSet = RandomSolver.ChooseByRandom<DungeonBattleEncounter>(
                        envData.BattleMashes.Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;
                    room.BattleEncounter = new BattleEncounter(monsterTreasureSet);
                    break;
                case AreaType.Empty:
                case AreaType.Entrance:
                case AreaType.Curio:
                case AreaType.Boss:
                    break;
                default:
                    Debug.LogError("Unexpected room type: " + room.Type.ToString());
                    break;
            }
        }
    }
    static void LoadHallEnviroment(Dungeon dungeon, DungeonEnviromentData envData, int mashIndex)
    {
        foreach (var hall in dungeon.Hallways.Values)
        {
            foreach (var sector in hall.Halls)
            {
                sector.TextureId = Random.Range(1, envData.HallVariations + 1).ToString();
                switch(sector.Type)
                {
                    case AreaType.Battle:
                        var monsterBattleSet = RandomSolver.ChooseByRandom<DungeonBattleEncounter>(
                            envData.BattleMashes.Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;
                        sector.BattleEncounter = new BattleEncounter(monsterBattleSet);
                        break;
                    case AreaType.Curio:
                        if(sector.Prop == null)
                        {
                            string curioName = RandomSolver.ChooseByRandom<DungeonPropsEncounter>(envData.HallCurios).PropName;
                            sector.Prop = DarkestDungeonManager.Data.Curios[curioName];
                        }
                        break;                   
                    case AreaType.Hunger:
                        break;
                    case AreaType.Obstacle:
                        string obstacleName = RandomSolver.ChooseByRandom<DungeonPropsEncounter>(envData.Obstacles).PropName;
                        sector.Prop = DarkestDungeonManager.Data.Obstacles[obstacleName];
                        break;
                    case AreaType.Trap:
                        string trapName = RandomSolver.ChooseByRandom<DungeonPropsEncounter>(envData.Traps).PropName;
                        sector.Prop = DarkestDungeonManager.Data.Traps[trapName];
                        break;
                    case AreaType.Empty:
                    case AreaType.Door:
                        break;
                    default:
                        Debug.LogError("Unexpected hall sector type: " + sector.Type.ToString());
                        break;
                }
            }
        }
    }
}

class GenHall
{
    public string Id;
    public bool Exists;

    public GenRoom roomA;
    public GenRoom roomB;

    public GenRoom GetOpposite(GenRoom room)
    {
        if (roomA.Id == room.Id)
            return roomB;
        else if (roomB.Id == room.Id)
            return roomA;

        Debug.LogError("Room not found.");
        return null;
    }
    public bool RoomsExist
    {
        get
        {
            return roomA.Exists && roomB.Exists;
        }
    }
    public GenHall()
    {
        Exists = false;
    }
}

class GenRoom
{
    public string Id;
    public bool Exists;

    public int MinPath = 999;

    public int gridX;
    public int gridY;

    public GenHall left;
    public GenHall right;
    public GenHall top;
    public GenHall bot;

    public int BorderingRooms
    {
        get
        {
            int conns = 0;
            if (left != null && left.GetOpposite(this).Exists)
                conns++;
            if (right != null && right.GetOpposite(this).Exists)
                conns++;
            if (top != null && top.GetOpposite(this).Exists)
                conns++;
            if (bot != null && bot.GetOpposite(this).Exists)
                conns++;
            return conns;
        }
    }
    public int ConnectedRooms
    {
        get
        {
            int conns = 0;
            if (left != null && left.Exists)
                conns++;
            if (right != null && right.Exists)
                conns++;
            if (top != null && top.Exists)
                conns++;
            if (bot != null && bot.Exists)
                conns++;
            return conns;
        }
    }

    public GenRoom LeftRoom
    {
        get
        {
            if (left == null)
                return null;
            else
                return left.GetOpposite(this);
        }
    }
    public GenRoom RightRoom
    {
        get
        {
            if (right == null)
                return null;
            else
                return right.GetOpposite(this);
        }
    }
    public GenRoom TopRoom
    {
        get
        {
            if (top == null)
                return null;
            else
                return top.GetOpposite(this);
        }
    }
    public GenRoom BotRoom
    {
        get
        {
            if (bot == null)
                return null;
            else
                return bot.GetOpposite(this);
        }
    }

    public GenRoom(int x, int y)
    {
        gridX = x;
        gridY = y;
        Exists = false;
    }
}