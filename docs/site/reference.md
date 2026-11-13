---
title: Settings, shortcuts and troubleshooting
description: Invoklet Preferences, keyboard shortcuts, data locations and solutions to common problems.
---

# Settings, shortcuts and troubleshooting

## Menu

The menu is the same on every system: at the top of the window on Windows and Linux, in the menu
bar on macOS. Every toolbar button and shortcut has a menu item too, and each item shows its
shortcut.

| Menu | Commands |
| --- | --- |
| **File** | New File, New Invoklet File, Open..., Quick Open..., Recent Files, Open Text File..., Import .linq Query..., Save, Save As..., Export Results as HTML..., Revert to Saved, Close Tab, Exit (++cmd+q++ on macOS) |
| **Edit** | Undo, Redo, Cut, Copy, Paste, Select All, Format Document, Format Selection, Go to Line..., Resolve All Usings |
| **View** | Toggle File Tree, Toggle Output Panel, Markdown Preview, Markdown Preview to the Side, Zoom In, Zoom Out, Reset Zoom |
| **Run** | Run / Continue, Stop, Execute Selection, Clear Results, Stop and Run Again, Debug Mode, Step Over, Step Into, Step Out, Pause, Run to Cursor |
| **Tools** | Dependencies..., Assembly Explorer..., Connections..., Secret Store..., Preferences... |
| **Help** | User Guide, Release Notes, Report an Issue, About Invoklet |

Commands that work on a script, such as Run or Zoom In, act on the active tab. On macOS the
menu bar holds only Invoklet's own commands: About Invoklet is under **Help**, and **File → Exit**
quits, from any Invoklet window.

## Preferences

Open **Tools → Preferences...**. Settings are grouped in a tree on the left; type in
*Search settings...* to find a page or a setting by its label, description, options, or value.
Invoklet opens the first matching page and highlights the matches. Changes apply immediately.

<figure markdown="span">
  [![The Interface page of Preferences: interface scale, appearance and the interface and editor themes](assets/screenshots/preferences_light.png#only-light)](assets/screenshots/preferences_light.png "Open full-size screenshot")
  [![The Interface page of Preferences: interface scale, appearance and the interface and editor themes](assets/screenshots/preferences.png#only-dark)](assets/screenshots/preferences.png "Open full-size screenshot")
</figure>

| Page | Settings |
| --- | --- |
| **Interface** | *Interface scale*, 90% to 200%, with *Reset to 100%* (see [Interface scale and shared code zoom](editor.md#interface-scale-and-shared-code-zoom)). *Appearance*: System, Dark or Light. *Interface theme* and *Editor theme* for the current appearance (defaults: *VS Code Dark* / *VS Code Light* and *Dark+* / *Light+*). |
| **Interface → System** | What Invoklet does differently on this system. On Linux: *Native Wayland window* (on by default; see [Linux on Wayland](install.md#linux-on-wayland)) and, on KDE, *Remember window position and center dialogs*. |
| **Editor** | *Restore open tabs and unsaved changes* (on by default; see [Recovering unsaved tabs](editor.md#recovering-unsaved-tabs)). Font family, font size and *Code zoom*; show line numbers, code folding, tabs, spaces, end of line, column ruler (column 80); word wrap; *Accept completion when typing punctuation*; *New files* (File → New File): language and kind. |
| **Keyboard** | Every key binding, grouped by area, with Reset and conflict warnings; see [Keyboard shortcuts](#keyboard-shortcuts). |
| **Languages → .NET** | *Dotnet path* (auto-detected from `PATH` and standard installation directories) and *Debugger path* (the `netcoredbg` executable). Shared by C# and Visual Basic. |
| **Languages → .NET → C#** | *Inlay hints*; *Code fixes and refactorings (lightbulb)*; *Excluded diagnostic IDs*. See [Diagnostics and analyzers](analyzers.md#settings). |
| **Plugins** | Lists loaded and rejected plugins with verification status and diagnostics. Install, enable, disable, or remove [theme packages](themes.md). *Refresh* checks the inventory for changes. *Restart now* applies pending changes after the usual confirmation for unsaved work. |
| **Logging** | What Invoklet writes about its own work to the `logs` folder and the console: *Off* (nothing, and no log file), *Fatal errors only* (crashes of Invoklet itself and a start that failed; the default), *Errors*, *Warnings*, *Information* or *Debug* (everything, including parts of your scripts). Takes effect at once. |
| **Experimental** | *Invoklet analyzers* (`RPAD…` warnings in C# scripts), off by default. |
| **Languages → .NET → Visual Basic** | No settings yet. |

## Keyboard shortcuts

Every shortcut can be changed in **Tools → Preferences... → Keyboard**. The page lists each
command with its key; type a new combination such as `Ctrl+Shift+R`, `Cmd+,` or `F6`, or clear
the field to remove the key. **Reset** brings back the default, and a warning appears under a
key that another command already uses. Only the keys you change are stored in `settings.json`.

The defaults follow common IDE conventions: ++f5++ runs, ++shift+f5++ stops, ++f10++ and ++f11++
step. On macOS, the Command key (++cmd++) takes the place of Control
where that is the Mac convention.

### General

| Command | Windows and Linux | macOS |
| --- | --- | --- |
| New File | ++ctrl+n++ | ++cmd+n++ |
| Open Script | ++ctrl+o++ | ++cmd+o++ |
| Quick Open | ++ctrl+comma++ | ++ctrl+comma++ |
| Save | ++ctrl+s++ | ++cmd+s++ |
| Save As | ++ctrl+shift+s++ | ++cmd+shift+s++ |
| Close Tab | ++ctrl+w++ | ++cmd+w++ |
| Exit | — (++alt+f4++ closes the window) | ++cmd+q++ |
| Dependencies | ++f4++ | ++f4++ |
| Preferences | — | ++cmd+comma++ |
| Toggle Output Panel | ++ctrl+r++ | ++cmd+r++ |
| Toggle Markdown Preview | ++ctrl+shift+v++ | ++cmd+shift+v++ |
| Toggle Markdown Preview to the Side | ++ctrl+alt+v++ | ++cmd+alt+v++ |
| Revert to Saved, Assembly Explorer, Toggle File Tree, Zoom In, Zoom Out, Reset Zoom | — | — |

### Run and debug

| Command | Windows and Linux | macOS |
| --- | --- | --- |
| Run / Continue | ++f5++ | ++f5++ |
| Stop | ++shift+f5++ | ++shift+f5++ |
| Execute Selection (Statements, Program) | ++ctrl+shift+e++ | ++cmd+shift+e++ |
| Stop and Run Again | ++alt+shift+x++ | ++cmd+shift+f5++ |
| Step Over | ++f10++ | ++f10++ |
| Step Into | ++f11++ | ++f11++ |
| Step Out | ++shift+f11++ | ++shift+f11++ |
| Run to Cursor | ++ctrl+f10++ | ++cmd+f10++ |
| Debug Mode, Pause | — | — |

On macOS, function keys may need the ++fn++ key, depending on your keyboard settings.

### Editor

| Command | Windows and Linux | macOS |
| --- | --- | --- |
| Find | ++ctrl+f++ | ++cmd+f++ |
| Replace | ++ctrl+h++ | ++cmd+alt+f++ |
| Find Next / Previous | ++f3++ / ++shift+f3++ | ++f3++ / ++shift+f3++ |
| Replace Next / All | ++alt+r++ / ++alt+a++ | ++alt+r++ / ++alt+a++ |
| Show Completion | ++ctrl+space++ | ++ctrl+space++ |
| Signature Help | ++ctrl+shift+space++ | ++cmd+shift+space++ |
| Go to Definition / Implementation | ++f12++ | ++f12++ |
| Find References | ++shift+f12++ | ++shift+f12++ |
| Rename Symbol | ++f2++ | ++f2++ |
| Jump to Matching Bracket | ++ctrl+bracket-right++ | ++ctrl+bracket-right++ |
| Toggle Line Comment | ++ctrl+slash++ | ++cmd+slash++ |
| Format Document | ++shift+alt+f++ | ++shift+alt+f++ |
| Format Selection | ++ctrl+shift+f++ | ++cmd+shift+f++ |
| Go to Line | ++ctrl+g++ | ++ctrl+g++ |
| Resolve All Usings | ++ctrl+alt+u++ | ++cmd+alt+u++ |

If the script changes while Rename Symbol is in progress, Invoklet cancels the rename. Press ++f2++ again on the updated code.

### File tree

| Command | Windows and Linux | macOS |
| --- | --- | --- |
| Rename | ++f2++ | ++enter++ |
| Delete | ++del++ | ++cmd+backspace++ |

### Assembly Explorer

| Command | Windows and Linux | macOS |
| --- | --- | --- |
| Back / Forward | ++alt+left++ / ++alt+right++ | ++cmd+bracket-left++ / ++cmd+bracket-right++ |
| Search Types and Members | ++ctrl+shift+f++ | ++cmd+shift+f++ |
| Analyze | ++ctrl+r++ | ++cmd+r++ |
| Remove Assembly | ++del++ | ++cmd+backspace++ |
| Next / Previous Tab | ++ctrl+tab++ / ++ctrl+shift+tab++ | ++ctrl+tab++ / ++ctrl+shift+tab++ |

Save and Close Tab use the General keys above.

### Keys that follow the system

These are not listed in Preferences: text editing uses the platform's own keys (Undo, Redo,
Cut, Copy, Paste, Select All — with ++cmd++ on macOS), ++tab++ expands a snippet, and holding
++ctrl++ (++cmd++ on macOS) while turning the mouse wheel zooms the editor or while clicking a
name in the Assembly Explorer goes to its definition.

## Where Invoklet stores data

| What | Location |
| --- | --- |
| Your scripts | `Invoklet Queries` in your Documents folder |
| Settings, including Recent Files | `settings.json` in Invoklet's data folder |
| Drafts of open tabs and unsaved changes | `recovery` folder in Invoklet's data folder |
| NuGet sources | `NuGet.Config` in Invoklet's data folder |
| Your connections from **Tools → Connections** | `connections.json` in Invoklet's data folder |
| Secrets from **Tools → Secret Store** | `secrets.json` in Invoklet's data folder (`INVOKLET_SECRETS_FILE` points elsewhere) |
| Debugger installed by the setup window | `netcoredbg` folder in Invoklet's data folder |
| Files written by `Util.Cache` and `Util.SaveString`/`SaveBytes` | `UserData` folder in Invoklet's data folder |
| Logs | `logs` folder in Invoklet's data folder |
| Build output | `Invoklet` folder in the system temporary folder. A saved script keeps its build folder between runs; folders unused for 14 days are removed when Invoklet starts. |
| Build output of the `invoklet` command | `cli-builds` folder in Invoklet's data folder, one folder for each script; folders unused for 14 days are removed. |

Invoklet's data folder is:

| System | Folder |
| --- | --- |
| Windows | `%APPDATA%\Invoklet` |
| macOS | `~/Library/Application Support/Invoklet` |
| Linux | `~/.config/Invoklet` (or `$XDG_CONFIG_HOME/Invoklet`) |

## Troubleshooting

### No .NET SDK found

Symptoms: the setup window shows ❌ next to *.NET SDK*, the SDK drop-down in the toolbar
is empty and Run does nothing, or the command-line runner prints
`No .NET SDK found (checked dotnet --list-sdks).`

- Run `dotnet --list-sdks` in a terminal. Invoklet needs at least one SDK of version 6 or
  newer.
- If Invoklet cannot find the SDK in its standard locations, or you installed it in a custom
  folder, set the full path to the executable in
  **Preferences → Languages → .NET → Dotnet path**, for example
  `/usr/local/share/dotnet/dotnet` on macOS or `C:\Program Files\dotnet\dotnet.exe` on
  Windows.
- Or restart Invoklet and use **Install** in the setup window.

### The debugger does not start or breakpoints are ignored

See [Debugging → Troubleshooting](debugging.md#troubleshooting).

### "Invalid document"

The file is not a valid `.inv` document: the front matter is missing or not closed, the
YAML is invalid, a `kind` or `language` value is not supported, or there is no run block
for the declared language. The Results tab explains which. See
[Scripts and .inv files](scripts.md#front-matter).

### A package cannot be restored

Check the package ID and version, and that a source in
[Package sources](dependencies.md#package-sources) has the package and is reachable.
Restore warnings and errors are printed in the Results tab.

### `Project references are no longer supported`

Reference the project's built `.dll` instead. See
[Project references are not supported](dependencies.md#project-references-are-not-supported).

### macOS refuses to open Invoklet

The package is ad-hoc signed but has no Apple Developer ID certificate or notarization.
See [Installing Invoklet on macOS](install-macos.md#3-allow-the-first-launch).

### No completion or underlines

These features exist only for C#. Check the `language` of the script; Visual Basic has
no live analysis yet. For C#, give the background analysis a moment after typing, and
check that the file is a valid document.
