using System.Collections.Generic;
using OpenGeometryEngine;

namespace Parametrization.Parametrization;

public interface IPort
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
}