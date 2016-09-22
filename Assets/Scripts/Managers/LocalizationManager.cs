using UnityEngine;
using System.Collections.Generic;
using System.Xml;

public class StringTable
{
    public string Name { get; set; }
    public Dictionary<string, List<string>> Data { get; set; }

    public StringTable()
    {
        Data = new Dictionary<string, List<string>>();
    }

    public string this[string stringId]
    {
        get
        {
            if (!Data.ContainsKey(stringId))
            {
                return stringId;
            }
            List<string> items = Data[stringId];
            if (items.Count == 1)
                return items[0];
            else
                return items[Random.Range(0, items.Count)];
        }
    }
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instanse { get; private set; }

    const string localizationDataPath = "Data/Localization/";
    List<string> stringTableNames = new List<string>
    {
        "Activity", "Actors", "Curios",
        "Dialogue", "Help", "Heroes",
        "Menu", "Misc", "Monsters",
        "Names", "PartyNames", "Quirks",
        "Kickstarter", "TownEvents",
        "Journal"
    };

    public StringTable MasterTable { get; set; }

    void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;
            MasterTable = new StringTable();

            foreach (var tableName in stringTableNames)
                FillStringTable(tableName, MasterTable.Data);
        }
    }

    void FillStringTable(string tableName, Dictionary<string, List<string>> dictionary)
    {
        TextAsset tableText = Resources.Load<TextAsset>(localizationDataPath + tableName);
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
                dictionary.Add(key, new List<string>(new string[] { value }));
        }
    }

    public string this[string stringId]
    {
        get
        {
            return Instanse.MasterTable[stringId];
        }
    }
    public static string GetString(string stringId)
    {
        return Instanse[stringId];
    }
}