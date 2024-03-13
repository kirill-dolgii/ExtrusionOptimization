using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;
using OpenGeometryEngine.Fillet;
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

    public override IPort CreatePort(SimplePortParameters portParameters)
    {
        var coreOffsetDir = -GetNormal(CoreLine, WebSegment.SingularityPoint);
        var coreOffsetVec = coreOffsetDir * portParameters.CoreOffset;
        var coreOffset = Matrix.CreateTranslation(coreOffsetVec);
        
        var cornerOffsetVec = coreOffsetDir * portParameters.CornerOffset;
        var cornerOffset = Matrix.CreateTranslation(cornerOffsetVec);

        var leftWebOffsetVec = GetNormal((Line)WebSegment.LeftSide.Geometry, WebSegment.Center) *
                               portParameters.LeftWebWidth;
        var leftWebOffset = Matrix.CreateTranslation(leftWebOffsetVec);

        var rightWebOffsetVec = GetNormal((Line)WebSegment.RightSide.Geometry, WebSegment.Center) *
                                portParameters.RightWebWidth;
        var rightWebOffset = Matrix.CreateTranslation(rightWebOffsetVec);

        var weldChamOffsetVec = coreOffsetDir * portParameters.WeldChamberOffset;

        var leftWebSide = WebSegment.LeftSide.CreateTransformedCopy(leftWebOffset);
        var rightWebSide = WebSegment.RightSide.CreateTransformedCopy(rightWebOffset);
        
        var leftWebLine = CreateTransformedLine(WebSegment.LeftSide.GetGeometry<Line>(), leftWebOffset);
        var rightWebLine = CreateTransformedLine(WebSegment.RightSide.GetGeometry<Line>(), rightWebOffset);

        var cornerLine = CreateTransformedLine(CoreLine, cornerOffset);

        var leftWebCornerPnt = cornerLine.IntersectCurve(leftWebLine).Single().FirstEvaluation.Point;
        var rightWebCornerPnt = cornerLine.IntersectCurve(rightWebLine).Single().FirstEvaluation.Point;

        var leftWebWeldChamPnt = leftWebCornerPnt + coreOffsetDir * (portParameters.WeldChamberOffset - portParameters.CornerOffset);
        var rightWebWeldChamPnt = rightWebCornerPnt + coreOffsetDir * (portParameters.WeldChamberOffset - portParameters.CornerOffset);

        var offsetWebSegment = new WebSegment(leftWebSide, rightWebSide);

        var coreLine = CreateTransformedLine(CoreLine, coreOffset);
        
        var coreSideLeftPnt = leftWebSide.Curve.IntersectCurve(coreLine).Single().FirstEvaluation.Point;
        var coreSideRightPnt = rightWebSide.Curve.IntersectCurve(coreLine).Single().FirstEvaluation.Point;
        var coreSideCenterPnt = (coreSideLeftPnt + coreSideRightPnt.Vector) / 2;

        var hasCoreSide = Vector.Dot(coreSideCenterPnt.Vector, coreOffsetDir) >
                          Vector.Dot(offsetWebSegment.SingularityPoint.Vector, coreOffsetDir);

        var portPoints = new List<Point>() { leftWebCornerPnt, leftWebWeldChamPnt, rightWebWeldChamPnt, rightWebCornerPnt };

        if (hasCoreSide)
        {
            portPoints.Add(coreSideRightPnt);
            portPoints.Add(coreSideLeftPnt);
        }
        else portPoints.Add(WebSegment.SingularityPoint);

        var cornerLineSegments = portPoints.Pairs(closed: true)
            .Select(pair => new LineSegment(pair.Item1, pair.Item2))
            .Cast<IBoundedCurve>()
            .ToList();

        var ret = new List<IBoundedCurve>();
        for (int i = 0; i < cornerLineSegments.Count; i++)
        {
            int j = (i + 1) % cornerLineSegments.Count;
            var fillets = LineSegmentFillet.Fillet((LineSegment)cornerLineSegments[i], 
                (LineSegment)cornerLineSegments[j], 
                portParameters.CutterRadius);
            cornerLineSegments[i] = fillets.Result.ElementAt(0);
            cornerLineSegments[j] = fillets.Result.ElementAt(2);
            ret.Add(fillets.Result.ElementAt(1));
        }

        return new SimplePort(ret.Concat(cornerLineSegments).ToList(), portParameters);
    }

    private Line CreateTransformedLine(Line line, Matrix matrix)
        => new Line(matrix * line.Origin, line.Direction);
}