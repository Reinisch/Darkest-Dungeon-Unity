using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PartyActivityRecordSlot : MonoBehaviour
{
    [SerializeField]
    private List<Image> portraits;
    [SerializeField]
    private List<Text> names;
    [SerializeField]
    private Text description;

    public void UpdatePartyActivity(PartyActivityRecord record)
    {
        int index = 0;
        for (int i = 0; i < record.Classes.Count; i++)
        {
            portraits[i].gameObject.SetActive(true);
            portraits[i].sprite = record.Alive[i] ? 
                DarkestDungeonManager.HeroSprites[record.Classes[i]]["A"].Portrait :
                DarkestDungeonManager.Data.Sprites["deadhero_portrait"];

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
