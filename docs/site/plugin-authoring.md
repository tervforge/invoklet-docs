# Plugin authoring guide

This page is the complete checklist for building an Invoklet plugin. Invoklet has three public
plugin roles:

- **theme** — data-only palettes and optional editor themes;
- **connection** — a .NET assembly that adds one or more connection drivers;
- **library** — a .NET assembly that other plugins share and list in `dependsOn`; it registers
  nothing itself.

Invoklet's own plugins are built this way too. The HTTP/OpenAPI, gRPC, SQLite, PostgreSQL and
SQL Server connectors, the EF Core library they share, and the VS Code, Rider and VS 2022 themes
live in the [invoklet-plugins](https://github.com/tervforge/invoklet-plugins) repository. Each is
a NuGet package. Invoklet includes the connectors in its releases; the themes are installed from
**Preferences → Plugins**. That repository is a complete example of every step below.

A package may contain only one role. Keep the package ID, manifest version and NuGet package
version stable and aligned. The host reads and validates `invoklet-plugin.json` before it loads
any plugin assembly.

## 1. Choose the package shape

Every plugin has this manifest at the root of its development folder and under `plugin/` in the
NuGet package:

```json
{
  "id": "example.connector.http",
  "version": "1.0.0",
  "displayName": "Sample HTTP Connector",
  "publisher": "Example",
  "roles": { "connection": 1 },
  "dependsOn": [],
  "entry": {
    "assembly": "Example.HttpConnector.dll",
    "type": "Example.HttpConnector.Plugin"
  }
}
```

For a theme, omit `entry` and list the palette files instead:

```json
{
  "id": "example.theme.ocean",
  "version": "1.0.0",
  "displayName": "Ocean",
  "publisher": "Example",
  "roles": { "theme": 1 },
  "dependsOn": [],
  "assets": { "themes": ["ocean-dark.json", "ocean-light.json"] }
}
```

Manifest rules:

- IDs are stable, lower-case, dot-separated identifiers. Do not reuse an ID for another plugin.
- `version` is SemVer (`major.minor.patch`, optionally with a pre-release label).
- `roles` contains the contract version. Use `1` for the current public theme and connection
  contracts.
- `dependsOn` contains plugin IDs, not NuGet IDs. Dependencies must be installed and loaded first.
- A connection manifest must name a public entry type with a public parameterless constructor.
- A theme manifest must not contain an entry point.
- Keep credentials, tokens and machine-specific paths out of the manifest and package metadata.

## 2. Contract packages and dependencies

### Connection plugin project

Reference the Invoklet contract packages. Leave their runtime assets out: Invoklet loads its own
copies of these assemblies, so a plugin never carries them.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>Example.HttpConnector</AssemblyName>
    <RootNamespace>Example.HttpConnector</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Invoklet.Plugins" Version="1.0.0" ExcludeAssets="runtime" />
    <PackageReference Include="Invoklet.Contracts" Version="1.0.0" ExcludeAssets="runtime" />
    <PackageReference Include="Invoklet.Format.MdRun" Version="1.0.0" ExcludeAssets="runtime" />
  </ItemGroup>
</Project>
```

The contract packages are published to the Tervforge GitHub Packages feed,
`https://nuget.pkg.github.com/tervforge/index.json`. GitHub Packages asks for a sign-in even for
public packages: add the feed with your GitHub user name and a token that has `read:packages`, and
keep the token out of the repository:

```sh
dotnet nuget add source https://nuget.pkg.github.com/tervforge/index.json --name tervforge \
  --username <github-user> --password <token> --store-password-in-clear-text
```

To build against unreleased contracts, pack them from an Invoklet checkout into a local folder and
add that folder as a source instead:

```sh
for p in Invoklet.Plugins Invoklet.Contracts Invoklet.Format.MdRun; do
  dotnet pack path/to/invoklet/src/core/$p -c Release -o artifacts/contracts
done
```

The three contract packages share one version. Its major version changes only when a role
contract changes incompatibly, together with the role version in the manifest. Minor and patch
releases only add. If your plugin uses something a minor release added, set the manifest's
`minAppVersion` to the first Invoklet release that has it.

`Invoklet.Plugins` provides `IInvokletPlugin`, `PluginDescriptor` and the host-facing plugin
contract. `Invoklet.Contracts` provides connection interfaces and value types. `Invoklet.Format.MdRun`
provides `MdRunConnection`, `MdRunValue` and the script-format types used by the connection API.

Do not reference the desktop application, view models, Avalonia, `Invoklet.Plugins.Hosting` or
another plugin's assemblies directly. A plugin depends only on the public contracts, its own
runtime libraries and, through `dependsOn`, other plugins. To share code between plugins, put it in
a library plugin, as the database connectors share `invoklet.connector.efcore`.

The driver declares the NuGet packages that the generated script client needs. Those packages are
script dependencies, not plugin-host dependencies:

```csharp
public IReadOnlyDictionary<string, string> Packages => new Dictionary<string, string>
{
    ["Example.Client"] = "1.4.0"
};
```

If package versions vary by script target framework, override `PackagesFor(string? targetFramework)`.
The returned packages are added to the script's `packages:` section and restored by the normal
script build.

## 3. Connection plugin implementation

The entry point implements `IConnectionPlugin` and exposes a matching descriptor:

```csharp
using Invoklet.Contracts.Connections;
using Invoklet.Plugins;

namespace Example.HttpConnector;

public sealed class Plugin : IConnectionPlugin
{
    public PluginDescriptor Descriptor { get; } =
        new("example.connector.http", "1.0.0");

    public void Register(ConnectionPluginContext context) =>
        context.RegisterDriver(new HttpDriver());
}
```

The host calls `Initialize` once when applicable and then `Register`. Registration should only wire
drivers; do not open a network connection, show UI or perform long-running work there.

### Required driver members

Implement `IConnectionDriver` for each connection kind:

```csharp
public sealed class HttpDriver : IConnectionDriver, ICSharpClientSource
{
    public string Id => "http";
    public string DisplayName => "HTTP";
    public ConnectionCategory Category => ConnectionCategory.Http;
    public IReadOnlyList<ConnectionParam> Params =>
    [ new("baseAddress", "Base address", ConnectionParamType.Endpoint, Required: true) ];

    public IReadOnlyDictionary<string, string> Packages =>
        new Dictionary<string, string> { ["Example.HttpClient"] = "1.4.0" };

    public IReadOnlyList<string> Validate(MdRunConnection connection) => [];

    public Task<ConnectionTestResult> TestAsync(
        IReadOnlyDictionary<string, MdRunValue> resolvedParams,
        CancellationToken cancellationToken) => ...;

    public ConnectionClientCode CreateClient(
        MdRunConnection connection, ConnectionCodeContext context) => ...;
}
```

`IConnectionDriver` responsibilities:

| Member | Responsibility |
| --- | --- |
| `Id` | Stable `driver:` value used in `.inv` files. |
| `DisplayName` | Human-readable name in Connections and Preferences. |
| `Category` | `Database`, `Http` or `Grpc`. |
| `Params` | Describes the generated connection form; no custom UI is required. |
| `Packages` / `PackagesFor` | NuGet packages required by the generated script client. |
| `Validate` | Fast validation of values as written, before placeholders are resolved. |
| `TestAsync` | Tests already-resolved values and returns a user-facing result; do not throw for an unreachable endpoint. |

Use `ConnectionParamType` accurately: `Text`, `Number`, `Boolean`, `Map`, `List`, `Endpoint`,
`File` or `ConnectionString`. Mark secrets as placeholders in the connection data; never put a
resolved secret into generated source or diagnostics.

### Generated client source

Implement `ICSharpClientSource` when the connection should be available to scripts. Return a
`ConnectionClientCode` containing a public sealed C# class named by `ConnectionCodeContext.ClassName`
with an internal parameterless constructor. Build expressions through `context.Text(...)` (and the
other context helpers) so secret and environment placeholders are evaluated when the script runs,
not when the client source is generated.

The returned `Namespaces` list adds namespaces available to the generated script without requiring
the user to write a `using` directive.

### Schema-aware drivers

Implement `IConnectionSchemaDriver` in addition to `IConnectionDriver` when the endpoint exposes a
schema that Invoklet must read before compiling the script (for example gRPC reflection or a
database schema):

```csharp
public sealed class Driver : IConnectionSchemaDriver, ICSharpClientSource
{
    public Task<ConnectionSchema> FetchSchemaAsync(
        IReadOnlyDictionary<string, MdRunValue> resolvedParams,
        ConnectionTools tools,
        CancellationToken cancellationToken) => ...;

    public bool UsesScriptDirectory(MdRunConnection connection) => false;
    public string SchemaVersion => "1";
}
```

`FetchSchemaAsync` returns a summary, generated C# files and driver-owned `Data`. Use
`tools.GetPackageFolderAsync` for schema-time NuGet tools and honor cancellation. Set
`UsesScriptDirectory` when a relative file path changes the schema; increment `SchemaVersion` when
the generated interpretation changes so old cached schemas are invalidated.

## 4. Theme plugin implementation

A theme is data-only. It needs no C# assembly and no package reference. Each palette JSON contains
an `id`, `name`, `base` (`dark` or `light`), and optional `extends`, `colors`, `metrics`, `icons`
and `editor` sections:

```json
{
  "id": "example.theme.ocean-dark",
  "name": "Ocean Dark",
  "base": "dark",
  "extends": "invoklet.theme.midnight",
  "colors": { "Invoklet.Sidebar.BackgroundBrush": "#162B36" },
  "metrics": { "Invoklet.Radius.Control": 6 },
  "icons": { "Icon.Play": "M8,5 L19,12 L8,19 Z" }
}
```

Use `theme-keys.md` for the complete list of allowed colors, metrics and icon geometry. A theme
may include VS Code TextMate JSON files under `plugin/`; reference them from `editor.themes`.
Theme inheritance must be declared twice: the palette ID goes in `extends`, and the package ID of
the parent goes in the manifest's `dependsOn`.

## 5. Build, test and run locally

For a connector, publish the assembly and copy the manifest beside the published files:

```sh
dotnet publish Example.HttpConnector.csproj -c Release -o artifacts/plugin
cp invoklet-plugin.json artifacts/plugin/
```

For a theme, the development folder itself is the package root. Start Invoklet with:

```sh
Invoklet --plugins-dev /absolute/path/to/artifacts/plugin
```

Development plugins are loaded without inventory approval. Use this only with code you trust and
restart after every rebuild. Verify **Preferences → Plugins** for diagnostics, then test the real
connection form, `Test`, schema refresh and script execution.

Before publishing, test at least:

- missing, malformed and duplicate manifest IDs;
- descriptor ID/version mismatches;
- missing entry assembly or type;
- invalid connection parameters and unreachable endpoints;
- cancellation of `TestAsync` and `FetchSchemaAsync`;
- a script with the declared NuGet packages restored;
- theme inheritance, missing parents, cycles and invalid keys;
- a package with modified contents after installation.

## 6. Pack and publish through NuGet

The package holds the plugin under `plugin/`: the manifest and, for a connector, the published
assembly with its dependencies, `.deps.json` and native libraries. Let `dotnet pack` publish the
project and pack the result. Read the package ID and version from the manifest so the two cannot
drift apart:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <_Manifest>$([System.IO.File]::ReadAllText('$(MSBuildProjectDirectory)/invoklet-plugin.json'))</_Manifest>
    <PackageId>$([System.Text.RegularExpressions.Regex]::Match($(_Manifest), '"id"\s*:\s*"([^"]+)"').Groups[1].Value)</PackageId>
    <Version>$([System.Text.RegularExpressions.Regex]::Match($(_Manifest), '"version"\s*:\s*"([^"]+)"').Groups[1].Value)</Version>
    <Authors>Example</Authors>
    <Description>HTTP connection support for Invoklet.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <PayloadDirectory>$(BaseIntermediateOutputPath)plugin-payload/</PayloadDirectory>
  </PropertyGroup>
  <ItemGroup>
    <None Include="invoklet-plugin.json" CopyToPublishDirectory="PreserveNewest" />
  </ItemGroup>
  <Target Name="PackPluginPayload" BeforeTargets="_GetPackageFiles">
    <RemoveDir Directories="$(PayloadDirectory)" />
    <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish"
             Properties="NoBuild=true;PublishDir=$([System.IO.Path]::GetFullPath('$(PayloadDirectory)'))" />
    <ItemGroup>
      <_Payload Include="$(PayloadDirectory)**" />
      <None Include="@(_Payload)" Pack="true"
            PackagePath="plugin/%(_Payload.RecursiveDir)%(_Payload.Filename)%(_Payload.Extension)" />
    </ItemGroup>
  </Target>
</Project>
```

The publish has no runtime identifier, so native libraries for every operating system travel
under `runtimes/` and the plugin's `.deps.json` chooses the right one at load time. Run:

```sh
dotnet pack Example.HttpConnector.csproj -c Release -o artifacts/packages
```

Dependencies between plugins are the manifest's `dependsOn`, not NuGet dependencies: Invoklet
installs and orders plugins by their manifests, and the package lists no dependencies. A user
installs the plugins a package depends on first.

Publish the `.nupkg` to your NuGet feed, for example GitHub Packages. Do not publish credentials
in the project or package. Users select the `.nupkg` in **Preferences → Plugins → Install
package…**, review the manifest, approve the package and restart Invoklet. No plugin code runs
before approval and restart.

Package limits: at most 2048 files and 256 MB unpacked, with no links.

A feed package appears under **Preferences → Plugins → Available plugins** when its ID is a plugin
ID and its NuGet tags include `plugin`. Invoklet installs the newest version whose manifest
`minAppVersion` and role contract versions fit the running app, so raise `minAppVersion` when a
version needs a newer Invoklet: users of older releases then get the previous version.

### How Invoklet's own plugins are released

The invoklet-plugins repository builds, tests and publishes its packages on every push to its
`main` branch:

- the six connectors share one version, and every theme has its own;
- a version is published only once GitHub Packages confirms it is not there yet, and a published
  version is never replaced: a change to a plugin needs a new version in its
  `invoklet-plugin.json`;
- the repository's CI builds against a pinned Invoklet commit (`InvokletCommit` in
  `Directory.Build.props`), and Invoklet's CI checks pull requests against a pinned
  invoklet-plugins commit; each pin moves together with the versions that need it;
- after publishing, the same workflow deploys the Invoklet plugin feed to GitHub Pages
  (`https://tervforge.github.io/invoklet-plugins/v3/index.json`): a static NuGet v3 feed of every
  published version that anyone can read without signing in. Preferences lists and installs from it.

To build Invoklet with locally built connectors, check out invoklet-plugins beside the Invoklet
checkout and pack it. Invoklet then includes the packages from
`../invoklet-plugins/artifacts/packages`:

```sh
dotnet pack ../invoklet-plugins/PluginPackages.slnx -c Release -o ../invoklet-plugins/artifacts/packages
dotnet build -c Release
```

## 7. Compatibility and release checklist

Before releasing a new package:

1. Increase the manifest version; the package version follows it.
2. Keep the plugin ID unchanged for compatible updates.
3. State the minimum Invoklet version and contract role version in the package documentation.
4. Update `dependsOn` when a theme or connection relies on another plugin.
5. Rebuild against the matching `Invoklet.Plugins`, `Invoklet.Contracts` and `Invoklet.Format.MdRun`
   release.
6. Verify the package on Windows, macOS and Linux when the connector uses native libraries.
7. Publish the package and its release notes; never replace an existing version.

See [Create a theme](create-theme.md) for palette-specific examples and [Write a connector
plugin](connector-plugins.md) for the connection API reference.
