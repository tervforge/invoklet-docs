---
title: Debugging
description: Breakpoints, conditions, stepping, locals, watches, call stack, threads and exception breaks with netcoredbg.
---

# Debugging

Invoklet debugs C# and Visual Basic scripts with [netcoredbg](https://github.com/Samsung/netcoredbg),
an open-source .NET debugger, over the Debug Adapter Protocol. The setup window installs it
on first run, or you can point **Preferences → Languages → .NET → Debugger path** at an
existing `netcoredbg` executable. See [Installation](install.md#first-run-net-sdk-and-debugger).

<figure markdown="span">
  [![The debugging-loop example paused on line 5 in its third iteration: the step buttons are in the toolbar, Threads and Call Stack on the left of the Debugger tab and Locals on the right](assets/screenshots/debugging_light.png#only-light)](assets/screenshots/debugging_light.png "Open full-size screenshot")
  [![The debugging-loop example paused on line 5 in its third iteration: the step buttons are in the toolbar, Threads and Call Stack on the left of the Debugger tab and Locals on the right](assets/screenshots/debugging.png#only-dark)](assets/screenshots/debugging.png "Open full-size screenshot")
</figure>

## Start a debug session

1. Click the **bug** button in the toolbar (*Toggle debug mode*). It stays highlighted
   while debug mode is on.
2. Set at least one breakpoint (see below).
3. Press ++f5++ or click **Run**.

With debug mode on, Run starts the script under the debugger. With it off, Run starts the
script normally and breakpoints are ignored. The debug-only toolbar buttons (Step Over,
Step Into, Step Out, Pause and Run to Cursor) appear only while a session is running.

## Breakpoints

Click the breakpoint column in the gutter beside the line numbers to add a breakpoint
on that line. Click the breakpoint again to remove it. Breakpoints can be added and
removed during a session; the
debugger receives the change the next time execution stops.

The gutter icon shows each breakpoint's state:

| Icon | Meaning |
| --- | --- |
| Filled circle | Set. Before the session starts, or until the debugger answers, it has not been checked yet. |
| Filled circle in the "verified" color | The debugger bound the breakpoint to code. |
| Hollow ring | The debugger rejected it, or the breakpoint is disabled. Hover over it to read the debugger's reason, for example that there is no code on that line. |
| Small diamond badge | The breakpoint has a condition or hit count. |

A breakpoint on a blank line, a comment or a line with only a brace usually ends up
unverified, because the compiler generated no code there. Move it to a line with a
statement.

### Conditions and disabling

Right-click the gutter on a line (or Control-click on macOS) to open the breakpoint condition dialog
centered on the window. It works for a line with or without a breakpoint:

- **Enabled** — clear it to keep the breakpoint in the gutter without stopping there.
  Disabled breakpoints are not sent to the debugger.
- **Condition** — an expression evaluated in the script's
  context, such as `i == 3`.

The condition stays on one line. **Done** applies the edit.
Press Enter to apply or Escape to cancel. Changes also update an active debug session.
C# conditions are checked without executing them and must resolve to Boolean at the breakpoint.
Errors disable Done. Scoped name and member suggestions support Down + Enter or a double-click.
Escape cancels the edit; clicking outside the modal dialog leaves it open.
Runtime evaluation is still performed by netcoredbg; its expression support can differ
from the C# compiler.

## When execution is paused

The line about to run is highlighted and marked with an arrow in the gutter. The
**Debugger** tab under the editor has these panels:

| Panel | Contents |
| --- | --- |
| **Locals** | Variables in the current scope with name, value and type. Objects and collections expand on demand. |
| **Watch** | Expressions you add with the **+** button and edit in the *Expression* column. They are re-evaluated every time execution stops. The **−** button deletes the selected row. |
| **Call Stack** | The method, file and line of each frame. |
| **Threads** | ID and name of every thread in the script's process. |

Threads and Call Stack are permanent tables on the left, separated by a draggable divider.
Locals and Watch share the right-hand pane.

While execution is paused, rest the pointer on a variable in the editor to see its value. Over
`c` in `c.Dump()` the tip shows `c`; over `Name` in `order.Customer.Name` it shows the whole
`order.Customer.Name`. The tip is a tree like **Locals**: each member shows its name, type and
value, and the arrow beside a member opens it under its parent. The tip stays open while you move
the pointer into it, and closes when the pointer leaves both the variable and the tip.

### Stepping

| Action | Default key | Toolbar |
| --- | --- | --- |
| Continue | ++f5++ | Run/Continue button |
| Step Over | ++f10++ | Step Over |
| Step Into | ++f11++ | Step Into |
| Step Out | ++shift+f11++ | Step Out |
| Pause | — | Pause |
| Run to Cursor | ++ctrl+f10++ | Run to Cursor (runs until the line that holds the caret) |
| Stop | ++shift+f5++ | Stop |
| Stop and Run Again | ++alt+shift+x++ | — (in the **Run** menu) |

The same commands are in the **Run** menu. The keys can be changed in
**Tools → Preferences... → Keyboard**; see [Keyboard shortcuts](reference.md#keyboard-shortcuts). On macOS, function keys may need
the ++fn++ key depending on your keyboard settings.

### Breaking on exceptions

Open the **Output settings** gear at the right of the Results/Debugger/IL header:

- **Break on unhandled exceptions** pauses when an exception is about to end the script.
- **Break on all exceptions (including caught)** pauses whenever an exception is thrown,
  even if a `catch` handles it.

The settings apply to the next debug session.

When the debugger stops on an exception, the top of the **Debugger** panel shows its
type and message. Additional details, such as a stack trace and inner exception, appear
when the debugger supplies them; limited debugger support may leave only a short message.

An unhandled exception can prevent execution from reaching later breakpoints. For
example, `int.Parse("not-a-number")` throws before the next statement runs. Read the
exception notice first: **Continue** lets an unhandled exception end the script, while
**Stop** ends the debug session. A step rejected by the debugger leaves execution paused
and shows a notice; it does not mean the step succeeded.

## Practice script

Use the `debugging-loop.inv` example from the Samples section. Its code block is:

```csharp linenums="1"
var total = 0;
for (var i = 1; i <= 5; i++)
{
    var square = Square(i);
    total += square;
    Console.WriteLine($"i = {i}, square = {square}, running total = {total}");
}

total.Dump("Sum of squares 1..5");

int Square(int value)
{
    var result = value * value;
    return result;
}
```

Try this:

1. Turn debug mode on and click the gutter on line 4. Press ++f5++. Execution stops on
   line 4 with `i` = 1 in **Locals**.
2. Press ++f11++ to step into `Square`. Execution moves to line 13. Press ++shift+f11++
   to return.
3. Add the watch expression `total * 2` in **Watch**, then press ++f5++ a few times and
   watch it change.
4. Right-click the breakpoint, enter the condition `i == 3` and click **OK**. Stop with
   ++shift+f5++ and run again: the first stop is now at `i` = 3.

Without a debugger the script prints five lines, from `i = 1, square = 1, running total = 1`
to `i = 5, square = 25, running total = 55`, and then a block **Sum of squares 1..5**
with the value 55.

## Limitations

- *Set Next Statement* is not available.
- Debug mode needs a configured netcoredbg; without one, use normal runs.

## Troubleshooting

**Debugging does nothing or fails to start.** Check **Preferences → Languages → .NET →
Debugger path**. It must point to the `netcoredbg` executable itself
(`netcoredbg.exe` on Windows), and the file must exist. Restarting Invoklet without a valid
path shows the setup window again, where **Install** downloads a matching build.

**The script runs, but breakpoints are ignored.** Make sure the bug button is
highlighted before you press Run. Without debug mode, breakpoints are not used.

**Later breakpoints are skipped, or stepping does not move.** Check the exception and
notice above the Debugger panels. A statement before the breakpoint may have thrown an
unhandled exception. If the debugger rejects stepping from that stop, use **Continue**
or **Stop**, correct the failing statement and start a new session.

**A breakpoint shows a hollow ring.** Hover over it to read the debugger's reason. Most
often the line has no executable code; move the breakpoint to a statement. Also check
that the breakpoint has not been disabled in its right-click dialog.

**No .NET SDK found.** See [Troubleshooting](reference.md#troubleshooting).
