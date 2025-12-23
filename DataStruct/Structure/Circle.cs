using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;

namespace SharpBoxesCore.DataStruct.Structure;

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

    public Circle() { }

    public override string ToString()
    {
        return $"Circle(Radius={Radius:F2}, CenterX={CenterX:F2}, CenterY={CenterY:F2})";
    }

    /// <summary>
    /// 获取圆的直径，并进行四舍五入。
    /// </summary>
    [JsonIgnore]
    public double Diameter => (2 * Radius).Round();

    /// <summary>
    /// 获取圆的中心点。
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Center => new(CenterX, CenterY);

    /// <summary>
    /// 获取圆的上方顶点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Top => new(CenterX, CenterY - Radius);

    /// <summary>
    /// 获取圆的下方顶点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Bottom => new(CenterX, CenterY + Radius);

    /// <summary>
    /// 获取圆的左侧顶点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Left => new(CenterX - Radius, CenterY);

    /// <summary>
    /// 获取圆的右侧顶点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Right => new(CenterX + Radius, CenterY);

    /// <summary>
    /// 获取任意角度的顶点
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public SharpBoxesCore.DataStruct.Structure.Point GetPoint(double angle)
    {
        double x = CenterX + Radius * Math.Cos(angle);
        double y = CenterY + Radius * Math.Sin(angle);
        return new(x, y);
    }

    public double Area => Math.PI * Radius * Radius;

    /// <summary>
    /// 获取圆的周长
    /// </summary>
    public double Perimeter => 2 * Math.PI * Radius;

    /// <summary>
    /// 判断点是否在圆内
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(SharpBoxesCore.DataStruct.Structure.Point point)
    {
        double distance = Math.Sqrt(
            Math.Pow(point.X - CenterX, 2) + Math.Pow(point.Y - CenterY, 2)
        );
        return distance <= Radius;
    }

    /// <summary>
    /// 判断是否与另一个形状相交
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Intersects(IShapeStructure other)
    {
        if (other is Circle circle)
        {
            double distance = Math.Sqrt(
                Math.Pow(circle.CenterX - CenterX, 2) + Math.Pow(circle.CenterY - CenterY, 2)
            );
            return distance <= Radius + circle.Radius;
        }
        else if (other is Rectangle1D rectangle)
        {
            double distanceX = Math.Abs(rectangle.CenterPoint.X - CenterX);
            double distanceY = Math.Abs(rectangle.CenterPoint.Y - CenterY);
            if (distanceX > (rectangle.Width / 2 + Radius))
            {
                return false;
            }
            if (distanceY > (rectangle.Height / 2 + Radius))
            {
                return false;
            }
            if (distanceX <= (rectangle.Width / 2))
            {
                return true;
            }
            if (distanceY <= (rectangle.Height / 2))
            {
                return true;
            }
            double cornerDistance = Math.Sqrt(
                Math.Pow(distanceX - rectangle.Width / 2, 2)
                    + Math.Pow(distanceY - rectangle.Height / 2, 2)
            );
            return cornerDistance <= Radius;
        }
        //带角度的矩形Rectangle2D
        else if (other is Rectangle2D rect)
        {
            // 将圆心变换到矩形的局部坐标系（以矩形中心为原点，逆旋转）
            double dx = CenterX - rect.CenterX;
            double dy = CenterY - rect.CenterY;

            // 逆旋转角度（弧度）
            double angleRad = -rect.AngleDegree * Math.PI / 180;
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);

            // 旋转后的局部坐标
            double localX = dx * cos - dy * sin;
            double localY = dx * sin + dy * cos;

            // 计算局部坐标点到矩形边界的最近距离
            double clampX = Math.Max(-rect.HalfWidth, Math.Min(localX, rect.HalfWidth));
            double clampY = Math.Max(-rect.HalfHeight, Math.Min(localY, rect.HalfHeight));

            // 最近点与圆心的距离
            double distanceX = localX - clampX;
            double distanceY = localY - clampY;
            double distanceSquared = distanceX * distanceX + distanceY * distanceY;

            // 判断距离是否小于等于半径的平方
            return distanceSquared <= Radius * Radius;
        }
        else if (other is Polygon polygon)
        {
            foreach (var point in polygon.Points)
            {
                if (Contains(point))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
