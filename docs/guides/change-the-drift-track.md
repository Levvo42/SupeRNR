# Change the drift track

The terrain and rails are generated from `tools/drift_track/layout.py`. Everything else in
`drift_track.tscn` is edited by hand.

| Part | Change it in |
|---|---|
| Road, widths, banking, edge styles, rails | `layout.py` |
| Terrain files (`.res`) | Never by hand: they're generated |
| `Rails` node | Never by hand: it's rewritten on every build |
| Walls, ramps, start line, car position | The Godot editor |

1. **Close the Godot editor,** because the build rewrites the scene.
2. **Edit the corner table** in `layout.py`. Its comments explain each column.
3. **Check the layout** without Godot:

   ```sh
   python tools/drift_track/preview.py
   ```

4. **Rebuild** the scene's rails and the terrain data, then bake the terrain:

   ```sh
   python tools/drift_track/build.py
   godot_console --headless --path . --script res://tools/drift_track/build_terrain.gd
   ```

5. **[Check the track](../standards/verification.md#handling-and-track-changes)** with an overhead
   render, then drive it.
6. **Update the lap time and length** if `ARCHITECTURE.md` or a decision quotes them.
7. **[Finish the change](finish-a-change.md).** Commit `layout.py`, `drift_track.tscn` and both
   `.res` files together.
