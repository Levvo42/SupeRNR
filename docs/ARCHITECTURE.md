# Architecture

This document has two parts: **Current** (what exists in the repository) and
**Intended direction** (agreed design that is not built yet). Move material from the second
part to the first only when it is implemented. The reasoning behind each choice lives in
[DECISIONS.md](DECISIONS.md).

## Current

- **Engine:** Godot 4.7.2 .NET, Forward+ renderer, D3D12 on Windows.
- **Physics:** Jolt Physics (3D), default 60 Hz physics tick.
- **C# project:** `SupeRNR.sln` → `SupeRNR.csproj` (Godot.NET.Sdk 4.7.2), `net10.0`,
  nullable reference types enabled, root namespace `SupeRNR`. The solution carries Godot's
  `Debug`, `ExportDebug` and `ExportRelease` configurations.
- **Gameplay code:** the car and its driving input in `game/vehicles/`. `Vehicle` is the
  D-1 spike with braking, reverse and slide handling added. The test tracks are still in
  `game/spike/`.
- **Handling tuning:** every handling number lives in a `VehicleTuning` resource (`Resource`
  with `[Export]` properties), one `.tres` per car; `default_tuning.tres` is the only one so
  far. Different cars mean different data, not subclasses. `Vehicle` copies `Tuning.Mass`
  onto its own `Mass` in `_Ready`, so the mass is set in the resource, not on the node.
- **Camera:** `game/cameras/follow_camera.tscn` is placed in each track, and its `Target`
  export is set to the car. The child `Camera3D` holds the fixed view angle (decision 19).
  `FollowCamera` moves the root node towards a point ahead of the car, smoothly and without
  ever turning it. Physics interpolation is on, so anything moved in `_PhysicsProcess` looks
  smooth at any frame rate.
- **Input actions** (`project.godot`): `accelerate`, `brake`, `steer_left`, `steer_right`,
  `handbrake`. Keyboard uses physical keys (arrows and WASD, Space); gamepad uses the
  triggers, the left stick and the bottom face button. Deadzone 0.2.
- **Driving input:** `PlayerDriver` is the only code that reads Godot's `Input`. Each physics
  tick the car asks the `PlayerDriver` assigned to its `Driver` export for a `DrivingInput`,
  an immutable `readonly record struct` of throttle, brake, steer and handbrake. With no
  driver assigned the car gets `DrivingInput.None`. The `PlayerDriver` node sits next to the
  car in the track scene, because the track decides who drives.
- **Track generator:** `tools/drift_track/` builds the spike drift track's terrain and rails
  from the corner table in `layout.py` (Python, plus one Godot script). With the editor
  closed, run `python tools/drift_track/build.py`, then
  `godot_console --headless --path . --script res://tools/drift_track/build_terrain.gd`.
  `preview.py` prints the layout checks without Godot. The ramps are not generated.

## Conventions

- **Layout:** feature folders under `game/`. A scene, its scripts and its resources live
  together (see [INDEX.md](INDEX.md#directory-map)).
- **Naming:** folders and Godot files in `snake_case` (`vehicle.tscn`, `sports_car.tres`);
  C# files in `PascalCase` matching the class (`Vehicle.cs`). Godot requires the class
  name of an attached script to match its file name.
- **Namespaces:** `SupeRNR.<Feature>`, in PascalCase and file-scoped, e.g. `SupeRNR.Vehicles`,
  `SupeRNR.Race`, `SupeRNR.Tracks`, `SupeRNR.Cameras`, `SupeRNR.UI`. Engine-free code uses
  `SupeRNR.Core.<Feature>`.
- **One type per file.** Small closely related types (e.g. an enum used only by one class)
  are the exception, not the rule.
- **Nodes and nullability:** node references assigned in `_Ready` are declared
  `private Foo _foo = null!;` and assigned via `GetNode<Foo>(...)` or `[Export]`.
- **Communication:** C# `event`s between C# classes; Godot `[Signal]`s only when wiring in
  the editor or talking to engine nodes. Parents call down to children; children report up
  with events or signals.
- **No autoloads (global singletons) for game state.** Race state belongs to a session
  object that can be recreated on restart.
- **Units:** 1 Godot unit = 1 metre; seconds; masses in kg. Handling tuning assumes this.
- **Formatting and style:** `.editorconfig`, enforced by `dotnet format`.
- **Godot/System name clashes:** Godot defines types that share names with `System` types
  (e.g. `Environment`, `Range`). With both `using Godot;` and `using System;`, qualify the
  one you mean (`System.Environment.Version`).

## Intended direction (not implemented)

### Driving input is separate from vehicle physics

```
PlayerDriver (reads Godot Input) ──► DrivingInput ──► Vehicle (applies forces)
AiDriver        (milestone 3)    ──►      ▲
Network input   (later)          ──►──────┘
```

- Humans, bots and (later) remote players all drive the same `Vehicle`. An input-source
  interface is introduced when the second source (AI) is built, not before.
- Because input is a plain value, it can later be sent to an authoritative server as a
  per-tick command.

### Vehicle physics

- `Vehicle` extends `RigidBody3D`. Jolt handles collisions, ramps and impacts; our code
  applies arcade forces each physics tick: engine/brake force along the car's forward
  axis, and a lateral-grip force that opposes sideways sliding up to a limit. Exceeding
  the limit (or using the handbrake) produces a drift.
- Visual-only effects (body roll, skid marks, smoke) read vehicle state and never
  feed back into physics.

### Race rules and ownership

- A `RaceSession` node owns the race state for one session and is rebuilt on restart.
- Checkpoints (`Area3D`) only report "vehicle X crossed checkpoint N". They contain no rules.
- Lap validation and timing are plain C# classes (e.g. a lap tracker) in the engine-free
  `core/` library, unit-tested in `tests/`. The Godot side feeds them events and time,
  and the HUD displays their state.
- The vehicle knows nothing about laps; the race knows nothing about tyre forces.

### Engine-free core library (introduced with lap timing)

- `core/SupeRNR.Core.csproj`: a plain `net10.0` class library with **no Godot reference**,
  so the compiler enforces the boundary. It is referenced by `SupeRNR.csproj` and by
  `tests/SupeRNR.Core.Tests.csproj` (xUnit).
- `core/` and `tests/` contain a `.gdignore` and are excluded from the Godot project's
  compile items.
- Only rules that are naturally engine-independent go here. Physics stays in Godot.

### Networking (long-term, no code now)

- Server-authoritative: clients send `DrivingInput`, the server simulates and sends state
  snapshots, clients interpolate. Jolt is not deterministic, so full rollback netcode is
  not a goal; see DECISIONS.md.
