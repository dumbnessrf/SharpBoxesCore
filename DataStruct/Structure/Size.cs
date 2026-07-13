using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoxesCore.DataStruct.Structure;

public class Size
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Size(double x, double y)
    {
        Width = x;
        Height = y;
    }

    public Size() { }

    public override string ToString()
    {
        return $"({Width}, {Height})";
    }

    public static Size operator +(Size p1, Size p2)
    {
        return new Size(p1.Width + p2.Width, p1.Height + p2.Height);
    }

    public static Size operator -(Size p1, Size p2)
    {
        return new Size(p1.Width - p2.Width, p1.Height - p2.Height);
    }

    public Size Round()
    {
        return new Size(Math.Round(Width, 2), Math.Round(Height, 2));
    }

    // 隐式转换：从你的 Point → System.Windows.Point
    public static implicit operator System.Windows.Size(Size p)
    {
        if (p == null) return new System.Windows.Size(0, 0); // 或抛出异常，根据需求
        return new System.Windows.Size(p.Width, p.Height);
    }

    // 隐式转换：从 System.Windows.Point → 你的 Point
    public static implicit operator Size(System.Windows.Size p)
    {
        return new Size(p.Width, p.Height);
    }
}
