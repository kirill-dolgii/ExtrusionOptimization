using System.Collections.Generic;
using OpenGeometryEngine;
using OpenGeometryEngine.Structures;

namespace Parametrization.Parametrization;

public interface IPort
{
    public ICollection<CurveSegment> CoreSide { get; }

    public ICollection<CurveSegment> WeldChamberSide { get; }

    public ICollection<CurveSegment> LeftWebSide { get; }

    public ICollection<CurveSegment> RightWebSide { get; }

    public PortParameters Parameters { get; }

    public ICollection<CurveSegment> Boundary { get; }

    public double Area { get; }
    public double Perimeter { get; }
    public Box Box { get; }
}