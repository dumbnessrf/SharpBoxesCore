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
}
