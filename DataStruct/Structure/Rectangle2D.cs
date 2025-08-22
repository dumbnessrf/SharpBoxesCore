using System.Diagnostics;
using SharpBoxesCore.DataStruct;
using SharpBoxesCore.DataStruct.Structure;

namespace SharpBoxesCore.DataStruct.Structure;

[DebuggerStepThrough]
public record Rectangle2D : IShapeStructure
{
    public double HalfWidth;
    public double HalfHeight;
    public double CenterX;
    public double CenterY;
    public double AngleDegree;

    public Rectangle2D(
        double halfWidth,
        double halfHeight,
        double centerX,
        double centerY,
        double angleDegree
    )
    {
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        CenterX = centerX;
        CenterY = centerY;
        AngleDegree = angleDegree;
    }

    public Rectangle2D() { }

    public Rectangle2D(
        SharpBoxesCore.DataStruct.Structure.Point pTopLeft,
        SharpBoxesCore.DataStruct.Structure.Point pTopRight,
        SharpBoxesCore.DataStruct.Structure.Point pBottomLeft,
        SharpBoxesCore.DataStruct.Structure.Point pBottomRight
    )
    {
        CenterX = (pTopLeft.X + pBottomRight.X) / 2;
        CenterY = (pTopLeft.Y + pBottomRight.Y) / 2;
        HalfWidth = pTopRight.Distance(pTopLeft) / 2;
        HalfHeight = pBottomLeft.Distance(pTopLeft) / 2;
        AngleDegree = pTopRight.Angle(pTopLeft).RadiansToDegrees();
    }

    public Rectangle2D(
        SharpBoxesCore.DataStruct.Structure.Point topLeftPoint,
        SharpBoxesCore.DataStruct.Structure.Point bottomRightPoint,
        double angleDegree
    )
    {
        CenterX = (topLeftPoint.X + bottomRightPoint.X) / 2;
        CenterY = (topLeftPoint.Y + bottomRightPoint.Y) / 2;
        HalfWidth = (bottomRightPoint.X - topLeftPoint.X) / 2;
        HalfHeight = (bottomRightPoint.Y - topLeftPoint.Y) / 2;
        AngleDegree = angleDegree;
    }

    public Rectangle2D(Rectangle1D rectangle1D, double angleDegree)
    {
        HalfWidth = rectangle1D.Width / 2;
        HalfHeight = rectangle1D.Height / 2;
        CenterX = rectangle1D.CenterPoint.X;
        CenterY = rectangle1D.CenterPoint.Y;
        AngleDegree = angleDegree;
    }

    public Rectangle2D(
        SharpBoxesCore.DataStruct.Structure.Point centerPoint,
        double halfWidth,
        double halfHeight,
        double angleDegree
    )
    {
        CenterX = centerPoint.X;
        CenterY = centerPoint.Y;
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        AngleDegree = angleDegree;
    }

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point CenterPoint
    {
        get { return new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY).Round(); }
        set
        {
            CenterX = value.X;
            CenterY = value.Y;
        }
    }

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point TopLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX - HalfWidth, CenterY - HalfHeight)
            .Rotate(AngleDegree, CenterPoint);

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point TopRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX + HalfWidth, CenterY - HalfHeight)
            .Rotate(AngleDegree, CenterPoint);

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point BottomLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX - HalfWidth, CenterY + HalfHeight)
            .Rotate(AngleDegree, CenterPoint);

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point BottomRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX + HalfWidth, CenterY + HalfHeight)
            .Rotate(AngleDegree, CenterPoint);

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point TopCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY - HalfHeight)
            .Rotate(AngleDegree, CenterPoint)
            .Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point BottomCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY + HalfHeight)
            .Rotate(AngleDegree, CenterPoint)
            .Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point LeftCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX - HalfWidth, CenterY)
            .Rotate(AngleDegree, CenterPoint)
            .Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RightCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX + HalfWidth, CenterY)
            .Rotate(AngleDegree, CenterPoint)
            .Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawTopLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(
            CenterX - HalfWidth,
            CenterY - HalfHeight
        ).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawTopRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(
            CenterX + HalfWidth,
            CenterY - HalfHeight
        ).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawBottomLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(
            CenterX - HalfWidth,
            CenterY + HalfHeight
        ).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawBottomRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(
            CenterX + HalfWidth,
            CenterY + HalfHeight
        ).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawTopCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY - HalfHeight).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawBottomCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY + HalfHeight).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawLeftCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX - HalfWidth, CenterY).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawRightCenter =>
        new SharpBoxesCore.DataStruct.Structure.Point(
            (CenterX + HalfWidth).ToFloat(),
            (CenterY).ToFloat()
        ).Round();

    [JsonIgnore]
    public double Radian => (-AngleDegree).DegreesToRadians().Round();

    public override string ToString() =>
        $"Rectangle2D(CenterX={CenterX:F2}, CenterY={CenterY:F2}, HalfWidth={HalfWidth:F2}, HalfHeight={HalfHeight:F2}, AngleDegree={AngleDegree:F2})";
}
