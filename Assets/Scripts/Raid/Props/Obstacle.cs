using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : Prop
{
    public List<Effect> FailEffects;
    public float HealthPenalty;
    public float TorchlightPenalty;

    public Obstacle(string id)
    {
        StringId = id;
        Type = AreaType.Obstacle;

        FailEffects = new List<Effect>();
    }
}