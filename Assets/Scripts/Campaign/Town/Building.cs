using UnityEngine;
using System.Collections;

public enum BuildingType { Abbey, Tavern, Sanitarium, Blacksmith, Guild, CampingTrainer, NomadWagon, StageCoach, Graveyard, Statue }

public class Building
{
    public string Name { get; set; }

    public int VisitPriority { get; set; }
    public int QuestsRequired { get; set; }
    public int HighestDungeonLevelRequired { get; set; }
}
