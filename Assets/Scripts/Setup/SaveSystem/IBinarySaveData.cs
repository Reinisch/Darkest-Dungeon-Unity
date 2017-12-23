using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public interface IBinarySaveData
{
    bool IsMeetingSaveCriteria { get; }

    void Write(BinaryWriter bw);
    void Read(BinaryReader br);
}

public static class BinarySaveDataHelper
{
    public static void Write<T>(this List<T> binaryDataList, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        var dataToSave = binaryDataList.FindAll(item => item.IsMeetingSaveCriteria);

        bw.Write(dataToSave.Count);
        dataToSave.ForEach(item => item.Write(bw));
    }

    public static void Read<T>(this List<T> binaryDataList, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        binaryDataList.Clear();
        int itemCount = br.ReadInt32();
        for (int i = 0; i < itemCount; i++)
            binaryDataList.Add(Create<T>(br));
    }

    public static void Write<T>(this List<List<T>> binaryDataLists, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        bw.Write(binaryDataLists.Count);
        binaryDataLists.ForEach(item => item.Write(bw));
    }

    public static void Read<T>(this List<List<T>> binaryDataLists, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        binaryDataLists.Clear();
        int listCount = br.ReadInt32();
        for (int i = 0; i < listCount; i++)
        {
            var newList = new List<T>();
            newList.Read(br);
            binaryDataLists.Add(newList);
        }
    }

    public static void Write<T>(this Dictionary<string, T> binaryDataDictionary, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        bw.Write(binaryDataDictionary.Count(item => item.Value.IsMeetingSaveCriteria));

        foreach (var entry in binaryDataDictionary)
            if (entry.Value.IsMeetingSaveCriteria)
                entry.Value.Write(bw);
    }

    public static void Read<T>(this Dictionary<string, T> binaryDataDictionary, Func<T, string> keySelector, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        binaryDataDictionary.Clear();
        int itemCount = br.ReadInt32();
        for (int i = 0; i < itemCount; i++)
        {
            T newEntry = Create<T>(br);
            binaryDataDictionary.Add(keySelector(newEntry), newEntry);
        }
    }

    public static void Write<T>(this Dictionary<int, Dictionary<string, T>> instancedDictionary, BinaryWriter bw) where T : class, IBinarySaveData, new()
    {
        bw.Write(instancedDictionary.Count);

        foreach (var entry in instancedDictionary)
        {
            bw.Write(entry.Key);
            entry.Value.Write(bw);
        }
    }

    public static void Read<T>(this Dictionary<int, Dictionary<string, T>> instancedDictionary, Func<T, string> keySelector, BinaryReader br) where T : class, IBinarySaveData, new()
    {
        instancedDictionary.Clear();
        int instancesCount = br.ReadInt32();
        for (int i = 0; i < instancesCount; i++)
        {
            var newInstance = new Dictionary<string, T>();
            var instanceId = br.ReadInt32();
            newInstance.Read(keySelector, br);
            instancedDictionary.Add(instanceId, newInstance);
        }
    }

    public static void Write(this Dictionary<string, int> binaryDataDictionary, BinaryWriter bw)
    {
        bw.Write(binaryDataDictionary.Count);

        foreach (var entry in binaryDataDictionary)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
    }

    public static void Read(this Dictionary<string, int> binaryDataDictionary, BinaryReader br)
    {
        binaryDataDictionary.Clear();

        int itemCount = br.ReadInt32();
        for (int i = 0; i < itemCount; i++)
            binaryDataDictionary.Add(br.ReadString(), br.ReadInt32());
    }

    public static void Write(this List<int> binaryDataList, BinaryWriter bw)
    {
        bw.Write(binaryDataList.Count);
        binaryDataList.ForEach(bw.Write);
    }

    public static void Read(this List<int> binaryDataList, BinaryReader br)
    {
        binaryDataList.Clear();
        int itemCount = br.ReadInt32();
        for (int i = 0; i < itemCount; i++)
            binaryDataList.Add(br.ReadInt32());
    }

    public static void Write(this List<string> binaryDataList, BinaryWriter bw)
    {
        bw.Write(binaryDataList.Count);
        for (int i = 0; i < binaryDataList.Count; i++)
            bw.Write(binaryDataList[i] ?? "");
    }

    public static void Read(this List<string> binaryDataList, BinaryReader br)
    {
        binaryDataList.Clear();
        int itemCount = br.ReadInt32();
        for (int i = 0; i < itemCount; i++)
            binaryDataList.Add(br.ReadString());
    }

    public static void Write(this List<bool> binaryDataList, BinaryWriter bw)
    {
        bw.Write(binaryDataList.Count);
        for (int i = 0; i < binaryDataList.Count; i++)
            bw.Write(binaryDataList[i]);
    }

    public static void Read(this List<bool> binaryDataList, BinaryReader br)
    {
        binaryDataList.Clear();
        int itemCount = br.ReadInt32();
        for (int i = 0; i < itemCount; i++)
            binaryDataList.Add(br.ReadBoolean());
    }

    public static T Create<T>(BinaryReader br) where T : class, IBinarySaveData
    {
        var saveDataType = typeof(T);
        T newBinaryData = null;

        if (typeof(Quest).IsAssignableFrom(saveDataType))
        {
            string plotGenId = br.ReadString();
            if (plotGenId == "tutorial")
                newBinaryData = new PlotQuest(plotGenId, new PlotTrinketReward { Amount = 0, Rarity = "very_common" }) as T;
            else if (plotGenId != "")
                newBinaryData = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(plQuest => plQuest.Id == plotGenId).Copy() as T;
            else
                newBinaryData = new Quest() as T;
        }
        else if (typeof(Prop).IsAssignableFrom(saveDataType))
        {
            AreaType propType = (AreaType)br.ReadInt32();

            switch (propType)
            {
                case AreaType.Door:
                    newBinaryData = new Door() as T;
                    break;
                case AreaType.Curio:
                    bool isQuestCurio = br.ReadBoolean();
                    string curioName = br.ReadString();

                    if (isQuestCurio)
                        newBinaryData = new Curio { IsQuestCurio = true, StringId = curioName} as T;
                    else
                        newBinaryData = DarkestDungeonManager.Data.Curios[curioName] as T;
                    break;
                case AreaType.Obstacle:
                    newBinaryData = DarkestDungeonManager.Data.Obstacles[br.ReadString()] as T;
                    break;
                case AreaType.Trap:
                    newBinaryData = DarkestDungeonManager.Data.Traps[br.ReadString()] as T;
                    break;
            }
        }

        if (newBinaryData == null)
            newBinaryData = Activator.CreateInstance<T>();

        newBinaryData.Read(br);
        return newBinaryData;
    }
}