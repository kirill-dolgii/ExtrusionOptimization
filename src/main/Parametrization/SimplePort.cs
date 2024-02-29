using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;

namespace Parametrization.Parametrization;

public class SimplePort : IPort
{
    public ICollection<IBoundedCurve> CoreSide { get; }
    public ICollection<IBoundedCurve> WeldChamberSide { get; }
    public ICollection<IBoundedCurve> LeftWebSide { get; }
    public ICollection<IBoundedCurve> RightWebSide { get; }
    public SimplePortParameters Parameters { get; }
    public ICollection<IBoundedCurve> Boundary { get; }
    public double Area { get; }
    public double Perimeter { get; }
    public Box Box { get; }

    public SimplePort(ICollection<IBoundedCurve> coreSide, ICollection<IBoundedCurve> weldChamberSide, 
        ICollection<IBoundedCurve> leftWebSide, ICollection<IBoundedCurve> rightWebSide, 
        SimplePortParameters parameters)
    {
        CoreSide = coreSide;
        WeldChamberSide = weldChamberSide;
        LeftWebSide = leftWebSide;
        RightWebSide = rightWebSide;
        Parameters = parameters;
        Boundary = coreSide.Concat(weldChamberSide).Concat(leftWebSide).Concat(rightWebSide).ToList();
    }
   
    public SimplePort(ICollection<IBoundedCurve> boundary, SimplePortParameters parameters)
    {
        Boundary = boundary;
        Parameters = parameters;
    }
}