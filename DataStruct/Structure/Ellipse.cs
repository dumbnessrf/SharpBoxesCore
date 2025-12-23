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

    public Ellipse() { }

    public override string ToString()
    {
        return $"Rectangle2D(RadiusX={RadiusX:F2}, RadiusY={RadiusY:F2}, CenterX={CenterX:F2}, CenterY={CenterY:F2}, RotationDegree={RotationDegree:F2})";
    }

    /// <summary>
    /// 获取旋转后的顶部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Top =>
        RawTop
            .Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY))
            .Round();

    /// <summary>
    /// 获取未旋转的顶部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawTop =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY - RadiusY);

    /// <summary>
    /// 获取旋转后的底部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Bottom =>
        RawBottom
            .Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY))
            .Round();

    /// <summary>
    /// 获取未旋转的底部点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawBottom =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY + RadiusY);

    /// <summary>
    /// 获取旋转后的左侧点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Left =>
        RawLeft
            .Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY))
            .Round();

    /// <summary>
    /// 获取未旋转的左侧点
    /// </summary>
    public SharpBoxesCore.DataStruct.Structure.Point RawLeft =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX - RadiusX, CenterY);

    /// <summary>
    /// 获取旋转后的右侧点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Right =>
        RawRight
            .Rotate(RotationDegree, new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY))
            .Round();

    /// <summary>
    /// 获取未旋转的右侧点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point RawRight =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX + RadiusX, CenterY);

    /// <summary>
    /// 获取中心点
    /// </summary>
    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Center =>
        new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY);

    public bool Contains(SharpBoxesCore.DataStruct.Structure.Point point)
    {
        var rotatedPoint = point.Rotate(
            -RotationDegree,
            new SharpBoxesCore.DataStruct.Structure.Point(CenterX, CenterY)
        );
        var dx = rotatedPoint.X - CenterX;
        var dy = rotatedPoint.Y - CenterY;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        return distance <= RadiusX && distance <= RadiusY;
    }

    /// <summary>
    /// 获取椭圆在指定角度方向上的边界点。
    /// 角度为从正X轴逆时针旋转的角度（单位：度）。
    /// </summary>
    /// <param name="angle">全局方向角（度）</param>
    /// <returns>椭圆边界上的点</returns>
    public Point GetPoint(double angle)
    {
        // 1. 转换为弧度
        double angleRad = angle * Math.PI / 180.0;

        // 2. 构造方向单位向量
        double dirX = Math.Cos(angleRad);
        double dirY = Math.Sin(angleRad);

        // 3. 将方向向量变换到椭圆的局部坐标系（逆旋转）
        double localAngle = RotationDegree * Math.PI / 180.0;
        double cos = Math.Cos(-localAngle);
        double sin = Math.Sin(-localAngle);
        double localDirX = dirX * cos - dirY * sin;
        double localDirY = dirX * sin + dirY * cos;

        // 4. 计算该方向与标准椭圆的交点
        // 椭圆方程: (x/a)^2 + (y/b)^2 = 1
        // 代入 x = t * localDirX, y = t * localDirY
        // 得: t^2 * ( (dx/a)^2 + (dy/b)^2 ) = 1
        // => t = 1 / sqrt( (dx/a)^2 + (dy/b)^2 )
        double a = RadiusX;
        double b = RadiusY;

        if (a == 0 || b == 0)
            return new Point(CenterX, CenterY); // 退化情况

        double t =
            1.0 / Math.Sqrt((localDirX * localDirX) / (a * a) + (localDirY * localDirY) / (b * b));

        double localX = t * localDirX;
        double localY = t * localDirY;

        // 5. 将局部点旋转回全局坐标系
        double globalX = CenterX + localX * Math.Cos(localAngle) - localY * Math.Sin(localAngle);
        double globalY = CenterY + localX * Math.Sin(localAngle) + localY * Math.Cos(localAngle);

        return new Point(globalX, globalY).Round();
    }
}
