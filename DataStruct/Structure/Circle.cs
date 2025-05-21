
using System.Diagnostics;
using System.Drawing;

namespace SharpBoxes.DataStruct.Structure;

[DebuggerStepThrough]
/// <summary>
/// 表示一个圆形，实现了IShapeStructure接口。
/// </summary>
public record Circle : IShapeStructure
{
    /// <summary>
    /// 圆的半径。
    /// </summary>
    public double Radius;

    /// <summary>
    /// 圆心的X坐标。
    /// </summary>
    public double CenterX;

    /// <summary>
    /// 圆心的Y坐标。
    /// </summary>
    public double CenterY;

    public Circle(double radius, double centerX, double centerY)
    {
        this.Radius = radius;
        this.CenterX = centerX;
        this.CenterY = centerY;
    }

    public override string ToString()
    {
        return $"Circle(Radius={Radius}, CenterX={CenterX}, CenterY={CenterY})";
    }

    /// <summary>
    /// 获取圆的直径，并进行四舍五入。
    /// </summary>
    public double Diameter => (2 * Radius).Round();

    /// <summary>
    /// 获取圆的中心点。
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point Center => new(CenterX, CenterY);

    /// <summary>
    /// 获取圆的上方顶点
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point Top => new(CenterX, CenterY - Radius);

    /// <summary>
    /// 获取圆的下方顶点
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point Bottom => new(CenterX, CenterY + Radius);

    /// <summary>
    /// 获取圆的左侧顶点
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point Left => new(CenterX - Radius, CenterY);

    /// <summary>
    /// 获取圆的右侧顶点
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point Right => new(CenterX + Radius, CenterY);
}
