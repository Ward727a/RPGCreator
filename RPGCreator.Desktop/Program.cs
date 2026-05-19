using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using RPGCreator.EngineLib;
using RPGCreator.UI;

namespace RPGCreator.Desktop;

public class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    // From what I know, this is normal for avalonia applications, as it will launch each Window and UI components in the "StartWithClassicDesktopLifetime" method (aka: Main loop).
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Byte[]; size: 318MB")] 
    public static void Main(string[] args)
    {

        EngineCore.InitCore();
        EngineCore.StartCore();

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

}
