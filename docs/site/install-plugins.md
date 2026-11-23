# Install plugins

Plugins add features to Invoklet without a new release of the app. There are three kinds:

- **connectors** add connection drivers, such as HTTP/OpenAPI, gRPC, SQLite, PostgreSQL or
  SQL Server, to [Connections](connections.md);
- **themes** add interface palettes and editor color themes to [Appearance](themes.md);
- **libraries** hold code that several connectors share. You never use a library directly: you
  install it because a connector needs it.

Every plugin is a NuGet package (`.nupkg`). You manage plugins under **Preferences → Plugins**,
and every change takes effect after Invoklet restarts. The command-line tool uses the same
plugins and settings as the desktop app.

The connectors are not included in Invoklet: install the ones you use from the plugin feed. If a
script uses a connection whose driver is not installed, the connection shows
*Driver '…' is not installed or is disabled.*

## Install from the plugin feed

The Invoklet plugin feed lists the connectors, themes and libraries published by the Invoklet
project. It is public and needs no account or sign-in.

1. Open **Preferences → Plugins** and select **Check the plugin feed**. The plugins of the feed
   appear under **Available plugins**. Plugins included with Invoklet and plugins you already
   installed are marked and can't be installed again.
2. Select **Install** next to a plugin.
3. Invoklet chooses a version and shows a confirmation dialog with the plugin's name, version,
   publisher and source. A plugin runs with the same permissions as Invoklet: approve only plugins
   you trust. Select **Approve and install**.
4. Install any plugins it depends on in the same way (see [Dependencies](#dependencies)).
5. Select **Restart now**.

Invoklet looks at the plugin's versions, newest first, and picks the newest one that works with
this Invoklet: the version's minimum Invoklet version is not newer than the running app, and its
plugin contract is supported. It downloads at most ten versions to find one and runs none of their
code. If no version fits, the page says why the newest one was skipped; update Invoklet to get it.

If the feed can't be reached, the page says so, and your installed plugins are not affected.

## Install a package file

To install a plugin that is not in the feed, for example one you downloaded from its author's
release page:

1. Open **Preferences → Plugins** and select **Install package…**.
2. Choose the `.nupkg` file.
3. Review the name, version, publisher and source, and select **Approve and install** if you trust
   the package.
4. Select **Restart now**.

A package is rejected before anything is installed if it has no `plugin/invoklet-plugin.json`
manifest, is not a theme, connector or library, uses the ID of a plugin included with Invoklet,
has more than 2048 files or 256 MB of content, or contains links.

## Dependencies

A plugin can depend on other plugins. For example, the database connectors share the EF Core
library plugin. Invoklet does not install dependencies for you: install them too,
before or after the plugin, and then restart. A plugin whose dependency is missing, disabled or
failed to load is not loaded, and its row on the Plugins page names the missing dependency.

## Update a plugin

To move an installed plugin to another version, remove it, then install the new version from the
feed or from a package file, and restart. You don't need to restart between the two steps.

A library can't be removed while other installed plugins depend on it. To update a library,
remove the plugins that depend on it first, then install them again after the library.

Plugins included with Invoklet are updated with Invoklet itself.

## Enable, disable or remove a plugin

Each row on the Plugins page has these buttons:

- **Disable** keeps the plugin installed but stops loading it. Disabling a plugin also disables the
  plugins that depend on it. To turn a dependent plugin back on, enable its dependencies first,
  then the plugin itself.
- **Enable** loads a disabled plugin again.
- **Remove** deletes a plugin you installed. Plugins included with Invoklet can be disabled but not
  removed.

**Remove** is refused while another installed plugin depends on the plugin, even if that plugin's
files changed since approval; the message names those plugins, so remove them first. It is also
refused while the manifest of another installed plugin can't be read, because its dependencies
are unknown; remove or repair that plugin first.

A disabled connector's driver becomes unavailable to scripts after the restart. If you disable or
remove the theme you are using, **Preferences → Appearance** keeps your choice and uses a built-in
palette until the theme is installed again.

## Plugin states

| State | Meaning |
| --- | --- |
| Loaded | The plugin loaded at this start. **Content verified** means its files matched the approved hash. |
| Rejected | The plugin could not load. Its row lists the reason without file paths or secrets; other plugins still load. |
| Disabled | The plugin is installed but turned off. |
| Content changed | The installed files no longer match the approved hash, so the plugin is not loaded. For a plugin installed through Preferences, remove it and install the package again to approve the new contents. For a plugin included with Invoklet, reinstall Invoklet. |
| Pending restart | An install, removal, **Enable** or **Disable** takes effect at the next start. **Restart now** appears while such a change is waiting. |

**Restart now** closes Invoklet through its usual exit, which asks about unsaved tabs. If you
cancel, Invoklet keeps running and nothing is restarted. Otherwise it starts again with the same
command-line arguments.

## Safety

- No code from a package runs while it is listed, downloaded or reviewed. It runs only after you
  approve it and restart.
- Approval records a hash of the package contents. At every start Invoklet checks the installed
  files against that hash, and does not load a plugin whose files changed.
- A connector or library runs with the same permissions as Invoklet and can read what Invoklet can
  read. Install them only from publishers you trust. A theme contains no code.

## Use another feed

To list plugins from another NuGet v3 feed instead of the Invoklet feed, such as a company feed or
a local test feed, start Invoklet with the environment variable `INVOKLET_PLUGIN_FEED` set to the
feed's `index.json` address:

```sh
INVOKLET_PLUGIN_FEED=https://example.com/invoklet-plugins/v3/index.json Invoklet
```

The variable replaces the Invoklet feed while it is set. Invoklet does not send credentials to
plugin feeds, so a feed that needs a sign-in, such as GitHub Packages, is reported as refusing
access. Download the package file from such a feed yourself and use **Install package…**.

To write a plugin of your own, see [Create and publish a plugin](publish-plugin.md).
