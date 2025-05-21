using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using SharpBoxes.DataStruct.Structure;
using SharpCanvas.Shapes.Structure;

namespace SharpBoxes.DataStruct;

public static class Extensions
{
    public static double Round(this double d, int digits = 5)
    {
        return Math.Round(d, digits);
    }

    public static decimal Round(this decimal d, int digits = 5)
    {
        return Math.Round(d, digits);
    }

    public static float Round(this float d, int digits = 5)
    {
        return (float)Math.Round(d, digits);
    }

    public static SharpBoxesCore.DataStruct.Structure.Point Round(this SharpBoxesCore.DataStruct.Structure.Point p, int digits = 5)
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(p.X.Round(digits), p.Y.Round(digits));
    }

    public static Point ToPoint(this SharpBoxesCore.DataStruct.Structure.Point p)
    {
        return new Point(p.X.ToInt(), p.Y.ToInt());
    }

    public static SharpBoxesCore.DataStruct.Structure.Point ToPointF(this SharpBoxesCore.DataStruct.Structure.Point p)
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(p.X.ToFloat(), p.Y.ToFloat());
    }

    public static SharpBoxesCore.DataStruct.Structure.Size Round(this SharpBoxesCore.DataStruct.Structure.Size s, int digits = 5)
    {
        return new SharpBoxesCore.DataStruct.Structure.Size(s.Width.Round(digits), s.Height.Round(digits));
    }

    public static Rectangle2D Round(this Rectangle2D r, int digits = 5)
    {
        return new Rectangle2D(
            r.HalfWidth.Round(digits),
            r.HalfHeight.Round(digits),
            r.CenterX.Round(digits),
            r.CenterY.Round(digits),
            r.AngleDegree
        );
    }

    /// <summary>
    /// 根据矩形中心扩大或缩小矩形
    /// </summary>
    /// <param name="r"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static Rectangle2D Scale(this Rectangle2D r, int size)
    {
        //var center = r.Center();
        //var halfSize = size / 2;
        var left = r.TopLeft.X - size;
        var top = r.TopLeft.Y - size;
        var right = r.BottomRight.X + size;
        var bottom = r.BottomRight.Y + size;
        return new Rectangle2D(
            new SharpBoxesCore.DataStruct.Structure.Point(left, top),
            right - left,
            bottom - top,
            0
        );
    }

    public static Rectangle1D ToRectangle1D(this Rectangle2D r)
    {
        return new Rectangle1D(r.HalfWidth*2, r.HalfHeight*2, r.TopLeft.X, r.TopLeft.Y);
    }

    public static Rectangle2D ToRectangle2D(this Rectangle1D r)
    {
        return new Rectangle2D(
            new SharpBoxesCore.DataStruct.Structure.Point(r.CenterPoint.X, r.CenterPoint.Y),
            r.Width/2,
            r.Height/2,
            0
        );
    }

    public static SharpBoxesCore.DataStruct.Structure.Point Center(this Rectangle2D r)
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(
            (r.CenterX + r.HalfWidth).ToFloat(),
            (r.CenterY + r.HalfHeight).ToFloat()
        );
    }

    /// <summary>
    /// 计算两个点之间的角度，单位为弧度
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static double Angle(this SharpBoxesCore.DataStruct.Structure.Point p1, SharpBoxesCore.DataStruct.Structure.Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Atan2(dy, dx);
    }

    public static double Distance(this SharpBoxesCore.DataStruct.Structure.Point p1, SharpBoxesCore.DataStruct.Structure.Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 计算点p到线段(ps, pe)的距离
    /// </summary>
    /// <param name="p">要计算距离的点</param>
    /// <param name="ps">线段的起点</param>
    /// <param name="pe">线段的终点</param>
    /// <returns>点p到线段(ps, pe)的距离</returns>
    public static double DistanceToLine(this SharpBoxesCore.DataStruct.Structure.Point p, SharpBoxesCore.DataStruct.Structure.Point ps, SharpBoxesCore.DataStruct.Structure.Point pe)
    {
        // 计算点 p 到线段起点 ps 的水平和垂直距离
        var A = p.X - ps.X;
        var B = p.Y - ps.Y;

        // 计算线段的向量（从 ps 到 pe）
        var C = pe.X - ps.X;
        var D = pe.Y - ps.Y;

        // 计算点 p 到线段向量的点积
        var dot = A * C + B * D;

        // 计算线段向量的平方长度
        var len_sq = C * C + D * D;

        // 初始化参数为 -1.0
        var param = -1.0;

        // 如果线段长度不为零，计算参数
        if (len_sq != 0) // 防止线段长度为零的情况
            param = dot / len_sq;

        double xx,
            yy;

        // 根据参数判断投影点的位置
        if (param < 0)
        {
            // 如果参数小于 0，投影点在 ps 之前，取 ps 的坐标
            xx = ps.X;
            yy = ps.Y;
        }
        else if (param > 1)
        {
            // 如果参数大于 1，投影点在 pe 之后，取 pe 的坐标
            xx = pe.X;
            yy = pe.Y;
        }
        else
        {
            // 否则，投影点在线段上，计算投影点的坐标
            xx = ps.X + param * C;
            yy = ps.Y + param * D;
        }

        // 计算点 p 到投影点的水平和垂直距离
        var dx = p.X - xx;
        var dy = p.Y - yy;

        // 返回点 p 到线段的垂直距离
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static double RadiansToDegrees(this double radians)
    {
        return radians * 180 / Math.PI;
    }

    public static double DegreesToRadians(this double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public static SharpBoxesCore.DataStruct.Structure.Point Translate(this SharpBoxesCore.DataStruct.Structure.Point p, double dx, double dy)
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(p.X + dx, p.Y + dy);
    }

    public static SharpBoxesCore.DataStruct.Structure.Point TranslateWithAngle(this SharpBoxesCore.DataStruct.Structure.Point p, double angle, double distance)
    {
        var radians = angle.DegreesToRadians();
        var x = p.X + distance * Math.Cos(radians);
        var y = p.Y + distance * Math.Sin(radians);
        return new SharpBoxesCore.DataStruct.Structure.Point(x, y);
    }

    public static List<SharpBoxesCore.DataStruct.Structure.Point> Translate(this List<SharpBoxesCore.DataStruct.Structure.Point> points, double dx, double dy)
    {
        return points.Select(p => p.Translate(dx, dy)).ToList();
    }

    public static SharpBoxesCore.DataStruct.Structure.Point Rotate(this SharpBoxesCore.DataStruct.Structure.Point p, double degree, SharpBoxesCore.DataStruct.Structure.Point center)
    {
        var radians = degree.DegreesToRadians();
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var x = (p.X - center.X) * cos - (p.Y - center.Y) * sin + center.X;
        var y = (p.X - center.X) * sin + (p.Y - center.Y) * cos + center.Y;
        return new SharpBoxesCore.DataStruct.Structure.Point(x, y);
    }

    public static SharpBoxesCore.DataStruct.Structure.Point ProjectionOfLine(this SharpBoxesCore.DataStruct.Structure.Point p, SharpBoxesCore.DataStruct.Structure.Point ps, SharpBoxesCore.DataStruct.Structure.Point pe)
    {
        double dx = pe.X - ps.X;
        double dy = pe.Y - ps.Y;
        double t = ((p.X - ps.X) * dx + (p.Y - ps.Y) * dy) / (dx * dx + dy * dy);
        double projectionX = ps.X + t * dx;
        double projectionY = ps.Y + t * dy;
        return new SharpBoxesCore.DataStruct.Structure.Point(projectionX, projectionY);
    }

    public static Rectangle2D Rotate(this Rectangle2D rect, double degree, SharpBoxesCore.DataStruct.Structure.Point center)
    {
        var radians = degree.DegreesToRadians();
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var p = new SharpBoxesCore.DataStruct.Structure.Point(rect.CenterX, rect.CenterY).Rotate(degree, center);
        var width = rect.HalfWidth*2 * cos + rect.HalfHeight*2 * sin;
        var height = rect.HalfWidth*2 * sin + rect.HalfHeight*2 * cos;
        return new Rectangle2D(width, height,p.X, p.Y, 0);
    }

    public static SharpBoxesCore.DataStruct.Structure.Point Centroid(this List<SharpBoxesCore.DataStruct.Structure.Point> points)
    {
        if (points == null || points.Count == 0)
        {
            throw new ArgumentException("Points list cannot be null or empty.");
        }
        var x = points.Sum(p => p.X) / points.Count;
        var y = points.Sum(p => p.Y) / points.Count;
        return new SharpBoxesCore.DataStruct.Structure.Point(x, y);
    }

    public static Rectangle2D ToRect(
        SharpBoxesCore.DataStruct.Structure.Point center,
        double width,
        double height,
        double degrees = 0,
        SharpBoxesCore.DataStruct.Structure.Point rotateCenter = default
    )
    {
        if (rotateCenter == default)
        {
            rotateCenter = center;
        }
        var rect = new Rectangle2D(width, height,center.X - width / 2, center.Y - height / 2, degrees);
        rect = rect.Rotate(degrees, rotateCenter);
        return rect;
    }

    /// <summary>
    /// 计算新点p在点集ps中的应该插入的索引
    /// </summary>
    /// <param name="ps">点集</param>
    /// <param name="p">新点</param>
    /// <returns>插入索引</returns>
    public static int GetIndexInPoints(this List<SharpBoxesCore.DataStruct.Structure.Point> ps, SharpBoxesCore.DataStruct.Structure.Point p)
    {
        //通过计算距离判断应该插入的索引
        List<double> distances = new();
        for (int i = 0; i < ps.Count - 1; i++)
        {
            distances.Add(p.DistanceToLine(ps[i], ps[i + 1]));
        }
        var minDistanceLast = distances.Min();
        var index1 = distances.IndexOf(minDistanceLast);
        return index1;
    }
}
