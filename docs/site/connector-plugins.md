# Write a connector plugin

A connector plugin is a .NET library with an `invoklet-plugin.json` manifest. Invoklet
checks the manifest, loads the entry assembly and calls `IConnectionPlugin.Register` at
startup. GUI and CLI use the same loader.

The Invoklet source checkout contains a working example under
`tests/fixtures/plugins/SampleHttpConnector/`. Its project targets .NET 10 through the
repository's build settings and references `Invoklet.Plugins`, `Invoklet.Contracts` and
`Invoklet.Format.MdRun` as projects of that checkout. A connector of your own references them as
NuGet packages; see [Contract packages and dependencies](plugin-authoring.md#2-contract-packages-and-dependencies).
The connectors that ship with Invoklet are full examples: they live in the
[invoklet-plugins](https://github.com/tervforge/invoklet-plugins) repository.

## Define the driver and entry point

| API | What to implement |
| --- | --- |
| `IConnectionPlugin` | A public entry class with a public parameterless constructor. Return a descriptor matching the manifest and register each driver with `RegisterDriver`. |
| `IConnectionDriver` | A unique driver ID, fields for the Connections form, parameter validation, and an async connection test. `PackagesFor` can select script dependencies by target framework. |
| `ICSharpClientSource` | Generate a C# class named `context.ClassName` with an internal parameterless constructor. Invoklet exposes the instance as `Connections.<name>`. |
| `IConnectionSchemaDriver` | Add this capability when client generation needs a schema from the endpoint. Return the schema and any generated source files through `FetchSchemaAsync`. |

The entry class in the sample registers one driver.

```csharp
using Invoklet.Contracts.Connections;
using Invoklet.Plugins;

namespace SampleHttpConnector;

public sealed class Plugin : IConnectionPlugin
{
    public PluginDescriptor Descriptor { get; } = new("example.connector.http", "1.0.0");

    public void Register(ConnectionPluginContext context) =>
        context.RegisterDriver(new SampleDriver());
}
```

Use `context.RuntimeText(...)` when generating expressions for addresses, connection strings or
headers. Literal values are passed at script startup, and placeholders are resolved at runtime.
Validate fields with placeholders before resolving them, and use the resolved parameters passed
to `TestAsync` or `FetchSchemaAsync` when contacting the endpoint. Keep credentials out of
generated source and diagnostics.

## Add the manifest

Put `invoklet-plugin.json` next to the entry DLL and its dependencies. The manifest's
`id` and `version` must match the descriptor. The driver ID is separate. The sample uses
`example.connector.http` for the package and `sample-http` for `driver:` in a script.

```json
{
  "id": "example.connector.http",
  "version": "1.0.0",
  "displayName": "Sample HTTP Connector",
  "roles": { "connection": 1 },
  "entry": {
    "assembly": "SampleHttpConnector.dll",
    "type": "SampleHttpConnector.Plugin"
  }
}
```

## Build and try the sample

From the Invoklet repository root, use PowerShell to publish the library and copy the manifest.
Substitute the path to your Invoklet development build.

```powershell
$sample = 'tests/fixtures/plugins/SampleHttpConnector'
$output = 'build/obj/sample-http'
dotnet publish "$sample/SampleHttpConnector.csproj" -c Release -o $output
Copy-Item "$sample/invoklet-plugin.json" $output
& 'C:\path\to\Invoklet.exe' --plugins-dev (Resolve-Path $output).Path
```

**Preferences → Plugins** shows the sample and its diagnostics. In **Connections**, add a
**Sample HTTP** connection named `Sample`, enter an HTTP or HTTPS address and attach the
connection to a C# script. Run `Connections.Sample.Http.BaseAddress.Dump();` to inspect the
client's configured address.

Development mode loads the specified package without inventory approval. Use it for code you
trust and restart Invoklet after rebuilding.

## Package a connector

Preferences installs plugins from NuGet packages (`.nupkg`). The package must contain the
manifest, the entry DLL and its dependencies under `plugin/`. The simplest way is a project that
publishes itself into `plugin/` when packed:
[Pack and publish through NuGet](plugin-authoring.md#6-pack-and-publish-through-nuget) has a
complete project file, and the invoklet-plugins repository packs its connectors this way.

## Install and publish a connector

[Install plugins](install-plugins.md) describes installing, updating, disabling and removing
connectors and the plugin states shown in **Preferences → Plugins**.
[Create and publish a plugin](publish-plugin.md) walks through building a connector from an empty
folder, packing it and publishing it.
