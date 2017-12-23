using UnityEngine;
using System.Collections.Generic;
using System.Xml;

public class LocalizationManager : MonoBehaviour
{
    private class StringTable
    {
        public Dictionary<string, List<string>> Data { get; private set; }

        public StringTable()
        {
            Data = new Dictionary<string, List<string>>();
        }

        public string this[string stringId]
        {
            get
            {
                if (!Data.ContainsKey(stringId))
                    return stringId;

                List<string> items = Data[stringId];
                return items.Count == 1 ? items[0] : items[Random.Range(0, items.Count)];
            }
        }
    }

    private static LocalizationManager Instanse { get; set; }

    private const string LocalizationDataPath = "Data/Localization/";

    private readonly List<string> stringTableNames = new List<string>
    {
        "Activity", "Actors", "Curios", "Dialogue", "Help", "Heroes", "Menu", "Misc",
        "Monsters", "Names", "PartyNames", "Quirks", "Kickstarter", "TownEvents", "Journal"
    };

    private StringTable MasterTable { get; set; }

    private void Awake()
    {
        if (Instanse != null)
            return;

        Instanse = this;
        MasterTable = new StringTable();

        foreach (var tableName in stringTableNames)
            FillStringTable(tableName, MasterTable.Data);
    }

    public static string GetString(string stringId)
    {
        return Instanse[stringId];
    }

    private void FillStringTable(string tableName, Dictionary<string, List<string>> dictionary)
    {
        TextAsset tableText = Resources.Load<TextAsset>(LocalizationDataPath + tableName);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(tableText.text);

        XmlNode xmlNode = xmlDoc.DocumentElement.SelectSingleNode("language");
        foreach (XmlElement entry in xmlNode.SelectNodes("entry"))
        {
            if (entry.IsEmpty)
                continue;

            string key = entry.Attributes["id"].Value;
            string value = entry.InnerText;

            if (dictionary.ContainsKey(key))
                dictionary[key].Add(value);
            else
                dictionary.Add(key, new List<string>(new [] { value }));
        }
    }

    private string this[string stringId]
    {
        get
        {
            return Instanse.MasterTable[stringId];
        }
    }
}