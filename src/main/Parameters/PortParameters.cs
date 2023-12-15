namespace Parametrization;

public class PortParameters
{
    public PortParameters(double coreFillet, double weldChamberFillet, double rightWebWidth, double leftWebWidth, double coreOffset, double weldChamberOffset)
    {
        CoreFillet = coreFillet;
        WeldChamberFillet = weldChamberFillet;
        RightWebWidth = rightWebWidth;
        LeftWebWidth = leftWebWidth;
        CoreOffset = coreOffset;
        WeldChamberOffset = weldChamberOffset;
    }

    public double CoreFillet { get; }
    public double WeldChamberFillet { get; }
    public double RightWebWidth { get; }
    public double LeftWebWidth { get; }
    public double CoreOffset { get; }
    public double WeldChamberOffset { get; }
}