# Naming

**(T)** means a tool enforces the rule. Everything else is checked in review.

## C#

| What | Style | Example |
|---|---|---|
| Class, struct, record, enum **(T)** | `PascalCase` | `Vehicle`, `DrivingInput` |
| Interface **(T)** | `I` + `PascalCase` | *planned:* `IScoreSource` |
| Method, property, event **(T)** | `PascalCase` | `ApplyWheel`, `Tuning` |
| `const` and `static readonly` **(T)** | `PascalCase`, never `ALL_CAPS` | `FrontLeftWheel` |
| Private field **(T)** | `_camelCase`, with no `s_` on statics | `_steerAngle` |
| Local variable, parameter **(T)** | `camelCase` | `forwardSpeed` |
| Positional record parameter | `PascalCase`, because it becomes a property | `Throttle` |
| Godot signal | `PascalCase` + `EventHandler`, which Godot requires | *planned:* `LapCompletedEventHandler` |
| Namespace **(T)** | `SupeRNR.<Feature>`, engine-free code `SupeRNR.Core.<Feature>` | `SupeRNR.Vehicles` |

The tools accept `MAX_SPEED`, so `ALL_CAPS` is a review item.

### Choosing the words

- **Values** are nouns: `forwardSpeed`. **Actions** are verbs: `ApplyWheel`.
- **Yes/no values** read as a statement: `reversing`, `onGround`.
- **Use full words.** `Acceleration`, not `Acc`. The accepted short forms are `dt` for the time
  step and `i` for a loop counter.
- **One word for one idea,** so a search finds everything. `delta` is only Godot's callback
  parameter; everywhere else it's `dt`.
- **Spelling:** code and comments use US spelling to match Godot and .NET (`tire`, `Color`). Docs
  use UK spelling (`tyre`, `licence`).

### Units

- Units are metres, seconds, kilograms and newtons. 1 Godot unit is 1 metre.
- Angles are radians unless the name says otherwise: `MaxSteerAngle` is radians.
- A tuning value's comment starts with its unit: `// m/s, ...`, `// N, total, ...`.

### Project patterns

| What | Pattern | Example |
|---|---|---|
| Input action name in code | Action name in `PascalCase` + `Action` | `SteerLeftAction` |
| Field for a child node | `_` + the node's name in `camelCase` | `_rayFrontLeft` for `RayFrontLeft` |
| Car corner | `Front` or `Rear`, then `Left` or `Right` | `RearRightWheel` |
| Event handler method | `On` + what happened | *planned:* `OnLapCompleted` |
| Test class | Class under test + `Tests` | *planned:* `LapTrackerTests` |
| Test method | `Method_Scenario_Expected` | *planned:* `RegisterCheckpoint_OutOfOrder_DoesNotCount` |

## Files and folders

| What | Style | Example |
|---|---|---|
| Folder | `snake_case` | `game/vehicles/`, `tools/drift_track/` |
| C# file | Exactly the class name | `Vehicle.cs` |
| Scene | `snake_case.tscn`, named for what it is | `vehicle.tscn`, `follow_camera.tscn` |
| Track scene | `<name>_track.tscn` | `drift_track.tscn` |
| Handling resource | `<car>_tuning.tres` | `default_tuning.tres` |
| Generated resource | `<source>_<what>.res` | `drift_track_terrain.res` |
| Python and GDScript tool | `snake_case`, starting with a verb where it runs something | `build.py`, `layout.py` |
| Main docs in `docs/` | `UPPER_CASE.md` | `ARCHITECTURE.md` |
| Standards and guides | `kebab-case.md`, guides starting with a verb | `add-a-car.md` |
| Session log | `YYYY-MM-DD_NNN_short-topic.md` | `2026-11-01_002_commits-and-next.md` |

## Scenes and nodes

Nodes are `PascalCase`.

| What | Pattern | Example |
|---|---|---|
| Scene root | The file name in `PascalCase` | `drift_track.tscn` → `DriftTrack` |
| Instanced scene | Its role in this scene | `Car`, an instance of `vehicle.tscn` |
| Child with a job | Job, then position | `RayFrontLeft`, `WheelRearRight`, `SmokeRearLeft` |
| Only child of its type | Keep Godot's type name | `CollisionShape3D`, `Camera3D` |
| Holder for similar nodes | Plural | `Walls`, `Rails`, `Jumps` |
| Numbered generated node | Name + number from 1 | `Rail1`, `RailPath1` |
| Compass direction | The direction alone | `Walls/North` |

Code finds a child by its exact name, so renaming a node means changing the matching `GetNode` too.

## Godot data

| What | Style | Example |
|---|---|---|
| Input action | `snake_case`: what the player wants, never the key | `accelerate`, `steer_left` |
| `[Export]` property | `PascalCase`; the Inspector adds the spaces | `FollowSpeed` shows as *Follow Speed* |
| `[ExportGroup]` | Sentence case | `"Engine and brakes"` |

## Project records

| What | Pattern | Example |
|---|---|---|
| Foundation task | `F-n` | `F-1` |
| Milestone 1 task | `D-n` | `D-8` |
| Code quality task | `Q-n` | `Q-1` |
| Bug | `B-n`, never reused | `B-3` |
| Decision | Next number in `DECISIONS.md` | `#21` |
| Commit | `type(scope): summary` | See [Repository](repository.md#commits) |
