using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public static class DungeonGenerator
{
    private class GenHall
    {
        public string Id;
        public bool Exists;

        public GenRoom RoomA;
        public GenRoom RoomB;

        public bool RoomsExist { get { return RoomA.Exists && RoomB.Exists; } }

        public GenRoom GetOpposite(GenRoom room)
        {
            if (RoomA.Id == room.Id)
                return RoomB;
            if (RoomB.Id == room.Id)
                return RoomA;

            Assert.IsTrue(false, "Generated opposite room not found!");
            return RoomA;
        }

        public GenHall()
        {
            Exists = false;
        }
    }

    private class GenRoom
    {
        public string Id;
        public bool Exists;
        public int MinPath = int.MaxValue;

        public readonly int GridX;
        public readonly int GridY;

        public GenHall Left;
        public GenHall Right;
        public GenHall Top;
        public GenHall Bot;

        public GenRoom LeftRoom { get { return Left == null ? null : Left.GetOpposite(this); } }
        public GenRoom RightRoom { get { return Right == null ? null : Right.GetOpposite(this); } }
        public GenRoom TopRoom { get { return Top == null ? null : Top.GetOpposite(this); } }
        public GenRoom BotRoom { get { return Bot == null ? null : Bot.GetOpposite(this); } }

        public int BorderingRooms
        {
            get
            {
                int conns = 0;
                if (Left != null && Left.GetOpposite(this).Exists)
                    conns++;
                if (Right != null && Right.GetOpposite(this).Exists)
                    conns++;
                if (Top != null && Top.GetOpposite(this).Exists)
                    conns++;
                if (Bot != null && Bot.GetOpposite(this).Exists)
                    conns++;
                return conns;
            }
        }

        public GenRoom(int x, int y)
        {
            GridX = x;
            GridY = y;
            Exists = false;
        }
    }

    public static Dungeon GenerateDungeon(Quest quest, int seed = 0)
    {
        if (seed != 0)
            Random.InitState(seed);
        
        string[] lengthes = {"", "short", "medium", "long"};

        Dungeon dungeon = new Dungeon();
        DungeonGenerationData genData = DarkestDungeonManager.Data.DungeonGenerationData.Find(item =>
            item.Dungeon == quest.Dungeon && item.Length == lengthes[quest.Length] && item.QuestType == quest.Type);
        DungeonEnviromentData envData = DarkestDungeonManager.Data.DungeonEnviromentData[quest.Dungeon];

        int roomsLeft = genData.BaseRoomNumber;
        int hallsLeft = genData.BaseCorridorNumber;
        int xSize = genData.GridSizeX;
        int ySize = genData.GridSizeY;
        dungeon.GridSizeX = xSize;
        dungeon.GridSizeY = ySize;

        List<GenRoom> areas = new List<GenRoom>();
        GenRoom[,] areaGrid = new GenRoom[xSize, ySize];

        GenerateRooms(areas, areaGrid, roomsLeft, xSize, ySize);
        GenRoom hub = FindMaxConnectivityRoom(areas);

        List<GenRoom> existingRooms = ForceBorderRooms(areas, hub, roomsLeft);
        List<GenHall> existingHalls = ForceHallConnection(existingRooms, hallsLeft);

        dungeon.Rooms = CreateFinalRooms(existingRooms);
        dungeon.Hallways = CreateFinalHallways(dungeon, existingHalls, genData);

        MarkEntrance(dungeon);
        PopulateQuestGoals(dungeon, quest, existingRooms, envData);
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

    private static void GenerateRooms(List<GenRoom> areas, GenRoom[,] areaGrid, int roomsLeft, int xSize, int ySize)
    {
        for (int i = 0; i < xSize; i++)
            for (int j = 0; j < ySize; j++)
            {
                areaGrid[i, j] = new GenRoom(i, j) {Id = string.Format("room{0}_{1}", i + 1, j + 1)};
                areas.Add(areaGrid[i, j]);
            }

        List<GenRoom> emptyAreas = new List<GenRoom>(areas);

        for (int i = 0; i < roomsLeft; i++)
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
                hall.RoomA = areaGrid[i, j];
                hall.RoomB = areaGrid[i - 1, j];
                hall.Id = string.Format("hall{0}_{1}", hall.RoomA.Id, hall.RoomB.Id);
                areaGrid[i, j].Left = hall;
                areaGrid[i - 1, j].Right = hall;
            }
        }

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 1; j < ySize; j++)
            {
                GenHall hall = new GenHall();
                hall.RoomA = areaGrid[i, j];
                hall.RoomB = areaGrid[i, j - 1];
                hall.Id = string.Format("hall{0}_{1}", hall.RoomA.Id, hall.RoomB.Id);
                areaGrid[i, j].Bot = hall;
                areaGrid[i, j - 1].Top = hall;
            }
        }
    }

    private static GenRoom FindMaxConnectivityRoom(List<GenRoom> areas)
    {
        GenRoom room = null;
        int maxConnectivity = -1;
        foreach (GenRoom generatedRoom in areas)
        {
            if (!generatedRoom.Exists)
                continue;

            if (generatedRoom.BorderingRooms <= maxConnectivity)
                continue;

            room = generatedRoom;
            maxConnectivity = generatedRoom.BorderingRooms;
        }
        return room;
    }

    private static GenRoom FindLongestPathRoom(GenRoom entrance, List<GenRoom> areas)
    {
        CalculateMinPath(entrance, 0);

        int maxPath = areas.Max(area => area.MinPath);
        var maxRooms = areas.FindAll(area => area.MinPath == maxPath);
        return maxRooms.Count > 0 ? maxRooms[Random.Range(0, maxRooms.Count)] : null;
    }

    private static List<GenRoom> FindBorderingRooms(GenRoom fromRoom, List<GenRoom> visited = null)
    {
        if (visited == null)
            visited = new List<GenRoom>();

        if (fromRoom == null || !fromRoom.Exists)
            return visited;

        visited.Add(fromRoom);
        if (!visited.Contains(fromRoom.LeftRoom))
            FindBorderingRooms(fromRoom.LeftRoom, visited);
        if (!visited.Contains(fromRoom.RightRoom))
            FindBorderingRooms(fromRoom.RightRoom, visited);
        if (!visited.Contains(fromRoom.TopRoom))
            FindBorderingRooms(fromRoom.TopRoom, visited);
        if (!visited.Contains(fromRoom.BotRoom))
            FindBorderingRooms(fromRoom.BotRoom, visited);

        return visited;
    }

    private static List<GenRoom> ForceBorderRooms(List<GenRoom> areas, GenRoom hub, int roomNumber)
    {
        List<GenRoom> visitedAreas = FindBorderingRooms(hub);

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
            visitedAreas = FindBorderingRooms(hub);
        }
        return visitedAreas;
    }

    private static List<GenHall> ForceHallConnection(List<GenRoom> existingRooms, int hallNumber)
    {
        List<GenHall> existingHalls = new List<GenHall>();
        foreach(var room in existingRooms)
        {
            if(room.Left != null && room.Left.RoomsExist)
            {
                if(!existingHalls.Contains(room.Left))
                {
                    room.Left.Exists = true;
                    existingHalls.Add(room.Left);
                }
            }
            if (room.Right != null && room.Right.RoomsExist)
            {
                if (!existingHalls.Contains(room.Right))
                {
                    room.Right.Exists = true;
                    existingHalls.Add(room.Right);
                }
            }
            if (room.Top != null && room.Top.RoomsExist)
            {
                if (!existingHalls.Contains(room.Top))
                {
                    room.Top.Exists = true;
                    existingHalls.Add(room.Top);
                }
            }
            if (room.Bot != null && room.Bot.RoomsExist)
            {
                if (!existingHalls.Contains(room.Bot))
                {
                    room.Bot.Exists = true;
                    existingHalls.Add(room.Bot);
                }
            }
        }

        while(existingHalls.Count != hallNumber)
        {
            if(existingHalls.Count > hallNumber)
            {
                // TODO: if after hall removal path is same then remove this hall
            }
            else if (existingHalls.Count < hallNumber)
            {
                // TODO: move 1 connectivity room to max connectivity
            }
            break;
        }
        return existingHalls;
    }

    private static Dictionary<string, DungeonRoom> CreateFinalRooms(List<GenRoom> rooms)
    {
        Dictionary<string, DungeonRoom> finalAreas = new Dictionary<string, DungeonRoom>();
        foreach (var genRoom in rooms)
        {
            DungeonRoom room = new DungeonRoom(genRoom.Id, 1 + (genRoom.GridX - 1) * 7, 1 + (genRoom.GridY - 1) * 7);

            if(genRoom.Left != null && genRoom.Left.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.Left.Id, Direction.Left));

            if (genRoom.Right != null && genRoom.Right.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.Right.Id, Direction.Right));

            if (genRoom.Top != null && genRoom.Top.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.Top.Id, Direction.Top));

            if (genRoom.Bot != null && genRoom.Bot.Exists)
                room.Doors.Add(new Door(genRoom.Id, genRoom.Bot.Id, Direction.Bot));

            finalAreas.Add(room.Id, room);
        }

        return finalAreas;
    }

    private static Dictionary<string, Hallway> CreateFinalHallways(Dungeon dungeon, List<GenHall> halls, DungeonGenerationData genData)
    {
        Dictionary<string, Hallway> finalHallways = new Dictionary<string, Hallway>();
 
        foreach (var genHall in halls)
        {
            Hallway hallway = new Hallway(genHall.Id);

            hallway.RoomA = dungeon.Rooms[genHall.RoomA.Id];
            hallway.RoomB = dungeon.Rooms[genHall.RoomB.Id];
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
                new Door(hallway.Id, genHall.RoomA.Id, Direction.Left)));

            for (int i = 1; i <= genData.Spacing; i++)
            {
                hallGridX += hallIncrementX;
                hallGridY += hallIncrementY;
                hallway.Halls.Add(new HallSector(hallway.Id + "_" + i, hallGridX, hallGridY, hallway));
            }

            hallGridX += hallIncrementX;
            hallGridY += hallIncrementY;
            hallway.Halls.Add(new HallSector(hallway.Id + "_" + (genData.Spacing + 1), hallGridX, hallGridY,
                hallway, new Door(hallway.Id, genHall.RoomB.Id, Direction.Right)));

            finalHallways.Add(hallway.Id, hallway);
        }

        return finalHallways;
    }

    private static void CalculateMinPath(GenRoom room, int currentPath)
    {
        if (room == null || room.MinPath <= currentPath)
            return;

        room.MinPath = currentPath;

        if (room.Left != null && room.Left.Exists)
            CalculateMinPath(room.LeftRoom, currentPath + 1);
        if (room.Right != null && room.Right.Exists)
            CalculateMinPath(room.RightRoom, currentPath + 1);
        if (room.Top != null && room.Top.Exists)
            CalculateMinPath(room.TopRoom, currentPath + 1);
        if (room.Bot != null && room.Bot.Exists)
            CalculateMinPath(room.BotRoom, currentPath + 1);
    }

    private static void MarkEntrance(Dungeon dungeon)
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
            List<DungeonRoom> rooms = dungeon.Rooms.Values.ToList();
            entranceRoom = rooms[Random.Range(0, rooms.Count)];
        }

        entranceRoom.Type = AreaType.Entrance;
        dungeon.StartingRoomId = entranceRoom.Id;
    }

    private static void PopulateRooms(Dungeon dungeon, DungeonGenerationData genData)
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

    private static void PopulateHalls(Dungeon dungeon, DungeonGenerationData genData)
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

    private static void PopulateQuestGoals(Dungeon dungeon, Quest quest, List<GenRoom> existingRooms, DungeonEnviromentData envData)
    {
        switch (quest.Goal.Type)
        {
            case "kill_monster":
                var killData = quest.Goal.QuestData as QuestKillMonsterData;
                if (killData != null)
                {
                    var bossGenRoom = FindLongestPathRoom(existingRooms.Find(room => room.Id == dungeon.StartingRoomId), existingRooms);
                    var bossRoom = dungeon.Rooms[bossGenRoom.Id];

                    bossRoom.Type = AreaType.Boss;
                    var bossEncounter = envData.BattleMashes.Find(mash => mash.MashId == quest.Difficulty).
                        BossEncounters.Find(encounter => encounter.MonsterSet.Contains(killData.MonsterNameIds[0]));
                    bossRoom.BattleEncounter = new BattleEncounter(bossEncounter.MonsterSet);
                }
                else
                    Debug.LogError("Missing boss data in dungeon!");
                break;
            case "activate":
                var activateData = quest.Goal.QuestData as QuestActivateData;
                if (activateData != null)
                {
                    var lastRoom = FindLongestPathRoom(existingRooms.Find(room => room.Id == dungeon.StartingRoomId), existingRooms);
                    for (int i = 0; i < activateData.Amount; i++)
                    {
                        var availableRooms = existingRooms.FindAll(room =>
                            room.MinPath >= (float)i / activateData.Amount * lastRoom.MinPath &&
                            room.MinPath <= (float)(i + 1) / activateData.Amount * lastRoom.MinPath);
                        int randomRoom = Random.Range(0, availableRooms.Count - 1);
                        var questRoom = dungeon.Rooms[availableRooms[randomRoom].Id];
                        if (questRoom.Type == AreaType.Empty)
                        {
                            var curio = new Curio(activateData.CurioName) { IsQuestCurio = true };

                            if (quest.Goal.StartingItems.Count > 0)
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
                    var lastRoom = FindLongestPathRoom(existingRooms.Find(room => room.Id == dungeon.StartingRoomId), existingRooms);
                    for (int i = 0; i < gatherData.Item.Amount; i++)
                    {
                        var availableRooms = existingRooms.FindAll(room =>
                            room.MinPath >= (float)i / gatherData.Item.Amount * lastRoom.MinPath &&
                            room.MinPath <= (float)(i + 1) / gatherData.Item.Amount * lastRoom.MinPath);
                        int randomRoom = Random.Range(0, availableRooms.Count - 1);
                        var questRoom = dungeon.Rooms[availableRooms[randomRoom].Id];
                        if (questRoom.Type == AreaType.Empty)
                        {
                            var curio = new Curio(gatherData.CurioName) { IsQuestCurio = true };

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
        }
    }

    private static void LoadRoomEnviroment(Dungeon dungeon, DungeonEnviromentData envData, int mashIndex)
    {
        foreach(var room in dungeon.Rooms.Values)
        {
            room.TextureId = envData.RoomVariations[Random.Range(0, envData.RoomVariations.Count)];
            switch (room.Type)
            {
                case AreaType.Battle:
                    var monsterBattleSet = RandomSolver.ChooseByRandom(envData.BattleMashes.
                        Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;

                    room.BattleEncounter = new BattleEncounter(monsterBattleSet);
                    break;
                case AreaType.BattleCurio:
                    if(room.Prop == null)
                    {
                        string curioName = RandomSolver.ChooseByRandom(envData.RoomTresures).PropName;
                        room.Prop = DarkestDungeonManager.Data.Curios[curioName];
                        var monsterCurioSet = RandomSolver.ChooseByRandom(envData.BattleMashes.
                            Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;

                        room.BattleEncounter = new BattleEncounter(monsterCurioSet);
                    }
                    break;
                case AreaType.BattleTresure:
                    if(room.Prop == null)
                    {
                        string tresureName = RandomSolver.ChooseByRandom(envData.RoomTresures).PropName;
                        room.Prop = DarkestDungeonManager.Data.Curios[tresureName];
                    }
                    var monsterTreasureSet = RandomSolver.ChooseByRandom(envData.BattleMashes.Find(mash =>
                        mash.MashId == mashIndex).RoomEncounters).MonsterSet;

                    room.BattleEncounter = new BattleEncounter(monsterTreasureSet);
                    break;
                case AreaType.Empty:
                case AreaType.Entrance:
                case AreaType.Curio:
                case AreaType.Boss:
                    break;
                default:
                    Assert.IsTrue(false, "Unexpected room type: " + room.Type);
                    break;
            }
        }
    }

    private static void LoadHallEnviroment(Dungeon dungeon, DungeonEnviromentData envData, int mashIndex)
    {
        foreach (var hall in dungeon.Hallways.Values)
        {
            foreach (var sector in hall.Halls)
            {
                sector.TextureId = Random.Range(1, envData.HallVariations + 1).ToString();
                switch(sector.Type)
                {
                    case AreaType.Battle:
                        var monsterBattleSet = RandomSolver.ChooseByRandom(
                            envData.BattleMashes.Find(mash => mash.MashId == mashIndex).RoomEncounters).MonsterSet;
                        sector.BattleEncounter = new BattleEncounter(monsterBattleSet);
                        break;
                    case AreaType.Curio:
                        if(sector.Prop == null)
                        {
                            string curioName = RandomSolver.ChooseByRandom(envData.HallCurios).PropName;
                            sector.Prop = DarkestDungeonManager.Data.Curios[curioName];
                        }
                        break;                   
                    case AreaType.Hunger:
                        break;
                    case AreaType.Obstacle:
                        string obstacleName = RandomSolver.ChooseByRandom(envData.Obstacles).PropName;
                        sector.Prop = DarkestDungeonManager.Data.Obstacles[obstacleName];
                        break;
                    case AreaType.Trap:
                        string trapName = RandomSolver.ChooseByRandom(envData.Traps).PropName;
                        sector.Prop = DarkestDungeonManager.Data.Traps[trapName];
                        break;
                    case AreaType.Empty:
                    case AreaType.Door:
                        break;
                    default:
                        Assert.IsTrue(false, "Unexpected hall sector type: " + sector.Type);
                        break;
                }
            }
        }
    }
}