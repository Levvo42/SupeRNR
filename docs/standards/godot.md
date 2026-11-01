# Godot

Names are in [Naming](naming.md), and where files go is in
[Project structure](project-structure.md).

## Scenes

- A thing used in more than one place is its own scene.
- A scene works by itself: it reaches nothing outside its own root.
- The track scene is where things are connected. It instances the car, the driver and the camera,
  and fills in their exports.
- A value that belongs to one placement is set on the instance; a value every placement shares is
  set in the scene or its resource.

## Dependencies

A class is handed what it needs instead of going out to find it. There's no DI container, because
`[Export]` and the scene tree do that job.

| Where | How | Example |
|---|---|---|
| Node to node, set up in a scene | `[Export]` | `Vehicle.Driver`, `FollowCamera.Target` |
| Node to its own child | `GetNode<T>("Name")` in `_Ready` | `GetNode<RayCast3D>("RayFrontLeft")` |
| Plain C# class | Constructor parameter | *planned:* `new LapTracker(checkpointCount)` |
| Node built by code | Parent sets a property before `AddChild` | *planned:* AI cars |

Never use `GetParent().GetParent()`, `/root/...` paths, or autoloads for game state. Race state
belongs to a session object that can be recreated on restart.

`[Export]` can't hold a C# interface (error GD0102). Where two node types must fit one export, use
an abstract class that extends `Node`.

## Communication

Call down, signal up. A parent calls methods on its children, a child announces events, and
siblings go through their parent.

| Use | When |
|---|---|
| C# `event` | Between our C# classes, and always in `core/` |
| `[Signal]` | Wiring in the editor, or engine nodes such as `Area3D.BodyEntered` |

Every `+=` on a C# event has a matching `-=`, usually in `_ExitTree`. A freed node stays subscribed
otherwise, and the event then throws `ObjectDisposedException`. Subscribe a named method, because a
lambda can't be removed.

## Exports and resources

- Handling numbers live in `VehicleTuning`, with one `.tres` per car. Different cars are different
  data, never subclasses.
- A `Resource` class has `[GlobalClass]`, so the Inspector can create and assign it.
- `[ExportGroup]` sections group related values.
- Use `[Export(PropertyHint.Range, ...)]` where a value has hard limits.
- A value that must also be on the node is copied in `_Ready`, like `Mass = Tuning.Mass;`.

## Input

- Only `PlayerDriver` reads `Input`. Everything else uses `DrivingInput`.
- Every action has a keyboard and a gamepad binding.
- Keyboard bindings use physical keys, so layouts other than QWERTY work.
- Actions use a deadzone of 0.2.

The current actions are listed in [ARCHITECTURE.md](../ARCHITECTURE.md#current).

## Physics and timing

| Callback | Runs | Use for |
|---|---|---|
| `_PhysicsProcess` | Fixed 60 Hz | Anything that moves things or affects movement |
| `_Process` | Once per drawn frame | Visuals that don't touch physics objects |

- Physics interpolation is on, so code in `_PhysicsProcess` looks smooth at any frame rate.
- Use `GlobalBasis` for world directions. `Basis` is relative to the parent.

## Engine behaviour

Tested in Godot 4.7.2. Check anything else with a throwaway file rather than from memory.

| What | What happens |
|---|---|
| Exception in a callback such as `_Ready` | It's logged and the game keeps running. The rest of that callback is skipped |
| `GetNode<T>` with a missing node | Logs an error and returns `null`, though its type says otherwise |
| `GetNode<T>` with the wrong type | Throws `InvalidCastException` |
| `GetNodeOrNull<T>` either way | Returns `null` quietly |
| A freed node | Isn't `null`. Check with `GodotObject.IsInstanceValid` |
| `SetPhysicsProcess(false)` in `_Ready` | Stops `_PhysicsProcess` being called |
| A sub-resource in a scene | Shared between instances unless made unique (B-3) |

So when the log is full of errors, read the **first** one, not the most frequent. Use `GetNode<T>`
for nodes that must exist, and `GetNodeOrNull<T>` with a check for optional ones.

## Working with files

- Move and rename Godot files in the editor, following the
  [guide](../guides/move-or-rename-a-file.md).
- Commit `.uid` and `.import` files.
- Don't edit `project.godot` or scenes on disk while the editor has the project open, because the
  editor overwrites them.
- Leave no debug `GD.Print` in committed code. Use `GD.PushError` and `GD.PushWarning` for problems.
