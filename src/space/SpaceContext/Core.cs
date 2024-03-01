using System;
using System.IO;
using SpaceClaim.Api.V19;
using SpaceClaim.Api.V19.Extensibility;
using System.Linq;
using OpenGeometryEngine;
using OpenGeometryEngine.Regions;
using Parametrization;
using SpaceClaim.Api.V19.Geometry;
using Frame = OpenGeometryEngine.Frame;
using ITrimmedCurve = SpaceClaim.Api.V19.Geometry.ITrimmedCurve;
using LineSegment = OpenGeometryEngine.LineSegment;
using Point = SpaceClaim.Api.V19.Geometry.Point;
using System.Reflection;

namespace SpaceContext;

public class Core : AddIn, IRibbonExtensibility, ICommandExtensibility, IExtensibility
{
    public string GetCustomUI()
    {
        var dirInfo = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;///Assembly.GetExecutingAssembly();
        var fileInfo = dirInfo.GetFiles("ribbon.xml").Single();
        var ribbon = File.ReadAllText(fileInfo.FullName);
        return ribbon;
    }

    public void Initialize()
    {
        StartTest = Command.Create("StartTest");
        StartTest.Executing += ((sender, args) =>
        {
            var ap = Window.ActiveWindow.ActiveContext.ActivePart;
            var comps = ap.Master.Components;
            var webCurves = comps.FirstOrDefault(cmp => cmp.Template.DisplayName == "Webs").Template.Curves.Select(dc => dc.Shape)
                .Select(ToGeCurve)
                .Cast<LineSegment>()
                .ToList();
            var profCurves = comps.FirstOrDefault(cmp => cmp.Template.DisplayName == "Profile").Template.Curves.Select(dc => dc.Shape)
                .Select(ToGeCurve)
                .ToList();
            var splitLine = (LineSegment)comps.FirstOrDefault(cmp => cmp.Template.DisplayName == "Split").Template.Curves.Select(dc => dc.Shape)
                .Select(ToGeCurve)
                .Single();

            var polygon = PolyLineRegion.CreatePolygons(profCurves, OpenGeometryEngine.Plane.PlaneXY).Single();

            var splitted = polygon.Split(splitLine.Line);

            var quadra = new Quadra(profCurves, webCurves);
            
            var defaultParams = new SimplePortParameters(0.003, 0.006, 0.006, 0.0, 0.008, 0.03);
            var ports = quadra.Generate(Enumerable.Repeat(defaultParams, 4).ToList());

            var portLines = ports.SelectMany(port => port.Boundary.Select(ToScTrimmedCurve)).ToList();
            var debLines = portLines.Select(ls => DesignCurve.Create(ap, ls)).ToList();

            quadra._map.Select((kv, kvI) =>
            {
                return kv.Value.Select((region, regionI) =>
                {
                    var pt = Part.Create(ap.Master.Document, $"{kvI} {regionI} area={region.Area}");
                    Component.Create(ap.Master, pt);
                    return region.Curves.Select(ToScTrimmedCurve).Select(itc => DesignCurve.Create(pt, itc)).ToList();
                }).ToList();
            }).ToList();

        });
    
        AddInTab = Command.Create("AddInTab");
    }

    private IBoundedCurve ToGeCurve(ITrimmedCurve curve)
    {
        switch (curve.Geometry)
        {
            case SpaceClaim.Api.V19.Geometry.Line line:
            {

                return new LineSegment(ToGePoint(curve.StartPoint), ToGePoint(curve.EndPoint));
            }
            case SpaceClaim.Api.V19.Geometry.Circle circle:
            {
                return new Arc(ToGeFrame(circle.Frame), circle.Radius,
                    new OpenGeometryEngine.Interval(curve.Bounds.Start, curve.Bounds.End));
            }
        }
        return new LineSegment(ToGePoint(curve.StartPoint), ToGePoint(curve.EndPoint));
    }
        
    private OpenGeometryEngine.Point ToGePoint(Point point)
        => new(point.X, point.Y, point.Z);

    private Frame ToGeFrame(SpaceClaim.Api.V19.Geometry.Frame frame) 
        => new Frame(ToGePoint(frame.Origin), ToGeVector(frame.DirX), ToGeVector(frame.DirY), ToGeVector(frame.DirZ));

    private Point ToScPoint(OpenGeometryEngine.Point point) 
        => Point.Create(point.X, point.Y, point.Z);
        
    private UnitVec ToGeVector(SpaceClaim.Api.V19.Geometry.Direction vector)
        => new UnitVec(vector.X, vector.Y, vector.Z);

    private ITrimmedCurve ToScTrimmedCurve(IBoundedCurve curve)
    {
        return curve switch
        {
            LineSegment ls => CurveSegment.Create(ToScPoint(ls.StartPoint), ToScPoint(ls.EndPoint)),
            Arc arc => CurveSegment.CreateArc(ToScPoint(arc.Circle.Frame.Origin), ToScPoint(arc.StartPoint),
                ToScPoint(arc.EndPoint),
                Direction.Create(arc.Circle.Frame.DirZ.X, arc.Circle.Frame.DirZ.Y, arc.Circle.Frame.DirZ.Z)),
            _ => throw new NotImplementedException()
        };
    }

    public bool Connect() => true;

    public void Disconnect()
    {

    }

    private Command StartTest { get; set; }

    private Command AddInTab { get; set; }
}