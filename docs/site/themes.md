# Themes

Open **Preferences → Appearance** to choose an interface palette and an editor theme for light
and dark mode. Editor themes color code in the editor, IL view, Markdown preview and Results.

To make your own palette and package it, follow [Create a theme](create-theme.md).

## Themes shipped with Invoklet

Invoklet Midnight and Invoklet Warm Light are included in the app and available on the first
launch. VS Code, Rider and VS 2022 are available from the separate
[Invoklet custom themes repository](https://github.com/tervforge/invoklet-custom-themes).
Each package contains a light and a dark palette.

If your saved palette is unavailable after updating Invoklet, **Preferences → Appearance**
shows which palette is missing and which embedded palette is active. The saved choice is
kept and is applied again after the package is installed and Invoklet restarts.

## Installing additional themes

A theme package contains an `invoklet-plugin.json` manifest and one or more JSON theme definitions.
It can also contain standalone VS Code TextMate themes.

1. Open **Preferences → Plugins**. Select **Install folder…** or **Install archive…** and choose
   the package. A ZIP or `.invplugin` archive must contain the manifest at its root. A NuGet
   `.nupkg` must contain the manifest and its theme files under `plugin/`.
2. Review the package ID, version, publisher and source, then select **Approve and install**.
3. Select **Restart now**, then choose the theme under **Preferences → Appearance**.

Use **Enable** or **Disable** to set whether a theme package loads on the next start.
**Remove** is available for user themes installed through Preferences. To replace an installed
theme with a different version, remove the installed version first.
**Content verified** means the loaded files matched the recorded hash at startup.

For local development, launch the desktop app with `--plugins-dev <directory>`.
Restart Invoklet after editing the package. Appearance lists themes from the current plugin session.

## Building the NuGet packages

From the root of the custom themes repository, build the three packages with the .NET 10 SDK.

```sh
dotnet pack ThemePackages.slnx -c Release -o artifacts
```

Each project under `themes/` packs its manifest and palettes. Keep the project version and
manifest version in sync when publishing a new version.

```json
{
  "id": "example.theme.ocean",
  "version": "1.0.0",
  "displayName": "Ocean",
  "roles": { "theme": 1 },
  "dependsOn": ["invoklet.theme.vscode"],
  "assets": { "themes": ["ocean.json"] }
}
```

The following `ocean.json` extends the dark VS Code palette. In `dependsOn`, use the package ID.
In `extends`, use the palette ID. To ship a light variant alongside it, add its JSON file
to the same `assets.themes` array and give it a distinct ID and `"base": "light"`.
The embedded Midnight and Warm Light themes can be
referenced directly with `invoklet.theme.midnight` or `invoklet.theme.warm-light`.

```json
{
  "id": "example.theme.ocean",
  "name": "Ocean",
  "base": "dark",
  "extends": "invoklet.theme.vscode-dark",
  "colors": {
    "Invoklet.Sidebar.BackgroundBrush": "#162B36",
    "Invoklet.Editor.BackgroundBrush": "#10222B"
  },
  "metrics": { "Invoklet.Radius.Control": 0 },
  "icons": { "Icon.Play": "M8,5 L19,12 L8,19 Z" },
  "editor": {
    "default": "ocean-code",
    "themes": { "ocean-code": "editor/ocean.json" }
  }
}
```

For example, `editor/ocean.json` can contain this TextMate rule:

```json
{
  "name": "Ocean Code",
  "tokenColors": [
    { "scope": "storage.type", "settings": { "foreground": "#4ED6AE" } }
  ]
}
```

| Field | Meaning |
| --- | --- |
| `id`, `name` | Unique stable identifier and the name displayed in Appearance. |
| `base` | `dark` or `light`. The closest declared base wins. A chain without a base uses dark. |
| `extends` | Optional parent theme ID. Missing parents and cycles produce diagnostics. |
| `colors` | Color strings such as `#10222B`. |
| `metrics` | Values from 0 to 512, with a minimum of 1 for font and control sizes. Thickness also accepts two components (horizontal, vertical) or four (left, top, right, bottom). Corner radii accept four components (top left, top right, bottom right, bottom left). |
| `icons` | SVG path data. |
| `editor` | Optional default editor theme ID and map of IDs to package-relative VS Code theme files. A file must contain its own `tokenColors` array. |

Values resolve from the selected theme through its parents, then through the built-in base
palette and shared metrics and icons. Unknown keys and invalid overrides appear in plugin
diagnostics. The remaining resources still apply. An explicit editor theme selected in Appearance
takes precedence over `editor.default`. A missing or unreadable editor theme falls back to the
built-in editor theme for the current mode.
When settings from an older release are migrated, the legacy Dark+ and Light+ names are
treated as automatic selections.

See the [interface theme key reference](theme-keys.md) for every supported resource.
