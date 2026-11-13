using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Invoklet;
using Invoklet.Avalonia.Themes;
using Invoklet.Contracts;
using Invoklet.Languages.CSharp;
using Invoklet.Languages.DotnetCommon;
using Invoklet.Languages.DotnetCommon.Sdk;
using Invoklet.Languages.VB;
using Invoklet.Services;
using Invoklet.Services.BuildCache;
using Invoklet.Services.Preferences;
using Invoklet.Services.Recovery;
using Invoklet.Services.Scaling;
using Invoklet.Services.Setup;
using Invoklet.Secrets;
using Invoklet.UserControls;
using Invoklet.Windows;
using Invoklet.Windows.Models;

namespace Shots;

// Invoklet's own App (its App.axaml styles and resources) without its composition root, which
// would read the real settings file and Documents folder.
public sealed class ShotsApp : App
{
    public override void OnFrameworkInitializationCompleted() { }
}

public sealed class MemoryPrefs : IPreferencesService
{
    public AppearanceSettings Appearance { get; } = new();
    public EditorSettings Editor { get; } = new();
    public DebuggerSettings Debugger { get; } = new();
    public CSharpLanguageSettings CSharp { get; } = new();
    public KeyboardSettings Keyboard { get; } = new();
    public LayoutSettings Layout { get; } = new();
    public SessionSettings Session { get; } = new();
    public ExperimentalSettings Experimental { get; } = new();
    public LoggingSettings Logging { get; } = new();
    public SecretsSettings Secrets { get; } = new();
    public Dictionary<string, bool> System { get; } = [];
    public void Save() => Changed?.Invoke();
    public event Action? Changed;
}

public static class Program
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<ShotsApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .WithInterFont()
            .UseReactiveUI();

    // This folder, and from it the site's screenshots and examples.
    private static string Here([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
    public static readonly string Out = Path.GetFullPath(Path.Combine(Here(), "..", "..", "docs", "site", "assets", "screenshots"));
    public static readonly string ExamplesDir = Path.GetFullPath(Path.Combine(Here(), "..", "..", "docs", "site", "examples"));
    public static bool Light;

    // Usage: dotnet run [scenario[-light] ...]; with no arguments, every scenario in both themes.
    public static int Main(string[] args)
    {
        // English number and date formats in Invoklet itself and in the scripts it starts.
        var english = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = english;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = english;
        System.Globalization.CultureInfo.CurrentCulture = english;
        Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1");
        Directory.CreateDirectory(Out);

        if (args is ["--one", var one]) return RunOne(one);

        // Each scenario in a process of its own: the interface scale is process-wide state, and
        // a fresh Invoklet per screenshot keeps them independent of each other.
        var names = args.Length > 0 ? args : Scenarios.All.SelectMany(n => new[] { n, n + "-light" }).ToArray();
        var self = Environment.ProcessPath!;
        var failed = 0;
        foreach (var name in names)
        {
            var start = new ProcessStartInfo(self) { UseShellExecute = false };
            // Set again by the child, after its own culture: only the scripts it starts run invariant.
            start.Environment.Remove("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT");
            if (Path.GetFileNameWithoutExtension(self) == "dotnet") start.ArgumentList.Add(typeof(Program).Assembly.Location);
            start.ArgumentList.Add("--one");
            start.ArgumentList.Add(name);
            using var child = Process.Start(start)!;
            child.WaitForExit();
            if (child.ExitCode != 0) failed++;
        }
        Console.WriteLine(failed == 0 ? "All screenshots saved." : $"{failed} scenario(s) failed.");
        return failed == 0 ? 0 : 1;
    }

    private static int RunOne(string scenario)
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(Program));
        try
        {
            session.Dispatch(async () => { await Scenarios.Run(scenario); return true; }, CancellationToken.None).GetAwaiter().GetResult();
            Console.WriteLine($"OK   {scenario}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL {scenario}: {ex}");
            return 1;
        }
    }

    public static ServiceProvider CreateServices(MemoryPrefs prefs, string root)
    {
        var services = new ServiceCollection();
        services.AddSingleton<Application>(Application.Current!);
        services.AddSingleton<IPreferencesService>(prefs);
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IFileSystemService>(sp => new FileSystemService(sp.GetRequiredService<ILogger<FileSystemService>>(), root));
        services.AddSingleton<ISdkProvider, DotnetSdkProvider>();
        services.AddSingleton<IRuntimeReferenceLocator, DotnetRuntimeReferenceLocator>();
        services.AddSingleton<INuGetSourceStore, InvokletNuGetConfigStore>();
        services.AddSingleton<IPackageSearchService, NuGetPackageSearchService>();
        services.AddSingleton<ILocalPackageCacheService, LocalPackageCacheService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IBuildCacheService>(sp => new BuildCacheService(sp.GetRequiredService<ILogger<BuildCacheService>>(), Path.Combine(root, "..", "build-cache.json")));
        services.AddSingleton<IPrerequisiteService, PrerequisiteService>();
        services.AddSingleton<ISetupDownloader, SetupDownloader>();
        services.AddSingleton<ISetupProcessRunner, SetupProcessRunner>();
        services.AddSingleton<ISetupInstallerService, SetupInstallerService>();
        services.AddLogging(lb => lb.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<ILanguageHostRegistry>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            LanguageHostRegistry registry = new();
            var runtimeReferences = sp.GetRequiredService<IRuntimeReferenceLocator>();
            registry.Register(new CSharpLanguageHostFactory(loggerFactory, runtimeReferences));
            registry.Register(new VbLanguageHostFactory(loggerFactory, runtimeReferences));
            return registry;
        });
        services.AddTransient<MainWindow>();
        services.AddSingleton<IRecoveryStore>(_ => new DiskRecoveryStore(Path.Combine(root, "..", "recovery")));
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PreferencesWindowViewModel>();
        services.AddTransient<DependenciesWindowViewModel>();
        services.AddTransient<PreferencesWindow>();
        services.AddTransient<DependenciesWindow>();
        services.AddSingleton<EditorTabsControl>();
        // Connections and secrets in a temporary folder next to the scripts, with the built-in drivers.
        services.AddSingleton<Invoklet.Secrets.ISecretStore>(_ => new Invoklet.Secrets.FileSecretStore(Path.GetFullPath(Path.Combine(root, "..", "secrets.json"))));
        services.AddSingleton<Invoklet.Services.Connections.IConnectionStore>(_ => new Invoklet.Services.Connections.FileConnectionStore(Path.GetFullPath(Path.Combine(root, "..", "connections.json"))));
        services.AddSingleton(_ =>
        {
            var drivers = new Invoklet.Contracts.Connections.ConnectionDriverRegistry();
            drivers.Register(new Invoklet.Connectors.Http.HttpConnectionDriver());
            drivers.Register(new Invoklet.Connectors.Grpc.GrpcConnectionDriver());
            drivers.Register(new Invoklet.Connectors.Sqlite.SqliteConnectionDriver());
            return drivers;
        });
        // The sidebar's Connections section, over the same list.
        services.AddSingleton(sp => new Invoklet.UserControls.Models.ConnectionNavigationViewModel(
            sp.GetRequiredService<Invoklet.Services.Connections.IConnectionStore>(),
            sp.GetRequiredService<Invoklet.Contracts.Connections.ConnectionDriverRegistry>(),
            secrets: sp.GetRequiredService<Invoklet.Secrets.ISecretStore>()));
        services.AddSingleton<Func<DependenciesWindow>>(sp => () => sp.GetRequiredService<DependenciesWindow>());
        services.AddSingleton<Func<PreferencesWindow>>(sp => () => sp.GetRequiredService<PreferencesWindow>());
        return services.BuildServiceProvider();
    }

    public static async Task Pump(int ms = 300)
    {
        var until = DateTime.UtcNow.AddMilliseconds(ms);
        while (DateTime.UtcNow < until)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            await Task.Delay(15);
        }
    }

    public static async Task WaitUntil(Func<bool> condition, int timeoutMs = 60000, string what = "condition")
    {
        var until = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > until) throw new TimeoutException($"Timed out waiting for {what}");
            await Pump(50);
        }
    }

    private static RenderTargetBitmap Render(TopLevel top, double scale)
    {
        top.UpdateLayout();
        var size = new PixelSize((int)Math.Round(top.Bounds.Width * scale), (int)Math.Round(top.Bounds.Height * scale));
        var rtb = new RenderTargetBitmap(size, new Vector(96 * scale, 96 * scale));
        rtb.Render(top);
        return rtb;
    }

    // The window, with its open popups (completion, menus, tooltips) drawn on top where they are
    // on screen: each popup is a top level of its own, which rendering the window leaves out.
    public static void Capture(TopLevel top, string name, double scale = 2)
    {
        using var window = Render(top, scale);
        var popups = top.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.Popup>().Concat(Avalonia.LogicalTree.LogicalExtensions.GetLogicalDescendants(top).OfType<Avalonia.Controls.Primitives.Popup>()).Distinct()
            .Where(p => p.IsOpen && p.Child is not null).Select(p => TopLevel.GetTopLevel(p.Child)).OfType<TopLevel>().Where(t => t != top).Distinct().ToList();
        var final = new RenderTargetBitmap(window.PixelSize, new Vector(96 * scale, 96 * scale));
        using (var ctx = final.CreateDrawingContext())
        {
            ctx.DrawImage(window, new Rect(0, 0, window.PixelSize.Width, window.PixelSize.Height), new Rect(0, 0, top.Bounds.Width, top.Bounds.Height));
            var origin = top.PointToScreen(new Point(0, 0));
            foreach (var popup in popups)
            {
                using var bmp = Render(popup, scale);
                var at = popup.PointToScreen(new Point(0, 0));
                var x = (at.X - origin.X) / top.RenderScaling;
                var y = (at.Y - origin.Y) / top.RenderScaling;
                ctx.DrawImage(bmp, new Rect(0, 0, bmp.PixelSize.Width, bmp.PixelSize.Height), new Rect(x, y, popup.Bounds.Width, popup.Bounds.Height));
                Console.WriteLine($"     popup at {x},{y} {popup.Bounds.Size}");
            }
        }
        if (Light) name = Path.GetFileNameWithoutExtension(name) + "_light.png";
        var path = Path.Combine(Out, name);
        final.Save(path);
        final.Dispose();
        Console.WriteLine($"     saved {path} {window.PixelSize.Width}x{window.PixelSize.Height}");
    }
}
