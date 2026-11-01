# Project structure

The repository root is also the Godot project root (`res://`). The full folder list is in the
[directory map](../INDEX.md#directory-map).

## Feature folders

Files are grouped by what they're for, not by file type. A scene, its scripts and its resources
sit together in `game/<feature>/`:

```text
game/
  cameras/    FollowCamera.cs, follow_camera.tscn
  tracks/     drift_track.tscn, drift_track_terrain.res, drift_track_terrain_shape.res
  vehicles/   Vehicle.cs, VehicleTuning.cs, PlayerDriver.cs, DrivingInput.cs,
              vehicle.tscn, default_tuning.tres
  spike/      test_track.tscn (flat test track)
```

- A feature's namespace matches its folder: `game/vehicles/` is `SupeRNR.Vehicles`.
- Create a folder when its first real file arrives, never as an empty placeholder.
- Never make folders such as `scripts/` or `scenes/`.

## Where code belongs

| Kind of code | Lives in | Example |
|---|---|---|
| Moves, draws or reads the engine | A node class in `game/<feature>/` | `Vehicle`, `FollowCamera` |
| Numbers that describe something | A `Resource` class and a `.tres` file | `VehicleTuning` |
| A small value passed around | A `readonly record struct` | `DrivingInput` |
| Rules that could run with no Godot | *planned:* `core/`, tested in `tests/` | Lap validation |
| Development tools | `tools/<tool>/` | `tools/drift_track/` |

`core/` never references Godot. Physics always stays in Godot.

## Who owns what

| Area | Owns | Never does |
|---|---|---|
| `Vehicle` | Forces and car state | Reads `Input`, knows about laps |
| `PlayerDriver` | Turns input into a `DrivingInput` | Touches physics |
| Track scene | Decides who drives, and connects car, driver and camera | Holds race rules |
| `FollowCamera` | Follows its `Target` | Changes the car |
| *planned:* Checkpoint | Reports "vehicle X crossed checkpoint N" | Holds rules |
| *planned:* Lap tracker | Lap validation and timing, in `core/` | Touches the engine |
| *planned:* `RaceSession` | One race's state, rebuilt on restart | Knows tyre forces |

Visual effects read vehicle state and never feed back into physics.

## Things outside the game

| Path | Contains | Rule |
|---|---|---|
| `docs/` | All project documentation | Has `.gdignore`, so Godot skips it |
| `private/` | Personal logs and lessons | Ignored by git and Godot; never committed |
| `assets/` | *planned:* third-party files, by source pack | Each needs a [credit](../guides/add-an-asset.md) |
| `builds/` | Exported games | Ignored by git |
| `.godot/` | Godot's cache | Generated; never committed or edited |
