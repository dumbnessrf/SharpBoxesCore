using SharpBoxesCore.DataStruct.Structure;

namespace SharpBoxesCore.DataStruct.Structure;


public record Rectangle1D : IShapeStructure
{
    public double Width;
    public double Height;
    public double X;
    public double Y;

    public Rectangle1D() { }
    [DebuggerStepThrough]
    public Rectangle1D(double width, double height, double x, double y)
    {
        this.Width = width;
        this.Height = height;
        this.X = x;
        this.Y = y;
    }
    [DebuggerStepThrough]
    public Rectangle1D(
        SharpBoxesCore.DataStruct.Structure.Point start,
        SharpBoxesCore.DataStruct.Structure.Point end
    )
    {
        this.Width = end.X - start.X;
        this.Height = end.Y - start.Y;
        this.X = start.X;
        this.Y = start.Y;
    }
    [DebuggerStepThrough]
    public Rectangle1D(
        SharpBoxesCore.DataStruct.Structure.Point center,
        double width,
        double height
    )
    {
        this.Width = width;
        this.Height = height;
        this.X = center.X - width / 2;
        this.Y = center.Y - height / 2;
    }

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point StartPoint =>
        new SharpBoxesCore.DataStruct.Structure.Point(X, Y).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point EndPoint =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width, Y + Height).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point CenterPoint =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width / 2, Y + Height / 2).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point TopLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(X, Y).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point TopRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width, Y).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point BottomLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(X, Y + Height).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point BottomRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width, Y + Height).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point TopCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width / 2, Y).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point BottomCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width / 2, Y + Height).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point LeftCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(X, Y + Height / 2).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RightCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(X + Width, Y + Height / 2).Round();

    public override string ToString()
    {
        return $"Rectangle1D(Width={Width:F2}, Height={Height:F2}, X={X:F2}, Y={Y:F2})";
    }

    public bool Contains(SharpBoxesCore.DataStruct.Structure.Point point)
    {
        return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
    }
}
