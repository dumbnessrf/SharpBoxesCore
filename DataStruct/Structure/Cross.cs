﻿

using SharpBoxesCore.DataStruct.Structure;

namespace SharpCanvas.Shapes.Structure;

[DebuggerStepThrough]
public record Cross : IShapeStructure
{
    public Cross(SharpBoxesCore.DataStruct.Structure.Point center, double angleDegrees, double size)
    {
        this.Size = size;
        this.Center = center;
        this.AngleDegree = angleDegrees;
    }

    public double Size;
    public SharpBoxesCore.DataStruct.Structure.Point Center;

    public double AngleDegree;
}
