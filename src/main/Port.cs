using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;

namespace Parametrization;
public class Port
{
    public Line LeftWeb { get; }
    public Line RightWeb { get; }
    public Line CoreLine { get; }
    public Line WeldChamberLine { get; }
    public PortParameters Parameters { get; }
    public ICollection<Point> Corners { get; }

    public Port(Line leftWeb, Line rightWeb, Line coreLine, Line weldChamberLine, PortParameters parameters)
    {
        LeftWeb = leftWeb;
        RightWeb = rightWeb;
        CoreLine = coreLine;
        WeldChamberLine = weldChamberLine;
        Parameters = parameters;

        var inters0 = RightWeb.IntersectCurve(coreLine).Single().FirstEvaluation.Point;
        var inters1 = RightWeb.IntersectCurve(WeldChamberLine).Single().FirstEvaluation.Point;
        var inters2 = leftWeb.IntersectCurve(WeldChamberLine).Single().FirstEvaluation.Point;
        var inters3 = leftWeb.IntersectCurve(coreLine).Single().FirstEvaluation.Point;

        Corners = new List<Point>() {inters0, inters1, inters2, inters3};
    }
}