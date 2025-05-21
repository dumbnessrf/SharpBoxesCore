using SharpBoxesCore.DataStruct;
using SharpBoxesCore.DataStruct.Structure;


namespace SharpCanvas.Shapes.Structure;

[DebuggerStepThrough]
public record Polygon : IShapeStructure
{
    public List<SharpBoxesCore.DataStruct.Structure.Point> Points;
    public bool IsClosed;

    public Polygon(List<SharpBoxesCore.DataStruct.Structure.Point> points, bool isClosed)
    {
        this.Points = points;
        this.IsClosed = isClosed;
    }

    public override string ToString()
    {
        return $"Length: {Points.Count}, IsClosed: {IsClosed} ";
    }

    public SharpBoxesCore.DataStruct.Structure.Point Centroid => Points.Centroid();
}
