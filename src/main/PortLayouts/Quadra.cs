using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;
using OpenGeometryEngine.Structures;
using Parametrization.Extensions;
using Parametrization.Parametrization;

namespace Parametrization;

public class Quadra
{
    public ICollection<CurveSegment> Profile;

    public Box ProfileBox;

    public List<PortCreatorBase> Creators;

    public Quadra(ICollection<CurveSegment> profile,
                  ICollection<LineSegment> webs)
    {
        if (profile == null) throw new ArgumentNullException();
        if (webs == null) throw new ArgumentNullException();
        if (webs.Count != 4) throw new ArgumentException();
        ProfileBox = Box.Unite(profile.Select(cs => cs.GetBoundingBox()));
            
        var orderedWebs = webs.OrderBy(cs =>
        {
            var points = new[] { cs.StartPoint, cs.EndPoint };
            var suitablePoint = points.OrderByDescending(p => (ProfileBox.Center - p).Magnitude).First();
            return suitablePoint.Vector.SignedAngle(Vector.UnitX, Vector.UnitZ);
        }).ToList();
        var webPairs = orderedWebs.Pairs(closed: true);
        Creators = webPairs.Select(pair =>
        {
            var webSegment = new WebSegment(pair.Item2, pair.Item1);
            var boxSide = webSegment.GetBoxSide(ProfileBox);
            return new SimplePortCreator(webSegment, (Line)boxSide.Geometry);
        }).Cast<PortCreatorBase>().ToList();
    }

    public ICollection<IPort> Generate(ICollection<PortParameters> parameters) =>
        Creators.Zip(parameters, (port, portParameters) => port.CreatePort(portParameters)).ToList();

    //    var webPairs = Webs.Pairs(closed: true);

    //    Ports = webPairs.Select(tpl =>
    //    {
    //        var leftWeb = tpl.Item2;
    //        var rightWeb = tpl.Item1;
    //        var profileBoxLine = (Line)GetBoxSide(leftWeb, rightWeb, ProfileBox).Geometry;
    //        var proj = profileBoxLine.ProjectPoint(ProfileBox.Center).Point;
    //        var offsetVec = (proj - ProfileBox.Center).Normalize();

    //        var coreOffset = offsetVec * 0.005;
    //        var weldChamOffset = offsetVec * 0.025;

    //        var segmentCenterPnt = (leftWeb.StartPoint + rightWeb.StartPoint.Vector + leftWeb.EndPoint.Vector +
    //                                rightWeb.EndPoint.Vector) / 4;

    //        var leftWebOffset = GetNormal((Line)leftWeb.Geometry, segmentCenterPnt) * 0.008;
    //        var rightWebOffset = GetNormal((Line)rightWeb.Geometry, segmentCenterPnt) * 0.008;

    //        var leftWebTransform = Matrix.CreateTranslation(leftWebOffset.X, leftWebOffset.Y, leftWebOffset.Z);
    //        var rightWebTransform = Matrix.CreateTranslation(rightWebOffset.X, rightWebOffset.Y, rightWebOffset.Z);

    //        var coreTransform = Matrix.CreateTranslation(coreOffset.X, coreOffset.Y, coreOffset.Z);
    //        var weldChamTransform = Matrix.CreateTranslation(weldChamOffset.X, weldChamOffset.Y, weldChamOffset.Z);

    //        var coreLine = Line.Create(coreTransform * profileBoxLine.Origin, profileBoxLine.Direction);
    //        var weldChamLine = Line.Create(weldChamTransform * profileBoxLine.Origin, profileBoxLine.Direction);

    //        var leftSideLine = Line.Create(leftWebTransform * ((Line)leftWeb.Geometry).Origin,
    //            ((Line)leftWeb.Geometry).Direction);
    //        var rightSideLine = Line.Create(rightWebTransform * ((Line)rightWeb.Geometry).Origin,
    //            ((Line)rightWeb.Geometry).Direction);

    //        return new SimplePortCreator(leftSideLine, rightSideLine, coreLine, weldChamLine, null);
    //    }).ToList();
    //}

    //private Vector GetNormal(Line line, Point referencePoint)
    //    => (referencePoint - line.ProjectPoint(referencePoint).Point).Normalize();

    //private Point GetSegmentCorner(Line leftLine, Line rightLine)
    //{
    //    if (leftLine == null) throw new ArgumentNullException(nameof(leftLine));
    //    if (rightLine == null) throw new ArgumentNullException(nameof(rightLine));
    //    var ip = leftLine.IntersectCurve(rightLine).Single().FirstEvaluation.Point;
    //    return ip;
    //}
}