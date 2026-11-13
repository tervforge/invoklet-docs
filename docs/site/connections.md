---
title: Connections
description: Save and browse HTTP, OpenAPI, gRPC and database connections, then use them in C# scripts.
---

# Connections

Add a saved connection to a C# script to use it through the generated `Connections` class.

## Manage connections

**Tools → Connections** keeps your saved connections in one window. It supports HTTP, OpenAPI/Swagger,
gRPC, SQLite, PostgreSQL and SQL Server. **Add** creates one of a type; the form shows
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
in the [Secret Store](secrets.md) and is filled in only when the connection is used.
`${env:NAME}` takes an environment variable instead, and `$${` writes a literal `${`.
**Test Connection** tries the selected connection with the resolved values. **Save** writes the
settings to `connections.json` in Invoklet's data folder. A `${secret:name}` reference is saved as
a placeholder. A password or token entered directly in a field, header, or connection string is
saved as part of the settings. Use placeholders for credentials, especially if you plan to share
a script that uses the connection. Literal parts of PostgreSQL and SQL Server connection strings,
HTTP and OpenAPI headers, and gRPC metadata are passed to the script at startup. Secret and
environment placeholders are resolved when the client is first used.

## Connections in a script

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
from the Secret Store of whoever runs the script, when it runs. Invoklet resolves them when you run the script, test the connection or explicitly refresh its
contract.

A connection that needs libraries, such as a database driver, brings them as ordinary packages of
the script: **Add to Script** names the packages and versions it would add and asks first, then
adds them to the **Packages** tab, where you can see and update them like any other package, and
they are restored the same way. Packages the script already has, in any version, are left as they
are and not asked about.

The tab shows, for each of the script's connections, its name in code, whether it is the same as
yours, and what would fail the build: a missing secret, a missing setting, a missing package, or a
connection type this Invoklet doesn't have. **Remove from Script** takes it out. **Import to Your
Connections** keeps a script's connection in your own list at once, for example one that came with
a script someone shared; if you already have a different connection of that name, choose **Replace
Saved Connection** or **Keep Existing**.

<figure markdown="span">
  [![The Dependencies window's Connections tab: billing in the script as Connections.Billing, github ready to add](assets/screenshots/dependencies-connections_light.png#only-light)](assets/screenshots/dependencies-connections_light.png "Open full-size screenshot")
  [![The Dependencies window's Connections tab: billing in the script as Connections.Billing, github ready to add](assets/screenshots/dependencies-connections.png#only-dark)](assets/screenshots/dependencies-connections.png "Open full-size screenshot")
</figure>

## gRPC connections

A gRPC server must offer [server reflection](https://grpc.io/docs/guides/reflection/): Invoklet asks
it for its services when the script is opened or run, and writes their message and client classes
with `protoc` from the Grpc.Tools package (downloaded once from NuGet). The connection's client has
a property for each service, and its calls send the connection's metadata:

```csharp
var item = await Connections.Inventory.Stock.GetItemAsync(new Inventory.V1.ItemRequest { Sku = "A1" });
item.Dump();
```

`Connections.Inventory.Channel` and `Connections.Inventory.CallInvoker` are there for anything
else, such as a client Invoklet didn't write. The script needs the `Grpc.Net.Client` and
`Google.Protobuf` packages, which **Add to Script** adds. Invoklet keeps what it read from the server
and doesn't ask again on its own. After the server's services change, select the connection on the
Dependencies window's **Connections** tab and choose **Refresh**, or use **Test Connection** in
**Tools → Connections**; open scripts that use the connection pick up the new classes at once. The
command-line runner reads them again with `--refresh-connections`. A server that can't be read,
because it is down, refuses the metadata or has no reflection, fails the build with the reason, and
the **Connections** tab shows it.

In **Tools → Connections**, double-click a gRPC service, method, or generated file to open its
source in [Assembly Explorer](assembly-explorer.md).

## OpenAPI/Swagger connections

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

## SQLite connections

A SQLite connection names a database file, either a full path or one relative to the script's
folder (so `data/shop.db` travels with the script). Invoklet reads its tables and views when the
script is opened or run, and the connection becomes an
[Entity Framework Core](https://learn.microsoft.com/ef/core/) context with a set for each of them,
named in PascalCase: the table `order_items` is `Connections.Shop.OrderItems`, of `OrderItem`
rows. Query them with LINQ, and change them with `SaveChanges()`:

```csharp
Connections.Shop.Customers.Where(c => c.City == "Oslo").OrderBy(c => c.Name).Dump();

Connections.Shop.Customers.Add(new Connections.ShopClient.Customer { Name = "Eve", City = "Bergen" });
Connections.Shop.SaveChanges();
```

A foreign key is a pair of properties, named as Entity Framework Core's reverse engineering names them: `order.Customer` on the table that holds
the key and `customer.Orders` on the table it points to (when a table has two keys to the same one, the names say which: `ParcelShippers` and
`ParcelReceivers`). They are loaded as EF Core loads any navigation, so add `using Microsoft.EntityFrameworkCore;` and `.Include(o => o.Customer)`.
The tables, views, sets and rows are named by EF Core too: `order_items` is the set `OrderItems` of `OrderItem` rows, and `log` is `Logs` of `Log`.

In the form, **Browse...** next to the database file picks the file, and the path is entered whole (type a relative path by hand).

Views, tables without a primary key and tables whose key can be empty (`NULL`) can be read but
not changed; computed (generated) columns are read but never written. Turn on **Read only** to open
the file for reading only. The script needs the `Microsoft.EntityFrameworkCore.Sqlite` package,
which **Add to Script** adds, in the release that fits the script's target framework (EF Core 8 for `net8.0`, 9 for `net9.0`, 10 otherwise). Invoklet never creates the file: a file that doesn't exist fails the
build with its full path. A relative path needs the script to be saved; **Test Connection** in
**Tools → Connections** needs a full path.

## PostgreSQL and SQL Server connections

A **PostgreSQL** or **SQL Server** connection is one connection string, with `${secret:name}` for the password, and an optional **Schema** that
limits what is read to it. The Connections window shows the string's main settings as fields of their own (**Host**, **Port**, **Database**,
**Username** and **Password** for PostgreSQL; **Server**, **Database**, **User** and **Password** for SQL Server) above the whole text: a field
changes only its setting, and settings without a field (`SSL Mode=Require`, `TrustServerCertificate=true`) are written in the text and kept as
they are. The keys the server also takes are recognised, such as `Server` for `Host` or `Initial Catalog` for `Database`. A new connection
starts with the server's own defaults: `Host=localhost;Port=5432;Database=postgres;Username=postgres` for PostgreSQL,
`Server=localhost;Database=master;User Id=sa;TrustServerCertificate=true` for SQL Server.

**Sign in with** chooses how the connection signs in, and shows only the fields that way uses:

| Server | Ways to sign in |
|--------|-----------------|
| PostgreSQL | **Password**; **pgpass file** (no password field; the file named, else `PGPASSFILE`, else `~/.pgpass`); **Kerberos (GSSAPI)** (`Integrated Security=true`) |
| SQL Server | **SQL Server login**; **Windows (Integrated Security)** (`Integrated Security=true`, no user or password); **Microsoft Entra ID** (`Authentication=Active Directory Default`) |

The choice is part of the connection string, so it is read back from the text: a string with no password and no pgpass file named opens as
**Password** with the password empty, which PostgreSQL then looks up in pgpass as it would anyway. Invoklet reads the database with Entity Framework Core's own reverse engineering, as `dotnet ef dbcontext scaffold` does:
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


## Sidebar navigation

The workspace sidebar is organized into three collapsible sections:

- **Connections** — saved connections and their user-defined groups;
- **Scripts** — the files in the current workspace;
- **Samples** — bundled examples grouped by folder.

The three sections use the same tree rows, icons, selection behavior and keyboard navigation.
Open sections share the sidebar's height: drag the line between two open sections to give one of them more
room and the other less. The heights are remembered between launches.
Connections can be reordered or moved between groups with the context menu, keyboard commands or
drag and drop. A connection can also be dragged onto the editor, or added with **Add to Script**.
The sidebar remembers which sections, connection groups and sample folders were expanded between
launches. Grouping is local navigation state and does not change a script's `connections:` metadata.

Invoklet ships HTTP, OpenAPI, gRPC, SQLite, PostgreSQL and SQL Server drivers as plugins.
Manage their startup state under **Preferences → Plugins**. A disabled driver becomes unavailable
after restarting Invoklet. The CLI uses the same plugin settings.
To add a driver of your own, follow [Write a connector plugin](connector-plugins.md).

Connection rows show whether a connection is used by the active script and explain missing drivers,
secrets, environment variables or unavailable/stale contracts. Use **Refresh** on a connection, or
**Refresh All**, to re-check its driver and contract.
