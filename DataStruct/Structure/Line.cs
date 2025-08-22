using SharpBoxesCore.DataStruct;
using SharpBoxesCore.DataStruct.Structure;

namespace SharpBoxesCore.DataStruct.Structure;

public record Line : IShapeStructure
{
    public double X1;
    public double Y1;
    public double X2;
    public double Y2;

    [DebuggerStepThrough]
    public Line(double x1, double y1, double x2, double y2)
    {
        this.X1 = x1;
        this.Y1 = y1;
        this.X2 = x2;
        this.Y2 = y2;
    }

    [DebuggerStepThrough]
    public Line(Point startPoint, Point endPoint)
    {
        this.X1 = startPoint.X;
        this.Y1 = startPoint.Y;
        this.X2 = endPoint.X;
        this.Y2 = endPoint.Y;
    }

    [DebuggerStepThrough]
    public Line() { }

    [DebuggerStepThrough]
    public override string ToString()
    {
        return $"Start: {X1:F2},{Y1:F2} End: {X2:F2},{Y2:F2}";
    }

    /// <summary>
    /// 获取线段的角度值
    /// </summary>
    [JsonIgnore]
    public double Degree => (Math.Atan2(Y2 - Y1, X2 - X1) * 180 / Math.PI).Round();

    /// <summary>
    /// 获取线段的长度值
    /// </summary>
    [JsonIgnore]
    public double Length => Math.Sqrt(Math.Pow(X2 - X1, 2) + Math.Pow(Y2 - Y1, 2)).Round();

    /// <summary>
    /// 获取线段的弧度值
    /// </summary>
    [JsonIgnore]
    public double Radian => Math.Atan2(Y2 - Y1, X2 - X1).Round();

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point StartPoint => new(X1, Y1);

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point EndPoint => new(X2, Y2);

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point CenterPoint =>
        new((X1 + X2) / 2, (Y1 + Y2) / 2);

    /// <summary>
    /// 给定一条直线，获取该条直线的起点开始的若干个点，点为均分点数的等分线段
    /// 例如：给定一条直线，起点为(0,0)，终点为(10,0)，点数为5，则返回的点为(0,0),(2,0),(4,0),(6,0),(8,0),(10,0)
    /// 若点数为1，则返回的点为(0,0)
    /// 若点数为0，则返回的点为空
    /// 若起点和终点重合，则返回的点为起点
    /// 若除不尽，则最后一个点的坐标为终点的坐标
    /// </summary>
    /// <param name="number"></param>
    /// <param name="isIncludeStartPoint"></param>
    /// <param name="isIncludeEndPoint"></param>
    /// <returns></returns>
    public List<SharpBoxesCore.DataStruct.Structure.Point> GetPointsInLineByNumber(
        int number,
        bool isIncludeStartPoint = true,
        bool isIncludeEndPoint = true
    )
    {
        var ps = new List<SharpBoxesCore.DataStruct.Structure.Point>() { StartPoint, EndPoint };
        if (number == 0)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>();
        }
        if (number == 1)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>() { ps[0] };
        }
        if (ps[0].X == ps[1].X && ps[0].Y == ps[1].Y)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>() { ps[0] };
        }
        //List<SharpBoxesCore.DataStruct.Structure.Point> points =
        //    new List<SharpBoxesCore.DataStruct.Structure.Point>();
        //var yDistance = Math.Abs(ps[1].Y - ps[0].Y);
        //var xDistance = Math.Abs(ps[1].X - ps[0].X);
        //for (int i = 0; i < number; i++)
        //{
        //    var p = new SharpBoxesCore.DataStruct.Structure.Point(
        //        ps[0].X + (i + 1) * xDistance / (number + 1),
        //        ps[0].Y + (i + 1) * yDistance / (number + 1)
        //    );
        //    points.Add(p);
        //}
        //if (isIncludeStartPoint)
        //{
        //    points.Insert(0, ps[0]);
        //}
        //if (isIncludeEndPoint)
        //{
        //    points.Add(ps[1]);
        //}
        var distance = Math.Sqrt(Math.Pow(ps[1].X - ps[0].X, 2) + Math.Pow(ps[1].Y - ps[0].Y, 2));
        var angle = Math.Atan2(ps[1].Y - ps[0].Y, ps[1].X - ps[0].X);
        var length = distance / (number + 1);
        List<SharpBoxesCore.DataStruct.Structure.Point> points =
            new List<SharpBoxesCore.DataStruct.Structure.Point>();
        if (isIncludeStartPoint)
        {
            points.Add(new SharpBoxesCore.DataStruct.Structure.Point(ps[0].X, ps[0].Y));
        }
        for (int i = 0; i < number; i++)
        {
            points.Add(
                new SharpBoxesCore.DataStruct.Structure.Point(
                    ps[0].X + (i * length + length) * Math.Cos(angle),
                    ps[0].Y + (i * length + length) * Math.Sin(angle)
                )
            );
        }
        if (isIncludeEndPoint)
        {
            points.Add(new SharpBoxesCore.DataStruct.Structure.Point(ps[1].X, ps[1].Y));
        }
        return points;
    }

    /// <summary>
    /// 给定一条直线，获取该条直线的起点开始的若干个点，点为均分距离的等分线段
    /// 例如：给定一条直线，起点为(0,0)，终点为(10,0)，距离为5，则返回的点为(0,0),(2,0),(4,0),(6,0),(8,0),(10,0)
    /// 若距离为1，则返回的点为(0,0)
    /// 若距离为0，则返回的点为空
    /// 若起点和终点重合，则返回的点为起点
    /// 若除不尽，则最后一个点的坐标为终点的坐标
    /// </summary>
    /// <param name="length"></param>
    /// <param name="isIncludeStartPoint"></param>
    /// <param name="isIncludeEndPoint"></param>
    /// <returns></returns>
    public List<SharpBoxesCore.DataStruct.Structure.Point> GetPointsInLineByLength(
        double length,
        bool isIncludeStartPoint = true,
        bool isIncludeEndPoint = true
    )
    {
        var ps = new List<SharpBoxesCore.DataStruct.Structure.Point>() { StartPoint, EndPoint };
        var angle = Math.Atan2(ps[1].Y - ps[0].Y, ps[1].X - ps[0].X);
        var distance = Math.Sqrt(Math.Pow(ps[1].X - ps[0].X, 2) + Math.Pow(ps[1].Y - ps[0].Y, 2));
        if (length == 0)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>();
        }
        if (length == 1)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>() { ps[0] };
        }
        if (ps[0].X == ps[1].X && ps[0].Y == ps[1].Y)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>() { ps[0] };
        }
        var count = (int)Math.Floor(distance / length);
        if (count == 0)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>();
        }
        if (count == 1)
        {
            return new List<SharpBoxesCore.DataStruct.Structure.Point>() { ps[0] };
        }
        
        List<SharpBoxesCore.DataStruct.Structure.Point> points =
            new List<SharpBoxesCore.DataStruct.Structure.Point>();
        if (isIncludeStartPoint)
        {
            points.Add(new SharpBoxesCore.DataStruct.Structure.Point(ps[0].X, ps[0].Y));
        }

        for (double i = length; i < distance; i += length)
        {
            points.Add(
                new SharpBoxesCore.DataStruct.Structure.Point(
                    ps[0].X + i * Math.Cos(angle),
                    ps[0].Y + i * Math.Sin(angle)
                )
            );
        }
        if (isIncludeEndPoint)
        {
            points.Add(new SharpBoxesCore.DataStruct.Structure.Point(ps[1].X, ps[1].Y));
        }
        return points;
    }

    public List<double> Xs => new List<double>() { X1, X2 };

    public List<double> Ys => new List<double>() { Y1, Y2 };
}
