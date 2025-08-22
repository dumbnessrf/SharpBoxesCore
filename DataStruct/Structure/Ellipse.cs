using SharpBoxesCore.DataStruct;
using SharpBoxesCore.DataStruct.Structure;


namespace SharpBoxesCore.DataStruct.Structure;

[DebuggerStepThrough]
public record Ellipse : IShapeStructure
{
    public double RadiusX;
    public double RadiusY;
    public double CenterX;
    public double CenterY;
    public double RotationDegree;

    public Ellipse(
        double radiusX,
        double radiusY,
        double centerX,
        double centerY,
        double rotationDegree = 0
    )
    {
        this.RadiusX = radiusX;
        this.RadiusY = radiusY;
        this.CenterX = centerX;
        this.CenterY = centerY;
        this.RotationDegree = rotationDegree;
    }
    public Ellipse()
    {
        
    }
    public override string ToString()
    {
        return $"Rectangle2D(RadiusX={RadiusX:F2}, RadiusY={RadiusY:F2}, CenterX={CenterX:F2}, CenterY={CenterY:F2}, RotationDegree={RotationDegree:F2})";
    }

    /// <summary>
    /// 获取旋转后的顶部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Top => RawTop.Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY)).Round();

    /// <summary>
    /// 获取未旋转的顶部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawTop => new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY - RadiusY);

    /// <summary>
    /// 获取旋转后的底部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Bottom => RawBottom.Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY)).Round();

    /// <summary>
    /// 获取未旋转的底部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawBottom => new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY + RadiusY);

    /// <summary>
    /// 获取旋转后的左侧点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Left => RawLeft.Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY)).Round();

    /// <summary>
    /// 获取未旋转的左侧点
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point RawLeft => new SharpBoxesCore.DataStruct.Structure.Point(CenterX - RadiusX, CenterY);

    /// <summary>
    /// 获取旋转后的右侧点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Right => RawRight.Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY)).Round();

    /// <summary>
    /// 获取未旋转的右侧点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawRight => new SharpBoxesCore.DataStruct.Structure.Point(CenterX + RadiusX, CenterY);

    /// <summary>
    /// 获取中心点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Center => new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY);
}
