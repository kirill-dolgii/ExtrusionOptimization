using OpenGeometryEngine;
using System.Linq;
using OpenGeometryEngine.Collections;
using Parametrization.Extensions;

namespace Parametrization.Parametrization;

public class WebSegment
{
    public LineSegment LeftSide { get; }
    public LineSegment RightSide { get; }
    public Point Center { get; }
    public Point SingularityPoint { get; }
    
    public WebSegment(LineSegment leftSide, LineSegment rightSide)
    {
        Center = (leftSide.StartPoint + leftSide.EndPoint.Vector + 
                  rightSide.StartPoint.Vector + rightSide.EndPoint.Vector) / 4;
        SingularityPoint = ((Line)leftSide.Geometry).IntersectCurve((Line)rightSide.Geometry)
            .Single().FirstEvaluation.Point;
        var sides = Iterate.Over(leftSide, rightSide)
            .Select(side => Vector.Dot(side.EndPoint - side.StartPoint, Center - SingularityPoint) < 0
                            ? new LineSegment(side.EndPoint, side.StartPoint)
                            : side).ToArray();
        LeftSide = sides[0];
        RightSide = sides[1];
    }

    public IBoundedCurve GetBoxSide(Box box)
    {
        var boxCorners = box.Corners.Take(4).ToList();
        var boxSides = boxCorners.Pairs(closed: true)
            .Select(points => new LineSegment(points.Item1, points.Item2));

        var pntComparer = new PointEqualityComparer();

        var segmentCenter = LeftSide.Curve.IntersectCurve(RightSide.Curve).Single().FirstEvaluation.Point;
        var intersPoints = boxSides.SelectMany(bs => bs.IntersectCurve(RightSide).Concat(bs.IntersectCurve(LeftSide)))
            .Select(ip => ip.FirstEvaluation.Point)
            .Distinct(pntComparer)
            .ToList();

        var zeroAxis = (intersPoints.First() - segmentCenter).Normalize();

        var segmentBoundAngles = intersPoints
            .Select(pnt => new
            {
                Pnt = pnt,
                Vec = (pnt - segmentCenter).Normalize(),
                Angle = Vector.SignedAngle(zeroAxis, (pnt - segmentCenter).Normalize(), Vector.UnitZ)
            })
            .OrderBy(dataObj => dataObj.Angle).ToList();

        var segmentAngleInterval = new Interval(segmentBoundAngles.First().Angle, segmentBoundAngles.ElementAt(1).Angle);
        var suitableBoxCorners = boxCorners.Where(pnt =>
            Accuracy.WithinAngleInterval(segmentAngleInterval, Vector.SignedAngle(zeroAxis, (pnt - segmentCenter), Vector.UnitZ)));

        var suitablePair = intersPoints.Concat(suitableBoxCorners).Distinct(pntComparer)
            .Select(pnt => new { Pnt = pnt, Angle = Vector.SignedAngle(zeroAxis, (pnt - segmentCenter), Vector.UnitZ) })
            .OrderBy(dataObj => dataObj.Angle)
            .Pairs().OrderByDescending(tpl => tpl.Item2.Angle - tpl.Item1.Angle).First();

        var boxSide = boxSides.Single(cs =>
            cs.ContainsPoint(suitablePair.Item1.Pnt) && cs.ContainsPoint(suitablePair.Item2.Pnt));
        return boxSide;
    }
}