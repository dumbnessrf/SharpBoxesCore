using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysPoint = global::System.Windows.Point;
namespace SharpBoxesCore.DataStruct.Structure;

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
    public Brush UsedBrush { get; set; }
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public Point() { }

    public override string ToString()
    {
        return $"({X:F2}, {Y:F2})";
    }

    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.X + p2.X, p1.Y + p2.Y);
    }

    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.X - p2.X, p1.Y - p2.Y);
    }

    public Point Round()
    {
        return new Point(Math.Round(X, 2), Math.Round(Y, 2));
    }

    public Line ToLine(Point endPoint)
    {
        return new Line(this, endPoint);
    }

    public double DistanceTo(Point other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // 隐式转换：从你的 Point → System.Windows.Point
    public static implicit operator SysPoint(Point p)
    {
        if (p == null) return new SysPoint(0, 0); // 或抛出异常，根据需求
        return new SysPoint(p.X, p.Y);
    }

    // 隐式转换：从 System.Windows.Point → 你的 Point
    public static implicit operator Point(SysPoint p)
    {
        return new Point(p.X, p.Y);
    }
}
