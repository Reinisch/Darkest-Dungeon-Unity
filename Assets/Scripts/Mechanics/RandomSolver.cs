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
    private static System.Random random = new System.Random();

    public static T ChooseAnyExcept<T>(IEnumerable<T> collection, T except) where T : class
    {
        var enumerable = collection as IList<T> ?? collection.ToList();
        var rnd = random.Next(enumerable.Sum(item => item == except ? 0 : 1));
        foreach (var item in enumerable)
        {
            if(item == except)
                continue;
            if (rnd < 1)
                return item;
            rnd -= 1;
        }
        return null;
    }

    public static T ChooseByRandom<T>(IEnumerable<T> collection) where T : IProportionValue
    {
        var enumerable = collection as IList<T> ?? collection.ToList();
        var rnd = random.Next(enumerable.Sum(item => item.Chance > 0 ? item.Chance : 0));
        foreach (var item in enumerable)
        {
            if (rnd < item.Chance)
                return item;
            rnd -= item.Chance;
        }
        return default(T);
    }

    public static T ChooseBySingleRandom<T>(IEnumerable<T> collection) where T : ISingleProportion
    {
        var enumerable = collection as IList<T> ?? collection.ToList();
        var rnd = UnityEngine.Random.Range(0, enumerable.Sum(item => item.Chance > 0 ? item.Chance : 0));
        foreach (var item in enumerable)
        {
            if (rnd < item.Chance)
                return item;
            rnd -= item.Chance;
        }
        return default(T);
    }

    public static int ChooseRandomIndex(List<float> chances)
    {
        var rnd = random.NextDouble() * chances.Sum(item => item);
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
        var rnd = random.Next(0, chances.Sum(item => item));
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

        return random.NextDouble() < chance;
    }

    public static int Next(int maxValue)
    {
        return random.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        return random.Next(minValue, maxValue);
    }

    public static double NextDouble()
    {
        return random.NextDouble();
    }

    public static void SetRandomSeed(int seed)
    {
        random = new System.Random(seed);
    }
}