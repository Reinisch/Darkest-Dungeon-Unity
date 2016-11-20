using UnityEngine;
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
    public static System.Random Random = new System.Random();

    public static T ChooseByRandom<T>(IEnumerable<T> collection)
        where T : IProportionValue
    {
        var rnd = UnityEngine.Random.Range(0, collection.Sum(item => item.Chance > 0 ? item.Chance : 0));
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
        var rnd = UnityEngine.Random.Range(0, collection.Sum(item => item.Chance > 0 ? item.Chance : 0));
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
        var rnd = Random.NextDouble() * chances.Sum(item => item);
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
        var rnd = Random.Next(0, chances.Sum(item => item));
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
        if (chance >= 1)
            return true;
        double result = Random.NextDouble();
        Debug.Log("RNG [CHECK " + chance + "]: " + result + (result < chance ? " true" : " false"));
        return result < chance;
    }
    public static int Next(int maxValue)
    {
        int result = Random.Next(maxValue);
        Debug.Log("RNG [" + maxValue + "]: " + result);
        return result;
    }
    public static int Next(int minValue, int maxValue)
    {
        int result = Random.Next(minValue, maxValue);
        Debug.Log("RNG [" + minValue + "," + maxValue + "]: " + result);
        return result;
    }
    public static double NextDouble()
    {
        double result = Random.NextDouble();
        Debug.Log("RNG [DOUBLE]: " + result);
        return result;
    }
    public static void SetRandomSeed(int seed)
    {
        Random = new System.Random(seed);
        Debug.Log("RNG [SET]: " + seed);
    }
}