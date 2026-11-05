---
title: Installing Invoklet on macOS
description: Download Invoklet for Apple Silicon or Intel, install it, allow the first launch, and configure the SDK and debugger.
---

# Installing Invoklet on macOS

This guide takes you from downloading Invoklet to running your first C# script.
The published macOS package includes the runtime needed to open Invoklet. To compile
and run your scripts, you also need a **.NET SDK**. Debugging additionally needs
**netcoredbg**. Invoklet's first-run setup can install both.

## 1. Choose the download for your Mac

Open **Apple menu → About This Mac** and look for **Chip** or **Processor**.

| Your Mac | Invoklet download | Architecture for the .NET SDK |
| --- | --- | --- |
| Apple Silicon, such as an M1, M2, M3 or later chip | `Invoklet-osx-arm64.dmg` | Arm64 |
| Intel processor | `Invoklet-osx-x64.dmg` | x64 |

Open the official [Invoklet releases page](https://github.com/tervforge/invoklet/releases),
choose the latest stable release, expand **Assets**, and download the matching `.dmg`.
Read that release's notes for known installation issues and compatibility information.
Keep Invoklet, your SDK and debugger on the same architecture.

For .NET's supported macOS versions, see
[Microsoft's macOS installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/macos).
The availability of a DMG for your processor does not by itself mean an older macOS
version is supported by the SDK you choose.

## 2. Copy Invoklet to Applications

1. In Finder, open **Downloads** and double-click the downloaded `.dmg`.
2. In the disk-image window, read `Read Me First.txt` and `License Agreement.txt`.
3. Drag **Invoklet.app** onto the **Applications** folder beside it. Wait for the copy
   to finish. If Finder requests permission to copy there, follow its prompt.
4. Eject the **Invoklet** disk image using the eject button in Finder's sidebar.
5. Open **Finder → Applications** and double-click **Invoklet**.

Launch the installed copy from Applications. Running directly from the mounted disk
image can leave you using a temporary copy that disappears when the image is ejected.
After installation, you can drag Invoklet from Applications to the Dock for easier access.

## 3. Allow the first launch

Invoklet's macOS package currently has an ad-hoc signature, without an Apple Developer ID
certificate or Apple notarization. macOS may block its first launch because it cannot
verify the developer or check the app for malicious software.

If you trust the copy downloaded from Invoklet's official releases:

1. Try opening **Applications → Invoklet**, then dismiss the blocked-launch message.
2. Open **Apple menu → System Settings → Privacy & Security**.
3. Scroll to **Security** and find the message about **Invoklet**.
4. Click **Open Anyway**, authenticate if prompted, and confirm **Open**.

This creates an exception for that app; subsequent launches normally work directly.
If the button is missing, try opening the installed copy again and return to settings.
Managed Macs may require help from your administrator. See
[Apple's explanation of first-launch alerts](https://support.apple.com/en-us/102445).

A warning about a **damaged app** or an app that **will damage your computer** is different
from an unidentified-developer alert. Follow the troubleshooting section below.

## 4. Set up the SDK and debugger

When Invoklet opens, it checks for a .NET SDK and a configured debugger. Missing components
are shown in the setup window. If both are already configured, the main window opens directly.

### Install through Invoklet

1. Click **Install** beside **.NET SDK**. Invoklet downloads Microsoft's installation
   script and installs the .NET 10 SDK into `~/.dotnet` for your account.
2. Wait for the operation to finish and the SDK check to succeed.
3. To use breakpoints and step through code, click **Install** beside **netcoredbg**.
   Invoklet downloads the matching debugger from Samsung's GitHub releases into its data folder.
4. Click **Continue to Invoklet** when setup is complete.

Both downloads need internet access. You can cancel an installation or continue setup
later. Without the SDK, scripts cannot compile or run. Without the debugger, ordinary
script execution still works.

### Use an existing installation

Use **Browse…** in setup to select the existing executable, or open
**Tools → Preferences... → Languages → .NET** later. On macOS you can also open Preferences
with ++cmd+comma++ using the default key binding.

| Field | What to select |
| --- | --- |
| **Dotnet path** | The `dotnet` executable, not the SDK folder or an installer package |
| **Debugger path** | The `netcoredbg` executable, not its archive or containing folder |

For the SDK installed through Invoklet, the executable is `~/.dotnet/dotnet`.
In a Finder file picker, use **Go → Go to Folder…** (++cmd+shift+g++), enter `~/.dotnet`,
and select `dotnet`. For a text field, use the full absolute path to that executable.

If you prefer Microsoft's installer, follow its
[macOS SDK installation instructions](https://learn.microsoft.com/en-us/dotnet/core/install/macos).
Choose the **SDK**, with **Arm64** for Apple Silicon or **x64** for Intel; installing
only a runtime does not provide the compiler SDK required by Invoklet.

Finder and Dock launches may not inherit the same `PATH` as Terminal. If `dotnet` works
in Terminal but Invoklet cannot find it, set **Dotnet path** explicitly. To identify an
existing installation in Terminal, run:

```bash
command -v dotnet
dotnet --list-sdks
```

For an installation made through Invoklet, you can check it without relying on `PATH`:

```bash
"$HOME/.dotnet/dotnet" --list-sdks
```

The output should contain at least one SDK version and its installation folder. An empty
list means no SDK was found at that location. See the general
[first-run setup guide](install.md#first-run-net-sdk-and-debugger) for detection details.

## 5. Check that scripts run

1. Choose **File → New File** or press ++cmd+n++.
2. Choose **C#** and **Statements** if your new-file defaults are different.
3. Enter:

   ```csharp
   Console.WriteLine("Hello from Invoklet on macOS!");
   new[] { 1, 2, 3 }.Dump();
   ```

4. Select an installed SDK in the toolbar and click **Run**.
5. Check that the **Results** tab shows the greeting and the dumped array, with a
   successful execution status.
6. Press ++cmd+s++ and save the script in **Documents → Invoklet Queries** so it appears
   in Invoklet's file tree.

To test debugging, set a breakpoint beside an executable line and follow the
[debugging guide](debugging.md). If function keys control brightness or media instead,
use the toolbar buttons or hold **Fn/Globe** while pressing the function key.

Continue with [Quick start](quick-start.md) and
[Keyboard shortcuts](reference.md#keyboard-shortcuts).

## Updating and uninstalling

To update, quit Invoklet, download the new matching DMG and repeat the copy to Applications.
When Finder asks, replace the existing app. Eject the image and launch the installed
copy. A new downloaded copy may require first-launch approval again.

To uninstall, quit Invoklet and move **Invoklet.app** from Applications to the Trash.
Removing the app does not remove your scripts, settings or separately installed SDK.

| Data | Default location |
| --- | --- |
| Scripts | `~/Documents/Invoklet Queries` |
| Settings, logs and installed debugger | `~/Library/Application Support/Invoklet` |
| SDK installed through Invoklet | `~/.dotnet` |

Use **Finder → Go → Go to Folder…** to visit these locations. Back up scripts before
deleting them. Other development tools may also use the SDK; retain it if needed.
See [Where Invoklet stores data](reference.md#where-invoklet-stores-data) for details.

## Troubleshooting

### The DMG cannot be opened or the app is reported as damaged

Download a fresh copy of the matching DMG from the official release, then copy the app
again. Do not treat a damaged-app or malware alert as the normal developer-verification
prompt. If it persists, report the exact alert and release version to the project.

### Invoklet closes immediately or never shows a window

Confirm you are opening the installed copy and have selected the correct architecture.
Try the latest release. If the app still fails after first-launch approval, open Terminal
and run the installed executable to capture its output:

```bash
/Applications/Invoklet.app/Contents/MacOS/Invoklet
```

This starts Invoklet; it does not change macOS security settings. Adjust the path if you
installed the app elsewhere. Errors mentioning `libhostpolicy`, `libSkiaSharp` or a
missing library can indicate a packaging problem; include that output in a report.

### The main window opens, but scripts do not run

Check that the SDK list in the toolbar is populated. Confirm **Dotnet path** points to
an SDK installation and run the `--list-sdks` check above. Review the error in **Results**.
If compilation succeeds but a script requests a missing runtime, inspect the installed
runtimes with the same executable's `--list-runtimes` option.

### Running works, but debugging does not

Check **Debugger path**, ensure it selects `netcoredbg` for your Mac's architecture,
and read [Debugging → Troubleshooting](debugging.md#troubleshooting). A working SDK
alone does not provide the debugger.

### An installation download fails

Check your connection and any company network restrictions. SDK installation downloads
from Microsoft; debugger installation downloads from GitHub. Use the manual SDK option
or **Browse…** for components you already have, then retry the affected operation.

### What to include in a bug report

Use [Invoklet's issue tracker](https://github.com/tervforge/invoklet/issues) and include the
Invoklet release, macOS version, Apple Silicon or Intel, the exact error and whether it
happens before the main window opens or only when running/debugging a script. Logs are
under `~/Library/Application Support/Invoklet/logs` when available. Review logs and terminal
output before posting: they may contain local paths or script content.
