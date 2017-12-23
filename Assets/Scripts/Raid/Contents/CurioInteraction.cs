using System.Collections.Generic;

public class CurioInteraction : IProportionValue
{
    public string ResultType { get; set; }
    public List<CurioResult> Results { get; set; }
    public int Chance { get; set; }

    public CurioInteraction()
    {
        Results = new List<CurioResult>();
    }

    public CurioInteraction(int chance, string resultType) : this()
    {
        Chance = chance;
        ResultType = resultType;
    }

    public virtual string ResultString()
    {
        if (ResultType == "scouting")
            return "scout";
        else
            return ResultType;
    }
}