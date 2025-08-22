using SharpBoxesCore.DataStruct;
using SharpBoxesCore.DataStruct.Structure;

namespace SharpBoxesCore.DataStruct.Structure;

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

    public Polygon() { }

    public override string ToString()
    {
        return $"Length: {Points.Count}, IsClosed: {IsClosed} ";
    }

    [JsonIgnore]
    public SharpBoxesCore.DataStruct.Structure.Point Centroid => Points.Centroid();
}
