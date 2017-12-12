public static class RaidExtensions
{
    public static Direction OppositeDirection(this Direction targetDirection)
    {
        switch (targetDirection)
        {
            case Direction.Bot:
                return Direction.Top;
            case Direction.Top:
                return Direction.Bot;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
        }
        return Direction.Left;
    }
}