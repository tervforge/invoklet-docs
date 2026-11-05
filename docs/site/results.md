---
title: Results and runtime helpers
description: Console output, .Dump() tables, DumpOptions, DumpContainer, progress bars, interactive input and other Util helpers.
---

# Results and runtime helpers

Every script is compiled against a small runtime library, `Invoklet.Runtime`. It provides
`.Dump()`, `DumpContainer`, `DumpOptions` and the `Util` class. Its namespace is imported
automatically, so none of these need a `using`. Scripts imported from `.linq` files
usually work without changes.

## The output tabs

Under the editor are three tabs:

- **Results** — console output and `.Dump()` blocks, in the order the script produces
  them. Text the script writes to `Console.Error`, compiler errors and unhandled
  exceptions are shown as errors. When the run ends, the status strip below shows the
  elapsed time and *Success*, *Cancelled*, *Compilation failed* or *Exit code N* when the
  script ends with a non-zero exit code.
- **Debugger** — the debugger panels; see [Debugging](debugging.md).
- **IL** — the IL disassembly of the last successful compile, as read-only text.

The button with the panel icon in the toolbar shows or hides the output area.

The **Output settings** gear at the right of the tabs has *Wrap long lines in results*: output,
errors and `.Dump()` text then wrap at the pane's width instead of scrolling sideways. The same
menu has the debugger's [exception break options](debugging.md#breaking-on-exceptions).

## `.Dump()`

`.Dump()` renders any value in the Results tab and returns the same value, so you can put
it in the middle of an expression:

```csharp
var evens = Enumerable.Range(1, 10)
    .Where(n => n % 2 == 0)
    .Dump("Even numbers")      // renders the list ...
    .Sum();                    // ... and the chain continues
evens.Dump();
```

How values are shown:

- A collection of objects becomes a table with one column per public property or field.
  A collection of simple values becomes a two-column table (`#`, `Value`).
- Nested objects and collections become nested tables, expanded up to **3 levels** by
  default.
- Collections show at most **1000 rows** by default; the rest is summarized as
  "… N more".
- Strings are shown in quotes and very long strings are shortened. `null` is shown as
  `null`.
- Dictionaries, enums, dates, numbers and booleans have their own formatting.

Without a label, the heading is the source text of the dumped expression. For
`total.Dump()` the heading is `total`.

### Overloads

| Call | Effect |
| --- | --- |
| `x.Dump()` | Heading is the expression text. |
| `x.Dump("label")` | Custom heading. |
| `x.Dump(5)` | Expands nested values 5 levels deep. No heading. |
| `x.Dump(5, 1)` | 5 levels deep; levels after the first start collapsed (click to expand). |
| `x.Dump("label", 5)` | Heading and depth. |
| `x.Dump(true)`, `x.Dump("label", true)` | Accepted for compatibility with imported scripts; the result looks the same as without `true`. |
| `x.Dump(new DumpOptions { … })` | Full control, see below. |

### `DumpOptions` and `Util.DumpDefaults`

```csharp
people.Dump(new DumpOptions
{
    Description = "People",
    MaxDepth = 2,
    MaxRows = 50,
    MembersToExclude = "Password,InternalId",
});
```

| Property | Meaning |
| --- | --- |
| `Description` | Heading text. |
| `MaxDepth` | Nesting levels to expand (default 3). |
| `CollapseTo` | Levels after this one start collapsed. |
| `MaxRows` | Maximum rows per collection (default 1000). |
| `MembersToInclude`, `MembersToExclude` | Comma-separated member names to show or hide. |
| `IncludePrivateMembers` | Also show non-public properties and fields. |
| `AlphabetizeMembers` | Sort columns by name. |
| `NoTotals` | Hide the row-count footer under tables. |
| `RepeatHeadersAt` | Repeat the header row every N rows. |
| `ForceExpansion` | Render expanded even for types that start collapsed by default. |
| `UseInvariantCultureForNumbers` | Format numbers with the invariant culture. |
| `FormatStrings` | Format strings for integers, real numbers, dates and times. |
| `ToDataGrid` | Accepted for compatibility with imported scripts; currently has no visible effect. |

To change the defaults for the rest of a run, set properties on `Util.DumpDefaults`, for
example `Util.DumpDefaults.MaxDepth = 5;`.

A type can control which members `.Dump()` shows by implementing `ICustomMemberProvider`.

## Clearing the results

**Run → Clear Results** (Ctrl+Shift+L, Cmd+Shift+L on macOS) empties the results panel of the current tab. A new run clears it too.

## Saving the results

**File → Export Results as HTML…**, or **Save results as HTML…** in the context menu of the results panel (right click), saves everything
in the results panel as one HTML page, with the colours of the current theme, that opens in any browser. The suggested name is the
script's own, in its folder. A tab with no results says so instead of saving an empty page.

Every table in the results has a **…** button in the header of its last column. Click it and choose **Export to CSV…** to save that table as a CSV file: the column names
first, then a line for each row, the values as the results show them (a `null` is an empty field). A table inside a cell of another is
the text of that cell. The file is UTF-8 with a byte order mark, so Excel reads accented letters and other scripts right. A field that
would be read as a formula (it starts with `=`, `+`, `@`, or `-` followed by something that is not a number) gets a `'` in front, so that
opening the file in a spreadsheet cannot run it. The suggested name is the script's, with `-2` for the second table when there are several.

## Highlights, styles and headings

`Util.Highlight`, `Util.HighlightIf`, `Util.WithStyle` and `Util.WithHeading` dress a value in the output of
`.Dump()`. They return a wrapper that is dumped like the value itself (at the same depth), so they work on their own or
inside what you dump, for instance in a column of a table:

```csharp
Util.Highlight(42).Dump();                          // the theme's marker; Util.Highlight(x, "lightgreen") picks another colour
Util.WithStyle("late", "color:red; font-weight:bold").Dump();
Util.WithHeading(orders, "Open orders").Dump();     // a heading above the value

orders.Select(o => new { o.Id, Qty = Util.HighlightIf(o.Qty, q => q < 5) }).Dump();
```

`Util.WithCssClass(value, "my-class")` puts a value in an element with a class of your own, and `Util.HtmlHead.AddStyles(css)` adds the
rules for it to the output from that point on:

```csharp
Util.HtmlHead.AddStyles(".late { color: crimson; font-weight: bold; }");
Util.WithCssClass("overdue", "late").Dump();
```

The terminal runner shows the value and ignores the styles.

`HighlightIf` highlights only when the condition holds and otherwise returns the value as it is. In the
[command-line runner](cli.md) a highlight is shown with a yellow background and a heading as an underlined line; a
style is not applied there.

## Live-updating output

`DumpContainer` is a placeholder block. Dump it once, then change its content; the same
block updates in place instead of adding a new one.

```csharp
var status = new DumpContainer("Starting...");
status.Dump("Status");
status.Content = "Halfway there";       // replaces the content in place
status.AppendContent("Still working");  // adds to it
status.ClearContent();                  // empties it
```

`UpdateContent`, `AppendContent`, `ClearContent`, `Refresh`, `Style` (inline CSS for the
block) and `DumpDepth` are available.

`Util.ProgressBar` works the same way:

```csharp
var progress = new Util.ProgressBar(0, "Processing").Dump();
for (var i = 1; i <= 5; i++)
{
    Thread.Sleep(200);
    progress.PercentComplete = i * 20;  // the same bar updates
}
progress.Caption = "Done";
```

The `live-output.inv` sample combines both. In the
[command-line runner](cli.md), each update is printed again below the previous output
instead of replacing it.

## Interactive input

A script can ask for input in the Results area while it runs:

```csharp
var name = Util.ReadLine("Your name?");
var age = Util.ReadLine<int>("Your age?");
var secret = Util.GetPassword("Password?");   // typed text is masked on screen
$"{name} is {age}".Dump();
```

An input bar appears under the output tabs with the prompt. Type the answer and press
++enter++. The script waits until you answer. `ReadLine<T>` converts the answer with the
invariant culture and works for types such as `int`, `double`, `bool` and `DateTime`.
If the run is cancelled before you answer, the result is `null` (or the default value).

Interactive input works in a run and in a debug session in the Invoklet window. The
command-line runner asks in the terminal: the prompt goes to standard error and the answer is
read from standard input, without echo for a password.

## Other `Util` helpers

| Member | What it does |
| --- | --- |
| `Util.CurrentQueryPath` | Full path of the running `.inv` file, or `null` for an unsaved tab. |
| `Util.GetSecret(name)` | A secret saved in **Tools → Secret Store**; see [Secret Store](secrets.md). |
| `Util.Cache(func, key, duration)`, `Util.CacheAsync(…)` | Returns a cached result for `key`, or runs the function and stores its result. The cache survives across runs; `duration` is an optional time-to-live. |
| `Util.SaveString`, `Util.LoadString`, `Util.SaveBytes`, `Util.LoadBytes` | Save and load files by name in Invoklet's per-user data folder. |
| `Util.Try(action)`, `Util.Try(func)` | Run code and get the exception back instead of having it thrown. |
| `Util.KeepRunning()` | Keeps the script's process alive until the returned object is disposed, for timers or background work. |
| `Util.CancellationToken` | Cancelled when you press **Stop**; see [Stopping a script](#stopping-a-script). |
| `Util.Cmd(…)` | Runs an external program, streams its output into the Results tab and returns its exit code. An overload captures the output into a string instead. |

`Util.Cache` and the `Save…` methods write files under the `UserData` folder inside
Invoklet's data folder (see [Where Invoklet stores data](reference.md#where-invoklet-stores-data)).
Cached entries stay there until they expire or you delete the files.

## Stopping a script

**Stop** (++shift+f5++) ends a running script at once — unless the script listens for it. A script
that uses `Util.CancellationToken`, or a Program script whose `Main` takes a `CancellationToken`,
sees the token cancelled and gets up to 3 seconds to close files, connections and child processes
before Invoklet ends it:

```csharp
async Task Main(string[] args, CancellationToken ct)
{
    using var connection = await OpenConnectionAsync();
    try
    {
        await ProcessAsync(connection, ct);
    }
    finally
    {
        Console.WriteLine("Closing the connection");
    }
}
```

The run still counts as cancelled. Under the debugger, Stop always ends the script at once.

<a id="sidebar-navigation"></a>
<a id="connections-in-a-script"></a>
<a id="grpc-connections"></a>
<a id="sqlite-connections"></a>
<a id="postgresql-and-sql-server-connections"></a>

## Connections

See [Connections](connections.md) for the sidebar, connection setup and typed clients.

<a id="secret-store"></a>

## Secrets

See [Secret Store](secrets.md) for credentials, storage and script access.
