---
title: Scripts and .inv files
description: The .inv file format, front matter fields and the Statements, Program and Expression kinds.
---

# Scripts and .inv files

## Opening scripts

Use **File → Open…** (**Ctrl+O**, or **Cmd+O** on macOS) to open a `.inv` file from
any local folder. It stays in that folder; opening it does not copy it into Queries or run it.
Opening an already-open path activates its existing tab and preserves unsaved changes.

**File → Recent Files** remembers the last 50 script paths, newest first, across restarts.
The full path distinguishes scripts with the same name. **Clear Recent Files** clears this history
without deleting files or closing tabs. If a file was moved or deleted, opening its old recent
entry reports an error; use Open to choose its new location.

**File → Quick Open…** (**Ctrl+,**, also on macOS; the key can be changed in Preferences → Keyboard) opens a script by name
without leaving the keyboard. Type part of the name, or the first letters of its words (`mfr` finds `MyFirstReport` and
`my_first_report`); the list narrows as you type and shows the folder of each script and when it was last changed, with the
recent scripts first. Use **Up** and **Down** to choose and **Enter** (or **Open**) to open; **Esc** closes the box.

- **My scripts** and **Samples** choose what is searched: the scripts folder and the recent scripts from anywhere else, and
  the sample scripts bundled with Invoklet (a sample opens as an untitled copy).
- **Search inside** also finds the scripts whose text contains what you typed.
- **Include other file types** adds the other files of the scripts folder; they open in the text editor.

## File format

An Invoklet script is a plain-text `.inv` file. It is a Markdown document with:

1. a YAML **front matter** block between two `---` lines at the very top;
2. exactly one **run block**: a fenced code block whose info string is `csharp run` or
   `vb run`, matching the script's language;
3. any other Markdown you like, before or after the run block.


````markdown
---
version: "1.0"
language: csharp
targetSdk: latest
kind: Statements
---

# Monthly totals

Notes about what this script does.

```csharp run
var total = new[] { 120, 80, 45 }.Sum();
total.Dump("Total");
```

More notes after the code.
````

The file is Markdown-compatible and diffs cleanly in version control. You can read it in
[Obsidian](obsidian.md) or use [GitHub Markdown preview](index.md#markdown-compatible-files)
without creating a separate `.md` copy. The overview explains the file-type hint needed
for GitHub rendering.

## How Invoklet edits the file

The editor shows only the code inside the run block. Invoklet keeps the Markdown before
and after the block exactly as it was and writes it back unchanged when it saves. The
front matter is changed through the UI: the **kind** drop-down in the toolbar, the
[Dependencies window](dependencies.md) and code fixes that add a namespace. To edit the
Markdown text around the code, use another text editor — for example Obsidian with the
Invoklet Markdown plugin; see [View your scripts in Obsidian](obsidian.md).

When Invoklet saves a script, it writes the front matter again from the fields it knows,
in a fixed order. YAML comments and unknown fields in the header are not kept.

## Markdown preview

To see the whole script as a document, as VS Code shows a Markdown file, open the preview:

- **View → Markdown Preview** (++ctrl+shift+v++, on macOS ++cmd+shift+v++) shows the preview in
  place of the editor; the same key brings the editor back.
- **View → Markdown Preview to the Side** (++ctrl+alt+v++, on macOS ++cmd+alt+v++) shows it beside
  the editor; drag the line between them to resize. The same key closes it.
- The **preview button** on the tab's toolbar, next to the debug-mode button, does the same as
  Markdown Preview to the Side, and is lit while any preview is shown; clicking it then hides the
  preview.

The preview shows the front matter as a table of settings, then the Markdown around the
code, with the code highlighted in the editor's colors and font. It follows what you type
after a short pause, and the kind, SDK and dependencies as soon as they change. The preview is
read-only. Whether it is shown, and where the dividers between the editor, the preview and the
output are, is the same for every tab, including tabs opened later.

Side by side, the editor and the preview scroll together: scroll either one and the other
follows to the same line. When you leave the preview shown in place of the editor, the editor
opens where you stopped reading.

<figure markdown="span">
  [![The products-overview example with the editor on the left and its Markdown preview on the right: the header as a table of settings, the title, the notes and the highlighted code](assets/screenshots/markdown-preview_light.png#only-light)](assets/screenshots/markdown-preview_light.png "Open full-size screenshot")
  [![The products-overview example with the editor on the left and its Markdown preview on the right: the header as a table of settings, the title, the notes and the highlighted code](assets/screenshots/markdown-preview.png#only-dark)](assets/screenshots/markdown-preview.png "Open full-size screenshot")
</figure>

## Front matter

All fields are optional. A missing or empty value falls back to the default.

| Field | Default | Meaning |
| --- | --- | --- |
| `version` | `"1.0"` | Version of the file format. |
| `language` | `csharp` | `csharp` or `vb`. Must match the run block's info string. |
| `targetSdk` | `latest` | The .NET version the script runs on: `latest` for the newest installed stable SDK, or a framework moniker such as `net8.0`. The toolbar's SDK drop-down shows and changes it, and the [command-line runner](cli.md) follows it too. If that version isn't installed, the script runs on the newest SDK with a warning. See [Choosing the SDK](install.md#choosing-the-sdk-a-script-runs-with). |
| `kind` | `Statements` | `Statements`, `Program` or `Expression`. See [Kinds](#kinds). |
| `packages` | none | NuGet packages as `PackageId: version` pairs. |
| `references` | none | Paths to `.dll` files. See [Packages and references](dependencies.md). |
| `namespaces` | none | Namespaces imported for the whole script, like global `using` (C#) or project-level `Imports` (VB). |
| `args` | empty list | String arguments passed to the script when running or debugging. CLI arguments can override this list for one invocation. |
| `connections` | none | The script's connections: each a `name`, a `driver` (such as `http`, `grpc` or `sqlite`) and its `params`. Add them in the Dependencies window. See [Connections in a script](connections.md#connections-in-a-script). |
| `allowUnsafe` | `false` | C# only: `true` compiles the script with unsafe code allowed. Written only when on. See [Compilation](dependencies.md#compilation). |

Field names are case-sensitive camelCase (`targetSdk`, not `TargetSdk`). The values of
`language` and `kind` are case-insensitive. Unknown fields are ignored when the file is
read.

A complete header:

```yaml
---
version: "1.0"
language: csharp
targetSdk: net10.0
kind: Statements
packages:
  Humanizer.Core: 2.14.1
references:
  - $(CurrentQueryPath)/lib/MyHelpers.dll
namespaces:
  - System.Text.Json
  - System.Globalization
---
```

!!! warning "Invalid files are opened as plain text"
    Invoklet refuses to run a file that has no front matter, an unclosed front matter
    block, invalid YAML (for example `kind: Script`), or no run block for the declared
    language. The tab shows the whole file as text, the Results tab explains the
    problem, and the status strip says *Invalid document*. Fix the file and reopen it.

### The run block

The opening fence must be exactly three backticks, the language id, whitespace, and
`run` (for example `` ```csharp run ``). Variations such as `csharp runner` or
`csharprun` are not run blocks. Invoklet uses the first matching block in the file. Other
fenced blocks, such as `csharp` examples without `run`, are ordinary Markdown.

### Namespaces imported by default

C# scripts get the namespaces of the .NET SDK's implicit usings plus Invoklet's runtime
namespace: `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`,
`System.Net.Http`, `System.Threading`, `System.Threading.Tasks` and `Invoklet.Runtime`.
Visual Basic scripts import the same list. Add anything else to `namespaces`, or let a
[quick fix](analyzers.md#quick-fixes-and-the-lightbulb) add it for you.

C# scripts compile with nullable reference types enabled and the default C# language
version of the selected SDK (for example C# 14 for .NET 10).

## Conditional code for Invoklet

Invoklet defines the `INVOKLET` preprocessor symbol for C# and Visual Basic scripts,
including scripts run through the CLI. Use it to keep Invoklet-specific code in a
script that is also used by another runner:

```csharp
#if INVOKLET
Console.WriteLine("Running in Invoklet");
#else
Console.WriteLine("Running elsewhere");
#endif
```

In Visual Basic, use `#If INVOKLET Then` with `#Else` and `#End If`.

## Script arguments

Declare a script's arguments, their types and defaults, and named sets of values in the
**Arguments** tab of the Dependencies window; see
[Script arguments](dependencies.md#script-arguments). The script reads them as typed
properties of `Arguments`, such as `Arguments.Count`.

Run and Debug also pass them on the command line of the script's process, as `--name value`
for each argument that has a value (a list as `--name` once per item). C# top-level
Statements and a Program's `Main(string[] args)` receive these strings in `args`;
Visual Basic Statements and `Sub Main(args As String())` receive them too. The
[command-line runner](cli.md#script-arguments-and-options) can pick another set or replace
the arguments without changing the file.

## External file changes

When another program changes an open script, its tab follows the file. Invoklet checks
when you switch back to it from another program, and as soon as a script in the scripts
folder changes:

- A tab without unsaved changes reloads quietly: the code, the header (kind, target,
  packages) and the preview. **Undo** brings back the text the reload replaced.
- A tab with unsaved changes asks whether to **Reload** the file and lose your changes, or
  **Keep My Changes**. Keeping them is the default, and Invoklet doesn't ask again about the
  same version of the file.
- A running script, or a file that was deleted, is left as it is.
- If the file now holds a script in the other language (C# instead of Visual Basic or the
  other way round), it reopens in a new tab in the same place.

**File → Revert to Saved** goes back to the script as it is on disk at any time, asking
first if that drops unsaved changes. It has no default key; you can assign one in
[Preferences](reference.md#keyboard-shortcuts).

If you keep your changes, the next save asks whether to **Save As…**, **Overwrite** or
**Cancel**. This also applies to the automatic save before Run/Debug and saves required
before closing or file operations. Cancel or an unsuccessful Save As stops that operation
and keeps your unsaved changes. Overwrite explicitly replaces the disk version; Save As
keeps it and saves your tab under another name. Invoklet never merges the two versions.

## Kinds

The kind decides how Invoklet turns your code into a program. Change it with the kind
drop-down in the toolbar; the choice is saved to the front matter.

### Statements

Top-level statements, like a modern C# `Program.cs`. You can declare local functions
and types after the statements.

```csharp
var words = new[] { "alpha", "beta", "gamma" };
words.Select(w => w.ToUpperInvariant()).Dump();
```

In Visual Basic, Invoklet wraps the code in a module with a `Sub Main`, so you write only
the statements:

```vb
Dim names = {"Ada", "Grace", "Linus"}
For Each name In names
    Console.WriteLine($"Hello, {name}!")
Next
names.Select(Function(n) New With {.Name = n, .Letters = n.Length}).Dump("Names")
```

### Program

You write a `Main` method and whatever else it needs: fields, helper methods, nested
types. In C#, Invoklet places your code inside a generated class and calls `Main` for you.
`Main` may be static or not, and may take the command-line arguments, a cancellation token, or
both:

| `Main` | Notes |
| --- | --- |
| `void Main()`, `async Task Main()` | The usual form. |
| `Main(string[] args)` | Receives the run's [script arguments](#script-arguments) as `--name value`, or the [CLI override](cli.md#script-arguments-and-options). |
| `Main(CancellationToken ct)`, `Main(string[] args, CancellationToken ct)` | `ct` is cancelled when you press **Stop**; see [Stopping a script](results.md#stopping-a-script). |
| `int Main(…)`, `async Task<int> Main(…)` | The returned number is the exit code; anything but 0 shows as *Exit code N*. |

If the script declares several `Main` overloads, Invoklet calls the one that takes the most:
arguments and a token, then a token, then arguments, then none. A `Main` under `#if DEBUG` or
another conditional symbol counts only when the script's target framework defines that symbol,
as it does for the compiler (`DEBUG` and `TRACE` are always defined).

```csharp
void Main()
{
    Console.WriteLine(Greet("Invoklet"));
}

string Greet(string name) => $"Hello, {name}!";
```

A static class with extension methods can sit next to `Main`, as in LINQPad. Invoklet then compiles
the script's static classes outside the generated class, because C# allows extension methods only
in a class that isn't nested; other types stay nested in it as usual. The static classes still see
the script's types, methods and fields by their simple names, but only those marked `internal` or
`public`: to extend a type of your own, declare it `internal class Point …` rather than
`class Point …`.

A Program script without any statement or method at the top level, written as an ordinary C#
program (`class Program { static void Main() … }`, with using directives and namespaces if you
like), is compiled exactly as written. The compiler picks its entry point and reports a missing
or ambiguous one.

A new Program script starts with an empty `Main`, and switching a script to **Program** in the
toolbar wraps its code in `Main` — `async Task Main()` when the code uses `await` — and keeps
methods and types after it. One **Undo** brings the old code back; a script that already has a
`Main` is left as it is.

In Visual Basic the code is compiled as written, so a Program script contains the whole
module (switching to Program wraps the code in one):

```vb
Module Program
    Sub Main()
        Console.WriteLine(Twice(21))
    End Sub

    Function Twice(value As Integer) As Integer
        Return value * 2
    End Function
End Module
```

### Expression

The whole block is one expression, written without a trailing semicolon. Invoklet
evaluates it and renders the result as if you had called `.Dump()` on it.

```csharp
Enumerable.Range(1, 10).Where(n => n % 2 == 0)
```

The [examples](examples.md) include one script of each C# kind.

## Where your scripts live

Invoklet shows the `.inv` files and folders inside **Invoklet Queries** in your Documents folder
(created on first start) in the **Scripts** section of the panel on the left. The tree refreshes
when files change on disk. The **Samples** section lists the sample scripts that come with Invoklet,
grouped by folder; see [Examples](examples.md). Right-click the Scripts tree to create C# or Visual
Basic files and folders,
rename, delete, or *Show in File Manager*; drag items to move them.

To open a script from another folder, use [File → Open...](#opening-scripts). `.linq` query files can be imported with **File → Import .linq Query...**; see
[Command line and .linq import](cli.md#importing-linq-queries).
