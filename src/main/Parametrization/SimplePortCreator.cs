using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;
using Parametrization.Extensions;

namespace Parametrization.Parametrization;

public class SimplePortCreator : PortCreatorBase
{
    public WebSegment WebSegment { get; }
    public Line CoreLine { get; }
    public SimplePortCreator(WebSegment webSegment, Line coreLine)
    {
        WebSegment = webSegment;
        CoreLine = coreLine;
    }
    
    private Vector GetNormal(Line line, Point refPoint) 
        => (refPoint - line.ProjectPoint(refPoint).Point).Normalize();

    public override IPort CreatePort(PortParameters portParameters)
    {
        var coreOffsetDir = -GetNormal(CoreLine, WebSegment.SingularityPoint);
        var coreOffsetVec = coreOffsetDir * portParameters.CoreOffset;
        var coreOffset = Matrix.CreateTranslation(coreOffsetVec);

        var leftWebOffsetVec = GetNormal((Line)WebSegment.LeftSide.Geometry, WebSegment.Center) *
                               portParameters.LeftWebWidth;
        var leftWebOffset = Matrix.CreateTranslation(leftWebOffsetVec);

        var rightWebOffsetVec = GetNormal((Line)WebSegment.RightSide.Geometry, WebSegment.Center) *
                                portParameters.RightWebWidth;
        var rightWebOffset = Matrix.CreateTranslation(rightWebOffsetVec);

        var weldChamOffsetVec = coreOffsetDir * portParameters.WeldChamberOffset;
        var weldChamOffset = Matrix.CreateTranslation(weldChamOffsetVec);

        var leftWebLine = CreateTransformedLine(WebSegment.LeftSide.GetGeometry<Line>(), leftWebOffset);
        var rightWebLine = CreateTransformedLine(WebSegment.RightSide.GetGeometry<Line>(), rightWebOffset);
        var coreLine = CreateTransformedLine(CoreLine, coreOffset);
        var weldChamLine = CreateTransformedLine(CoreLine, weldChamOffset);
        
        var sides = new List<Line>() { coreLine, rightWebLine, weldChamLine, leftWebLine };
        var corners = sides.Pairs(closed: true)
            .Select(pair => pair.Item1.IntersectCurve(pair.Item2).Single().FirstEvaluation.Point).ToList();

        var cornerLineSegments = corners.Pairs(closed: true)
            .Select(pair => LineSegment.Create(pair.Item1, pair.Item2))
            .Cast<CurveSegment>().ToList();

        return new SimplePort(cornerLineSegments.Take(1).ToList(),
            cornerLineSegments.Skip(2).Take(1).ToList(),
            cornerLineSegments.Skip(3).ToList(),
            cornerLineSegments.Skip(1).Take(1).ToList(), portParameters);
    }

    private Line CreateTransformedLine(Line line, Matrix matrix)
        => Line.Create(matrix * line.Origin, line.Direction);
}