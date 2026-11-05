---
title: Packages and references
description: Add NuGet packages, assembly references and namespace imports to a script with the Dependencies window.
---

# Packages and references

A script's dependencies live in its front matter: `packages`, `references` and
`namespaces`. You can edit them in the **Dependencies** window instead of typing YAML.

<figure markdown="span">
  [![The Dependencies window on the Packages tab: the local cache filtered on the left, online search results for Humanizer in the middle and the selected package's details and versions on the right](assets/screenshots/dependencies_light.png#only-light)](assets/screenshots/dependencies_light.png "Open full-size screenshot")
  [![The Dependencies window on the Packages tab: the local cache filtered on the left, online search results for Humanizer in the middle and the selected package's details and versions on the right](assets/screenshots/dependencies.png#only-dark)](assets/screenshots/dependencies.png "Open full-size screenshot")
</figure>

## Open the Dependencies window

Choose **Tools → Dependencies...** or press ++f4++. The window edits the script in the active
tab. It has five tabs, described below: **Packages**, **References**, **Namespace Imports**,
**Compilation** (C# scripts) and **Connections**. Click **Save** to apply your changes to the
script, or **Cancel** to discard them. Saving updates the front matter, writes the file if
it already has a name, and refreshes IntelliSense so new types are recognized.

## NuGet packages

The **Packages** tab has three columns:

- **Left** — the packages already in this script, followed by the packages in your local
  NuGet cache. Type in *Search local package cache* to filter them.
- **Middle** — online search. Type in *Search online*; the search runs after a short
  pause, or press ++enter++ to search at once. Use the arrow buttons to page through results,
  and pick a source in the drop-down: *All sources*, one enabled source, or
  *(configure sources...)*.
- **Right** — *Include Prerelease*, and details of the selected package with a
  **Version** list.

Select a package and a version, then:

- **Add** puts the package in the script;
- **Update** changes the version of a package the script already uses;
- **Remove** takes it out of the script.

The rows in the lists also have small buttons for the common cases: **+** adds the
package, the circular arrows update it and the bin removes it; each one names its action in
its tooltip. A mark next to a package shows that it is already in this script.

In the file, packages are stored as `PackageId: version` pairs:

```yaml
packages:
  Humanizer.Core: 2.14.1
```

When you run the script, Invoklet restores the packages itself (no `dotnet restore`
command is involved) and caches the result, so later runs with the same packages and code
skip the restore and compile steps. Restoring a package that is not in your local cache
needs access to its package source.

### Package sources

Choose *(configure sources...)* in the source drop-down to open **NuGet Package
Sources**. There you can add, remove and reorder sources (**Move to Top**, **Move Up**,
**Move Down**), enable or disable each one, and set:

- **Name** and **Source** — a feed URL such as `https://example.com/nuget/v3/index.json`,
  or a local folder (use the **...** button);
- **Authentication** — *None*, or *Basic* with a username and a password or personal
  access token.

These sources are used both for search and for restoring every script. Invoklet keeps them
in its own `NuGet.Config` in its data folder, created the first time from your effective
NuGet settings. Your global NuGet settings are not changed. See
[Where Invoklet stores data](reference.md#where-invoklet-stores-data).

## Assembly references

The **References** tab lists `.dll` files the script references. Type a path in
*Path to a .dll* or use **Browse…**, then click the **+** button. The bin button beside a
reference takes it out.

```yaml
references:
  - "C:/Tools/MyCompany.Utilities.dll"
  - $(CurrentQueryPath)/lib/Helpers.dll
```

- Only paths ending in `.dll` are used as assembly references.
- Other `.dll` files in the same folder that the referenced assembly depends on are
  picked up automatically.
- Relative paths such as `lib/Helpers.dll` are resolved next to the `.inv` file,
  consistently in Invoklet, the CLI and IntelliSense. An unsaved tab has no containing
  folder yet: save it first or use an absolute path.
- A missing DLL produces a warning and is not loaded; compilation also fails if the
  script needs types from that assembly.
- Paths are written with `/`. Existing Windows drive paths (`C:\Tools\Helpers.dll`)
  and UNC paths (`\\server\share\Helpers.dll`) are still recognized. On macOS/Linux,
  an unavailable Windows path produces an error; Invoklet does not guess a mount point.
  Ambiguous drive-relative paths such as `C:lib/Helpers.dll` are rejected.
- On macOS/Linux a backslash in a relative or Unix path can be a literal filename
  character, so it is preserved. Use `/` for directory separators.
- **Save As** into another folder rebases relative references to keep pointing at the
  same DLLs. Moving a script through the Queries tree does not currently rewrite them;
  check its references after moving it. No referenced files are copied.

Macros remain unchanged in the saved header and are expanded only when resolving
references, in Invoklet, the command-line runner and IntelliSense:

| Macro | Expands to |
| --- | --- |
| `$(CurrentQueryPath)` | The folder that contains the running `.inv` file. Requires a saved script; reported as an error for an unsaved tab. |
| `$(MyDocuments)` | Your Documents folder. |
| `$(RuntimeDirectory)` | The folder of the .NET runtime that Invoklet itself runs on. |

### Project references are not supported

Invoklet compiles scripts in-process with Roslyn and does not use MSBuild, so a script
cannot reference a `.csproj` or `.vbproj`. If `references` contains one, the run stops
with:

```text
Project references are no longer supported: MyLibrary.csproj. Reference the project's built .dll instead.
```

Build the project with your usual tools and reference the resulting `.dll` instead.

## Namespace imports

The **Namespace Imports** tab is a text box with one namespace per line, for example
`System.Text.Json`. These namespaces are imported for the whole script: as global
`using` directives in C#, or project-level `Imports` in Visual Basic. They are stored in
`namespaces` in the front matter.

You rarely need to type them yourself: accepting a completion item for a type from
another namespace, a `using …;` [quick fix](analyzers.md#quick-fixes-and-the-lightbulb)
and **Resolve All Usings** (++ctrl+alt+u++) all add namespaces here. The default
namespaces listed in [Scripts and .inv files](scripts.md#namespaces-imported-by-default)
are always imported and do not need to be added.

## Script arguments

The **Arguments** tab declares the values a script takes and names sets of them.

**Declared arguments** lists each argument with its *Name*, *Type* and default *Value*. Types are
`string`, `int`, `bool`, `double`, `decimal`, `datetime`, `timespan`, `string[]` (one item per
line) and `enum`. For an `enum`, type its values separated by commas, such as `dev, test, prod`,
and pick the default from them. An empty value means the argument has no default. The **+**
button adds an argument and the bin button beside a row removes it.

The script reads them as typed properties of `Arguments`, named in PascalCase. An `enum`
argument gets an enum type of its own: `environment` is `Arguments.Environment` of type
`ArgumentsEnvironment`, with values such as `ArgumentsEnvironment.Prod`.

```csharp
if (Arguments.Verbose)
    Console.WriteLine($"Deploying to {Arguments.Environment}, {Arguments.Count} times");
```

**Argument sets** are named groups of values, such as `smoke`, added with **+** and removed with
**−**. A set gives a value for some arguments and leaves the others at their defaults.
**Next run uses** picks the set for the next run, or **Defaults** for none. A run passes each
argument as `--name value`, a list as `--name` once per item; an argument without a value in
the set or a default is left out. On the command line, `--args-set NAME` picks a set, and
arguments after the script replace them all (see [Command line](cli.md)).

```yaml
arguments:
  - name: environment
    type: enum
    default: dev
    values:
      - dev
      - test
      - prod
  - name: count
    type: int
    default: "1"
  - name: verbose
    type: bool
argumentSets:
  smoke:
    environment: test
    verbose: "true"
selectedArgumentSet: smoke
```

## Compilation

The **Compilation** tab, shown for C# scripts only, has **Allow unsafe code**. It compiles the
script with C#'s `/unsafe` option, which pointers, `fixed` buffers and `sizeof` of your own
structs need, and is stored as `allowUnsafe: true` in the front matter. It is off unless a script
turns it on, also for imported LINQPad queries. It is a compiler setting, not a sandbox: a script
always runs with your full permissions, with or without it.

## Connections

The **Connections** tab adds your saved connections (Tools → Connections) to the script, so its
code gets a client for each, and keeps a script's connection in your own list. See
[Connections in a script](results.md#connections-in-a-script).
