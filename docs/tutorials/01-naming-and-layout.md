# 1. Naming and layout

Code is read far more often than it's written. If every file follows the same patterns,
you can tell what something is from how it looks: `_steerAngle` is a private field,
`FrontLeftWheel` is a constant or a member, `forward` is a local variable. You get that
information without scrolling up to find the declaration.

## 1. Naming conventions

| What | Style | Example from our code |
|---|---|---|
| Class, struct, record, enum | `PascalCase` | `Vehicle`, `DrivingInput` |
| Interface | `I` + `PascalCase` | *planned:* `IDriver` |
| Method, property, event | `PascalCase` | `ApplyWheel`, `Tuning` |
| `const` and `static readonly` | `PascalCase` | `FrontLeftWheel` |
| Private field | `_camelCase` | `_steerAngle` |
| Local variable, parameter | `camelCase` | `forwardSpeed`, `delta` |
| Positional record parameter | `PascalCase` | `Throttle` in `DrivingInput` |
| Namespace | `SupeRNR.<Feature>` | `SupeRNR.Vehicles` |
| C# file | same as the class | `Vehicle.cs` |
| Godot file, folder | `snake_case` | `vehicle.tscn`, `game/vehicles/` |

Three notes on that table:

**Why positional record parameters are PascalCase.** In a record, each parameter becomes a
public property, so it's named like a property:

```csharp
public readonly record struct DrivingInput(float Throttle, float Brake, float Steer, bool Handbrake)
```

A class's primary constructor parameters stay `camelCase`, because they don't become public
properties. The record is the exception.

**Why constants aren't `MAX_SPEED`.** That style comes from C and Java. Microsoft, the .NET
runtime and Godot's C# guide all use `MaxSpeed`. Our build can't catch this one: its PascalCase
check passes `MAX_SPEED`, so it's a rule for review. (We tested it.)

**Godot signals have a required suffix.** A signal is declared as a delegate whose name ends
in `EventHandler`, because Godot's source generator looks for that:

```csharp
[Signal] public delegate void LapCompletedEventHandler(double lapTime);
```

C# generates a matching `LapCompleted` event for you to subscribe to. (*Planned:* we don't
have this signal yet.)

### Who enforces this

`dotnet build` rejects most naming mistakes, through the rules in `.editorconfig`. Rider also
marks them as you type. Try it: rename `_steerAngle` to `steerAngle` and build, and you'll get:

```text
error IDE1006: Naming rule violation: Missing prefix: '_'
```

(With the verification command, warnings count as errors. Undo the rename afterwards.)

## 2. Meaningful names

A name should say what a value means, not just what type it is.

```csharp
// Hard to follow
float x = v.Dot(f);

// Clear: it's the car's speed along its own forward direction
float forwardSpeed = LinearVelocity.Dot(forward);
```

Some habits that help:

- **Yes/no values read as a statement.** `reversing`, `onGround` and `braking` make `if`
  lines read like sentences: `if (onGround && !freeRolling)`.
- **Methods are verbs, values are nouns.** `ApplyWheel` does something; `SmokeAmount` works
  something out. A method returning a value can be a noun phrase, but one that changes the
  world should start with a verb.
- **Spell words out.** `Acceleration`, not `Acc`. Microsoft's naming guide favours clarity
  over brevity. Our accepted short forms are `dt` (time step) and very common loop counters
  like `i`.
- **Say the unit when it isn't obvious.** A float called `FollowSpeed` could mean almost
  anything, so its comment says "Per second". Our units are metres, seconds and kilograms
  ([ARCHITECTURE.md](../ARCHITECTURE.md#conventions)), and angles are radians unless the name
  says otherwise.
- **One word for one idea.** `Vehicle.cs` uses `dt` in `_PhysicsProcess` and `delta` in its
  helpers for the same value, and "tire" eight times but "tyre" once. Pick one and stay with
  it, so a search finds everything.

## 3. Organising a file

### One type per file, in a namespace

```csharp
using Godot;                      // 1. usings, outside the namespace

namespace SupeRNR.Vehicles;       // 2. file-scoped namespace: the semicolon form

public partial class Vehicle : RigidBody3D   // 3. one type
{
}
```

The file-scoped form applies to the whole file, which saves a level of indentation. If a file
needs `System` usings too, they go first, before the others.

`partial` is required on anything that inherits from a Godot class, because Godot's source
generator adds a second part of the class behind the scenes.

### Order of members

The same order in every class means you know where to look:

1. `[Signal]` delegates and `event`s
2. `[Export]` properties
3. Other public properties
4. Constants and `static readonly` fields
5. Private fields
6. Godot callbacks (`_Ready`, `_Process`, `_PhysicsProcess`)
7. Public methods
8. Private methods, each close to the code that calls it

### No regions

`#region` lets an editor fold part of a file away. We don't use it, because it hides the
real problem: if a class has so many parts that you want to fold them, it probably has too
many jobs. Split it instead.

`Vehicle.cs` has a milder version of the same thing, banner comments:

```csharp
// ---------------------------------------------------------------- Steering
```

They work, but at 422 lines they're a sign the class is ready to be divided. That's on the
[backlog](../BACKLOG.md#code-quality) as Q-5.

### Formatting

- Braces go on their own line, and are always written, even around one statement.
- One statement per line.
- One blank line between members, and at most one blank line anywhere.
- Lines stay under about 100 characters. Godot's guide asks for that, and past 120 you're
  scrolling sideways. `Vehicle.cs` has 7 lines over 120.

For a long method call, break after the opening bracket:

```csharp
return new DrivingInput(
    Throttle: Input.GetActionStrength("accelerate"),
    Brake: Input.GetActionStrength("brake"),
    Steer: Input.GetAxis("steer_right", "steer_left"),
    Handbrake: Input.IsActionPressed("handbrake"));
```

`Throttle:` is a *named argument*. It's optional, but it makes it obvious which value is which.

### Comments

- On their own line, not after the code.
- A space after `//`, then a capital letter.
- Full sentences end in a period. A short label in a method, like `// Input`, doesn't need one.
- **Explain why, not what.** The code already says what it does.

```csharp
// Adds nothing: the code says the same thing
// Add one to the tipped time
_tippedTime += delta;

// Useful: nothing else in the code tells you this
// Cast now: the ray's own update would give last frame's result
ray.ForceRaycastUpdate();
```

### Language habits

These come from Microsoft's conventions, and `.editorconfig` covers most of them:

| Do | Rather than | Why |
|---|---|---|
| `int`, `string`, `float` | `Int32`, `String`, `Single` | The C# keywords are the standard |
| `var car = new Vehicle();` | `var speed = GetSpeed();` | Use `var` only when the type is visible on the line |
| `Vector3 offset = new(1f, 0f, 0f);` | Repeating the type twice | `new()` takes the type from the left side |
| `$"Mass: {Mass} kg"` | `"Mass: " + Mass + " kg"` | Interpolation reads better |
| `nameof(Tuning)` | `"Tuning"` | The compiler catches a rename |
| `int` for a count | `uint` | Signed `int` is the standard, unless you have a specific need |

Godot's guide adds one for floats: write the type suffix, as in `1f` rather than `1`, so it's
always clear you meant a `float`.

## Try it yourself

`game/cameras/FollowCamera.cs` is yours, and it has a few formatting slips that no tool
caught. `dotnet format` passes it. Fix them by eye:

1. Line 7: a blank line straight after `{`.
2. Line 8: `//The car to follow` needs a space after `//` and a period at the end.
3. Lines 9 and 10: properties with no blank line between them.
4. Lines 35 and 36: no blank line between the method and the next comment.
5. Line 36: a double space in "remaining  distance".

Then add a one-line comment above the class saying what it's for, as `Vehicle` has.

## Check yourself

1. What does `_tippedTime` tell you before you see its declaration?
2. Why isn't `MaxSteerAngle` written `MAX_STEER_ANGLE`, and would the build stop you?
3. You want to fold part of a class away. What should you ask first?
4. Which of these is good: `var slip = ComputeSlip();` or `var offset = new Vector3();`?

<details>
<summary>Answers</summary>

1. It's a private field of the class, and it holds a time.
2. PascalCase is the .NET and Godot standard. The build wouldn't stop you, so it's up to review.
3. Whether the class has too many jobs and should be split.
4. The second. In the first, you can't see the type on the line, so write `float slip = ...`.

</details>
