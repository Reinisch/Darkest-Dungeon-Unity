public class Door : Prop
{
    public string TargetArea { get; set; }
    public Direction Direction { get; set; }

    public Door(string areaId, string targetAreaId, Direction direction)
    {
        StringId = areaId + targetAreaId;
        TargetArea = targetAreaId;
        Direction = direction;
        Type = AreaType.Door;
    }
}