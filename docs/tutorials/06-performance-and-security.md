# 6. Performance and security

Both of these go wrong in the same way: time is spent on the imagined problem instead of the
real one. For each, start by working out what actually matters in *this* game.

## Part 1: Performance

### Measure first

The game has 16.6 milliseconds to make each frame at 60 FPS. A change only helps if it saves time
somewhere that time is being spent, so look before you optimise:

- **Debugger → Profiler** in the Godot editor shows time per function.
- **Debugger → Monitors** shows frame time, physics time and object counts.
- Godot's memory tools [don't show C# allocations](https://github.com/godotengine/godot-docs/issues/9916),
  so use Rider's profiler for those.

Clear code comes first. Our current code has no measured performance problem, so nothing below
is urgent. These are habits for new code, and they matter more when several cars are on track.

### How often does it run?

How careful to be depends on how often code runs:

| Runs | Example | How careful |
|---|---|---|
| Per tick, per car or per wheel | `ApplyWheel`, `ApplySuspension` | Careful |
| Per tick, once | `PlayerDriver.Read`, `FollowCamera` | Reasonably careful |
| On an event | Lap completed, checkpoint hit | Write it clearly |
| Once | `_Ready`, loading | Write it clearly |

### Allocations and the garbage collector

Each `new` of a *class* puts an object on the heap. When enough pile up, .NET's *garbage collector*
pauses to clean them. In a racing game, a pause is a visible stutter, so we avoid creating garbage
every tick. Good news: **structs don't count**. `Vector3`, `Basis` and `DrivingInput` are values,
so `new Vector3(...)` costs nothing here.

These allocate, so avoid them in per-tick code. They're fine anywhere else:

| Allocates | Example | Instead |
|---|---|---|
| New classes, lists, arrays | `new List<float>()` | Create once as a field, and reuse with `Clear()` |
| String building | `$"Speed: {speed}"` | Only when the value changes |
| LINQ | `laps.Where(IsValid).ToList()` | A plain `foreach` in hot code |
| Lambdas that capture a local | `x => x + offset` | A named method |

LINQ is good in code that isn't hot. Finding the best lap once per lap is exactly what it's for.

### Crossing into the engine

Godot's engine is C++, and your C# talks to it across a bridge. Plain C# maths is fast, but each
read of a property like `LinearVelocity` crosses that bridge. So when one method reads the same
property many times, Godot's docs suggest keeping it in a local:

```csharp
// Three crossings
float a = LinearVelocity.X;
float b = LinearVelocity.Z;
float c = LinearVelocity.Length();

// One crossing
Vector3 velocity = LinearVelocity;
float a = velocity.X;
float b = velocity.Z;
float c = velocity.Length();
```

One catch: a local is a copy. If you assign `LinearVelocity = ...` afterwards, your local is now
out of date. `ApplyGlide` does exactly that, which is a good reason not to change it casually.
`ApplyForce` doesn't have this problem, since a force only affects the velocity at the next
physics step.

`Vehicle.cs` reads `LinearVelocity` on 14 lines and `AngularVelocity` on 13. We're leaving that
alone until the profiler says it matters.

### Strings passed to Godot

This is the one improvement ready to make today. `PlayerDriver` runs every physics tick:

```csharp
Throttle: Input.GetActionStrength("accelerate"),
```

Godot's method wants a `StringName`, not a `string`. C# quietly converts it, and that creates a new
`StringName` object on every call: five per tick. Keep one instead:

```csharp
private static readonly StringName AccelerateAction = "accelerate";
```

Breaking that line down:

- `static`: one copy for the whole class, not one per object.
- `readonly`: set once, never reassigned.
- Why not `const`? `const` only works for simple values known at compile time, like numbers and
  strings. A `StringName` is an object created when the game runs.
- `= "accelerate"`: the same conversion as before, but it now happens only once.

The gain here is small, with one player. It's still worth doing, because it's easy, and input
checks like these are the kind of code that gets copied around.

Godot generates the same thing for your own members, so use these rather than typed strings:

```csharp
EmitSignal(SignalName.LapCompleted, lapTime);    // Instead of "LapCompleted"
```

### Collections

| Use | When |
|---|---|
| `List<T>`, `Dictionary<K, V>`, arrays | By default, for anything inside C# |
| `Godot.Collections.Array` and `Dictionary` | Only when handing data to the engine |

Each read of a Godot collection crosses the bridge, and it's why LINQ on them is especially slow.

### Physics or process?

- `_PhysicsProcess`: fixed 60 Hz. Anything that affects movement.
- `_Process`: once per drawn frame. Only for visuals that don't move physics objects.

Physics interpolation is on, so code in `_PhysicsProcess` still looks smooth at any frame rate.

## Part 2: Security

Most general C# security advice is about websites. SQL injection, for example, needs a database,
and we don't have one. These are the risks a game does have.

### The main rule: untrusted data

Anything from outside the game is *untrusted*: save files someone edited, a downloaded mod, a
packet from another player. It might be damaged or deliberately malicious. **Check it, and never
let it choose what code runs.**

| Risk | Rule |
|---|---|
| Save files (*planned*) | Plain data as JSON through `System.Text.Json`. Validate every value after loading |
| Loading objects from bytes | Never use `BinaryFormatter` (.NET has removed it) |
| Godot `FileAccess.GetVar` | Leave `allowObjects` as `false` for anything from outside |
| Downloaded `.tres` or `.tscn` | Treat them as code: Godot resources can carry scripts. Load only our own |
| Multiplayer (*planned*) | The server decides. Never trust what a client says about itself |
| File paths from outside | Only inside `user://` |

Godot's documentation is blunt about `GetVar` with objects allowed:

> Deserialized objects can contain code which gets executed. Do not use this option if the
> serialized object comes from untrusted sources to avoid potential security threats such as
> remote code execution.

### Multiplayer

The planned design already has the right shape: clients send only their `DrivingInput`, and the
server simulates everything. A client that says "I'm doing 900 km/h" has nothing to say that to.
Input can still be checked: a server should clamp `Steer` to the allowed range, in case someone
sends 50.

### Things in the repository

- **No secrets in git.** No API keys, tokens or passwords, even in a private repository. History
  keeps them after you delete them.
- **Check dependencies.** `dotnet list package --vulnerable` asks NuGet about known problems, and
  `dotnet restore` warns about them too.
- **Only add packages you need.** Each one is code running on your players' machines.
- **Assets:** each new one needs a licence entry in `assets/CREDITS.md`.

## Try it yourself

This is `PlayerDriver.cs` today. Rewrite it so every action name is a cached `StringName`:

```csharp
using Godot;

namespace SupeRNR.Vehicles;

public partial class PlayerDriver : Node
{
    public DrivingInput Read()
    {
        return new DrivingInput(
            Throttle: Input.GetActionStrength("accelerate"),
            Brake: Input.GetActionStrength("brake"),
            Steer: Input.GetAxis("steer_right", "steer_left"),
            Handbrake: Input.IsActionPressed("handbrake"));
    }
}
```

1. Add five `private static readonly StringName` fields above `Read`, one per action. Use names
   ending in `Action`, like `AccelerateAction`.
2. Replace each string in `Read` with its field.
3. Build, press F5, and check all five inputs still work.

A typo in an action string is exactly as wrong as before, but it's now in one place.

## Check yourself

1. Does `new Vector3(1f, 2f, 3f)` in `_PhysicsProcess` create garbage?
2. Why can't `AccelerateAction` be a `const`?
3. Should we rewrite `Vehicle.cs` to cache every `LinearVelocity` read today?
4. A player's save says their best lap is -5 seconds. What should the game do?

<details>
<summary>Answers</summary>

1. No. `Vector3` is a struct, a value that isn't placed on the garbage-collected heap.
2. `const` needs a compile-time value of a simple type, and `StringName` is an object.
3. No. Nothing shows it's a problem, and a local can go out of date when the velocity is assigned.
4. Reject that value, because it fails validation, and keep running without it.

</details>
