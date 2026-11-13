# Create a theme

A theme is a data plugin. One package can contain a light palette, a dark palette and optional
TextMate editor themes. Start with a folder named `ocean` containing these three files.

### `invoklet-plugin.json`

```json
{
  "id": "example.theme.ocean",
  "version": "1.0.0",
  "displayName": "Ocean",
  "roles": { "theme": 1 },
  "assets": { "themes": ["ocean-dark.json", "ocean-light.json"] }
}
```

### `ocean-dark.json`

```json
{
  "id": "example.theme.ocean-dark",
  "name": "Ocean Dark",
  "base": "dark",
  "extends": "invoklet.theme.midnight",
  "colors": { "Invoklet.Sidebar.BackgroundBrush": "#162B36" }
}
```

### `ocean-light.json`

```json
{
  "id": "example.theme.ocean-light",
  "name": "Ocean Light",
  "base": "light",
  "extends": "invoklet.theme.warm-light",
  "colors": { "Invoklet.Sidebar.BackgroundBrush": "#E3F1F4" }
}
```

Preferences uses the package ID to enable or disable both palettes. Appearance saves the
individual palette IDs. Use unique, stable IDs for your own package and palettes. If you
extend a palette from another package, put its palette ID in `extends` and its package ID
in the manifest's `dependsOn` array. The two embedded palettes in this example are always
available. See [Themes](themes.md) for TextMate assets and [Theme keys](theme-keys.md) for
the available colors, metrics and icons.

## Try it locally

Launch Invoklet with `--plugins-dev` and the path to the `ocean` folder. On Windows, run this
from the folder that contains `ocean`, substituting your executable path.

```powershell
& 'C:\path\to\Invoklet.exe' --plugins-dev (Resolve-Path ./ocean).Path
```

Check **Preferences → Plugins** for diagnostics and **Preferences → Appearance** for Ocean
Dark and Ocean Light. Restart Invoklet after editing a file.

## Build a NuGet package

With the .NET 10 SDK, save this as `Ocean.csproj` in the same folder.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <PackageId>example.theme.ocean</PackageId>
    <Version>1.0.0</Version>
    <Authors>Example</Authors>
    <Description>Ocean light and dark palettes for Invoklet.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
  </PropertyGroup>
  <ItemGroup>
    <None Update="invoklet-plugin.json;ocean-dark.json;ocean-light.json"
          Pack="true" PackagePath="plugin/" />
  </ItemGroup>
</Project>
```

```sh
dotnet pack Ocean.csproj -c Release -o artifacts
```

Keep the manifest and project versions in sync. Include any extra theme assets under
`plugin/` with the relative paths used by your palette files. For example, package
`editor/ocean.json` at `plugin/editor/ocean.json`.

Close the development session, launch Invoklet normally, and select the generated `.nupkg`
in **Preferences → Plugins → Install archive…**. Approve it and restart.
