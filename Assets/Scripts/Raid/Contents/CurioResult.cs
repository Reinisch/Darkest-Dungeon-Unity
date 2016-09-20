using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CurioResult : IProportionValue
{
    public string Item { get; set; }
    public int Draws { get; set; }
    public int Chance
    {
        get;
        set;
    }
}