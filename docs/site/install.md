---
title: Installation and first run
description: Install Invoklet on Windows, macOS or Linux and set up the .NET SDK and netcoredbg.
---

# Installation and first run

Invoklet is a desktop application. To compile and run scripts it uses a .NET SDK installed
on your machine, and to debug them it uses the open-source
[netcoredbg](https://github.com/Samsung/netcoredbg) debugger. The first-run setup can
install both for you.

## Download

Download Invoklet from the [releases page](https://github.com/tervforge/invoklet/releases).
The newest release is at the top; pick the file for your system from its **Assets**
list:

=== "Windows"

    | File | What it is |
    | --- | --- |
    | `Invoklet-win-x64.msi` | Installer. Adds a Start Menu shortcut and an entry in *Add or remove programs*. A newer version installs over the old one. |
    | `Invoklet-win-x64.zip` | Portable build. Unzip it anywhere and run `Invoklet.exe`. Nothing is installed. |

=== "macOS"

    | File | What it is |
    | --- | --- |
    | `Invoklet-osx-arm64.dmg` | Disk image for Apple Silicon Macs (M1 and later). |
    | `Invoklet-osx-x64.dmg` | Disk image for Intel Macs. |

    Apple menu → **About This Mac** shows which kind of Mac you have.

=== "Linux"

    | File | What it is |
    | --- | --- |
    | `invoklet_<version>_amd64.deb` | Debian/Ubuntu package. Install it with `sudo dpkg -i invoklet_*.deb` or your software center. It puts `invoklet` on `PATH` and adds a menu entry. |
    | `Invoklet-x86_64.AppImage` | Single-file portable build. Make it executable (`chmod +x`) and run it. Nothing is installed system-wide. |
    | `Invoklet-linux-x64.tar.gz` | Archive. Unpack it and run `Invoklet`. |

## macOS

For architecture selection, first-launch approval, SDK setup and troubleshooting, see
[Installing Invoklet on macOS](install-macos.md).

1. Open the disk image for your Mac.
2. Drag `Invoklet.app` onto the `Applications` folder shown next to it.
3. Eject the disk image and start Invoklet from Applications.

The build is not yet signed or notarized with an Apple Developer certificate, so on the
first launch macOS may refuse to open it because it cannot verify the developer. To open
it anyway:

1. Open **System Settings → Privacy & Security**.
2. Click **Open Anyway** next to the message about Invoklet, then confirm.

macOS offers that button only for a while after a blocked launch. Depending on your
macOS version and security settings, it may not offer it at all. The disk image also
contains a `Read Me First.txt` with the same steps.

## Linux on Wayland

In a Wayland session, Invoklet runs natively on Wayland and draws its own window frames: rounded
corners, a shadow, a border in the theme's colors and the same title bar in every window. The
mouse pointer follows the desktop's cursor theme, and the interface uses the desktop's font.

If a window misbehaves, turn off **Preferences → Interface → System → Native Wayland window** and
restart Invoklet: it then runs through XWayland with the system's frame. When Invoklet can't start at
all, start it with `INVOKLET_WAYLAND=0` instead.

On Wayland only the desktop places windows. On KDE, **Remember window position and center
dialogs** in the same place adds two window rules to KDE's settings (System Settings → Window
Rules), so KDE puts the main window back where it was and opens dialogs in the middle of the
screen. Turning it off removes them.

## First run: .NET SDK and debugger

On slower launches, a startup screen stays visible while Invoklet checks the installed tools,
and then while it reopens the scripts from your last session (*Opening scripts…*), so the
window appears with all its tabs.
If startup fails, it shows the log location and a **Close** button.

When Invoklet starts, it checks two prerequisites:

- **.NET SDK** — Invoklet looks for `dotnet` on `PATH`, then in standard installation directories.
  The selected executable must report at least one SDK of version 6 or newer with `--list-sdks`.
  If you configure a specific executable, Invoklet uses that path.
- **netcoredbg** — found when a debugger path is configured and the file exists.

If both are present, the main window opens directly. If either is missing, a setup
window opens first. For each item it shows ✅ or ❌, a **Browse…** button to pick an
existing executable, and an **Install** button:

- **Install** for the .NET SDK downloads Microsoft's official `dotnet-install` script
  and installs the latest .NET 10 SDK for your user account: `%LOCALAPPDATA%\Microsoft\dotnet`
  on Windows, `~/.dotnet` on macOS and Linux.
- **Install** for netcoredbg downloads the latest release for your system from the
  [Samsung/netcoredbg](https://github.com/Samsung/netcoredbg/releases) GitHub releases
  and extracts it into Invoklet's data folder (see
  [Where Invoklet stores data](reference.md#where-invoklet-stores-data)).

Before anything is downloaded, a dialog names the version and the folder it goes to; **Cancel**
installs nothing.

Both installers need internet access. A long-running install can be cancelled. You can
also click **Continue to Invoklet** without installing anything and configure the paths
later in **Preferences → Languages → .NET** (fields *Dotnet path* and *Debugger path*).
With *Dotnet path* empty, Invoklet uses the `dotnet` on `PATH`, then the folder .NET was
installed to, then the usual install folders of your system, and `~/.dotnet` last.

!!! note "What works without each prerequisite"
    Without a .NET SDK, Invoklet cannot compile or run scripts. Without netcoredbg,
    normal runs work but debugging does not.

## Choosing the SDK a script runs with

A script runs on the .NET version its `targetSdk` names, and the SDK drop-down in the
toolbar above the editor shows and changes it:

- **latest (.NET 10.0.100)** — the newest installed stable SDK; the file keeps
  `targetSdk: latest`, so the script moves to a newer SDK when you install one.
- **.NET 8.0.400** and the other entries — the newest installed SDK of each .NET version
  from .NET 6 to .NET 10; picking one writes its version (`targetSdk: net8.0`) to the file.
  An SDK newer than .NET 10, a preview included, is not listed until Invoklet's C# compiler
  supports it.
- The list comes from the *Dotnet path* in **Preferences → Languages → .NET** (by default the
  `dotnet` on `PATH`): SDKs installed beside another `dotnet` don't appear.

If the version a script names isn't available, it runs on the newest SDK: the Results tab
starts with a warning, and the status bar shows *net6.0 (not available)*. The
[command-line runner](cli.md) follows the same rule. See [Front matter](scripts.md#front-matter).

## Updating and uninstalling

Invoklet does not update itself. To update, download the new version from the
[releases page](https://github.com/tervforge/invoklet/releases):

- **Windows installer**: install the new `.msi` over the old one. Uninstall from
  *Add or remove programs*.
- **Portable builds** (zip, AppImage, tar.gz): replace or delete the files.
- **Debian package**: install the new `.deb` the same way; remove it with your package
  manager.
- **macOS**: replace or delete `Invoklet.app` in Applications.

Your scripts and settings live outside the application folder, so updating or removing
Invoklet does not delete them. See [Where Invoklet stores data](reference.md#where-invoklet-stores-data).
