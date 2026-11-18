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

## Install a connector for regular use

1. Start Invoklet normally and open **Preferences → Plugins**.
2. Choose **Install package…** and select the `.nupkg` file.
3. Review the name, version, publisher and source. A connector runs with the same permissions
   as Invoklet. If you trust the package, choose **Approve and install**, then restart.
4. Use **Disable** or **Remove** in **Preferences → Plugins** to stop loading a connector.
   The change takes effect after restarting Invoklet.

Disabling a plugin also disables its dependent plugins. To turn a dependent plugin back on,
enable its dependencies first, then enable the plugin itself.

Approval records the hash of the selected package contents. At startup, Invoklet checks the
installed files against that hash before loading the connector. No code from the package runs
until you approve it and restart.

**Remove** is refused while another installed plugin depends on the connector, even if that
plugin's files changed since approval; the message names those plugins. Remove them first.
It is also refused while the manifest of another installed plugin cannot be read, because its
dependencies are unknown; remove or repair that plugin first.

### Plugin states

| State | Meaning |
| --- | --- |
| Loaded | The plugin loaded at this start. **Content verified** means its files matched the approved hash. |
| Rejected | The plugin could not load. Its row lists the reason without file paths or secrets; other plugins still load. |
| Disabled | The plugin is installed but turned off. |
| Content changed | The installed files no longer match the approved hash, so the plugin is not loaded. For a plugin installed through Preferences, remove it and install the package again to approve the new contents. For a plugin included with Invoklet, reinstall Invoklet. |
| Pending restart | An install, removal, **Enable** or **Disable** takes effect at the next start. **Restart now** appears while such a change is waiting. |

**Restart now** closes Invoklet through its usual exit, which asks about unsaved tabs. If you
cancel, Invoklet keeps running and nothing is restarted. Otherwise it starts again with the
same command-line arguments.
