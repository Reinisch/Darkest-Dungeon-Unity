public enum AreaType
{
    Empty, 
    Entrance,
    Tresure,
    Curio,
    Boss,
    Battle,
    Trap,
    Hunger,
    Obstacle,
    Door,
    BattleCurio,
    BattleTresure,
}
public enum Knowledge
{
    Hidden = 0,
    Scouted,
    Visited,
    Completed,
}
public enum Direction
{
    Top,
    Bot,
    Left,
    Right,
}

public abstract class Prop
{
    public string StringId { get; protected set; }
    public AreaType Type { get; protected set; }
}

public abstract class Area
{
    public string Id { get; set; }
    public string TextureId { get; set; }
    public int MashId { get; set; }

    public AreaType Type { get; set; }
    public Knowledge Knowledge { get; set; }

    public Prop Prop { get; set; }
    public BattleEncounter BattleEncounter { get; set; }

    public Direction OppositeDirection(Direction targetDirection)
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

    public bool HasActiveBattle
    {
        get
        {
            return BattleEncounter != null && BattleEncounter.Cleared == false;
        }
    }
}