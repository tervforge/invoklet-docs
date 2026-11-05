---
title: View your scripts in Obsidian
description: Read and edit the notes and code of your .inv scripts in Obsidian with the Invoklet Markdown plugin.
---

# View your scripts in Obsidian

A `.inv` script is a Markdown document, so it reads well outside Invoklet too. The
**Invoklet Markdown** plugin for [Obsidian](https://obsidian.md) opens `.inv` files in
Obsidian's own Markdown editor: the title and notes look like any other note, the code shows
as a highlighted C# or Visual Basic block, and the parts Invoklet needs are protected from
accidental edits. Use it to browse your scripts, write the notes around the code, or keep
them next to the rest of your knowledge base.

[Download the plugin](https://github.com/tervforge/obsidian-inv/releases/latest/download/invoklet-markdown.zip){ .md-button .md-button--primary }
[Installation](#install-the-plugin){ .md-button }

<figure markdown="span">
  [![The debugging-loop.inv example open in Obsidian: a collapsed Properties section, the title and notes as formatted Markdown, and the C# code block](assets/screenshots/obsidian_light.png#only-light)](assets/screenshots/obsidian_light.png "Open full-size screenshot")
  [![The debugging-loop.inv example open in Obsidian: a collapsed Properties section, the title and notes as formatted Markdown, and the C# code block](assets/screenshots/obsidian.png#only-dark)](assets/screenshots/obsidian.png "Open full-size screenshot")
</figure>

## What you get

- **Reads like a note.** The title and the Markdown before and after the code are rendered
  as usual; the lines that open and close the run block are hidden, so only the code shows.
- **Properties at a glance.** The [front matter](scripts.md#front-matter) (kind, language,
  packages and so on) is shown as a read-only *Properties* table, collapsed by default.
- **Edit notes and code.** Change the title, the description and the documentation after the
  code, or the code itself, with Obsidian's normal Markdown editing.
- **Protected structure.** The front matter and the lines that open and close the run block
  cannot be changed, and the run block cannot be closed early or duplicated. Extra code blocks
  are not allowed in the notes, so the script keeps exactly one runnable block. An edit that
  would break this is simply not applied.
- **Placeholders for empty parts.** An empty title, description or documentation shows a
  hint where the text goes; a missing one can be added with one click.
- **Your language.** The plugin follows Obsidian's interface language and is translated into
  16 languages.

The plugin does not run or debug scripts — open them in Invoklet for that.

## Install the plugin

You need Obsidian 1.8.7 or later.

1. Download [**invoklet-markdown.zip**](https://github.com/tervforge/obsidian-inv/releases/latest/download/invoklet-markdown.zip)
   from the [latest release](https://github.com/tervforge/obsidian-inv/releases/latest).
2. In Obsidian, choose **Open folder as vault** and select the **Invoklet Queries** folder in
   your Documents folder — the folder Invoklet keeps your scripts in (see
   [Where your scripts live](scripts.md#where-your-scripts-live)). Any other vault works too;
   the plugin opens every `.inv` file in it.
3. Open **Settings → Community plugins**, turn off *Restricted mode* if it is on, and click
   the folder icon next to *Installed plugins* to open the vault's plugins folder.
4. Unpack the archive into that folder. It contains a `runpad-markdown` folder with the
   plugin's three files (`main.js`, `manifest.json` and `styles.css`), so you end up with
   `.obsidian/plugins/invoklet-markdown/`.
5. Back in **Community plugins**, click the reload icon and turn on **Invoklet Markdown**.

Your `.inv` files now appear in Obsidian's file list and open in the Markdown editor. If
they do not appear, turn on **Settings → Files and links → Detect all file extensions**.

To **update** the plugin, unpack the archive from the new release over the old
`runpad-markdown` folder, then turn **Invoklet Markdown** off and on again (or restart Obsidian).

## Things to know

- Obsidian keeps its settings in a `.obsidian` folder inside the vault. Leave it in place.
  Invoklet 0.6.0 hides folders whose names start with a dot, including `.obsidian`, and their
  contents from the sidebar's **Scripts** section; it does not delete or change them on disk.
  In Invoklet 0.5.0 those folders are still visible.
- Avoid editing the same script in Invoklet and Obsidian at the same time. Invoklet rewrites the
  front matter when it saves (see [How Invoklet edits the file](scripts.md#how-invoklet-edits-the-file)),
  so make changes in one app and save before switching to the other.
  If another program changed the disk file, Invoklet now asks before replacing it; choose
  Save As to keep both versions, Overwrite to replace the disk version, or Cancel to
  keep editing. It does not automatically merge or reload external edits.
- A file whose structure the plugin does not recognise — no front matter, a missing or
  unclosed run block, or several run blocks — opens read-only, so the problem stays visible
  instead of being "fixed" automatically.
- The protection applies to editing in Obsidian's editor. Other Obsidian plugins, sync tools
  and other programs can still change the file.

The plugin is free and open source under the MIT license; the code and issue tracker are in
[`tervforge/obsidian-inv`](https://github.com/tervforge/obsidian-inv).
