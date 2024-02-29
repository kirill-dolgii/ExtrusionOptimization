using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometryEngine;
using OpenGeometryEngine.Regions;
using Parametrization.Extensions;
using Parametrization.Parametrization;
using Parametrization.Validation;

namespace Parametrization;

public sealed class Quadra
{
    public readonly Dictionary<PortCreatorBase, ICollection<PolyLineRegion>> _map;

    public ICollection<IBoundedCurve> Profile { get; }

    public PolyLineRegion ProfileRegion { get; }

    public Box ProfileBox { get; }

    public List<PortCreatorBase> Creators { get; }

    public Quadra(ICollection<IBoundedCurve> profile,
                  ICollection<LineSegment> webs)
    {
        if (profile == null) throw new ArgumentNullException();
        if (webs == null) throw new ArgumentNullException();
        if (webs.Count != 4) throw new ArgumentException();
        
        Profile = profile;
        ProfileRegion = PolyLineRegion.CreatePolygons(profile, Plane.PlaneXY).Single();
        ProfileBox = Box.Unite(ProfileRegion.InnerRegions.SelectMany(reg => ((PolyLineRegion)reg).Curves.Select(cs => cs.GetBoundingBox())));

            var orderedWebs = webs.OrderBy(cs =>
        {
            var points = new[] { cs.StartPoint, cs.EndPoint };
            var suitablePoint = points.OrderByDescending(p => (ProfileBox.Center - p).Magnitude).First();
            var angle = Vector.SignedAngle((suitablePoint.Vector - ProfileBox.Center.Vector), Vector.UnitX, Vector.UnitZ);
            if (angle < 0) angle = 2 * Math.PI + angle;
            return angle;
        }).ToList();
        var webPairs = orderedWebs.Pairs(closed: true);

        Creators = webPairs.Select(pair =>
        {
            var webSegment = new WebSegment(pair.Item2, pair.Item1);
            var boxSide = webSegment.GetBoxSide(ProfileBox);
            return new SimplePortCreator(webSegment, (Line)boxSide.Geometry);
        }).Cast<PortCreatorBase>().ToList();

        _map = Creators.ToDictionary(c => c, c =>
        {
            var left = ProfileRegion.Split(((SimplePortCreator)c).WebSegment.RightSide.Line).Second;
            return (ICollection<PolyLineRegion>)left.SelectMany(region =>
                    region.Split(((SimplePortCreator)c).WebSegment.LeftSide.Line).First)
                .ToArray();
        });
    }

	public ICollection<IPort> Generate(ICollection<SimplePortParameters> parameters)
	{
		var validator = new SimplePortParametersValidator();
		for (int i = 0; i < 4; i++)
		{
			var result = validator.Validate(parameters.ElementAt(i));
		}
		return Creators.Zip(parameters, (port, portParameters) => port.CreatePort(portParameters)).ToList();
	}

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