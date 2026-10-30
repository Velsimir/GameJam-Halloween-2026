# Changelog

## [0.3.0]

### Added
- Localization tab: picking a string (click, right-click on a key, a new key) also opens it in the Editor tab without switching tabs. Undo/Redo, recheck, sync and import do not change the Editor target. The 🔒 lock, an open "was / now" dialog and unapplied Editor edits keep the current target; unwritten words of a scene string open the "was / now" dialog first. New members: `LocalizationTab.TargetSelected`, `EditorTab.ShowTableKey(ITextEditTarget)`.
- `TableKeyTarget(ITableBackend backend, string table, string key, Func<string> locale)` — an edit target for a table string (reads through `MarkupLayers.Display`, writes through `MarkupLayers.Write`; read-only for the pseudo-locale). The Localization tab publishes it with the shared language, not the language of the clicked column; while the Editor tab has no unapplied edits it shows the current table text. If the shared language changes while there are unapplied edits, the target is pinned to the language the edit was made in (`IsPinned`, `Pinned(locale)`, `Followed()`; the title shows that language) and "Apply" writes there; once nothing is unapplied, it follows the shared language again and rereads the text.

### Changed
- Built-in string tables (`TableTextSource`): the language picked in the Localization tab's «Сцена:» dropdown (`SelectLocale`) is remembered in EditorPrefs (`Typecraft.SceneLocale`) and applied by `Activate()`, so Play Mode and new editor sessions start in it instead of the base language. Editor only; builds choose the language as before.
- Dialogues tab: the language dropdown is no longer a board-only preview setting. Picking a language calls `ITextSource.SelectLocale` like «Сцена:» (global, remembered in `Typecraft.SceneLocale`, works in Edit and Play Mode), and the dropdown follows `Typecraft.Locale` when its toolbar reloads.
- Editor tab: the preview language dropdown is shared too — it calls `ITextSource.SelectLocale` (same language as «Сцена:» and the Dialogues tab, remembered in `Typecraft.SceneLocale`) and follows `Typecraft.Locale` whenever it changes; a table string without unapplied edits switches its text with it.

### Known limitations
- "Apply" on a string opened from the Localization tab writes changed words to the table at once, without the "was / now" dialog.
- With unapplied edits in the Editor tab, cell edits made meanwhile in the Localization tab are not picked up: "Apply" writes the Editor's words over them.

## [0.2.0]

### Added
- Dialogue module: `DialogueAsset` graphs, conditions and flags, `DialogueRunner`, `DialoguePlayer` (with `State` / `Restore` for saves), box, subtitle and world-text presenters, `DialogueTrigger`, `DialogueInputAdvance`, the Dialogues tab with a GraphView board (`TYPECRAFT_NO_DIALOGUE_GRAPH` switches to a list view).
- Markup layer: styling lives next to the string table, translators see plain text; stale markup falls back to plain text.
- Table sync with CSV/TSV, `.xlsx` and Google Sheets; export to xlsx, CSV, TSV and JSON; key usage search.
- Custom styles, material presets and effects from the editor.
- Icons sit on the baseline; image checks and per-icon offset and scale.
- Responsive window layout with resizable panes.
- Welcome window and automatic install of TMP Essential Resources, the bundled TMPEffects 0.3.2 and its resources.
- Showcase sample, Manual and API documentation in Russian and English.

### Changed
- `ITextSource`: `BaseLocale` and `Request(table, key, locale, Action<TextEntry>)`.
- `EXTENDING.md` merged into `Documentation~/API.*.md`.

### Fixed
- `<!wait>` and other TMPEffects commands on localized text.

## [0.1.0]

- First version: markup, style and icon libraries, input prompts, font roles, string tables, the Typecraft window.
