# C#

Based on Microsoft's conventions, the .NET runtime style and Godot's C# guide. Where they differ,
[decision 21](../DECISIONS.md) records our choice. **(T)** means a tool enforces the rule.

Names are in [Naming](naming.md).

## File layout

```csharp
using Godot;

namespace SupeRNR.Vehicles;

// One line saying what this class is for.
public partial class Example : Node
{
}
```

- **(T)** Usings go first, outside the namespace, with `System` usings before the others.
- **(T)** Namespaces are file-scoped: the form ending in a semicolon.
- One type per file. A small type used by only one class is the exception.
- Each class has a one-line comment saying what it's for.
- `partial` is required on any class inheriting from a Godot type.
- New classes are `sealed` unless they're designed for inheritance.
- No `#region`, and no new banner comments. If a class needs them, split it.

Members go in this order:

1. `[Signal]` delegates and `event`s
2. `[Export]` properties
3. Other public properties
4. Constants and `static readonly` fields
5. Private fields
6. Godot callbacks (`_Ready`, `_Process`, `_PhysicsProcess`, `_ExitTree`)
7. Public methods
8. Private methods, near the code that calls them

## Formatting

- **(T)** Four spaces, LF line endings, a final newline, no trailing spaces.
- **(T)** Braces are always written and sit on their own line.
- One statement per line.
- One blank line between members, and never two in a row.
- Lines stay under 100 characters where possible, with 120 as the limit.
- For a long call, break after the opening bracket and name the arguments:

```csharp
return new DrivingInput(
    Throttle: Input.GetActionStrength(AccelerateAction),
    Brake: Input.GetActionStrength(BrakeAction),
    Steer: Input.GetAxis(SteerRightAction, SteerLeftAction),
    Handbrake: Input.IsActionPressed(HandbrakeAction));
```

## Language

| Use | Rather than |
|---|---|
| **(T)** Written access modifiers: `private float _lead;` | `float _lead;` |
| **(T)** `_lead` | `this._lead` |
| `int`, `string`, `float` | `Int32`, `String`, `Single` |
| `var car = new Vehicle();` | `var speed = GetSpeed();`, where the type isn't visible |
| `Vector3 offset = new(1f, 0f, 0f);` | Writing the type twice |
| `1f` | `1` for a `float` |
| `$"Mass: {Mass} kg"` | `"Mass: " + Mass + " kg"` |
| `nameof(Tuning)` | `"Tuning"` |
| `int` for a count | `uint` |

Godot and `System` share some type names, such as `Environment` and `Range`. With both usings,
qualify the one you mean: `System.Environment.Version`.

Modern C# is fine when it's clearer. C# 14's `field`, `?.=`, `extension` blocks and `with` on Godot
structs all compile here.

## Comments

- On their own line, with a space after `//` and a capital letter.
- Full sentences end with a period. A short label such as `// Input` doesn't need one.
- Explain why, not what. If the code already says it, leave it out.
- New public types and tuning values use `///`. Everything else uses `//` when the reason isn't
  obvious.
- A wrong comment is a bug. Change it with the code.

## Methods and data

- A method does one job, described without "and".
- Four parameters at most. Values that travel together become a `readonly record struct`.
- Maths goes in `private static` methods that use only their parameters.
- Don't repeat knowledge. Make a helper at the third copy, if the copies mean the same thing.
- Bare numbers: handling values go in `VehicleTuning`; anything else unexplained gets a named
  `const`. `0f`, `1f` and `2f` in plain arithmetic are fine.
- State others may read is `{ get; private set; }`.
- Add an interface with its second implementation, never before.
- No placeholder classes or speculative interfaces. Build the current milestone only.

## Null

- Nullable checking is on. Never leave a nullable warning unresolved.
- `= null!` is only for node references assigned in `_Ready`. Any other `!` needs a comment.
- An optional `[Export]` is marked `?`; a required one isn't.
- Handle a maybe-null value with `is null`, `?.` or `??`:

```csharp
DrivingInput input = Driver?.Read() ?? DrivingInput.None;
```

## Errors

| Situation | Example | Do this |
|---|---|---|
| Expected | No driver assigned, car in the air | Check it. No exception |
| Setup mistake in a node | Required export missing | Report it and turn the node off |
| Bad argument to a plain class | Checkpoint count of zero | Throw |
| Outside world | Damaged save file | Catch the specific exception, or use a `Try` method |

A setup check in a node:

```csharp
public override void _Ready()
{
    if (Target is null)
    {
        GD.PushError($"{Name}: no Target assigned.");
        SetPhysicsProcess(false);
        return;
    }
}
```

Throwing, in a plain class:

| Situation | Use |
|---|---|
| Required argument is null | `ArgumentNullException.ThrowIfNull(argument);` |
| Number out of range | `ArgumentOutOfRangeException.ThrowIfNegative(value);` and similar |
| Argument invalid in another way | `throw new ArgumentException("message", nameof(argument));` |
| Object not in a usable state | `throw new InvalidOperationException("message");` |

- Never `throw new Exception`, an empty `catch`, or `catch (Exception)`.
- **(T)** Rethrow with `throw;`, not `throw exception;`.
- Don't use exceptions for normal flow. Use checks and `Try` methods such as `int.TryParse`.

## Performance

Clear code comes first. These rules apply to code that runs every physics tick; elsewhere, only
change code for performance when a profiler shows a problem. Use Godot's **Debugger → Profiler**
for time, and Rider's profiler for C# allocations, which Godot's tools don't show.

- No allocations per tick: no new classes, lists or arrays, string building, LINQ or capturing
  lambdas. Structs such as `Vector3` are fine.
- Cache strings passed to Godot in a `static readonly StringName` field:

```csharp
private static readonly StringName AccelerateAction = "accelerate";
```

- Use the generated `SignalName`, `MethodName` and `PropertyName` instead of typed strings.
- When one method reads an engine property repeatedly, keep it in a local. Don't do this when the
  method also assigns that property, because the local goes out of date.
- Use .NET collections by default, and Godot collections only when calling the engine.
