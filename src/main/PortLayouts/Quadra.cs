using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;
using OpenGeometryEngine.Structures;
using Parametrization.Extensions;

namespace Parametrization;

public class Quadra
{
    public ICollection<CurveSegment> Profile;

    public List<CurveSegment> Webs;

    public Box ProfileBox;

    public List<Port> Ports;

    public Quadra(List<CurveSegment> profile, List<CurveSegment> webs)
    {
        Profile = profile;
        ProfileBox = Box.Unite(Profile.Select(cs => cs.GetBoundingBox()));

        var boxLines = ProfileBox.Corners.Take(4).Pairs(closed: true)
                        .Select(points => LineSegment.Create(points.Item1, points.Item2));

        Webs = webs.OrderByDescending(cs =>
        {
            var points = new [] { cs.StartPoint, cs.EndPoint };
            var suitablePoint = points.OrderByDescending(p => (ProfileBox.Center - p).Magnitude).First();
            return suitablePoint.Vector.SignedAngle(Vector.UnitX, Vector.UnitZ);
        }).ToList();

        Webs.Reverse();

        var webPairs = Webs.Pairs(closed: false);

        Ports = webPairs.Select(tpl =>
        {
            var leftWeb = tpl.Item1;
            var rightWeb = tpl.Item2;
            var profileBoxLine = (Line)boxLines.Single(cs => cs.IntersectCurve(tpl.Item1).Any() &&
                                                             cs.IntersectCurve(tpl.Item2).Any()).Geometry;
            var proj = profileBoxLine.ProjectPoint(ProfileBox.Center).Point;
            var offsetVec = (proj - ProfileBox.Center).Normalize();
            var coreOffset = offsetVec * 0.005;
            var weldChamOffset = offsetVec * 0.025;

            var coreTransform = Matrix.CreateTranslation(coreOffset.X, coreOffset.Y, coreOffset.Z);
            var weldChamTransform = Matrix.CreateTranslation(weldChamOffset.X, weldChamOffset.Y, weldChamOffset.Z);

            var coreLine = Line.Create(coreTransform * profileBoxLine.Origin, profileBoxLine.Direction);
            var weldChamLine = Line.Create(weldChamTransform * profileBoxLine.Origin, profileBoxLine.Direction);

            return new Port((Line)leftWeb.Geometry, (Line)rightWeb.Geometry, coreLine, weldChamLine, null);
        }).ToList();

    }
}