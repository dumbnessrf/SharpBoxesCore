using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.DataStruct.Structure;

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }

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

    public  Point Round()
    {
        return new Point(Math.Round(X, 2), Math.Round(Y, 2));
    }
}
