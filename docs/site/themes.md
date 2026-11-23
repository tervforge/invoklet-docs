# Themes

Open **Preferences → Appearance** to choose an interface palette and an editor theme for light
and dark mode. Editor themes color code in the editor, IL view, Markdown preview and Results.

To make your own palette and package it, follow [Create a theme](create-theme.md).

## Themes shipped with Invoklet

Invoklet Midnight and Invoklet Warm Light are included in the app and available on the first
launch. VS Code, Rider and VS 2022 are theme packages in the
[Invoklet plugins repository](https://github.com/tervforge/invoklet-plugins), next to the connectors.
Each package contains a light and a dark palette.

If your saved palette is unavailable after updating Invoklet, **Preferences → Appearance**
shows which palette is missing and which embedded palette is active. The saved choice is
kept and is applied again after the package is installed and Invoklet restarts.

## Installing additional themes

A theme package contains an `invoklet-plugin.json` manifest and one or more JSON theme definitions.
It can also contain standalone VS Code TextMate themes.

Themes from the Invoklet plugin feed, such as VS Code, Rider and VS 2022, are listed under
**Preferences → Plugins → Available plugins**; see
[Install from the plugin feed](install-plugins.md#install-from-the-plugin-feed). To install a
package file instead:

1. Open **Preferences → Plugins**, select **Install package…** and choose the NuGet package
   (`.nupkg`). It must contain the manifest and its theme files under `plugin/`; see
   [Create a theme](create-theme.md#build-a-nuget-package).
2. Review the package ID, version, publisher and source, then select **Approve and install**.
3. Select **Restart now**, then choose the theme under **Preferences → Appearance**.

Use **Enable** or **Disable** to set whether a theme package loads on the next start.
**Remove** is available for user themes installed through Preferences. To replace an installed
theme with a different version, remove the installed version first.
**Content verified** means the loaded files matched the recorded hash at startup. The other
states on the Plugins page, and how removal handles dependent packages, are described in
[Plugin states](install-plugins.md#plugin-states).

For local development, launch the desktop app with `--plugins-dev <directory>`.
Restart Invoklet after editing the package. Appearance lists themes from the current plugin session.

## Building the NuGet packages

From the root of the custom themes repository, build the three packages with the .NET 10 SDK.

```sh
dotnet pack ThemePackages.slnx -c Release -o artifacts
```

Each project under `themes/` packs its manifest and palettes. Keep the project version and
manifest version in sync when publishing a new version.

## Theme file format

A theme package is a folder with `invoklet-plugin.json` at its root. A theme file can have any
name: the manifest lists every theme file, relative to the package folder, in `assets.themes`.
Each file describes one palette. This manifest declares one palette that extends a palette
from the VS Code package:

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

The following `ocean.json` extends the dark VS Code palette. In `dependsOn`, use the plugin ID.
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
| `id`, `name` | Required. A stable ID in the plugin ID format (lower-case segments joined by dots, such as `example.theme.ocean-dark`) and the name displayed in Appearance. The ID must not repeat a built-in palette or a palette from another package. |
| `base` | `dark` or `light`. The closest declared base wins. A chain without a base uses dark. |
| `extends` | Optional parent theme ID. Missing parents and cycles produce diagnostics. |
| `colors` | Color strings such as `#10222B`. |
| `metrics` | Values from 0 to 512, with a minimum of 1 for font and control sizes. Thickness also accepts two components (horizontal, vertical) or four (left, top, right, bottom). Corner radii accept four components (top left, top right, bottom right, bottom left). |
| `icons` | SVG path data. |
| `editor` | Optional default editor theme ID and map of IDs to package-relative VS Code theme files. A file must contain its own `tokenColors` array. |

Each key belongs in one section by its type, as listed in the [key reference](theme-keys.md):

| Type | Section | Value |
| --- | --- | --- |
| `color` | `colors` | A color string such as `#10222B`, `#8010222B` or a named color. |
| `double` | `metrics` | A number. |
| `thickness` | `metrics` | One number, or an array of two or four numbers. |
| `cornerRadius` | `metrics` | One number, or an array of four numbers. |
| `geometry` | `icons` | SVG path data. |

Values resolve in this order, and a later step overrides an earlier one:

1. the shared metrics and icons;
2. the built-in Midnight palette for a dark base or Warm Light for a light base;
3. the theme's ancestors, starting with the most distant `extends` parent;
4. the theme itself.

An alias key, such as `ControlCornerRadius`, sets its canonical key. When a theme sets both, the
canonical key wins. Only keys from the key reference can be set. Other resources of the
application cannot be overridden.

A theme file that is not valid JSON, lacks `id` or `name`, has an invalid `base` or `extends`,
has an ID that is not in the plugin ID format or is already used, extends a missing parent, forms
an inheritance cycle, or extends a palette from another package that is not listed in the
manifest's `dependsOn` rejects the whole package, and the Plugins page shows the reason. An unknown key, a key in the wrong section or an
invalid value is reported as a warning on the Plugins page and skipped. Numbers must be between
0 and 512, and the `Invoklet.Type.*` and `Invoklet.Size.*` keys must be at least 1. The remaining resources
still apply.

An explicit editor theme selected in Appearance takes precedence over `editor.default`. A
missing or unreadable editor theme falls back to the built-in editor theme for the current mode.
When settings from an older release are migrated, the legacy Dark+ and Light+ names are
treated as automatic selections.

An editor theme file is a standalone VS Code color theme: a JSON object with a `tokenColors`
array whose rules each have a `settings` object. Comments and trailing commas are allowed. An
optional `"type"` of `dark`, `light`, `hcDark` or `hcLight` marks it as dark or light;
otherwise it follows the palette's base. Files that use `include` are not supported. A file that
cannot be read is skipped with a warning on the Plugins page, and the palette still loads.
Appearance stores the editor theme as `<palette ID>/<editor theme ID>`, for example
`example.theme.ocean/ocean-code`. An editor theme that a palette inherits from its parent keeps
the ID of the palette that declares it.

Installed themes are read once at startup. Installing, updating, enabling, disabling or
removing a theme package takes effect after restarting Invoklet.

See the [interface theme key reference](theme-keys.md) for every supported resource.
