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
        
        var cornerOffsetVec = coreOffsetDir * portParameters.CornerOffset;
        var cornerOffset = Matrix.CreateTranslation(cornerOffsetVec);

        var leftWebOffsetVec = GetNormal((Line)WebSegment.LeftSide.Geometry, WebSegment.Center) *
                               portParameters.LeftWebWidth;
        var leftWebOffset = Matrix.CreateTranslation(leftWebOffsetVec);

        var rightWebOffsetVec = GetNormal((Line)WebSegment.RightSide.Geometry, WebSegment.Center) *
                                portParameters.RightWebWidth;
        var rightWebOffset = Matrix.CreateTranslation(rightWebOffsetVec);

        var weldChamOffsetVec = coreOffsetDir * portParameters.WeldChamberOffset;
        //var weldChamOffset = Matrix.CreateTranslation(weldChamOffsetVec);

        var leftWebSide = WebSegment.LeftSide.CreateTransformedCopy(leftWebOffset);//CreateTransformedLine(WebSegment.LeftSide.GetGeometry<Line>(), leftWebOffset);
        var rightWebSide = WebSegment.RightSide.CreateTransformedCopy(rightWebOffset);//CreateTransformedLine(WebSegment.RightSide.GetGeometry<Line>(), rightWebOffset);
        
        var leftWebLine = CreateTransformedLine(WebSegment.LeftSide.GetGeometry<Line>(), leftWebOffset);
        var rightWebLine = CreateTransformedLine(WebSegment.RightSide.GetGeometry<Line>(), rightWebOffset);

        var cornerLine = CreateTransformedLine(CoreLine, cornerOffset);

        var leftWebCornerPnt = cornerLine.IntersectCurve(leftWebLine).Single().FirstEvaluation.Point;
        var rightWebCornerPnt = cornerLine.IntersectCurve(rightWebLine).Single().FirstEvaluation.Point;

        var leftWebWeldChamPnt = leftWebCornerPnt + coreOffsetDir * (portParameters.WeldChamberOffset - portParameters.CornerOffset);
        var rightWebWeldChamPnt = rightWebCornerPnt + coreOffsetDir * (portParameters.WeldChamberOffset - portParameters.CornerOffset);

        var offsettedWebSegment = new WebSegment(leftWebSide, rightWebSide);

        var coreLine = CreateTransformedLine(CoreLine, coreOffset);
        //var weldChamLine = CreateTransformedLine(CoreLine, weldChamOffset);
        //var weldChamSide = LineSegment.Create(leftWebSide.Geometry.IntersectCurve(weldChamLine).Single().FirstEvaluation.Point,
        //                                      rightWebSide.Geometry.IntersectCurve(weldChamLine).Single().FirstEvaluation.Point);
        
        var coreSideLeftPnt = leftWebSide.Geometry.IntersectCurve(coreLine).Single().FirstEvaluation.Point;
        var coreSideRightPnt = rightWebSide.Geometry.IntersectCurve(coreLine).Single().FirstEvaluation.Point;
        var coreSideCenterPnt = (coreSideLeftPnt + coreSideRightPnt.Vector) / 2;

        var hasCoreSide = Vector.Dot(coreSideCenterPnt.Vector, coreOffsetDir) >
                          Vector.Dot(offsettedWebSegment.SingularityPoint.Vector, coreOffsetDir);

        var portPoints = new List<Point>() { leftWebCornerPnt, leftWebWeldChamPnt, rightWebWeldChamPnt, rightWebCornerPnt };

        if (hasCoreSide)
        {
            portPoints.Add(coreSideRightPnt);
            portPoints.Add(coreSideLeftPnt);
        }
        else portPoints.Add(WebSegment.SingularityPoint);

        var cornerLineSegments = portPoints.Pairs(closed: true)
            .Select(pair => LineSegment.Create(pair.Item1, pair.Item2))
            .Cast<CurveSegment>().ToList();

        return new SimplePort(cornerLineSegments, portParameters);
    }

    private Line CreateTransformedLine(Line line, Matrix matrix)
        => Line.Create(matrix * line.Origin, line.Direction);
}