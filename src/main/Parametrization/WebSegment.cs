using OpenGeometryEngine;
using OpenGeometryEngine.Structures;
using System.Linq;
using Parametrization.Extensions;

namespace Parametrization.Parametrization;

public class WebSegment
{
    public CurveSegment LeftSide { get; }
    public CurveSegment RightSide { get; }
    public Point Center { get; }
    public Point SingularityPoint { get; }
    
    public WebSegment(CurveSegment leftSide, CurveSegment rightSide)
    {
        LeftSide = leftSide;
        RightSide = rightSide;
        Center = (leftSide.StartPoint + leftSide.EndPoint.Vector + 
                  rightSide.StartPoint.Vector + rightSide.EndPoint.Vector) / 4;
        SingularityPoint = ((Line)leftSide.Geometry).IntersectCurve((Line)rightSide.Geometry)
            .Single().FirstEvaluation.Point;
    }

    public CurveSegment GetBoxSide(Box box)
    {
        var boxCorners = box.Corners.Take(4).ToList();
        var boxSides = boxCorners.Pairs(closed: true)
            .Select(points => LineSegment.Create(points.Item1, points.Item2));

        var segmentCenter = LeftSide.Geometry.IntersectCurve(RightSide.Geometry).Single().FirstEvaluation.Point;
        var intersPoints = boxSides.SelectMany(bs => bs.IntersectCurve(RightSide).Concat(bs.IntersectCurve(LeftSide)))
            .Select(ip => ip.FirstEvaluation.Point)
            .ToList();

        var zeroAxis = (intersPoints.First() - segmentCenter).Normalize();

        var segmentBoundAngles = intersPoints
            .Select(pnt => new
            {
                Pnt = pnt,
                Vec = (pnt - segmentCenter).Normalize(),
                Angle = zeroAxis.SignedAngle((pnt - segmentCenter).Normalize(), Vector.UnitZ)
            })
            .OrderBy(dataObj => dataObj.Angle).ToList();

        var segmentAngleInterval = new Interval(segmentBoundAngles.First().Angle, segmentBoundAngles.ElementAt(1).Angle);
        var suitableBoxCorners = boxCorners.Where(pnt =>
            Accuracy.WithinAngleInterval(segmentAngleInterval, zeroAxis.SignedAngle((pnt - segmentCenter), Vector.UnitZ)));

        var suitablePair = intersPoints.Concat(suitableBoxCorners)
            .Select(pnt => new { Pnt = pnt, Angle = zeroAxis.SignedAngle((pnt - segmentCenter), Vector.UnitZ) })
            .OrderBy(dataObj => dataObj.Angle)
            .Pairs().OrderByDescending(tpl => tpl.Item2.Angle - tpl.Item1.Angle).First();

        var boxSide = boxSides.Single(cs =>
            cs.ContainsPoint(suitablePair.Item1.Pnt) && cs.ContainsPoint(suitablePair.Item2.Pnt));
        return boxSide;
    }
}