using UnityEngine;
using UnityEngine.UI;

public class RaidMapHallSectorSlot : MonoBehaviour
{
    [SerializeField]
    private Image hallSectorIcon;
    [SerializeField]
    private Image marker;
    [SerializeField]
    private RectTransform rectTransform;

    public HallSector Sector { get; private set; }
    public RectTransform RectTransform { get { return rectTransform; } }

    public void SetSector(HallSector sector)
    {
        Sector = sector;
        UpdateSector();
    }

    public void SetIndicator(RectTransform indicator)
    {
        indicator.position = RectTransform.position;
        indicator.gameObject.SetActive(true);
    }

    public void RemoveIndicator(RectTransform indicator)
    {
        indicator.gameObject.SetActive(false);
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
