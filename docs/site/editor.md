---
title: Editor
description: Syntax and semantic highlighting, completion, signature help, navigation, rename and formatting in the Invoklet editor.
---

# Editor

The editor is the code area of a tab. Most of its intelligence comes from an in-process
Roslyn workspace that follows your text as you type. That workspace exists only for C#;
Visual Basic gets the lexical features described in [Visual Basic](#visual-basic).

<figure markdown="span">
  [![A highlighted C# script with the completion list open after a dot, Take selected and its documentation beside the list](assets/screenshots/editor-highlighting_light.png#only-light)](assets/screenshots/editor-highlighting_light.png "Open full-size screenshot")
  [![A highlighted C# script with the completion list open after a dot, Take selected and its documentation beside the list](assets/screenshots/editor-highlighting.png#only-dark)](assets/screenshots/editor-highlighting.png "Open full-size screenshot")
</figure>

## Running a selection and restarting

In **Statements** and **Program** mode, select a block and choose **Run → Execute Selection**
(Ctrl+Shift+E, or Cmd+Shift+E on macOS). Only that block runs, as Statements, with the script's current
arguments, packages, namespaces and references. The command does not save the source file.
Select complete statements and include any declarations they need: variables from earlier runs
are not retained. Empty selections do nothing. With a rectangular selection (hold ++alt++ while selecting), only
the selected columns run.

In a Program script the selection is usually a few lines inside `Main` or another method. They
run on their own, without the rest of the script: a selection that uses a method, field, type or
variable declared outside it fails to compile, and the output says why. Expression scripts have no
statements to select. Selection execution runs without the debugger.

**Run → Stop and Run Again** (Alt+Shift+X, or Cmd+Shift+F5 on macOS) cancels the current execution, waits for its host to
finish and starts the whole script again using the current debug-mode setting. Pressing it
repeatedly while cancellation is pending queues only one restart. Stop cancels a queued restart.
The new full run uses the usual save/conflict checks; cancelling that save prevents execution.

Change or disable both shortcuts in **Preferences → Keyboard**.

## Extension methods in C# Program scripts

In **Program** mode, put extension methods in a top-level static class alongside `Main`:

```csharp
void Main()
{
    Console.WriteLine(21.Twice());
}

static class NumberExtensions
{
    public static int Twice(this int value) => value * 2;
}
```

The same applies to generic extension methods and custom `GetAwaiter` methods used by
an asynchronous `Main`. With a .NET 10 target, C# 14 extension blocks are supported too.
Keep extension classes at script level, rather than nesting them inside another class.
Other helper types remain nested in the query. If an extension class uses a helper type,
make that type `internal` or `public` so it is accessible from the extension class.
Methods and fields of the query remain members of the query.

## Arranging tabs and windows

Drag a tab along the strip to change the order; a line shows where it will go, and the strip scrolls
when you carry the tab to one of its ends. Drop the tab well below the strip, or outside the window,
and it opens in a window of its own; **Move to New Window** in the tab's context menu does the same.
Such a window shows only that tab, with its editor and results: no tab strip, and the connections and
scripts panels stay in the main window. Closing the window, or closing the tab with its key, puts the
tab back into the main window as it was, unsaved changes included. New scripts always open in the
main window, and opening a script that is already in a window of its own brings that window forward.
Closing the main window takes every tab back into it before asking about unsaved changes, and quits
Invoklet; the next launch opens all tabs in the main window.

## Recovering unsaved tabs

**Preferences → Editor → Restore open tabs and unsaved changes** keeps your workspace locally when you exit
Invoklet normally. It is enabled by default; restart Invoklet after changing the setting.

On the next launch, Invoklet restores the tab order, active tab, unsaved text, caret and selection,
and script settings such as arguments, references and packages. Original script files are not
modified by this checkpoint. Clean files are read again from disk; a missing file can be recovered
from its cached text. Explicitly closing a tab and choosing **Don't Save** discards that tab.

When you close Invoklet with unsaved tabs, it asks once what to do with them:

- **Save All** saves every unsaved tab, with the usual Save As for untitled ones and the usual
  question about files changed on disk. Cancelling or a failed save keeps Invoklet open; files
  saved before that stay saved.
- **Keep in Stash** closes without writing the files; the changes come back on the next launch.
- **Cancel** keeps Invoklet open.

Invoklet waits for pending Save As and file moves before writing the checkpoint. If writing fails,
the window stays open so you can retry or save your work. Recovery data is stored in the
`recovery` folder of [Invoklet's data folder](reference.md#where-invoklet-stores-data). A damaged checkpoint can fall back
to the previous one. Each running instance keeps its own recovery data. On startup Invoklet restores
the most recent session and also brings in the unsaved drafts of other Invoklet windows that have
been closed since; a running window's drafts are left alone. A draft of a file that is already
open comes back as an untitled copy named after the file, so neither version is overwritten: it
still finds the file's relative references, and it can't be saved over that file or over one
another tab has open.

The other window's draft data is deleted once Invoklet has kept the restored drafts in two of its
own checkpoints, so a damaged newest checkpoint can't lose them. Startup reports what it restored
in a notice above the editor instead of a message box; **Dismiss** hides it. Draft data that cannot be read is kept unchanged and its recovery folders are listed in
the notice (the text can be selected and copied). Damaged session data is skipped when an older
readable session is available.

This is recovery on normal exit, not continuous autosave: the latest edits are not guaranteed
after a crash or forced termination. Results, running processes, undo history and separate text
editor windows are not restored.

## Highlighting

- **Syntax highlighting** uses TextMate grammars for both languages. Choose the color
  theme in **Preferences → Interface → Editor theme**; the defaults are *Dark+* and
  *Light+*.
- **Semantic highlighting** (C#) colors identifiers by what they are after Roslyn has
  analyzed the code: types, interfaces, methods, properties, fields, events and
  namespaces each get their own color.
- **Occurrences** (C#): when the caret is on a symbol, the other uses of that symbol in
  the script are highlighted.
- **Brace matching** highlights the matching bracket. ++ctrl+bracket-right++ jumps to it.
- **Toggle Line Comment** (++ctrl+slash++, ++cmd+slash++ on macOS) comments or uncomments every
  line the selection touches, or the caret's line, with `//` in C# and `'` in Visual Basic.

## Completion

Completion (C#) opens automatically while you type:

- after `.` and `?.` for members, namespaces and static members;
- after `new ` for types you can construct, and after `override ` for members you can
  override (accepting one inserts a stub with a generated body);
- after `<` for generic type arguments;
- where an argument starts in a call, with `name:` for each parameter you can still pass by
  name;
- while you type an identifier or keyword.

Press ++ctrl+space++ to open it yourself. The list is filtered by context (no member
suggestions inside strings or comments, for example), members declared in your own
script are shown in bold, and overloads are collapsed into one entry.

**Matching**: the list matches the start of a name, then any part of it, then the starts of
its words: `ArgEnv` or `ae` finds `ArgumentsEnvironment`, `SBu` finds `StringBuilder`. When
two items match equally well, `Dump` wins over other members.

!!! tip "Control-Space on macOS"
    macOS may use Control-Space to switch input sources. If the list does not open,
    rely on automatic completion or assign the input-source switch to a different key in
    **System Settings → Keyboard → Keyboard Shortcuts**.

**Automatic imports**: the list also offers types from namespaces that are not imported
yet: from the script's NuGet packages and its own assemblies as soon as you type, and from
.NET itself after three letters. Each shows its namespace, so when two libraries have a
type of the same name you pick the one you mean. Accepting one adds its namespace to the
`namespaces:` list in the front matter instead of inserting a `using` line.

**Commit characters**: with **Preferences → Editor → Accept completion when typing
punctuation** turned on (the default), typing punctuation such as `.`, `(` or `;` accepts
the selected item and then inserts the character. Space and `=` never accept an item.
With the option off, accept items with ++enter++, ++tab++ or a click.

### Snippets

In C#, type a snippet keyword and press ++tab++ to expand it. The caret lands where you
continue typing.

| Keyword | Expands to |
| --- | --- |
| `for`, `foreach`, `while`, `do` | Loops |
| `if`, `ife` | `if` and `if`/`else` |
| `try`, `tryf` | `try`/`catch (Exception ex)` and `try`/`finally` |
| `sw` | `switch` with a `case` and `default` |
| `prop`, `propg` | Auto-property with `set` or `private set` |
| `class`, `interface`, `ctor` | Type and constructor skeletons |
| `using`, `lock` | `using (var resource = …)` and `lock (…)` blocks |

After `prop` or `propg` expands, ++tab++ moves from the type to the name; ++enter++ or
++esc++ leaves the snippet. Typing `{` does not insert `}`: instead, pressing ++enter++ in a new,
still empty class, struct, record or interface body offers a property template, which ++tab++
inserts; any other key leaves the code as it is.

## Signature help and quick info

- **Signature help** (C#) opens when you type `(` or `,` in a call, or with
  ++ctrl+shift+space++. It picks the overload that matches the arguments typed so far,
  highlights the current parameter and shows the XML documentation summary. Inside a
  nested call such as `Send(new Message(` it shows the inner call, and the outer one again
  once the inner `)` is typed; a named argument such as `timeout:` highlights that parameter.
  A long signature lists one parameter per line instead of running off the editor.
- **Quick info** (C#): rest the mouse pointer over a symbol for a moment to see its
  signature and documentation.
- **Inlay hints** (C#) show the inferred type next to `var` declarations and `foreach`
  variables. They are off by default; turn them on in **Preferences → Languages → .NET → C#**.

## Navigation and refactoring

These commands work on the symbol under the caret in C# scripts. The table shows the
Windows and Linux defaults; see [Keyboard shortcuts](reference.md#keyboard-shortcuts)
for the macOS equivalents and custom bindings.

| Command | Key | What it does |
| --- | --- | --- |
| Go to Definition / Implementation | ++f12++ | If the symbol has implementations (for example an interface method), goes to the implementation, or shows a menu when there are several. Otherwise goes to the definition. A definition outside your script opens decompiled in the [Assembly Explorer](assembly-explorer.md). |
| Find References | ++shift+f12++ | Shows a menu of every line in the script that uses the symbol; pick one to jump there. |
| Rename | ++f2++ | Asks for a new name and renames every occurrence in the script as one undoable edit. |
| Format Document | ++shift+alt+f++ | Reformats the whole script with Roslyn's formatter. |
| Format Selection | ++ctrl+shift+f++ | Reformats the lines of the selection, or the line of the caret when nothing is selected; the rest of the script stays as it is. |
| Resolve All Usings | ++ctrl+alt+u++ | Adds the namespace for every unresolved type that has exactly one match. |
| Go to Line | ++ctrl+g++ | Asks for a line number and moves the caret there. |

++f2++ in the file tree renames the selected file instead; the key acts on whichever
panel has focus.

## Find and replace

Press ++ctrl+f++ to search the script, or ++ctrl+h++ (++cmd+alt+f++ on macOS) to search
and replace. A strip opens across the top of the editor with the search field; the
arrow at its left shows or hides the replacement row. Text selected on one line becomes
the search text.

<figure markdown="span">
  [![The search strip over the editor: word in the search field, the replacement row open, 10 of 10 matches, and every match highlighted in the code](assets/screenshots/editor-search_light.png#only-light)](assets/screenshots/editor-search_light.png "Open full-size screenshot")
  [![The search strip over the editor: word in the search field, the replacement row open, 10 of 10 matches, and every match highlighted in the code](assets/screenshots/editor-search.png#only-dark)](assets/screenshots/editor-search.png "Open full-size screenshot")
</figure>

- Every match is highlighted as you type, and the strip counts them, for example
  *3 of 10*.
- The buttons inside the search field turn on **Match case** (*Cc*), **Match whole words**
  (*W*) and **regular expressions** (*.\**).
- ++f3++ and ++shift+f3++, or the arrows, go to the next and previous match.
- **Replace** (++alt+r++) replaces the current match and moves to the next one;
  **Replace All** (++alt+a++) replaces every match as one undoable edit.
- ++escape++ closes the strip.

All of these keys can be changed in **Preferences → Keyboard**, and the buttons' hints show
the keys you assigned. On a narrow window the strip keeps every button within reach.

## Folding, display and zoom

- **Folding**: multi-line C# blocks fold at braces; a one-line `{ … }` block does not get
  a folding arrow. Visual Basic blocks fold at keyword pairs such as `Sub` … `End Sub`.
  Move the pointer over the folding column in the gutter to reveal the arrows for open
  blocks. A folded block keeps its arrow visible so you can expand it. Turn folding off
  in **Preferences → Editor → Show code folding**.
- **Display options** in **Preferences → Editor**: line numbers, visible tabs, spaces
  and line ends, a column ruler at column 80, word wrap, font size and font family.
- **Zoom**: hold ++ctrl++ (++cmd++ on macOS) and turn the mouse wheel, use **View → Zoom In**
  and **Zoom Out**, or use the percentage button at the
  right of the status strip. It offers *Zoom In*, *Zoom Out*, *Reset (100%)* and presets
  from 50% to 200% of the font size set in Preferences.

### Interface scale and shared code zoom

Available from **Invoklet 0.6.0**.

**Preferences → Interface → Interface scale** changes the size of the interface,
including code, results, panels, dialogs, icons and click targets. The default is 100%;
choose a preset from 90% to 200%. The native macOS menu bar keeps its system size.

Code zoom is an extra adjustment on top of the interface scale. It is shared by existing
and new editor tabs and separate code editor windows, and both settings are kept after
a restart. Use the wheel, **View → Zoom In / Zoom Out**, or the percentage button in the
status strip to change it. **Preferences → Editor → Code zoom** shows the current value.

For example, font size 12 with interface scale 125% and code zoom 110% gives an effective
code size of 16.5. Resetting interface scale leaves code zoom alone, and resetting code
zoom leaves interface scale alone. To return to the original sizes, reset both to 100%.

## Visual Basic

Visual Basic scripts compile, run and debug like C# scripts, and the editor provides
syntax highlighting, brace matching and keyword-based folding. There is no completion,
signature help, quick info, live diagnostics, quick fixes, semantic highlighting, inlay
hints, snippets, rename, navigation or formatting for Visual Basic yet. Compiler errors
appear in the Results tab when you run the script.

## Tabs and spaces

In **Preferences → Editor → Editing**, enable **Use tab characters for indentation**
to insert tabs, or leave it cleared to insert spaces. **Indent / tab width** controls
both the indentation size and the displayed width of a tab (1–16 columns, default 4).
The settings apply immediately to open editors, automatic indentation and C# formatting.
Changing them does not rewrite existing text; format the document to normalize indentation.
