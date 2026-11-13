using System.Reflection;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Invoklet.Avalonia.Themes;
using Invoklet.Connectors.Http;
using Invoklet.Contracts.Connections;
using Invoklet.Format.MdRun;
using Invoklet.Services.Connections;
using Invoklet.Services.Secrets;
using Invoklet.Secrets;
using Invoklet.Services;
using Invoklet.Services.Preferences;
using Invoklet.Services.Scaling;
using Invoklet.UserControls;
using Invoklet.UserControls.Models;
using Invoklet.Windows;
using Invoklet.Windows.Models;
using static Shots.Program;

namespace Shots;

public sealed class Ctx : IAsyncDisposable
{
    public required MemoryPrefs Prefs;
    public required ServiceProvider Services;
    public required MainWindow Window;
    public required MainWindowViewModel Vm;
    public required EditorTabsControl Tabs;
    public required string Root;

    // sampleConnections: false for a scenario that writes its own connection list.
    public static async Task<Ctx> Create(double width = 1100, double height = 700, Action<MemoryPrefs>? configure = null,
        bool sampleConnections = true)
    {
        var tempRoot = Path.Combine(Path.GetDirectoryName(Program.Out)!, ".screenshot-temp");
        Directory.CreateDirectory(tempRoot);
        var temp = Path.Combine(tempRoot, "Invoklet-shots-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        var root = Path.Combine(temp, "Invoklet Queries");
        Directory.CreateDirectory(root);
        var prefs = new MemoryPrefs();
        prefs.Appearance.Mode = Program.Light ? ThemeMode.Light : ThemeMode.Dark;
        // Invoklet's own interface themes; the editor keeps its default Dark+ / Light+ colors.
        prefs.Appearance.DarkInterfaceTheme = "Invoklet Midnight";
        prefs.Appearance.LightInterfaceTheme = "Invoklet Warm Light";
        prefs.Appearance.RestoreUnsavedTabs = false;
        // The debugger the first-run setup installs on Windows, unless INVOKLET_NETCOREDBG names another.
        prefs.Debugger.DebuggerPath = Environment.GetEnvironmentVariable("INVOKLET_NETCOREDBG") is { Length: > 0 } dbg ? dbg
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Invoklet", "netcoredbg", "netcoredbg", "netcoredbg.exe");
        prefs.Layout.SidebarWidth = 220;
        configure?.Invoke(prefs);
        if (sampleConnections) SeedConnections(temp);
        var sp = CreateServices(prefs, root);
        sp.GetRequiredService<IThemeService>();
        InterfaceScaling.Attach(ViewScale.For(prefs));
        var window = sp.GetRequiredService<MainWindow>();
        window.Width = width;
        window.Height = height;
        window.Show();
        await Pump(500);
        return new Ctx
        {
            Prefs = prefs,
            Services = sp,
            Window = window,
            Vm = (MainWindowViewModel)window.DataContext!,
            Tabs = sp.GetRequiredService<EditorTabsControl>(),
            Root = root,
        };
    }

    // A small list for the sidebar's Connections section: one group and one connection outside it.
    // The addresses are placeholders; no scenario connects to them.
    private static void SeedConnections(string temp)
    {
        static MdRunConnection Http(string name, string address) => new()
        {
            Name = name,
            Driver = "http",
            Params = new(StringComparer.Ordinal) { ["baseAddress"] = MdRunValue.Of(address) },
        };
        var path = Path.Combine(temp, "connections.json");
        new FileConnectionStore(path).Save([Http("catalog", "https://catalog.example.com/"),
            Http("orders", "https://orders.example.com/"), Http("weather", "https://api.weather.example.com/")], null);
        var groups = new ConnectionGroupingModel();
        var shop = groups.Create("Shop");
        groups.MoveConnection("catalog", shop.Id);
        groups.MoveConnection("orders", shop.Id);
        new FileConnectionGroupingStore(path).Save(groups.Snapshot());
    }

    public string AddScript(string name, string content)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content.ReplaceLineEndings("\n"));
        return path;
    }

    public string AddExample(string file, string? as_ = null)
        => AddScript(as_ ?? file, File.ReadAllText(Path.Combine(Program.ExamplesDir, file)));

    public async Task<(EditorTabItemViewModel Tab, EditorsTabItemControl Ctrl)> Open(string path)
    {
        await Vm.OpenScriptAsync(path);
        var tab = Vm.Tabs.Single(t => t.FilePath == path);
        EditorsTabItemControl? ctrl = null;
        await WaitUntil(() => (ctrl = Tabs.ControlForTests(tab)) is not null, 10000, "tab control");
        await Pump(1500);
        return (tab, ctrl!);
    }

    public async Task RunAndWait(EditorTabItemViewModel tab)
    {
        Tabs.Commands.Bindings[KeyCommands.RunOrContinue]();
        await WaitUntil(() => !tab.EnableRunButton, 10000, "run start");
        await WaitUntil(() => tab.EnableRunButton, 120000, "run end");
        await Pump(1500);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var t in Vm.Tabs) t.IsDirty = false;
        Window.Close();
        await Pump(200);
        Window.Dispose();
        Vm.Dispose();
        InterfaceScaling.Detach();
        await Services.DisposeAsync();
    }
}

public static class Scenarios
{
    public static readonly string[] All =
        ["overview", "editor", "search", "diagnostics", "debugging", "preview", "preferences", "dependencies", "explorer", "connections", "secrets", "script-connections"];

    public static async Task Run(string name)
    {
        Program.Light = name.EndsWith("-light");
        if (Program.Light) name = name[..^"-light".Length];
        switch (name)
        {
            case "overview": await Overview(); break;
            case "editor": await Editor(); break;
            case "search": await Search(); break;
            case "diagnostics": await Diagnostics(); break;
            case "debugging": await Debugging(); break;
            case "preview": await Preview(); break;
            case "preferences": await Preferences(); break;
            case "dependencies": await Dependencies(); break;
            case "explorer": await Explorer(); break;
            case "connections": await Connections(); break;
            case "secrets": await Secrets(); break;
            case "script-connections": await ScriptConnections(); break;
            default: throw new ArgumentException("Unknown scenario " + name);
        }
    }

    private static void AddTree(Ctx c)
    {
        c.AddExample("quick-start.inv");
        c.AddExample("live-output.inv");
        c.AddExample("debugging-loop.inv");
        c.AddExample("program-kind.inv", Path.Combine("Samples", "program-kind.inv"));
        c.AddExample("expression-kind.inv", Path.Combine("Samples", "expression-kind.inv"));
    }

    public static void ScrollResultsHome(Ctx c)
    {
        foreach (var panel in c.Window.GetVisualDescendants().OfType<Invoklet.Avalonia.HtmlRenderer.HtmlPanel>())
        {
            var bar = (Avalonia.Controls.Primitives.ScrollBar?)typeof(Invoklet.Avalonia.HtmlRenderer.HtmlPanel)
                .GetField("_verticalScrollBar", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(panel);
            if (bar is null) continue;
            bar.Value = 0;
            typeof(Invoklet.Avalonia.HtmlRenderer.HtmlPanel).GetMethod("UpdateScrollOffsets", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(panel, null);
        }
    }

    public static async Task Caret(EditorsTabItemControl ctrl, int line, int column)
    {
        var editor = ctrl.State.Editor;
        editor.TextArea.Focus();
        editor.TextArea.Caret.Position = new TextViewPosition(line, column);
        editor.TextArea.Caret.BringCaretToView();
        await Pump(800);
    }

    private static async Task Overview()
    {
        await using var c = await Ctx.Create(1100, 720, p => p.Layout.DebugPanelHeight = 330);
        AddTree(c);
        var path = c.AddExample("products-overview.inv");
        var (tab, ctrl) = await c.Open(path);
        await c.RunAndWait(tab);
        await Caret(ctrl, 10, 1);
        ScrollResultsHome(c);
        await Pump(500);
        Capture(c.Window, "overview.png");
    }

    private const string EditorCode = """
        ---
        version: "1.0"
        language: csharp
        targetSdk: latest
        kind: Statements
        ---

        ```csharp run
        // Word frequencies in a short text
        var text = "the quick brown fox jumps over the lazy dog the end";

        var counts = text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .GroupBy(word => word)
            .Select(g => new WordCount(g.Key, g.Count()))
            .OrderByDescending(w => w.Count)
            .ThenBy(w => w.Word);

        foreach (var (word, count) in counts.Take(3))
        {
            Console.WriteLine($"{word,-6} {count}");
        }

        counts.Dump("All words");

        record WordCount(string Word, int Count);
        ```
        """;

    private static async Task Editor()
    {
        // Inlay hints off: while ".Ta" is unfinished, the hint for counts would read "?".
        await using var c = await Ctx.Create(1100, 640, p => p.CSharp.InlayHintsEnabled = false);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddScript("word-counts.inv", EditorCode));
        // Hide the output panel: this one is about the code.
        c.Tabs.Commands.Bindings[KeyCommands.ToggleOutputPanel]();
        await Pump(3000);
        // Continue the query on a new line to open completion: ".ThenBy(w => w.Word)" + "\n    .Ta;".
        var doc = ctrl.State.Editor.Document;
        var line9 = doc.GetLineByNumber(9);
        doc.Insert(line9.EndOffset - 1, "\n    ");
        await Pump(500);
        await Caret(ctrl, 10, 5);
        c.Window.KeyTextInput(".");
        await Pump(2500);
        c.Window.KeyTextInput("Ta");
        await Pump(3000);
        Capture(c.Window, "editor-highlighting.png");
    }

    private static async Task Search()
    {
        await using var c = await Ctx.Create(1100, 560);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddScript("word-counts.inv", EditorCode));
        c.Tabs.Commands.Bindings[KeyCommands.ToggleOutputPanel]();
        await Caret(ctrl, 4, 1);
        c.Window.KeyPress(Key.H, RawInputModifiers.Control, PhysicalKey.H, "h");
        await Pump(800);
        c.Window.KeyTextInput("word");
        await Pump(1200);
        Capture(c.Window, "editor-search.png");
    }

    private static async Task Diagnostics()
    {
        await using var c = await Ctx.Create(1100, 640, p => p.CSharp.InlayHintsEnabled = false);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddExample("analyzer-diagnostics.inv"));
        c.Tabs.Commands.Bindings[KeyCommands.ToggleOutputPanel]();
        await Pump(4000);
        // The caret on the unknown JsonSerializer, and its lightbulb menu open.
        await Caret(ctrl, 1, 20);
        await Pump(3000);
        var margin = ctrl.State.Editor.TextArea.LeftMargins.OfType<Invoklet.Avalonia.CodeTextEditor.CodeFix.CodeFixMargin>().First();
        margin.ShowFixMenu(margin.Fixes);
        await Pump(1500);
        Capture(c.Window, "diagnostics.png");
    }

    private static async Task Debugging()
    {
        await using var c = await Ctx.Create(1100, 720);
        AddTree(c);
        var (tab, ctrl) = await c.Open(Path.Combine(c.Root, "debugging-loop.inv"));
        tab.DebugModeEnabled = true;
        ctrl.State.Editor.AddBreakpoint(5);
        await Pump(300);
        c.Tabs.Commands.Bindings[KeyCommands.RunOrContinue]();
        await WaitUntil(() => tab.IsPaused, 120000, "first stop");
        await Pump(1000);
        // Continue to the third iteration, so the values tell a story.
        c.Tabs.Commands.Bindings[KeyCommands.RunOrContinue]();
        await WaitUntil(() => !tab.IsPaused, 20000, "resume");
        await WaitUntil(() => tab.IsPaused, 20000, "second stop");
        c.Tabs.Commands.Bindings[KeyCommands.RunOrContinue]();
        await WaitUntil(() => !tab.IsPaused, 20000, "resume");
        await WaitUntil(() => tab.IsPaused, 20000, "third stop");
        await Pump(1500);
        await Caret(ctrl, 5, 9);
        await Pump(1000);
        Capture(c.Window, "debugging.png");
        c.Tabs.Commands.Bindings[KeyCommands.Stop]();
        await WaitUntil(() => tab.EnableRunButton, 30000, "stop");
    }

    private static async Task Preview()
    {
        await using var c = await Ctx.Create(1280, 720);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddExample("products-overview.inv"));
        c.Tabs.Commands.Bindings[KeyCommands.ToggleOutputPanel]();
        tab.TogglePreviewToSide();
        await Pump(2500);
        await Caret(ctrl, 10, 1);
        Capture(c.Window, "markdown-preview.png");
    }

    private static async Task Preferences()
    {
        // The default: the recovery store is a temporary folder here anyway.
        await using var c = await Ctx.Create(1100, 700, p => p.Appearance.RestoreUnsavedTabs = true);
        var window = c.Services.GetRequiredService<PreferencesWindow>();
        window.Width = 820;
        window.Height = 560;
        window.Show(c.Window);
        await Pump(1500);
        Capture(window, "preferences.png");
        window.Close();
    }

    private static async Task Dependencies()
    {
        await using var c = await Ctx.Create(1100, 700);
        var (tab, ctrl) = await c.Open(c.AddExample("quick-start.inv"));
        var window = c.Services.GetRequiredService<DependenciesWindow>();
        window.Width = 1100;
        window.Height = 680;
        window.SetFrontMatter(ctrl.State.MdRunDocument?.FrontMatter);
        window.Show(c.Window);
        await Pump(1500);
        var vm = (DependenciesWindowViewModel)window.DataContext!;
        vm.LocalFilter = "Humanizer";
        vm.SearchText = "Humanizer";
        await WaitUntil(() => vm.OnlinePackages.Count > 0, 60000, "online results");
        await Pump(1000);
        vm.SelectedOnlinePackage = vm.OnlinePackages[0];
        await Pump(5000);
        Capture(window, "dependencies.png");
        window.Close();
    }

    private static SecretStoreWindowViewModel SampleSecrets(string root, out ISecretStore store)
    {
        store = new FileSecretStore(Path.GetFullPath(Path.Combine(root, "..", "secrets.json")));
        store.Save([new Secret("billing-token", "sk_live_51H8xQ2"), new Secret("github-token", "ghp_example"), new Secret("orders-db", "p@ssw0rd")], null);
        return new SecretStoreWindowViewModel(store);
    }

    private static async Task Connections()
    {
        await using var c = await Ctx.Create(1100, 700, sampleConnections: false);
        SampleSecrets(c.Root, out var secrets);
        var store = new FileConnectionStore(Path.GetFullPath(Path.Combine(c.Root, "..", "connections.json")));
        // A real database, so that the browser on the right shows tables, columns, types and keys as a script would get them.
        // A short path, as a user would have one, rather than the temporary folder the scenario runs in.
        var shopDatabase = OperatingSystem.IsWindows() ? Path.GetFullPath(Path.Combine(c.Root, "..", "shop.db")) : "/tmp/shop.db";
        File.Delete(shopDatabase);
        using (var db = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={shopDatabase};Pooling=False"))
        {
            db.Open();
            using var create = db.CreateCommand();
            create.CommandText = """
                CREATE TABLE customers (id INTEGER PRIMARY KEY, name TEXT NOT NULL, city TEXT, created DATETIME);
                CREATE TABLE orders (id INTEGER PRIMARY KEY, customer_id INTEGER NOT NULL REFERENCES customers(id), placed DATETIME NOT NULL, total DECIMAL(10, 2));
                CREATE TABLE order_items (order_id INTEGER NOT NULL REFERENCES orders(id), line INTEGER NOT NULL, product TEXT NOT NULL, qty INT NOT NULL, PRIMARY KEY (order_id, line));
                CREATE VIEW big_orders AS SELECT id, customer_id, total FROM orders WHERE total > 100;
                """;
            create.ExecuteNonQuery();
        }
        store.Save(
        [
            new MdRunConnection
            {
                Name = "billing", Driver = "http",
                Params = new(StringComparer.Ordinal)
                {
                    ["baseAddress"] = MdRunValue.Of("https://billing.example.com/api/"),
                    ["headers"] = MdRunValue.Map([new("Authorization", MdRunValue.Of("Bearer ${secret:billing-token}")), new("Accept", MdRunValue.Of("application/json"))]),
                },
            },
            new MdRunConnection
            {
                Name = "inventory", Driver = "grpc",
                Params = new(StringComparer.Ordinal) { ["address"] = MdRunValue.Of("https://inventory.example.com:5001") },
            },
            new MdRunConnection
            {
                Name = "pets", Driver = "openapi",
                Params = new(StringComparer.Ordinal) { ["baseAddress"] = MdRunValue.Of("https://petstore.example.com/v3/") },
            },
            new MdRunConnection
            {
                Name = "shop", Driver = "sqlite",
                Params = new(StringComparer.Ordinal) { ["file"] = MdRunValue.Of(shopDatabase) },
            },
            new MdRunConnection
            {
                Name = "github", Driver = "http",
                Params = new(StringComparer.Ordinal) { ["baseAddress"] = MdRunValue.Of("https://api.github.com/") },
            },
        ], null);
        var registry = new ConnectionDriverRegistry();
        registry.Register(new HttpConnectionDriver());
        registry.Register(new Invoklet.Connectors.Grpc.GrpcConnectionDriver());
        registry.Register(new Invoklet.Connectors.Sqlite.SqliteConnectionDriver());
        registry.Register(new OpenApiConnectionDriver());
        var schemas = new ConnectionSchemas(registry, Path.GetFullPath(Path.Combine(c.Root, "..", "connection-schemas")), Invoklet.Languages.DotnetCommon.DotnetConnectionTools.Default, _ => null);
        var model = new ConnectionsWindowViewModel(store, registry, secrets, schemas);
        // The outcome of the last test of two of them, as the list shows it.
        model.Connections.First(x => x.Name == "billing").TestResult = "Success: The server answered 200 OK.";
        model.Connections.First(x => x.Name == "inventory").TestResult = "Failed: Could not reach the server: connection refused.";
        model.SelectedConnection = model.Connections.First(x => x.Name == "shop");
        var window = new ConnectionsWindow(model) { Width = 1180, Height = 700 };
        window.Show(c.Window);
        await Pump(3000);
        // The tables open, so that the columns, their types and the keys show.
        var tree = window.FindControl<TreeView>("ResourceTree")!;
        foreach (var name in new[] { "orders" })
            if (model.Resources.FirstOrDefault(r => r.Name == name) is { } table && tree.ContainerFromItem(table) is TreeViewItem node)
                node.IsExpanded = true;
        model.SelectedResource = model.Resources.FirstOrDefault(r => r.Name == "orders");
        await Pump(800);
        Capture(window, "connections.png");
        window.Close();
    }

    private const string ConnectionScript = """
        ---
        version: "1.1"
        language: csharp
        targetSdk: latest
        kind: Statements
        connections:
          - name: billing
            driver: http
            params:
              baseAddress: https://billing.example.com/api/
              headers:
                Authorization: Bearer ${secret:billing-token}
        ---

        # Invoices

        ```csharp run
        var invoices = await Connections.Billing.GetStringAsync("invoices");
        invoices.Dump();
        ```
        """;

    // The Dependencies window's Connections tab, then the editor completing the connection's client.
    private static async Task ScriptConnections()
    {
        await using var c = await Ctx.Create(1100, 640, p => p.CSharp.InlayHintsEnabled = false, sampleConnections: false);
        c.Services.GetRequiredService<IConnectionStore>().Save(
        [
            new MdRunConnection
            {
                Name = "billing", Driver = "http",
                Params = new(StringComparer.Ordinal)
                {
                    ["baseAddress"] = MdRunValue.Of("https://billing.example.com/api/"),
                    ["headers"] = MdRunValue.Map([new("Authorization", MdRunValue.Of("Bearer ${secret:billing-token}"))]),
                },
            },
            new MdRunConnection { Name = "github", Driver = "http", Params = new(StringComparer.Ordinal) { ["baseAddress"] = MdRunValue.Of("https://api.github.com/") } },
        ], null);
        c.Services.GetRequiredService<ISecretStore>().Save([new Secret("billing-token", "t0k")], null);
        var (tab, ctrl) = await c.Open(c.AddScript("invoices.inv", ConnectionScript));
        c.Tabs.Commands.Bindings[KeyCommands.ToggleOutputPanel]();

        var window = c.Services.GetRequiredService<DependenciesWindow>();
        window.Width = 1000;
        window.Height = 560;
        window.SetFrontMatter(ctrl.State.MdRunDocument?.FrontMatter);
        window.Show(c.Window);
        await Pump(1000);
        window.FindControl<TabControl>("Tabs")?.SetValue(TabControl.SelectedItemProperty, window.FindControl<TabItem>("ConnectionsTab"));
        window.FindControl<TabItem>("ConnectionsTab")!.IsSelected = true;
        await Pump(1500);
        Capture(window, "dependencies-connections.png");
        window.Close();

        await Pump(3000);
        var doc = ctrl.State.Editor.Document;
        doc.Insert(doc.TextLength, "Connections");
        await Caret(ctrl, 3, 12);
        await Pump(500);
        c.Window.KeyTextInput(".");
        await Pump(2500);
        c.Window.KeyTextInput("Bi");
        await Pump(2500);
        c.Window.KeyTextInput(".");
        await Pump(2500);
        c.Window.KeyTextInput("GetS");
        await Pump(3000);
        Capture(c.Window, "connections-completion.png");
    }

    private static async Task Secrets()
    {
        await using var c = await Ctx.Create(1100, 700);
        var vm = SampleSecrets(c.Root, out _);
        vm.SelectedSecret = vm.Secrets[0];
        var window = new SecretStoreWindow(vm) { Width = 600, Height = 440 };
        window.Show(c.Window);
        await Pump(1500);
        Capture(window, "secret-store.png");
        window.Close();
    }

    private static async Task Explorer()
    {
        await using var c = await Ctx.Create(1100, 700);
        var ws = c.Services.GetRequiredService<IWindowService>();
        var window = new AssemblyExplorerWindow(c.Services.GetRequiredService<IThemeService>(), ws, c.Root)
        {
            Preferences = c.Prefs,
            Width = 1100,
            Height = 720,
        };
        window.Show(c.Window);
        await Pump(1000);
        var linq = typeof(Enumerable).Assembly.Location;
        await window.LoadAssemblyAsync(linq);
        await Pump(1000);
        await window.SearchAsync("Enumerable.Where");
        await Pump(1500);
        var first = window.SearchResultList.Items.OfType<AssemblyTreeNode>().First();
        window.SearchResultList.SelectedItem = first;
        await window.OpenSearchResultAsync(first);
        await Pump(3000);
        Capture(window, "assembly-explorer.png");
        window.Close();
    }
}
