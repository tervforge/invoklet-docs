# Create and publish a plugin

This walkthrough builds a small connector plugin from an empty folder, tries it in Invoklet,
packs it as a NuGet package and publishes it so that other people can install it. The
[Plugin authoring guide](plugin-authoring.md) is the reference for every step; this page puts the
steps in order.

To make a theme instead, follow [Create a theme](create-theme.md): a theme is data only, needs no
C# and is packed and published the same way as described below from
[Pack the plugin](#5-pack-the-plugin) on.

You need:

- the .NET 10 SDK;
- Invoklet;
- a GitHub account and a personal access token (classic) with the `read:packages` scope, to
  download the Invoklet contract packages.

## 1. Add the contract packages feed

A plugin compiles against three contract packages: `Invoklet.Plugins`, `Invoklet.Contracts` and
`Invoklet.Format.MdRun`. They are published to the Tervforge feed on GitHub Packages, which asks
for a sign-in even for public packages. Add it once on your machine, and keep the token out of
your repository:

```sh
dotnet nuget add source https://nuget.pkg.github.com/tervforge/index.json --name tervforge \
  --username <github-user> --password <token> --store-password-in-clear-text
```

## 2. Create the project

Make a folder named `Example.HttpConnector` with three files.

### `invoklet-plugin.json`

The manifest names the plugin. The package ID and version are read from it, so this is the only
place they are written.

```json
{
  "id": "example.connector.http",
  "version": "1.0.0",
  "displayName": "Example HTTP Connector",
  "publisher": "Example",
  "roles": { "connection": 1 },
  "dependsOn": [],
  "entry": {
    "assembly": "Example.HttpConnector.dll",
    "type": "Example.HttpConnector.Plugin"
  }
}
```

Choose an ID that nobody else uses, such as your name or organization followed by the kind and
name of the plugin. Never change it later: Invoklet tells plugins apart by ID.

### `Example.HttpConnector.csproj`

The project references the contracts without copying them (Invoklet has its own copies) and
packs the published plugin under `plugin/` in the package:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <_Manifest>$([System.IO.File]::ReadAllText('$(MSBuildProjectDirectory)/invoklet-plugin.json'))</_Manifest>
    <PackageId>$([System.Text.RegularExpressions.Regex]::Match($(_Manifest), '"id"\s*:\s*"([^"]+)"').Groups[1].Value)</PackageId>
    <Version>$([System.Text.RegularExpressions.Regex]::Match($(_Manifest), '"version"\s*:\s*"([^"]+)"').Groups[1].Value)</Version>
    <Title>Example HTTP Connector</Title>
    <Authors>Example</Authors>
    <Description>An HTTP connection for Invoklet scripts.</Description>
    <PackageTags>invoklet plugin</PackageTags>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <PayloadDirectory>$(BaseIntermediateOutputPath)plugin-payload/</PayloadDirectory>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Invoklet.Plugins" Version="1.0.0" ExcludeAssets="runtime" />
    <PackageReference Include="Invoklet.Contracts" Version="1.0.0" ExcludeAssets="runtime" />
    <PackageReference Include="Invoklet.Format.MdRun" Version="1.0.0" ExcludeAssets="runtime" />
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

The `invoklet` and `plugin` tags let the Plugins page find the package in a NuGet feed; see
[Publish the plugin](#6-publish-the-plugin).

### `Plugin.cs`

The entry class registers one driver. The driver describes the fields of the connection form,
validates and tests a connection, and generates the C# class that a script uses as
`Connections.<name>`:

```csharp
using Invoklet.Contracts.Connections;
using Invoklet.Format.MdRun;
using Invoklet.Plugins;

namespace Example.HttpConnector;

public sealed class Plugin : IConnectionPlugin
{
    public PluginDescriptor Descriptor { get; } = new("example.connector.http", "1.0.0");

    public void Register(ConnectionPluginContext context) => context.RegisterDriver(new HttpDriver());
}

internal sealed class HttpDriver : IConnectionDriver, ICSharpClientSource
{
    public string Id => "example-http";
    public string DisplayName => "Example HTTP";
    public ConnectionCategory Category => ConnectionCategory.Http;
    public IReadOnlyList<ConnectionParam> Params { get; } = [new("baseAddress", "Base address", Required: true)];
    public IReadOnlyDictionary<string, string> Packages { get; } = new Dictionary<string, string>();

    public IReadOnlyList<string> Validate(MdRunConnection connection) =>
        connection.Params.TryGetValue("baseAddress", out var address) && !string.IsNullOrWhiteSpace(address.Text)
            ? [] : ["The base address is missing."];

    public async Task<ConnectionTestResult> TestAsync(IReadOnlyDictionary<string, MdRunValue> resolvedParams,
        CancellationToken cancellationToken)
    {
        if (!resolvedParams.TryGetValue("baseAddress", out var value)
            || !Uri.TryCreate(value.Text, UriKind.Absolute, out var address)
            || address.Scheme is not ("http" or "https"))
            return new(false, "An HTTP or HTTPS base address is required.");

        using var client = new HttpClient();
        try
        {
            using var response = await client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return new(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}.");
        }
        catch (HttpRequestException)
        {
            return new(false, "The endpoint could not be reached.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "The endpoint did not respond in time.");
        }
    }

    public ConnectionClientCode CreateClient(MdRunConnection connection, ConnectionCodeContext context) => new($$"""
        public sealed class {{context.ClassName}} : global::System.IDisposable
        {
            public global::System.Net.Http.HttpClient Http { get; }

            internal {{context.ClassName}}()
            {
                Http = new global::System.Net.Http.HttpClient { BaseAddress = new global::System.Uri({{context.RuntimeText(connection.Params["baseAddress"].Text!)}}) };
            }

            public void Dispose() => Http.Dispose();
        }
        """);
}
```

The descriptor's ID and version must match the manifest. The driver ID, `example-http`, is a
separate value: it is what a script's `driver:` field names. `context.RuntimeText(...)` makes the
address an expression evaluated when the script runs, so a secret or environment placeholder in
it is never written into generated source.

The [Plugin authoring guide](plugin-authoring.md#3-connection-plugin-implementation) describes every
driver member, script NuGet packages and schema-aware drivers.

## 3. Try it in development mode

Publish the plugin into a folder and start Invoklet with that folder:

```sh
dotnet publish -c Release -o artifacts/plugin
Invoklet --plugins-dev "$PWD/artifacts/plugin"
```

On Windows, in PowerShell:

```powershell
dotnet publish -c Release -o artifacts/plugin
& 'C:\path\to\Invoklet.exe' --plugins-dev (Resolve-Path artifacts/plugin).Path
```

Invoklet shows a warning while development mode is on and loads the folder without asking for
approval, so use it only with code you trust. Then:

1. Open **Preferences → Plugins** and check that **Example HTTP Connector** is loaded. A rejected
   plugin shows the reason in its row.
2. In **Connections**, add an **Example HTTP** connection named `Example`, enter an address such as
   `https://example.com/` and select **Test**.
3. Attach the connection to a C# script and run `Connections.Example.Http.BaseAddress.Dump();`.

After every change, publish again and restart Invoklet.

## 4. Test the failure cases

Before you share the plugin, check at least that an empty or invalid address gives a clear
message, an unreachable address does not hang or crash, and cancelling a test works. The
[authoring checklist](plugin-authoring.md#5-build-test-and-run-locally) lists more cases.

## 5. Pack the plugin

```sh
dotnet pack -c Release -o artifacts/packages
```

This makes `artifacts/packages/example.connector.http.1.0.0.nupkg`. Check it the way a user
will: start Invoklet without `--plugins-dev`, select **Preferences → Plugins → Install package…**,
choose the file, approve it and restart. The package must have the manifest and the plugin's
files under `plugin/`, at most 2048 files and 256 MB, and no links.

## 6. Publish the plugin

Pick one or more of these ways to let people install your plugin.

### Attach the package to a release

Upload the `.nupkg` to a release page, such as a GitHub release of your repository. Users
download it and install it with **Install package…**. This needs no feed and works for everyone.

### Publish to a public NuGet feed

Push the package to a NuGet v3 feed that can be read without signing in, such as nuget.org:

```sh
dotnet nuget push artifacts/packages/example.connector.http.1.0.0.nupkg \
  --source https://api.nuget.org/v3/index.json --api-key <key>
```

The Plugins page shows only the Invoklet plugin feed by default. To list your feed's plugins
under **Available plugins**, users start Invoklet with `INVOKLET_PLUGIN_FEED` set to the feed's
`index.json` address; see [Use another feed](install-plugins.md#use-another-feed). The page lists a
package when:

- the feed finds it when searching for `invoklet`, which the `invoklet` tag ensures;
- its NuGet tags include `plugin`;
- its package ID is a valid plugin ID, the same as the manifest's `id`;
- it has at least one stable (not pre-release) version.

Invoklet does not send credentials to feeds, so a feed that needs a sign-in, such as GitHub
Packages, can't be browsed from the Plugins page. Users of such a feed download the package and
install it with **Install package…**.

### Propose it for the Invoklet plugin feed

The Invoklet plugin feed is built from the
[invoklet-plugins](https://github.com/tervforge/invoklet-plugins) repository. To have a plugin
listed there for every user, open an issue or pull request in that repository.

## 7. Release new versions

1. Change the plugin, then raise `version` in `invoklet-plugin.json` and in the descriptor. Keep
   the ID unchanged.
2. If the new version needs features of a newer Invoklet, set `minAppVersion` in the manifest to
   the first Invoklet release that has them, for example `"minAppVersion": "0.13.0"`. Users of older
   releases then keep getting the previous version from the feed.
3. Pack and publish the new package. Never replace a version that was already published: users
   who installed it would keep the old contents under the same version number.
4. Tell users how to update: remove the old version in **Preferences → Plugins**, install the new
   one and restart; see [Update a plugin](install-plugins.md#update-a-plugin).

If your plugin depends on another plugin, list that plugin's ID in `dependsOn` and tell users to
install it too: Invoklet does not install dependencies automatically.
