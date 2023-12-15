using System.IO;
using SpaceClaim.Api.V19;
using SpaceClaim.Api.V19.Extensibility;

namespace SpaceTest;

public class Core : AddIn, IRibbonExtensibility, ICommandExtensibility, IExtensibility
{
    public string GetCustomUI()
    {
        var fileInfo = new FileInfo("C:\\projects\\ExtrusionOptimization\\src\\space\\SpaceTest\\ribbon.xml");
        var ribbon = File.ReadAllText(fileInfo.FullName);
        return ribbon;
    }

    public void Initialize()
    {
        StartTest = Command.Create("StartTest");
        StartTest.Executing += ((sender, args) =>
        {

        });

        AddinTab = Command.Create("AddinTab");
    }

    public bool Connect() => true;

    public void Disconnect()
    {

    }

    private Command StartTest { get; set; }

    private Command AddinTab { get; set; }
}