using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpBoxesCore.DataStruct.Structure;

namespace SharpBoxesCore.DataStruct;

public static class ShapeExtensions
{
    /// <summary>
    /// 对点的坐标进行四舍五入
    /// </summary>
    /// <param name="p">要四舍五入的点</param>
    /// <param name="digits">保留的小数位数，默认为5</param>
    /// <returns>四舍五入后的新点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point Round(
        this SharpBoxesCore.DataStruct.Structure.Point p,
        int digits = 5
    )
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(p.X.Round(digits), p.Y.Round(digits));
    }

    /// <summary>
    /// 将点的坐标转换为float类型
    /// </summary>
    /// <param name="p">要转换的点</param>
    /// <returns>转换后的新点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point ToPointF(
        this SharpBoxesCore.DataStruct.Structure.Point p
    )
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(p.X.ToFloat(), p.Y.ToFloat());
    }

    /// <summary>
    /// 对尺寸的宽度和高度进行四舍五入
    /// </summary>
    /// <param name="s">要四舍五入的尺寸</param>
    /// <param name="digits">保留的小数位数，默认为5</param>
    /// <returns>四舍五入后的新尺寸</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Size Round(
        this SharpBoxesCore.DataStruct.Structure.Size s,
        int digits = 5
    )
    {
        return new SharpBoxesCore.DataStruct.Structure.Size(
            s.Width.Round(digits),
            s.Height.Round(digits)
        );
    }

    /// <summary>
    /// 对矩形的属性进行四舍五入
    /// </summary>
    /// <param name="r">要四舍五入的矩形</param>
    /// <param name="digits">保留的小数位数，默认为5</param>
    /// <returns>四舍五入后的新矩形</returns>
    [DebuggerStepThrough]
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
    /// <param name="r">要缩放的矩形</param>
    /// <param name="size">缩放大小，正数为扩大，负数为缩小</param>
    /// <returns>缩放后的新矩形</returns>
    [DebuggerStepThrough]
    public static Rectangle2D Scale(this Rectangle2D r, int size)
    {
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

    /// <summary>
    /// 根据矩形中心扩大或缩小矩形
    /// </summary>
    /// <param name="r">要缩放的矩形</param>
    /// <param name="size">缩放大小，正数为扩大，负数为缩小</param>
    /// <returns>缩放后的新矩形</returns>
    [DebuggerStepThrough]
    public static Rectangle1D Scale(this Rectangle1D r, int size)
    {
        var left = r.TopLeft.X - size;
        var top = r.TopLeft.Y - size;
        var right = r.BottomRight.X + size;
        var bottom = r.BottomRight.Y + size;
        return new Rectangle1D(right - left, bottom - top, left, top);
    }

    /// <summary>
    /// 根据圆心扩大或缩小圆
    /// </summary>
    /// <param name="c">要缩放的圆</param>
    /// <param name="size">缩放大小，正数为扩大，负数为缩小</param>
    /// <returns>缩放后的新圆</returns>
    [DebuggerStepThrough]
    public static Circle Scale(this Circle c, int size)
    {
        return new Circle(c.Radius + size, c.CenterX, c.CenterY);
    }

    /// <summary>
    /// 根据椭圆中心扩大或缩小椭圆
    /// </summary>
    /// <param name="e">要缩放的椭圆</param>
    /// <param name="size">缩放大小，正数为扩大，负数为缩小</param>
    /// <returns>缩放后的新椭圆</returns>
    [DebuggerStepThrough]
    public static Ellipse Scale(this Ellipse e, int size)
    {
        return new Ellipse(
            e.RadiusX + size,
            e.RadiusY + size,
            e.CenterX,
            e.CenterY,
            e.RotationDegree
        );
    }

    /// <summary>
    /// 根据线段中心扩大或缩小线段
    /// </summary>
    /// <param name="line">要缩放的线段</param>
    /// <param name="size">缩放大小，正数为扩大，负数为缩小</param>
    /// <returns>缩放后的新线段</returns>
    [DebuggerStepThrough]
    public static Line Scale(this Line line, int size)
    {
        return line.ExtendLine(size, size);
    }

    /// <summary>
    /// 获取形状的外接矩形
    /// </summary>
    /// <param name="shape">形状</param>
    /// <returns>外接矩形</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Rectangle1D GetBoundingRectangle(
        this SharpBoxesCore.DataStruct.Structure.IShapeStructure shape
    )
    {
        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        if (shape is SharpBoxesCore.DataStruct.Structure.Circle circle)
        {
            double x = circle.CenterX - circle.Radius;
            double y = circle.CenterY - circle.Radius;
            double width = circle.Radius * 2;
            double height = circle.Radius * 2;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width, height, x, y);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle1D rectangle1D)
        {
            return rectangle1D;
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle2D rectangle2D)
        {
            double x = rectangle2D.CenterPoint.X - rectangle2D.HalfWidth;
            double y = rectangle2D.CenterPoint.Y - rectangle2D.HalfHeight;
            double width = rectangle2D.HalfWidth * 2;
            double height = rectangle2D.HalfHeight * 2;
            double angle = rectangle2D.AngleDegree;
            double cos = Math.Cos(angle * Math.PI / 180);
            double sin = Math.Sin(angle * Math.PI / 180);
            double x1 = x + width * cos;
            double y1 = y + height * sin;
            double x2 = x - width * cos;
            double y2 = y - height * sin;
            double minX = Math.Min(x1, x2);
            double minY = Math.Min(y1, y2);
            double maxX = Math.Max(x1, x2);
            double maxY = Math.Max(y1, y2);
            double width2 = maxX - minX;
            double height2 = maxY - minY;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width2, height2, minX, minY);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Line line)
        {
            double minX = Math.Min(line.X1, line.X2);
            double minY = Math.Min(line.Y1, line.Y2);
            double maxX = Math.Max(line.X1, line.X2);
            double maxY = Math.Max(line.Y1, line.Y2);
            double width = maxX - minX;
            double height = maxY - minY;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width, height, minX, minY);
        }
        else if (
            shape is SharpBoxesCore.DataStruct.Structure.Polygon polygon
            && polygon.Points != null
            && polygon.Points.Count > 0
        )
        {
            double minX = polygon.Points.Min(p => p.X);
            double minY = polygon.Points.Min(p => p.Y);
            double maxX = polygon.Points.Max(p => p.X);
            double maxY = polygon.Points.Max(p => p.Y);
            double width = maxX - minX;
            double height = maxY - minY;
            return new SharpBoxesCore.DataStruct.Structure.Rectangle1D(width, height, minX, minY);
        }
        else
        {
            throw new NotSupportedException($"不支持的形状类型: {shape.GetType().Name}");
        }
    }

    /// <summary>
    /// 获取形状的外接圆
    /// </summary>
    /// <param name="shape">形状</param>
    /// <returns>外接圆</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Circle GetBoundingCircle(
        this SharpBoxesCore.DataStruct.Structure.IShapeStructure shape
    )
    {
        if (shape == null)
        {
            throw new ArgumentNullException(nameof(shape));
        }

        if (shape is SharpBoxesCore.DataStruct.Structure.Circle circle)
        {
            return circle;
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle1D rectangle1D)
        {
            double centerX = rectangle1D.CenterPoint.X;
            double centerY = rectangle1D.CenterPoint.Y;
            double radius = Math.Sqrt(
                Math.Pow(rectangle1D.Width / 2, 2) + Math.Pow(rectangle1D.Height / 2, 2)
            );
            return new SharpBoxesCore.DataStruct.Structure.Circle(radius, centerX, centerY);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Rectangle2D rectangle2D)
        {
            double centerX = rectangle2D.CenterPoint.X;
            double centerY = rectangle2D.CenterPoint.Y;
            double radius = Math.Sqrt(
                Math.Pow(rectangle2D.HalfWidth, 2) + Math.Pow(rectangle2D.HalfHeight, 2)
            );
            return new SharpBoxesCore.DataStruct.Structure.Circle(radius, centerX, centerY);
        }
        else if (shape is SharpBoxesCore.DataStruct.Structure.Line line)
        {
            double centerX = (line.X1 + line.X2) / 2;
            double centerY = (line.Y1 + line.Y2) / 2;
            double radius =
                Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)) / 2;
            return new SharpBoxesCore.DataStruct.Structure.Circle(radius, centerX, centerY);
        }
        else if (
            shape is SharpBoxesCore.DataStruct.Structure.Polygon polygon
            && polygon.Points != null
            && polygon.Points.Count > 0
        )
        {
            SharpBoxesCore.DataStruct.Structure.Point centroid = polygon.Centroid;
            double maxDistance = 0;
            foreach (var point in polygon.Points)
            {
                double distance = point.DistanceTo(centroid);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
            return new SharpBoxesCore.DataStruct.Structure.Circle(
                maxDistance,
                centroid.X,
                centroid.Y
            );
        }
        else
        {
            throw new NotSupportedException($"不支持的形状类型: {shape.GetType().Name}");
        }
    }

    /// <summary>
    /// 将Rectangle2D转换为Rectangle1D
    /// </summary>
    /// <param name="r">要转换的Rectangle2D</param>
    /// <returns>转换后的Rectangle1D</returns>
    [DebuggerStepThrough]
    public static Rectangle1D ToRectangle1D(this Rectangle2D r)
    {
        return new Rectangle1D(r.HalfWidth * 2, r.HalfHeight * 2, r.TopLeft.X, r.TopLeft.Y);
    }

    /// <summary>
    /// 将Rectangle1D转换为Rectangle2D
    /// </summary>
    /// <param name="r">要转换的Rectangle1D</param>
    /// <returns>转换后的Rectangle2D</returns>
    [DebuggerStepThrough]
    public static Rectangle2D ToRectangle2D(this Rectangle1D r)
    {
        return new Rectangle2D(
            new SharpBoxesCore.DataStruct.Structure.Point(r.CenterPoint.X, r.CenterPoint.Y),
            r.Width / 2,
            r.Height / 2,
            0
        );
    }

    /// <summary>
    /// 获取Rectangle2D的中心点
    /// </summary>
    /// <param name="r">矩形</param>
    /// <returns>中心点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point Center(this Rectangle2D r)
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(
            (r.CenterX + r.HalfWidth).ToFloat(),
            (r.CenterY + r.HalfHeight).ToFloat()
        );
    }

    /// <summary>
    /// 计算从点p1到点p2的角度（弧度）
    /// </summary>
    /// <param name="p1">起点</param>
    /// <param name="p2">终点</param>
    /// <returns>角度（弧度）</returns>
    [DebuggerStepThrough]
    public static double Angle(
        this SharpBoxesCore.DataStruct.Structure.Point p1,
        SharpBoxesCore.DataStruct.Structure.Point p2
    )
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Atan2(dy, dx);
    }

    /// <summary>
    /// 计算点p到线段(ps, pe)的距离
    /// </summary>
    /// <param name="p">要计算距离的点</param>
    /// <param name="ps">线段的起点</param>
    /// <param name="pe">线段的终点</param>
    /// <returns>点p到线段(ps, pe)的距离</returns>
    [DebuggerStepThrough]
    public static double DistanceToLine(
        this SharpBoxesCore.DataStruct.Structure.Point p,
        SharpBoxesCore.DataStruct.Structure.Point ps,
        SharpBoxesCore.DataStruct.Structure.Point pe
    )
    {
        var A = p.X - ps.X;
        var B = p.Y - ps.Y;
        var C = pe.X - ps.X;
        var D = pe.Y - ps.Y;
        var dot = A * C + B * D;
        var len_sq = C * C + D * D;
        var param = -1.0;
        if (len_sq != 0)
            param = dot / len_sq;

        double xx,
            yy;

        if (param < 0)
        {
            xx = ps.X;
            yy = ps.Y;
        }
        else if (param > 1)
        {
            xx = pe.X;
            yy = pe.Y;
        }
        else
        {
            xx = ps.X + param * C;
            yy = ps.Y + param * D;
        }

        var dx = p.X - xx;
        var dy = p.Y - yy;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 对点进行平移
    /// </summary>
    /// <param name="p">要平移的点</param>
    /// <param name="dx">X轴方向的平移量</param>
    /// <param name="dy">Y轴方向的平移量</param>
    /// <returns>平移后的新点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point Translate(
        this SharpBoxesCore.DataStruct.Structure.Point p,
        double dx,
        double dy
    )
    {
        return new SharpBoxesCore.DataStruct.Structure.Point(p.X + dx, p.Y + dy);
    }

    /// <summary>
    /// 延伸线段
    /// </summary>
    /// <param name="line">要延伸的线段</param>
    /// <param name="distanceFromStart">从起点延伸的距离</param>
    /// <param name="distanceFromEnd">从终点延伸的距离</param>
    /// <returns>延伸后的新线段</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Line ExtendLine(
        this SharpBoxesCore.DataStruct.Structure.Line line,
        double distanceFromStart,
        double distanceFromEnd
    )
    {
        var start = line.StartPoint.TranslateWithAngle(line.Degree, distanceFromStart);
        var end = line.EndPoint.TranslateWithAngle(line.Degree, distanceFromEnd);
        return new SharpBoxesCore.DataStruct.Structure.Line(start, end);
    }

    /// <summary>
    /// 根据角度和距离对点进行平移
    /// </summary>
    /// <param name="p">要平移的点</param>
    /// <param name="angle">角度（度）</param>
    /// <param name="distance">平移距离</param>
    /// <returns>平移后的新点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point TranslateWithAngle(
        this SharpBoxesCore.DataStruct.Structure.Point p,
        double angle,
        double distance
    )
    {
        var radians = (angle).DegreesToRadians();
        var x = p.X + distance * Math.Cos(radians);
        var y = p.Y + distance * Math.Sin(radians);
        return new SharpBoxesCore.DataStruct.Structure.Point(x, y);
    }

    /// <summary>
    /// 对一组点进行平移
    /// </summary>
    /// <param name="points">要平移的点集</param>
    /// <param name="dx">X轴方向的平移量</param>
    /// <param name="dy">Y轴方向的平移量</param>
    /// <returns>平移后的新点集</returns>
    [DebuggerStepThrough]
    public static List<SharpBoxesCore.DataStruct.Structure.Point> Translate(
        this List<SharpBoxesCore.DataStruct.Structure.Point> points,
        double dx,
        double dy
    )
    {
        return points.Select(p => p.Translate(dx, dy)).ToList();
    }

    /// <summary>
    /// 绕指定中心点对点进行旋转
    /// </summary>
    /// <param name="p">要旋转的点</param>
    /// <param name="degree">旋转角度（度）</param>
    /// <param name="center">旋转中心</param>
    /// <returns>旋转后的新点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point Rotate(
        this SharpBoxesCore.DataStruct.Structure.Point p,
        double degree,
        SharpBoxesCore.DataStruct.Structure.Point center
    )
    {
        var radians = (-degree).DegreesToRadians();
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var x = (p.X - center.X) * cos - (p.Y - center.Y) * sin + center.X;
        var y = (p.X - center.X) * sin + (p.Y - center.Y) * cos + center.Y;
        return new SharpBoxesCore.DataStruct.Structure.Point(x, y);
    }

    /// <summary>
    /// 计算点p在线段(ps, pe)上的投影点
    /// </summary>
    /// <param name="p">要计算投影的点</param>
    /// <param name="ps">线段的起点</param>
    /// <param name="pe">线段的终点</param>
    /// <returns>投影点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point ProjectionOfLine(
        this SharpBoxesCore.DataStruct.Structure.Point p,
        SharpBoxesCore.DataStruct.Structure.Point ps,
        SharpBoxesCore.DataStruct.Structure.Point pe
    )
    {
        double dx = pe.X - ps.X;
        double dy = pe.Y - ps.Y;
        double t = ((p.X - ps.X) * dx + (p.Y - ps.Y) * dy) / (dx * dx + dy * dy);
        double projectionX = ps.X + t * dx;
        double projectionY = ps.Y + t * dy;
        return new SharpBoxesCore.DataStruct.Structure.Point(projectionX, projectionY);
    }

    /// <summary>
    /// 绕指定中心点对矩形进行旋转
    /// </summary>
    /// <param name="rect">要旋转的矩形</param>
    /// <param name="degree">旋转角度（度）</param>
    /// <param name="center">旋转中心</param>
    /// <returns>旋转后的新矩形</returns>
    [DebuggerStepThrough]
    public static Rectangle2D Rotate(
        this Rectangle2D rect,
        double degree,
        SharpBoxesCore.DataStruct.Structure.Point center
    )
    {
        var radians = (-degree).DegreesToRadians();
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var p = new SharpBoxesCore.DataStruct.Structure.Point(rect.CenterX, rect.CenterY).Rotate(
            degree,
            center
        );
        var width = rect.HalfWidth * 2 * cos + rect.HalfHeight * 2 * sin;
        var height = rect.HalfWidth * 2 * sin + rect.HalfHeight * 2 * cos;
        return new Rectangle2D(width, height, p.X, p.Y, 0);
    }

    /// <summary>
    /// 计算点集的重心（几何中心）
    /// </summary>
    /// <param name="points">点集</param>
    /// <returns>重心点</returns>
    [DebuggerStepThrough]
    public static SharpBoxesCore.DataStruct.Structure.Point Centroid(
        this List<SharpBoxesCore.DataStruct.Structure.Point> points
    )
    {
        if (points == null || points.Count == 0)
        {
            throw new ArgumentException("Points list cannot be null or empty.");
        }
        var x = points.Sum(p => p.X) / points.Count;
        var y = points.Sum(p => p.Y) / points.Count;
        return new SharpBoxesCore.DataStruct.Structure.Point(x, y);
    }

    /// <summary>
    /// 创建一个矩形
    /// </summary>
    /// <param name="center">矩形的中心点</param>
    /// <param name="width">矩形的宽度</param>
    /// <param name="height">矩形的高度</param>
    /// <param name="degrees">旋转角度（度），默认为0</param>
    /// <param name="rotateCenter">旋转中心，默认为矩形中心点</param>
    /// <returns>创建的矩形</returns>
    [DebuggerStepThrough]
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
        var rect = new Rectangle2D(
            width,
            height,
            center.X - width / 2,
            center.Y - height / 2,
            degrees
        );
        rect = rect.Rotate(degrees, rotateCenter);
        return rect;
    }

    /// <summary>
    /// 计算新点p在点集ps中的应该插入的索引
    /// </summary>
    /// <param name="ps">点集</param>
    /// <param name="p">新点</param>
    /// <returns>插入索引</returns>
    [DebuggerStepThrough]
    public static int GetIndexInPoints(
        this List<SharpBoxesCore.DataStruct.Structure.Point> ps,
        SharpBoxesCore.DataStruct.Structure.Point p
    )
    {
        List<double> distances = new();
        for (int i = 0; i < ps.Count - 1; i++)
        {
            distances.Add(p.DistanceToLine(ps[i], ps[i + 1]));
        }
        var minDistanceLast = distances.Min();
        var index1 = distances.IndexOf(minDistanceLast);
        return index1;
    }

    /// <summary>
    /// 从起点和终点创建线段
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <returns>创建的线段</returns>
    [DebuggerStepThrough]
    public static Line ToLine(
        this SharpBoxesCore.DataStruct.Structure.Point start,
        SharpBoxesCore.DataStruct.Structure.Point end
    )
    {
        return new Line(start, end);
    }

    /// <summary>
    /// 从点、角度和长度创建线段
    /// </summary>
    /// <param name="p1">起点</param>
    /// <param name="angle">角度（度）</param>
    /// <param name="length">长度</param>
    /// <returns>创建的线段</returns>
    [DebuggerStepThrough]
    public static Line ToLine(
        this SharpBoxesCore.DataStruct.Structure.Point p1,
        double angle,
        double length
    )
    {
        var p2 = p1.TranslateWithAngle(angle, length);
        return new Line(p1, p2);
    }

    /// <summary>
    /// 判断两条线段是否相交
    /// </summary>
    /// <param name="line1">第一条线段</param>
    /// <param name="line2">第二条线段</param>
    /// <returns>如果相交返回true，否则返回false</returns>
    [DebuggerStepThrough]
    public static bool IsIntersect(this Line line1, Line line2)
    {
        var x1 = line1.StartPoint.X;
        var y1 = line1.StartPoint.Y;
        var x2 = line1.EndPoint.X;
        var y2 = line1.EndPoint.Y;
        var x3 = line2.StartPoint.X;
        var y3 = line2.StartPoint.Y;
        var x4 = line2.EndPoint.X;
        var y4 = line2.EndPoint.Y;
        var denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
        if (denominator == 0)
        {
            return false;
        }
        var ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denominator;
        var ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denominator;
        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }
}
