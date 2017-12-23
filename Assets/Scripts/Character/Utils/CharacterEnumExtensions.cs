public static class CharacterEnumExtensions
{
    public static bool IsSaveData(this BuffSourceType buffSourceType)
    {
        return buffSourceType == BuffSourceType.Adventure || buffSourceType == BuffSourceType.Estate;
    }
}