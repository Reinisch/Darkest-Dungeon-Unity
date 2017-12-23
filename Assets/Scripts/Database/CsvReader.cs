using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public static class CsvReader
{
    public static string[,] SplitCsvGrid(string csvText)
    {
        string[] lines = csvText.Split("\n"[0]);
        int width = lines.Select(SplitCsvLine).Aggregate(0, (current, row) => Mathf.Max(current, row.Length));

        string[,] outputGrid = new string[lines.Length + 1, width + 1];
        for (int x = 0; x < lines.Length; x++)
        {
            string[] row = SplitCsvLine(lines[x]);
            for (int y = 0; y < row.Length; y++)
            {
                outputGrid[x, y] = row[y];
                outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputGrid;
    }

    private static string[] SplitCsvLine(string line)
    {
        return (from Match m in Regex.Matches(line, @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
            RegexOptions.ExplicitCapture) select m.Groups[1].Value).ToArray();
    }
}
