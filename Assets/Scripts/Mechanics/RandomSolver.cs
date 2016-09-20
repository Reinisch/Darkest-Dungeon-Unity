using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface IProportionValue
{
    int Chance { get; set; }
}

public interface ISingleProportion
{
    float Chance { get; set; }
}

public static class RandomSolver 
{
    static System.Random random = new System.Random();

    public static T ChooseByRandom<T>(IEnumerable<T> collection)
        where T : IProportionValue
    {
        var rnd = Random.Range(0, collection.Sum(item => item.Chance > 0 ? item.Chance : 0));
        foreach (var item in collection)
        {
            if (rnd < item.Chance)
                return item;
            rnd -= item.Chance;
        }
        return default(T);
    }
    public static T ChooseBySingleRandom<T>(IEnumerable<T> collection)
        where T : ISingleProportion
    {
        var rnd = Random.Range(0, collection.Sum(item => item.Chance > 0 ? item.Chance : 0));
        foreach (var item in collection)
        {
            if (rnd < item.Chance)
                return item;
            rnd -= item.Chance;
        }
        return default(T);
    }
    public static int ChooseRandomIndex(List<float> chances)
    {
        var rnd = Random.value * chances.Sum(item => item);
        for (int i = 0; i < chances.Count; i++)
        {
            if (rnd < chances[i])
                return i;
            rnd -= chances[i];
        }
        return 0;
    }
    public static int ChooseRandomIndex(List<int> chances)
    {
        var rnd = Random.Range(0, chances.Sum(item => item));
        for (int i = 0; i < chances.Count; i++)
        {
            if (rnd < chances[i])
                return i;
            rnd -= chances[i];
        }
        return 0;
    }
    public static bool CheckSuccess(float chance)
    {
        return random.NextDouble() < chance;
    }
}