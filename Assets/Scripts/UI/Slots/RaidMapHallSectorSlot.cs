using UnityEngine;
using UnityEngine.UI;

public class RaidMapHallSectorSlot : MonoBehaviour
{
    public Image hallSectorIcon;
    public Image marker;
    public GameObject indicator;
    public RectTransform rectTransform;

    public HallSector Sector { get; set; }

    public void SetSector(HallSector sector)
    {
        Sector = sector;
        indicator.SetActive(false);
        UpdateSector();
    }

    public void SetIndicator()
    {
        indicator.SetActive(true);
    }
    public void RemoveIndicator()
    {
        indicator.SetActive(false);
    }

    public void UpdateSector()
    {
        if (Sector.Type != AreaType.Entrance)
        {
            if (Sector.Knowledge == Knowledge.Hidden)
            {
                hallSectorIcon.enabled = true;
                hallSectorIcon.sprite = DarkestDungeonManager.Data.MapHallKnowledgeSet[Knowledge.Hidden];
                marker.enabled = false;
            }
            else if (Sector.Knowledge == Knowledge.Scouted || Sector.Knowledge == Knowledge.Visited)
            {
                hallSectorIcon.enabled = true;
                hallSectorIcon.sprite = DarkestDungeonManager.Data.MapHallIconSet[Sector.Type];
                if(Sector.Type == AreaType.Hunger)
                {
                    marker.enabled = false;
                    hallSectorIcon.sprite = DarkestDungeonManager.Data.MapHallIconSet[AreaType.Empty];
                }
                else
                    marker.enabled = false;
            }
            else if (Sector.Knowledge == Knowledge.Completed)
            {
                hallSectorIcon.enabled = true;
                hallSectorIcon.sprite = DarkestDungeonManager.Data.MapHallIconSet[Sector.Type];
                marker.enabled = true;
                marker.sprite = DarkestDungeonManager.Data.MapHallKnowledgeSet[Knowledge.Completed];
            }
        }
    }
}
