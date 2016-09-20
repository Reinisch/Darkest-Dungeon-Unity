using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PartyActivityRecordSlot : MonoBehaviour
{
    public RectTransform rectTransform;

    public List<Image> portraits;
    public List<Text> names;
    public Text description;

    public void UpdatePartyActivity(PartyActivityRecord record)
    {
        int index = 0;
        for (int i = 0; i < record.Classes.Count; i++)
        {
            portraits[i].gameObject.SetActive(true);
            if (record.Alive[i])
                portraits[i].sprite = DarkestDungeonManager.HeroSprites[record.Classes[i]]["A"].Portrait;
            else
                portraits[i].sprite = DarkestDungeonManager.Data.Sprites["deadhero_portrait"];

            names[i].text = record.Names[i];
            index = i;
        }
        for (int i = index + 1; i < names.Count; i++)
            portraits[i].gameObject.SetActive(false);

        description.text = record.Description;
        gameObject.SetActive(true);
    }
    public void ResetPartyActivity()
    {
        gameObject.SetActive(false);
    }
}
