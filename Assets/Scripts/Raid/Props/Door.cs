using System.IO;

public class Door : Prop
{
    public string TargetArea { get; set; }
    public Direction Direction { get; set; }

    public Door()
    {
        Type = AreaType.Door;
    }

    public Door(string areaId, string targetAreaId, Direction direction) : this()
    {
        StringId = areaId + targetAreaId;
        TargetArea = targetAreaId;
        Direction = direction;
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);

        bw.Write(TargetArea);
        bw.Write((int)Direction);
    }

    public override void Read(BinaryReader br)
    {
        base.Read(br);

        TargetArea = br.ReadString();
        Direction = (Direction)br.ReadInt32();
        StringId = "to" + TargetArea;
    }
}