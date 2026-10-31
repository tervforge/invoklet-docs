---
title: Results and runtime helpers
description: Console output, .Dump() tables, DumpOptions, DumpContainer, progress bars, interactive input and other Util helpers.
---

# Results and runtime helpers

Every script is compiled against a small runtime library, `RunPad.Runtime`. It provides
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

**File → Export Results as HTML…**, or **Save results as HTML…** in the output settings (the gear above the results), saves everything
in the results panel as one HTML page, with the colours of the current theme, that opens in any browser. The suggested name is the
script's own, in its folder. A tab with no results says so instead of saving an empty page.

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

The `live-output.rpad` sample combines both. In the
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

Interactive input works only in a normal run in the RunPad window. It is not available
while debugging, and the command-line runner does not answer input requests, so such a
script waits there until you press ++ctrl+c++.

## Secrets

Keep passwords, tokens and keys out of your scripts: save them once in **Tools → Secret Store**
and read them by name.

```csharp
var token = Util.GetSecret("billing-api");
```

<figure markdown="span">
  [![The Secret Store window: three secrets by name, the selected one's value hidden](assets/screenshots/secret-store_light.png#only-light)](assets/screenshots/secret-store_light.png "Open full-size screenshot")
  [![The Secret Store window: three secrets by name, the selected one's value hidden](assets/screenshots/secret-store.png#only-dark)](assets/screenshots/secret-store.png "Open full-size screenshot")
</figure>

The Secret Store window lists the names of your secrets. Select one to change its name or value;
the value stays hidden until you tick **Show**. **Add** and **Remove** change the list, **Save**
saves it and **Cancel** drops your changes. Names must differ, letter case aside, and
`Util.GetSecret` finds them the same way. If another RunPad window changed the store meanwhile,
**Save** says so instead of overwriting it.

The secrets belong to you on this computer and live outside every script, in `secrets.json` in
RunPad's data folder (see [Where RunPad stores data](reference.md#where-runpad-stores-data)). On
Windows the values are encrypted for your Windows account; on macOS and Linux the file is
readable only by you, like `~/.pgpass`. A script asking for a secret that isn't there fails with
`KeyNotFoundException`, whose message names the secret and the store. The command-line runner
reads the same store. To use another file, for example on a build server, set the
`RUNPAD_SECRETS_FILE` environment variable to its path.

On macOS and Linux the values are encrypted with a key kept in the system keychain (Keychain or the Secret Service), which may ask you to
unlock it. Invoklet touches the keychain only when something needs a value: starting the program, running a script that never mentions
`Util.GetSecret` or `Util.SetSecret` and listing the names of your secrets (in the Connections windows, say) do not. A script that reaches a
secret in a way its own text does not show, through a library it references, gets no key: put a `Util.GetSecret` call in the script.

## Sidebar navigation

The workspace sidebar is organized into three collapsible sections:

- **Connections** — saved connections and their user-defined groups;
- **Scripts** — the files in the current workspace;
- **Samples** — bundled examples grouped by folder.

The three sections use the same tree rows, icons, selection behavior and keyboard navigation.
Connections can be reordered or moved between groups with the context menu, keyboard commands or
drag and drop. A connection can also be dragged onto the editor, or added with **Add to Script**.
The sidebar remembers which sections, connection groups and sample folders were expanded between
launches. Grouping is local navigation state and does not change a script's `connections:` metadata.

Connection rows show whether a connection is used by the active script and explain missing drivers,
secrets, environment variables or unavailable/stale contracts. Use **Refresh** on a connection, or
**Refresh All**, to re-check its driver and contract.

## Connections

**Tools → Connections** keeps your own connections in one window: HTTP APIs, gRPC servers and
SQLite databases and OpenAPI/Swagger APIs, with other databases to follow. **Add** creates one of a type; the form shows
that type's settings. For an HTTP API they are the base address, headers sent with every request
and a timeout; for a gRPC server, its host and port (with a **Use SSL** checkbox) and metadata sent with every call; for SQLite, the
database file (with a **Browse...** button), and for OpenAPI/Swagger the same as for HTTP.

Each connection shows the icon of its type, and the outcome of its last **Test Connection** as an icon: a check mark, a warning (hover for the
answer) or a spinner while it runs. The panel on the right, **Browse resources**, shows what the connection offers: for a SQLite database the
tables and views with their columns (type, `null` or `not null`, primary key) and foreign keys, read as soon as you select the connection (for a
path relative to a script, choose that script's folder first); for a gRPC server its services and their methods, with the request and response
messages and whether they stream; for an OpenAPI/Swagger API its operations by tag, each with parameters, request body and answers, and its models.
Servers are read when you press the reload button or **Test Connection**, which loads the contract instead of listing it in the answer.

<figure markdown="span">
  [![The Connections window: billing, inventory, pets, shop and github with their type icons and test outcomes; shop (SQLite) selected, its tables, columns with types and a foreign key in the Browse resources panel](assets/screenshots/connections_light.png#only-light)](assets/screenshots/connections_light.png "Open full-size screenshot")
  [![The Connections window: billing, inventory, pets, shop and github with their type icons and test outcomes; shop (SQLite) selected, its tables, columns with types and a foreign key in the Browse resources panel](assets/screenshots/connections.png#only-dark)](assets/screenshots/connections.png "Open full-size screenshot")
</figure>

Write a password or token as `${secret:name}` (or pick it from **Insert secret**): the value stays
in the [Secret Store](#secrets) and is filled in only when the connection is used.
`${env:NAME}` takes an environment variable instead, and `$${` writes a literal `${`.
**Test Connection** tries the selected connection with those values. **Save** saves the list in
`connections.json` in RunPad's data folder; secrets are never written there.

### Connections in a script

In a C# script, open **Tools → Dependencies…** and the **Connections** tab, pick one of your
connections and choose **Add to Script**. Once you save, the script's code has a client for it,
named after the connection in PascalCase: `billing` is `Connections.Billing`, `orders-db` is
`Connections.OrdersDb`. For an HTTP connection the client is an `HttpClient` with the base address,
headers and timeout already set:

```csharp
var invoices = await Connections.Billing.GetStringAsync("invoices");
invoices.Dump();
```

The editor completes the client and its members right away, before the script runs.

<figure markdown="span">
  [![The editor completing Connections.Billing.GetS with GetStreamAsync and GetStringAsync](assets/screenshots/connections-completion_light.png#only-light)](assets/screenshots/connections-completion_light.png "Open full-size screenshot")
  [![The editor completing Connections.Billing.GetS with GetStreamAsync and GetStringAsync](assets/screenshots/connections-completion.png#only-dark)](assets/screenshots/connections-completion.png "Open full-size screenshot")
</figure>

The script keeps its own copy of the connection under `connections:` in its front matter (see
[Front matter](scripts.md#front-matter)), so it runs anywhere, also in the
[command-line runner](cli.md). Secrets stay `${secret:name}` placeholders in the file and are read
from the Secret Store of whoever runs the script, when it runs. RunPad itself reads them only to
test a connection or to read a gRPC server's services (see below).

A connection that needs libraries, such as a database driver, brings them as ordinary packages of
the script: **Add to Script** names the packages and versions it would add and asks first, then
adds them to the **Packages** tab, where you can see and update them like any other package, and
they are restored the same way. Packages the script already has, in any version, are left as they
are and not asked about.

The tab shows, for each of the script's connections, its name in code, whether it is the same as
yours, and what would fail the build: a missing secret, a missing setting, a missing package, or a
connection type this RunPad doesn't have. **Remove from Script** takes it out. **Import to Your
Connections** keeps a script's connection in your own list at once, for example one that came with
a script someone shared; if you already have a different connection of that name, choose **Replace
Saved Connection** or **Keep Existing**.

<figure markdown="span">
  [![The Dependencies window's Connections tab: billing in the script as Connections.Billing, github ready to add](assets/screenshots/dependencies-connections_light.png#only-light)](assets/screenshots/dependencies-connections_light.png "Open full-size screenshot")
  [![The Dependencies window's Connections tab: billing in the script as Connections.Billing, github ready to add](assets/screenshots/dependencies-connections.png#only-dark)](assets/screenshots/dependencies-connections.png "Open full-size screenshot")
</figure>

### gRPC connections

A gRPC server must offer [server reflection](https://grpc.io/docs/guides/reflection/): RunPad asks
it for its services when the script is opened or run, and writes their message and client classes
with `protoc` from the Grpc.Tools package (downloaded once from NuGet). The connection's client has
a property for each service, and its calls send the connection's metadata:

```csharp
var item = await Connections.Inventory.Stock.GetItemAsync(new Inventory.V1.ItemRequest { Sku = "A1" });
item.Dump();
```

`Connections.Inventory.Channel` and `Connections.Inventory.CallInvoker` are there for anything
else, such as a client RunPad didn't write. The script needs the `Grpc.Net.Client` and
`Google.Protobuf` packages, which **Add to Script** adds. RunPad keeps what it read from the server
and doesn't ask again on its own. After the server's services change, select the connection on the
Dependencies window's **Connections** tab and choose **Refresh**, or use **Test Connection** in
**Tools → Connections**; open scripts that use the connection pick up the new classes at once. The
command-line runner reads them again with `--refresh-connections`. A server that can't be read,
because it is down, refuses the metadata or has no reflection, fails the build with the reason, and
the **Connections** tab shows it.

An **OpenAPI/Swagger** connection has the same settings as an HTTP one (`baseAddress`, `headers`, `timeout`) and nothing else: the API's
OpenAPI 3 or Swagger 2 description, in JSON, is looked for at the usual addresses under the base address (`openapi.json`, `swagger.json`,
`swagger/v1/swagger.json`, `v3/api-docs` and a few more), with the connection's headers. The script gets a method for each operation and a
class for each schema, with bodies and answers as JSON:

```csharp
var pets = await Connections.Pets.ListPetsAsync("dog");
var added = await Connections.Pets.AddPetAsync(new Connections.PetsClient.Pet { Name = "Rex" });
```

A failed call throws an `HttpRequestException` that carries the status and the body of the answer. The connection is also an `HttpClient`, so
anything the description does not cover can be called by hand. **Tools → Connections** lists the operations by tag with their parameters,
request bodies and answers, and the models. YAML descriptions are not read.

### SQLite connections

A SQLite connection names a database file, either a full path or one relative to the script's
folder (so `data/shop.db` travels with the script). RunPad reads its tables and views when the
script is opened or run, and the connection becomes an
[Entity Framework Core](https://learn.microsoft.com/ef/core/) context with a set for each of them,
named in PascalCase: the table `order_items` is `Connections.Shop.OrderItems`, of `OrderItem`
rows. Query them with LINQ, and change them with `SaveChanges()`:

```csharp
Connections.Shop.Customers.Where(c => c.City == "Oslo").OrderBy(c => c.Name).Dump();

Connections.Shop.Customers.Add(new Connections.ShopClient.Customer { Name = "Eve", City = "Bergen" });
Connections.Shop.SaveChanges();
```

A foreign key is a pair of properties: `order.Customer` on the table that holds the key and `customer.Orders` on the table
it points to (named after the column without its `Id`, and after the table when the key is of several columns; a second key to
the same table gets the name of its column added). They are loaded as EF Core loads any navigation, so add
`using Microsoft.EntityFrameworkCore;` and `.Include(o => o.Customer)`. A key whose columns have different types, or that
points to a table without a primary key, stays an ordinary column.

In the form, **Browse...** next to the database file picks the file, and the path is entered whole (type a relative path by hand).

Views, tables without a primary key and tables whose key can be empty (`NULL`) can be read but
not changed; computed (generated) columns are read but never written. Turn on **Read only** to open
the file for reading only. The script needs the `Microsoft.EntityFrameworkCore.Sqlite` package,
which **Add to Script** adds, in the release that fits the script's target framework (EF Core 8 for `net8.0`, 9 for `net9.0`, 10 otherwise). RunPad never creates the file: a file that doesn't exist fails the
build with its full path. A relative path needs the script to be saved; **Test Connection** in
**Tools → Connections** needs a full path.

### PostgreSQL and SQL Server connections

A **PostgreSQL** or **SQL Server** connection is one connection string, with `${secret:name}` for the password, and an optional **Schema** that
limits what is read to it. Invoklet reads the database with Entity Framework Core's own reverse engineering, as `dotnet ef dbcontext scaffold` does:
tables, views, columns with their types, defaults and generated values, keys and foreign keys, named the way EF Core names them (`order_items` is
the set `OrderItems` of `OrderItem` rows). The connection becomes an EF Core context like the SQLite one: `Connections.Shop.Customers`,
navigation properties for foreign keys, and keyless views and tables that can only be read. The script needs the provider package
(`Npgsql.EntityFrameworkCore.PostgreSQL` or `Microsoft.EntityFrameworkCore.SqlServer`), which **Add to Script** adds in the release that fits the
script's target framework (EF Core 8 for `net8.0`, 9 for `net9.0`, 10 otherwise).

You do not have to put a password in the connection string. **PostgreSQL** takes it from your pgpass file when the string has none: `~/.pgpass`
(`%APPDATA%\postgresql\pgpass.conf` on Windows), or the file named by `Passfile=` in the string or by the `PGPASSFILE` environment
variable, with one `host:port:database:user:password` line per server. Invoklet and the script read it the same way, as you, so nothing is stored
in Invoklet or in the script. When the server asks for a password and no line matches, the error says which `host:port:database:user` was looked for.
**SQL Server** signs in as your Windows account with `Integrated Security=true`; on macOS and Linux that needs a Kerberos ticket (`kinit`), and the
error says so when it fails. `${env:NAME}` and `${secret:name}` work anywhere in the string.

In **Tools → Connections**, the panel on the right browses the connection: tables and views with their columns (type, `null` or
`not null`, primary key) and foreign keys, read when you select the connection. For a path relative to a script, choose that script's
folder in the field below the status line.

A gRPC connection is browsed the same way: its services, and for each the methods with their request and response messages and
whether they stream. **Test Connection** says whether the server answered and loads what it offers into the browser.

Connections work in C# scripts for now.

## Other `Util` helpers

| Member | What it does |
| --- | --- |
| `Util.CurrentQueryPath` | Full path of the running `.rpad` file, or `null` for an unsaved tab. |
| `Util.GetSecret(name)` | A secret saved in **Tools → Secret Store**; see [Secrets](#secrets). |
| `Util.Cache(func, key, duration)`, `Util.CacheAsync(…)` | Returns a cached result for `key`, or runs the function and stores its result. The cache survives across runs; `duration` is an optional time-to-live. |
| `Util.SaveString`, `Util.LoadString`, `Util.SaveBytes`, `Util.LoadBytes` | Save and load files by name in RunPad's per-user data folder. |
| `Util.Try(action)`, `Util.Try(func)` | Run code and get the exception back instead of having it thrown. |
| `Util.KeepRunning()` | Keeps the script's process alive until the returned object is disposed, for timers or background work. |
| `Util.CancellationToken` | Cancelled when you press **Stop**; see [Stopping a script](#stopping-a-script). |
| `Util.Cmd(…)` | Runs an external program, streams its output into the Results tab and returns its exit code. An overload captures the output into a string instead. |

`Util.Cache` and the `Save…` methods write files under the `UserData` folder inside
RunPad's data folder (see [Where RunPad stores data](reference.md#where-runpad-stores-data)).
Cached entries stay there until they expire or you delete the files.

## Secret Store

!!! note "Unreleased"
    Secret Store is available in development builds.

Open **Tools → Secret Store** to add named passwords, tokens or keys. Values stay hidden
until you tick **Show**. **Save** writes your edits; **Cancel** leaves the file unchanged.
A script reads a saved value without embedding it in its source:

```csharp
var token = Util.GetSecret("api-token");
```

Names are case-insensitive. A missing name raises `KeyNotFoundException` with the store's
location. On Windows values are encrypted for your Windows account; on macOS and Linux
they are plain text in an owner-only file. Expand **Storage and usage** to see its location.
`RUNPAD_SECRETS_FILE` selects a different store for the application and scripts; use an
absolute path.

If writing fails, the window stays open with your edits and an error message; fix the
file access or disk problem and retry **Save**. If another window changed the store since
you opened it, saving refuses to overwrite that newer file. Both the newer file and your
unsaved edits are preserved. Reopen the store with its latest contents and reapply your
edits before saving.

## Connections

!!! note "Unreleased"
    Connections are available in development builds, for C# scripts. The examples below use HTTP.

Open **Tools → Connections** to keep named HTTP connections with a base address, headers
and an optional timeout. Use `${secret:api-token}` in a header value to read a token from
Secret Store, or `${env:API_HOST}` for an environment variable. **Test Connection** sends
an HTTP GET with the resolved settings. Secret values are not copied into script metadata.

In **Dependencies → Connections**, add a saved connection to the script. Its settings are
copied into the script; later changes to your saved list do not change that copy. Required
libraries become ordinary script packages. **Import** copies a script connection into your
saved list immediately. Replacing a different connection with the same name requires
**Replace Saved Connection**; **Keep Existing** cancels the replacement. These imports are
already saved, so **Cancel** in Dependencies does not undo them.

The editor and compiler expose a client such as `Connections.BillingApi` for a connection
named `billing-api`. For HTTP this is an `HttpClient` with the address, headers and timeout
already set:

```csharp
var text = await Connections.BillingApi.GetStringAsync("invoices");
text.Dump();
```

Header names must be unique, ignoring letter case and surrounding spaces. Duplicates stay
editable and block saving and testing until corrected. A failed save keeps the window open
with your edits for retry. If another window or an import changed your saved list, a stale
window refuses to overwrite it: reopen the list and reapply your edits to its latest contents.
The code name `Connections` is reserved; choose another connection name.

### gRPC connections

Choose **gRPC** in **Tools → Connections**, enter the server address and any required metadata,
then use **Test Connection** to read its services. The server must support gRPC server reflection.
Add the connection to the C# script through **Dependencies → Connections**. Each service has a
client, for example:

```csharp
var response = await Connections.Inventory.Stock.GetItemAsync(
    new Inventory.V1.ItemRequest { Sku = "A1" });
response.Dump();
```

The service and message names depend on the server's contract. After the first successful build,
ordinary script edits reuse a compiled contract in that script's build cache. Changes to the schema,
SDK target or dependency assemblies require a new contract build. Completion is also available
before the first run. If the server's contract changes, read its description again with
**Test Connection** before rebuilding the script.

Scripts with `partial` extensions of generated types retain source-based compilation so the
extensions and generated declarations remain in the same assembly.

### SQLite connections

In a development build, choose **SQLite** under **Tools → Connections → Add** and enter an
existing database file. A relative path is resolved from the saved script's folder. Add the
connection on **Dependencies → Connections** to get an EF Core context and the required package
in a C# script targeting .NET 10 or later:

```csharp
using var shop = Connections.Shop;
shop.Customers.Where(c => c.City == "Oslo").ToList().Dump();
```

Tables with a non-null primary key support changes through `SaveChanges()`. Views, tables without
a primary key, and tables whose primary key permits `NULL` are available for reading without
tracking. The connection's **Read only** setting also prevents writes to the file.

SQLite `VIRTUAL` and `STORED` computed columns appear on row objects. The database calculates them;
EF Core refreshes their values after inserts and updates instead of writing assigned values.

## Stopping a script

**Stop** (++shift+f5++) ends a running script at once — unless the script listens for it. A script
that uses `Util.CancellationToken`, or a Program script whose `Main` takes a `CancellationToken`,
sees the token cancelled and gets up to 3 seconds to close files, connections and child processes
before RunPad ends it:

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
