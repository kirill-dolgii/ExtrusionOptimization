using System.IO;
using SpaceClaim.Api.V19;
using SpaceClaim.Api.V19.Extensibility;

using System.Drawing;
using System.Linq;
using Parametrization;
using SpaceClaim.Api.V19.Geometry;

namespace SpaceContext;

public class Core : AddIn, IRibbonExtensibility, ICommandExtensibility, IExtensibility
{
    public string GetCustomUI()
    {
        var fileInfo = new FileInfo("C:\\projects\\ExtrusionOptimization\\src\\space\\SpaceContext\\ribbon.xml");
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
                .ToList();
            var profCurves = comps.FirstOrDefault(cmp => cmp.Template.DisplayName == "Profile").Template.Curves.Select(dc => dc.Shape)
                .Select(ToGeCurve)
                .Cast<OpenGeometryEngine.CurveSegment>()
                .ToList();

            var quadra = new Parametrization.Quadra(profCurves, webCurves);
            var defaultParams = new PortParameters(0, 0, 0.005, 0.005, 0.002, 0.02);
            var ports = quadra.Generate(Enumerable.Repeat(defaultParams, 4).ToList());

            var portLines = ports.SelectMany(port => port.Boundary.Select(ToScTrimmedCurve)).ToList();
            var debLines = portLines.Select(ls => DesignCurve.Create(ap, ls)).ToList();
        });

        AddInTab = Command.Create("AddInTab");
    }

    private OpenGeometryEngine.LineSegment ToGeCurve(ITrimmedCurve curve)
    {
        return OpenGeometryEngine.LineSegment.Create(ToGePoint(curve.StartPoint), ToGePoint(curve.EndPoint));
    }

    private OpenGeometryEngine.Point ToGePoint(Point point)
        => new(point.X, point.Y, point.Z);

    private Point ToScPoint(OpenGeometryEngine.Point point) 
        => Point.Create(point.X, point.Y, point.Z);

    private ITrimmedCurve ToScTrimmedCurve(OpenGeometryEngine.CurveSegment curve)
    {
        var lineSeg = (OpenGeometryEngine.LineSegment)curve;
        return CurveSegment.Create(ToScPoint(lineSeg.StartPoint), ToScPoint(lineSeg.EndPoint));
    }

    public bool Connect() => true;

    public void Disconnect()
    {

    }

    private Command StartTest { get; set; }

    private Command AddInTab { get; set; }
}