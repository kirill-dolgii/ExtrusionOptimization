using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;
using OpenGeometryEngine.Structures;

namespace Parametrization.Parametrization;

public class SimplePort : IPort
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

    public SimplePort(ICollection<CurveSegment> coreSide, ICollection<CurveSegment> weldChamberSide, 
        ICollection<CurveSegment> leftWebSide, ICollection<CurveSegment> rightWebSide, 
        PortParameters parameters)
    {
        CoreSide = coreSide;
        WeldChamberSide = weldChamberSide;
        LeftWebSide = leftWebSide;
        RightWebSide = rightWebSide;
        Parameters = parameters;
        Boundary = coreSide.Concat(weldChamberSide).Concat(leftWebSide).Concat(rightWebSide).ToList();
    }
   
    public SimplePort(ICollection<CurveSegment> boundary, PortParameters parameters)
    {
        Boundary = boundary;
        Parameters = parameters;
    }
}