//using OpenGeometryEngine;
//using OpenGeometryEngine.Structures;
//using Parametrization;
//using Parametrization.Extensions;

//[TestFixture]
//public class QuadraTests
//{
//    [Test]
//    public void QUADRA_CREATION()
//    {
//        var vertices = new Point[]
//        {
//            new(0.1, -0.1, 0),
//            new(0.1, 0.1, 0),
//            new(-0.1, 0.1, 0),
//            new(-0.1, -0.1, 0),
//        };

//        var prof = vertices.Pairs(closed: true)
//            .Select(points => (CurveSegment)LineSegment.Create(points.Item1, points.Item2)).ToList();

//        var profileBox = Box.Unite(prof.Select(cs => cs.GetBoundingBox()));

//        var webs = new CurveSegment[]
//        {
//            LineSegment.Create(Point.Origin, new Point(0.09, 0.11, 0)),
//            LineSegment.Create(Point.Origin, new Point(-0.09, 0.11, 0)),
//            LineSegment.Create(Point.Origin, new Point(-0.09, -0.11, 0)),
//            LineSegment.Create(Point.Origin, new Point(0.09, -0.11, 0)),
//        }.ToList();

//        var webPairs = webs.Pairs(closed: true);

//        var sides = webPairs.Select(tpl => Quadra.GetBoxSide(tpl.Item2, tpl.Item1, profileBox));

//        var quadra = new Quadra(prof, webs);
//    }
//}