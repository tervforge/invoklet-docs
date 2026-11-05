---
title: Diagnostics and analyzers
description: Compiler diagnostics, Roslyn IDE analyzers and quick fixes in C# scripts.
---

# Diagnostics and analyzers

While you edit a C# script, Invoklet analyzes it in the background and underlines problems.
This page explains where those messages come from, how quick fixes work, and what the
analysis does not promise.

<figure markdown="span">
  [![An error underline on the unknown JsonSerializer, with the lightbulb menu offering using System.Text.Json and other fixes](assets/screenshots/diagnostics_light.png#only-light)](assets/screenshots/diagnostics_light.png "Open full-size screenshot")
  [![An error underline on the unknown JsonSerializer, with the lightbulb menu offering using System.Text.Json and other fixes](assets/screenshots/diagnostics.png#only-dark)](assets/screenshots/diagnostics.png "Open full-size screenshot")
</figure>

## Three kinds of analysis

| Source | IDs | Where you see it | When |
| --- | --- | --- | --- |
| C# compiler | `CS…` (for example `CS0103`) | Editor underlines; also printed in the Results tab when you run | While you type and on every run |
| Roslyn IDE analyzers | `IDE…` | Mostly as lightbulb suggestions; many have no underline | While you type |
| Assembly Explorer analysis | none | The Assembly Explorer's analysis pane | When you ask for it |

**Compiler diagnostics** are the same errors and warnings the build reports. In the
editor they appear about half a second after you stop typing. When you run, Invoklet
prints every compiler diagnostic in the Results tab as
`(line,column): severity ID: message`, with line numbers counted in the code block. Any
error stops the run; warnings do not.

**Roslyn IDE analyzers** come from Microsoft's Roslyn features library, the same family
as the `IDE` suggestions in Visual Studio ("use `var`", "remove unnecessary using", …).
Most of them report at *hidden* severity, so there is no underline; they make a
lightbulb fix available on the caret's line instead.

**Assembly Explorer analysis** is a different tool: it searches compiled assemblies for
code that uses, reads, assigns or implements a member. See
[Assembly Explorer](assembly-explorer.md#analyze).

!!! info "Live analysis is C# only"
    Visual Basic scripts have no live diagnostics. Their compiler errors appear in the
    Results tab when you run.

Hover over an underline to read its message. The tooltip shows the message text only;
the diagnostic ID is not shown in the editor.

## Example: a missing namespace

Download [`analyzer-diagnostics.inv`](examples/analyzer-diagnostics.inv) and open it.
**It is meant to be read in the editor, not run.** The code block is:

```csharp
var settings = JsonSerializer.Serialize(new { Name = "Invoklet", Version = 1 });
Console.WriteLine(settings);

var small = byte.Parse("300");
var large = int.Parse("3000000000");
Console.WriteLine($"{small} {large}");
```

What you see in the editor:

- **Line 1** has an error underline: `JsonSerializer` is not in scope because
  `System.Text.Json` is not imported. If you run now, the Results tab shows
  `(1,16): error CS0103: The name 'JsonSerializer' does not exist in the current context`
  and nothing runs.
### Applying the quick fix

1. Put the caret on line 1. A lightbulb appears in the gutter on that line.
2. Click the lightbulb. The menu lists the fixes for that line, grouped by kind, with
   `using System.Text.Json;` first.
3. Choose `using System.Text.Json;`.

Invoklet adds `System.Text.Json` to the `namespaces:` list in the script's front matter
rather than inserting a `using` line into the code, and the error disappears.

## Quick fixes and the lightbulb

The lightbulb shows only on the line that holds the caret, and only when that line has
something to offer. Clicking it always opens a menu; nothing is applied without your
choice. Entries come from Roslyn's built-in code fix and refactoring providers, for
example:

- add a missing namespace (written to the front matter, as above);
- generate a missing variable or method;
- implement an interface or abstract class;
- refactorings offered at the caret position, such as conversions between equivalent
  forms of code.

Refactorings appear after the caret rests for a moment; fixes follow the background
analysis of your edits.

To add every missing namespace at once, use **Resolve All Usings** (++ctrl+alt+u++). It
adds the namespace for each unresolved type that has exactly one candidate.

## A secret written to the output

Invoklet warns (`INV0001`) when the value of `Util.GetSecret` reaches `.Dump()` or
`Console.Write`/`WriteLine`, directly or through a variable, a string built from it, `?:` or
`??`. What a run writes is shown in the results and saved with the script's output, so the
secret would no longer be kept only in the Secret Store. The lightbulb offers **Remove .Dump() of
the secret '…'**, which takes away only the `.Dump()` call (`Util.GetSecret("api").Dump();` becomes
`Util.GetSecret("api");`; a line left as just `token;` is removed), or **Remove the output of the
secret '…'** for a `Console` line. What is computed from a secret, such as its length, is not
warned about. The warning is always on.

## Settings

Open **Preferences → Languages → .NET → C#**:

| Setting | Effect |
| --- | --- |
| *Code fixes and refactorings (lightbulb)* | Turns the lightbulb on or off. Underlines are not affected. |
| *Excluded diagnostic IDs* | One ID per line, matched exactly and case-insensitively (for example `IDE0007`). Hides lightbulb fixes for those diagnostics. Underlines are not affected, and refactorings, which have no diagnostic ID, cannot be excluded this way. The default list contains `IDE0055` (formatting). |
| *Inlay hints* | Shows or hides inferred types next to `var` and `foreach` variables. Off by default. |

## What analysis does not promise

- IDE analyzers find **specific** patterns. A clean editor does not mean the script is
  correct.
- IDE analyzer results are editor-only. They do not run during compilation, so they never
  stop a run and do not appear in the Results tab or in the [command-line runner](cli.md).
- Analysis runs after a short pause in typing, so underlines can lag behind very fast
  edits by a moment.
