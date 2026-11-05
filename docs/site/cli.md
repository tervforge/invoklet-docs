---
title: Command line and .linq import
description: Run .inv scripts from a terminal with the invoklet CLI and convert .linq queries.
---

# Command line and .linq import

## The `invoklet` command-line runner

`invoklet` compiles and runs the code block of a `.inv` file in a terminal, without the
Invoklet window and without the debugger. It is useful for automation and CI. It runs C#
and Visual Basic scripts and honors `kind`, `packages`, `references` (including the path
macros) and `namespaces` the same way the Invoklet window does.

!!! info "Getting the runner"
    The runner is not included in the release packages. On Linux, the `invoklet` command
    that the `.deb` package puts on `PATH` starts the Invoklet window, not this runner.

    The runner is the `Invoklet.Cli` project in the
    [Invoklet source code](https://github.com/campforge/run-pad). Build it from a clone with
    the .NET 10 SDK:

    ```bash
    dotnet build src/main/Invoklet.Cli -c Release
    dotnet src/main/Invoklet.Cli/bin/Release/net10.0/invoklet.dll path/to/script.inv
    ```

    Or run it straight from source:

    ```bash
    dotnet run --project src/main/Invoklet.Cli -- path/to/script.inv
    ```

    The build also produces a native launcher, `invoklet` (`invoklet.exe` on Windows), that
    works without the `dotnet` prefix if it can find the .NET runtime. With a
    non-standard .NET install, for example one from Homebrew or Linuxbrew, it may not, so
    use the `dotnet …/invoklet.dll` form, which always works.

### Usage

```text
invoklet [run] [options] <script.inv> [--] [script arguments...]
invoklet convert <input.linq> [output.inv]
```

Running with no arguments prints this usage and exits with code 2. `--help` and
`--version` print information and exit with code 0.

- Script output goes to standard output. Text written to `Console.Error` and compiler
  errors go to standard error.
- `.Dump()` output is rendered in the terminal as tables, with nested tables for nested
  values and colors by value type. Colors are used only when the output is a terminal,
  not when it is redirected to a file or pipe.
- `DumpContainer` and `Util.ProgressBar` updates are printed one after another instead of
  updating in place.
- The script runs on the SDK its `targetSdk` names, found by `dotnet --list-sdks` on `PATH`
  (`latest`: the newest stable one). If that version isn't installed, the newest SDK runs it
  and a warning goes to standard error.
- Each run compiles into a new temporary folder that is deleted afterwards. The runner
  does not use the Invoklet window's build cache.
- NuGet sources come from Invoklet's own `NuGet.Config` if the Invoklet window has created
  one; otherwise your normal NuGet settings are used.
- [Interactive input](results.md#interactive-input) is asked in the terminal: the prompt goes to
  standard error and the answer is read from standard input (a password without echo). When
  standard input has ended, the script gets an empty answer.
- `Util.GetSecret` and `Util.SetSecret` read and save the same Secret Store as the Invoklet
  window; a missing secret is asked for in the terminal and kept there for next time.

### Script arguments and options

```bash
invoklet report.inv "April totals" 2026
invoklet run report.inv -- --kind --help
invoklet --kind program --sdk 10 report.inv -- "April totals"
invoklet --compile-only report.inv
invoklet --args-set smoke report.inv
```

With no arguments after the script, it gets its [declared arguments](dependencies.md#script-arguments)
from the set the file selects, or their defaults; `--args-set NAME` picks another set.
Arguments following the script replace them all, and a bare `--` after the script passes
none. Use your shell's normal quoting for spaces or quotes inside one argument.

Recognized runner options are accepted before **or after** the script path. Put `--`
after the path to pass everything that follows literally to the script, including
`--help`, `--kind` or `-s`. Without it, those names are interpreted by the runner.
Unknown options after the script are passed through, except recognized unsupported
runner options, which produce an error. Always use `--` when passing option-like values.

| Option | Meaning |
| --- | --- |
| `--kind`, `-k`, `-lang=<kind>` | Override `Statements`, `Program` or `Expression`; `s`, `p`, `e` also work. `VBStatements`, `VBProgram` and `VBExpression` require a Visual Basic file; they do not translate C# code. |
| `--sdk`, `-s` | Override `targetSdk`, for example `10`, `10.0`, `net10.0` or `latest`. The normal installed-SDK selection and warning rules apply. |
| `--compile-only`, `-compileonly` | Check compilation and report diagnostics without executing the script. The temporary compilation output is removed afterwards. |
| `--refresh-connections` | Read every connection's description again before the build, such as a gRPC server's services after they changed. Without it, a description Invoklet already read is reused. |
| `--help`, `-h`, `-?` | Show usage. |
| `--version` | Show the runner version. |

Overrides affect this invocation only; the script file is not rewritten. These spellings
support common `lprun`/`npad` command lines, not every option of those tools. `-nunuget`,
`--no-cache`, `--rebuild` and `-b` are accepted with no effect; package restoration is
not disabled. Options such as output format, evaluation, database connections and
optimization are not implemented and are reported as unsupported.

### Exit codes

Once the script has run, the runner exits with the script's own exit code: `0` when it
finishes normally, `42` after `Environment.Exit(42)`, and a non-zero code after an
unhandled exception. Text on standard error alone does not fail the run. Otherwise:

| Code | Meaning |
| --- | --- |
| `2` | Bad run arguments, file not found, an invalid `.inv` document, no .NET SDK found, a compile error, or the compiled script could not be started. |
| `1` | The `convert` command failed; conversion retains its existing error code. |
| `130` | Interrupted with ++ctrl+c++. |

### Example

```console
$ dotnet invoklet.dll debugging-loop.inv
i = 1, square = 1, running total = 1
i = 2, square = 4, running total = 5
i = 3, square = 9, running total = 14
i = 4, square = 16, running total = 30
i = 5, square = 25, running total = 55
Sum of squares 1..5
55
```

## Importing .linq queries

Invoklet converts `.linq` query files to `.inv` scripts.

- **In Invoklet:** choose **File → Import .linq Query...** and pick the `.linq` file.
  Invoklet writes a `.inv` file with the same name **next to the `.linq` file** and opens
  it. If that name is taken, it adds a number (`Query 2.inv`). The file appears in the
  file tree only if it is inside your Invoklet Queries folder.
- **From a terminal:** `invoklet convert query.linq` writes `query.inv` next to the input.
  Give a second argument to choose the output path. The command **overwrites** an
  existing output file.

### What is converted

| `.linq` | `.inv` |
| --- | --- |
| `Kind="Statements"`, `"Program"`, `"Expression"` | `language: csharp` with the same kind |
| `Kind="VBStatements"`, `"VBProgram"`, `"VBExpression"` | `language: vb` with the same kind |
| `<NuGetReference Version="…">Id</NuGetReference>` | `packages:` entry (`latest` when no version is given) |
| `<Namespace>` | `namespaces:` entry |
| `<Reference>` (or its `Relative` attribute) | `references:` entry |
| The code after `</Query>` | The run block, under a heading with the file name |

!!! note "Unreleased: C# namespace imports"
    C# `.linq` imports now write LINQPad's standard namespace profile into the converted
    file's `namespaces`, together with explicit `<Namespace>` entries, without exact duplicates.
    This includes `System.Text`, `System.Data` and `System.Xml.Linq`. These imports are editable
    script metadata used by both compilation and the editor. Existing `.inv` files, new ordinary
    scripts, global defaults and Visual Basic imports are unchanged.

    Custom defaults configured only on the original LINQPad machine are not present in the
    `.linq` header and cannot be inferred; add any missing custom imports to the converted file.

Other kinds, such as SQL or F# queries, are rejected with an error that lists the
supported kinds. Database connections are not converted. The converted file
gets `targetSdk: latest`.

Most `.Dump()` calls work unchanged; see [Results and runtime helpers](results.md). The long
positional `Dump(description, depth, collapseTo, toDataGrid, …)` overloads are not provided; use
`Dump(new DumpOptions { … })` instead.

### Try it

Download [`sample-query.linq`](examples/sample-query.linq):

```xml
<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
</Query>

var culture = CultureInfo.InvariantCulture;
var prices = new[] { 9.99m, 24.50m, 3.25m };

prices
    .Select(p => new { Price = p, Text = p.ToString("N2", culture) })
    .Dump("Formatted prices");
```

`invoklet convert sample-query.linq` produces (with the unreleased import profile):

````markdown
---
version: 1.0
language: csharp
targetSdk: latest
kind: Statements
namespaces:
  - System
  - System.Collections
  - System.Collections.Generic
  - System.Data
  - System.Diagnostics
  - System.IO
  - System.Linq
  - System.Linq.Expressions
  - System.Reflection
  - System.Text
  - System.Text.RegularExpressions
  - System.Threading
  - System.Transactions
  - System.Xml
  - System.Xml.Linq
  - System.Xml.XPath
  - System.Globalization
---

# sample-query

```csharp run
var culture = CultureInfo.InvariantCulture;
var prices = new[] { 9.99m, 24.50m, 3.25m };

prices
    .Select(p => new { Price = p, Text = p.ToString("N2", culture) })
    .Dump("Formatted prices");
```
````

Running the converted script shows a **Formatted prices** table with the prices and the
texts `"9.99"`, `"24.50"` and `"3.25"`.
