using SharpBoxes.DataStruct;
using SharpBoxes.DataStruct.Structure;



namespace SharpCanvas.Shapes.Structure;

[DebuggerStepThrough]
public record Line : IShapeStructure
{
    public double X1;
    public double Y1;
    public double X2;
    public double Y2;

    public Line(double x1, double y1, double x2, double y2)
    {
        this.X1 = x1;
        this.Y1 = y1;
        this.X2 = x2;
        this.Y2 = y2;
    }

    public override string ToString()
    {
        return $"Start: {X1},{Y1} End: {X2},{Y2}";
    }

    public double Degree => (Math.Atan2(Y2 - Y1, X2 - X1) * 180 / Math.PI).Round();

    public double Length => Math.Sqrt(Math.Pow(X2 - X1, 2) + Math.Pow(Y2 - Y1, 2)).Round();
    public double Radian => Math.Atan2(Y2 - Y1, X2 - X1).Round();

    public SharpBoxesCore.DataStruct.Structure.Point StartPoint => new(X1, Y1);
    public SharpBoxesCore.DataStruct.Structure.Point EndPoint => new(X2, Y2);
}
