namespace Parametrization;

public class SimplePortParameters
{
    public SimplePortParameters(double cutterRadius, 
        double rightWebWidth, double leftWebWidth, double coreOffset, 
        double cornerOffset, double weldChamberOffset)
    {
        CutterRadius = cutterRadius;
        RightWebWidth = rightWebWidth;
        LeftWebWidth = leftWebWidth;
        CoreOffset = coreOffset;
        CornerOffset = cornerOffset;
        WeldChamberOffset = weldChamberOffset;
    }

    public double CutterRadius { get; }
    public double RightWebWidth { get; }
    public double LeftWebWidth { get; }
    public double CoreOffset { get; }
    public double CornerOffset { get; }
    public double WeldChamberOffset { get; }
}