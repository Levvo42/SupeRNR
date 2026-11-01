# 4. Design

Design is deciding which class knows what. A good test: when a rule of the game changes, you
should be able to name the one place to change it.

## 1. SOLID

SOLID is five guidelines for classes. They're easiest to understand through our own game.

### S: Single responsibility

*A class should have one reason to change.*

`PlayerDriver` only turns keyboard and gamepad input into a `DrivingInput`. `Vehicle` never reads
`Input`. So changing the controls can't break the physics, and the car doesn't care who's driving.

Our planned race code follows the same rule: a checkpoint only reports "vehicle X crossed
checkpoint N" and holds no rules. The lap tracker decides what that means.

`Vehicle` itself is the class under most strain here: forces, drift assists, flip recovery, wheel
meshes and smoke all live in it. That's normal for a class that grew out of a spike, and it's
backlog item Q-5.

### O: Open/closed

*Add behaviour by adding things, not by editing what works.*

A new car in SupeRNR is a new `.tres` file holding a `VehicleTuning`. No code changes, and no
`SportsCar : Vehicle` subclass. Data beats inheritance when the only difference is numbers.

### L: Liskov substitution

*A child class must work anywhere its parent works.*

`Vehicle : RigidBody3D` is one: anything that takes a `RigidBody3D` can take a `Vehicle`, which
is why `FollowCamera.Target` works on our car. The rule matters most when you override behaviour.
A *planned* AI driver must keep `Read()`'s promises, such as `Steer` staying between -1 and 1,
or code that relies on them breaks.

### I: Interface segregation

*Many small interfaces beat one big one.*

If `Vehicle` only needs `Read()`, it shouldn't depend on something that also offers
`GetPlayerName()` and `ShowRumble()`. Ask for the least you need.

### D: Dependency inversion

*Depend on the idea, not the detail.*

`Vehicle` depends on `DrivingInput`, a plain value that says what the driver wants. It doesn't
depend on keyboards. That's why humans, bots and network players can all share it later.

### When to add an interface

Only when the second implementation arrives. With one implementation you'd be guessing at the
shape, and you'd have two things to maintain instead of one. This is a project rule.

### A Godot limit on interfaces

Godot's `[Export]` can't hold a plain C# interface, because the Inspector only understands engine
types. This doesn't compile:

```csharp
[Export] public IDriver? Driver { get; set; }   // error GD0102
```

An abstract class that extends `Node` does work, so that's the likely shape for the AI milestone:

```csharp
public abstract partial class Driver : Node
{
    public abstract DrivingInput Read();
}
```

`abstract` means nobody can create a plain `Driver`, and every child class must supply `Read`.
`PlayerDriver : Driver` and a later `AiDriver : Driver` then both fit in `[Export] public Driver?
Driver`. (*Planned.* It's a decision for milestone 3, not something to build now.)

### Composition over inheritance

Inherit from Godot types because you must (`Vehicle : RigidBody3D`). For our own code, build
things out of parts. In a Godot scene that's natural: a car is a body with child rays, meshes and
particles. It *has* wheels; it doesn't *inherit* wheelness.

## 2. Dependency injection, the Godot way

*Dependency injection* sounds grand, but it only means: **a class is handed what it needs,
instead of going out to find it.**

```csharp
// Finds its own target: breaks if the scene is rearranged
RigidBody3D target = GetNode<RigidBody3D>("/root/DriftTrack/Car");

// Is handed its target: works in any scene
[Export] public RigidBody3D? Target { get; set; }
```

The second version is what `FollowCamera` does, and it's real dependency injection. The track
scene is the "injector": it knows about both the camera and the car, and connects them.

Larger .NET applications use a *DI container* library for this. We don't need one, because Godot
creates our nodes with an empty constructor, and the scene tree already wires them up.

The three ways we pass dependencies:

| Where | How | Example |
|---|---|---|
| Node to node, set up in a scene | `[Export]` | `Vehicle.Driver`, `FollowCamera.Target` |
| Plain C# class | Constructor parameter | *Planned:* `new LapTracker(checkpointCount)` |
| Node built by code | The parent sets a property, before `AddChild` | *Planned:* spawning AI cars |

What to avoid:

- `GetParent().GetParent()`, or absolute paths like `/root/...`. The node now depends on exactly
  where it sits.
- Autoloads for game state. They make everything depend on a global, and restarting a race means
  resetting it by hand. See [decision 12](../DECISIONS.md).

`GetNode<T>("RayFrontLeft")` on your own children is fine. Those children belong to your scene,
and you control them.

## 3. How objects talk

The rule is **call down, signal up**, and it's the same in Godot's own docs:

```text
       Track
      /     \            A parent calls methods on its children.
   Car       Hud         A child announces events to whoever is listening.
                         Siblings go through their parent.
```

A child never needs to know who's above it, which keeps it reusable.

### C# events

Between C# classes, we use `event`:

```csharp
public sealed class LapTracker
{
    // Anyone can listen. Only this class can fire it.
    public event Action<TimeSpan>? LapCompleted;

    private void FinishLap(TimeSpan lapTime)
    {
        LapCompleted?.Invoke(lapTime);
    }
}
```

Line by line:

- `Action<TimeSpan>`: the listener must be a method taking one `TimeSpan` and returning nothing.
- `?`: with no listeners, the event is null.
- `LapCompleted?.Invoke(lapTime)`: calls every listener, and does nothing if there are none.

Listening looks like this:

```csharp
public override void _Ready()
{
    _tracker.LapCompleted += OnLapCompleted;
}

public override void _ExitTree()
{
    _tracker.LapCompleted -= OnLapCompleted;
}

private void OnLapCompleted(TimeSpan lapTime)
{
    _lapLabel.Text = lapTime.ToString(@"m\:ss\.ff");
}
```

**Always unsubscribe.** A plain C# event doesn't know when a node is freed. If you skip `-=`, the
tracker keeps calling a dead node, and you get an `ObjectDisposedException`. Use a named method
as above: a lambda (`time => ...`) can't be removed, because you have no name to remove.

(*Planned:* none of this code exists yet. It's the shape D-9 and D-11 will take.)

### C# events or Godot signals?

| Use | When |
|---|---|
| C# `event` | Between our C# classes. Type-checked, and needed in `core/`, where Godot doesn't exist |
| `[Signal]` | Wiring in the Godot editor, or engine nodes such as `Area3D.BodyEntered` |

## 4. Protecting state

*Encapsulation* means a class controls how its data changes.

```csharp
public int LapCount { get; private set; }
```

Anyone can read `LapCount`, but only the class can set it, so nobody can skip the rules. Value
types help too: `DrivingInput` is `readonly`, so a value can't change after it's made.

`sealed` is another useful promise. It says nobody may inherit from a class, which keeps your
options open, and it works on Godot node classes. Use it on new classes unless you're designing
for inheritance.

## 5. Where code belongs

| Kind of code | Lives in | Example |
|---|---|---|
| Moves, draws or reads the engine | A node in `game/<feature>/` | `Vehicle`, `FollowCamera` |
| Numbers that describe something | A `Resource` and `.tres` file | `VehicleTuning` |
| A small value passed around | A `readonly record struct` | `DrivingInput` |
| Rules that need no engine (*planned*) | `core/`, tested in `tests/` | Lap validation |

The test for `core/`: could this run with no Godot at all? Lap rules can. They only need numbers
and times. That's what makes them easy to unit test.

## Try it yourself

D-9, the lap tracker, is coming up. Before writing any code, answer these on paper:

1. Which class decides whether a checkpoint was crossed in the right order?
2. Which class knows the car touched a checkpoint?
3. How does the HUD find out a lap finished?
4. What does the lap tracker need in its constructor?
5. Where does the current time come from, and how does the tracker get it?

Then compare your answers with [ARCHITECTURE.md](../ARCHITECTURE.md#race-rules-and-ownership).

## Check yourself

1. Why isn't a new car a subclass of `Vehicle`?
2. What's the dependency injection in `FollowCamera`?
3. Why can't you write `[Export] public IDriver? Driver`?
4. What goes wrong if a HUD subscribes to a C# event and never unsubscribes?

<details>
<summary>Answers</summary>

1. Cars differ in numbers, so they differ in data: a new `.tres` file.
2. `Target`. The scene hands it the car, and the camera never looks it up.
3. `[Export]` only supports types Godot understands. An interface isn't one (error GD0102).
4. When the HUD is freed, the event still calls it, which throws `ObjectDisposedException`.

</details>
