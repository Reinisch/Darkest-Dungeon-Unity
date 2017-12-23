using UnityEngine;

public static class DataExtensions
{
    public static Color ToColor(this string hexColor)
    {
        Color outColor;
        if (ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors[hexColor], out outColor))
            return outColor;

        Debug.LogError("Missing colour: " + hexColor);
        return Color.black;
    }
}
