# Move or rename a file

Moving a file outside Godot breaks `res://` references, so always start in the editor.

## Scenes, resources and folders

1. In the Godot editor's **FileSystem** dock, drag the file to its new folder, or right-click it and
   choose **Rename**.
2. Save any scene the editor shows as changed.
3. [Finish the change](finish-a-change.md). Git should show the file as renamed, with its `.uid`
   file if it has one.

## C# files

The class name must match the file name, so these change together.

1. Rename or move the `.cs` file in the **FileSystem** dock, so the script's `.uid` moves with it.
2. In Rider, rename the class to match, using **Refactor → Rename** so every use changes.
3. If it moved to another feature, change its `namespace` to match the new folder.
4. Build, then check the scenes using it still load.

## Then search for the old name

The editor only updates Godot's own references. Search the repository for the old path and fix what
it finds, usually:

- `project.godot` is updated by the editor, but check `run/main_scene`.
- `tools/drift_track/` holds the track's paths in `build.py` and `build_terrain.gd`.
- `drift_track.tscn` refers to its `.res` terrain files by path, so move them together.
- `README.md`, `docs/ARCHITECTURE.md` and `docs/INDEX.md` name main files and folders.

Old entries in `DECISIONS.md` stay as written.
