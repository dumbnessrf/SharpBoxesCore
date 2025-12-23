using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpBoxesCore.DataStruct.Structure;

/// <summary>
/// 表示一个六边形，实现了 IShapeStructure 接口。
/// 支持正六边形构造和任意六边形。
/// </summary>
public record Hexagon : IShapeStructure
{
    public Point P1 { get; init; }
    public Point P2 { get; init; }
    public Point P3 { get; init; }
    public Point P4 { get; init; }
    public Point P5 { get; init; }
    public Point P6 { get; init; }

    /// <summary>
    /// 创建任意六边形
    /// </summary>
    public Hexagon(Point p1, Point p2, Point p3, Point p4, Point p5, Point p6)
    {
        P1 = p1 ?? throw new ArgumentNullException(nameof(p1));
        P2 = p2 ?? throw new ArgumentNullException(nameof(p2));
        P3 = p3 ?? throw new ArgumentNullException(nameof(p3));
        P4 = p4 ?? throw new ArgumentNullException(nameof(p4));
        P5 = p5 ?? throw new ArgumentNullException(nameof(p5));
        P6 = p6 ?? throw new ArgumentNullException(nameof(p6));
    }

    /// <summary>
    /// 创建正六边形（水平方向）
    /// </summary>
    /// <param name="width">六边形总宽度（从左顶点到右顶点）</param>
    /// <param name="height">六边形总高度（从上顶点到下顶点）</param>
    /// <param name="center">中心点</param>
    /// <param name="isVertical">是否为垂直方向（平顶）</param>
    public Hexagon(double width, double height, Point center, bool isVertical = false)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("宽度和高度必须大于0");
        if (center == null)
            throw new ArgumentNullException(nameof(center));

        double side = width / 2.0; // 正六边形外接圆半径 ≈ width/2

        if (isVertical)
        {
            // 垂直方向（平顶）
            double dx = side * Math.Cos(Math.PI / 6); // cos(30°)
            double dy = side * Math.Sin(Math.PI / 6); // sin(30°)

            P1 = new Point(center.X, center.Y - side).Round();
            P2 = new Point(center.X + dx, center.Y - dy).Round();
            P3 = new Point(center.X + dx, center.Y + dy).Round();
            P4 = new Point(center.X, center.Y + side).Round();
            P5 = new Point(center.X - dx, center.Y + dy).Round();
            P6 = new Point(center.X - dx, center.Y - dy).Round();
        }
        else
        {
            // 水平方向（尖顶）
            double dx = side * Math.Cos(Math.PI / 3); // cos(60°)
            double dy = side * Math.Sin(Math.PI / 3); // sin(60°)

            P1 = new Point(center.X - side, center.Y).Round();
            P2 = new Point(center.X - dx, center.Y - dy).Round();
            P3 = new Point(center.X + dx, center.Y - dy).Round();
            P4 = new Point(center.X + side, center.Y).Round();
            P5 = new Point(center.X + dx, center.Y + dy).Round();
            P6 = new Point(center.X - dx, center.Y + dy).Round();
        }
    }

    public override string ToString()
    {
        return $"Hexagon(P1={P1}, P2={P2}, P3={P3}, P4={P4}, P5={P5}, P6={P6})";
    }

    /// <summary>
    /// 获取六边形的顶点数组
    /// </summary>
    [JsonIgnore]
    public Point[] Points => new[] { P1, P2, P3, P4, P5, P6 };

    /// <summary>
    /// 获取中心点（顶点平均值）
    /// </summary>
    [JsonIgnore]
    public Point Center =>
        new Point(
            (P1.X + P2.X + P3.X + P4.X + P5.X + P6.X) / 6,
            (P1.Y + P2.Y + P3.Y + P4.Y + P5.Y + P6.Y) / 6
        );

    /// <summary>
    /// 计算六边形面积（使用鞋带公式）
    /// </summary>
    [JsonIgnore]
    public double Area
    {
        get
        {
            var points = Points;
            double area = 0;
            int j = points.Length - 1;

            for (int i = 0; i < points.Length; i++)
            {
                area += (points[j].X + points[i].X) * (points[j].Y - points[i].Y);
                j = i;
            }

            return Math.Abs(area / 2);
        }
    }

    /// <summary>
    /// 计算周长（各边长度之和）
    /// </summary>
    [JsonIgnore]
    public double Perimeter
    {
        get
        {
            var points = Points;
            double perimeter = 0;

            for (int i = 0; i < points.Length; i++)
            {
                int j = (i + 1) % points.Length;
                perimeter += points[i].DistanceTo(points[j]);
            }

            return perimeter;
        }
    }

    /// <summary>
    /// 判断点是否在六边形内部（射线法）
    /// </summary>
    /// <param name="point">要判断的点</param>
    /// <returns>是否在内部</returns>
    public bool Contains(Point point)
    {
        if (point == null)
            return false;

        var points = Points;
        bool inside = false;

        for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
        {
            if (
                ((points[i].Y > point.Y) != (points[j].Y > point.Y))
                && (
                    point.X
                    < (points[j].X - points[i].X)
                        * (point.Y - points[i].Y)
                        / (points[j].Y - points[i].Y)
                        + points[i].X
                )
            )
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
