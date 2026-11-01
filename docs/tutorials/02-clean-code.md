# 2. Clean code

Clean code is about keeping changes cheap. The question to ask of any code is: when this has
to change, how many places do I have to touch, and how much must I hold in my head?

## 1. Don't repeat yourself

DRY means **each piece of knowledge lives in one place**. It's about knowledge, not about
lines that happen to look alike.

`Vehicle.cs` builds the car's velocity with the height removed in two places (lines 150 and
251):

```csharp
Vector3 flatVelocity = new(LinearVelocity.X, 0f, LinearVelocity.Z);
```

That's one idea, "movement across the ground", written twice. A small helper names it once:

```csharp
// Removes the height part of a vector, leaving movement across the ground.
private static Vector3 Flat(Vector3 vector)
{
    return vector with { Y = 0f };
}
```

Going through that line by line:

- `private`: only this class can call it.
- `static`: it belongs to the class and uses no car data, so it can't accidentally change the
  car. Everything it needs comes in through its parameter.
- `vector with { Y = 0f }`: makes a copy of the vector with `Y` changed. `Vector3` is a struct,
  so `with` works on it. The original isn't touched.

Now the call reads `Vector3 flatVelocity = Flat(LinearVelocity);`, and the same helper handles
`new Vector3(forward.X, 0f, forward.Z)` at lines 151 and 262.

### Don't merge too early

Two pieces of code that look the same today might change for different reasons. A common
guide is the **rule of three**: copying once is fine, and the third copy is when you make a
helper. If the two uses mean different things, leave them apart even at three. A wrong
abstraction costs more than a little duplication.

### Don't write the same thing four times per wheel

`Vehicle` repeats itself for each wheel: ten `GetNode` lines, four `ApplySuspension` calls, four
`ApplyWheel` calls. The usual fix is an array and a loop, with a small type holding one wheel's
parts. That's a bigger change, so it's a backlog item (Q-5) rather than an exercise here.

## 2. No magic numbers

A *magic number* is a bare value in code with no name, so a reader can't tell what it means or
whether it's safe to change.

```csharp
bool reversing = forwardSpeed < -3f;      // Line 76: why 3?
if (!onGround || flatVelocity.Length() < 5f)  // Line 255: why 5?
```

Our rule: **a number that affects how the car feels goes in `VehicleTuning`**, one property
with a comment saying what it does and in what units:

```csharp
// Metres per second backwards before the car counts as reversing.
[Export] public float ReverseStartSpeed { get; set; } = 3f;
```

Now it has a name, it shows up in Godot's Inspector, and different cars can have different
values. A number that isn't a tuning choice can be a named `const`, such as the `0.5f` that
means "tipped over 60°" at line 380:

```csharp
// Cosine of 60°: below this, the car is tipped too far to drive.
private const float TippedUpDot = 0.5f;
```

Numbers like `0f`, `1f` and `2f` in plain arithmetic are fine. `(1f - scrub)` isn't magic.

## 3. Documentation

C# has two kinds of comment:

- `//` is for whoever reads the code.
- `///` is an *XML documentation comment*. It's for whoever *uses* the code: Rider shows it when
  you hover over the name anywhere in the project.

```csharp
/// <summary>The car's handling numbers. One resource per car.</summary>
public partial class VehicleTuning : Resource
```

The tags you'll use:

```csharp
/// <summary>Pushes the car at one wheel.</summary>
/// <param name="maxForce">The most this wheel can push, in newtons.</param>
/// <returns>The wheel's sideways slip in metres per second.</returns>
private float ApplyWheel(/* ... */)
```

Our rule is modest, because a solo project doesn't need a page of docs on every getter:

- **New public types and tuning values get `///`.** The hover text pays for itself when you're
  in another file trying to remember what `SlideGrip` does.
- **Everything else** gets a `//` comment when the reason isn't obvious.
- **A comment must stay true.** A wrong comment is worse than none, so update it with the code.

The existing `//` comments on public members don't have to be converted in one go. Change them
when you're working in that file.

## 4. Small, focused methods

A method should do one thing, and its name should say what that is. The test is: **can you
describe it without using "and"?** `ApplySuspension` is close, though its comment admits it also
moves the wheel mesh and returns whether the wheel is touching.

`Vehicle._PhysicsProcess` is about 70 lines, and its `//` labels show where the pieces are:

```csharp
// Input
// Where the car is going
// Engine and brakes
// Grip budgets
// Suspension
// Wheels
// Drift helpers, recovery and effects
```

A label like that is a method waiting to be extracted.

### Too many parameters

```csharp
private void ApplyGlide(float throttle, Vector3 forward, bool handbrake, bool wheelspin, bool onGround, float delta)
```

Six parameters, three of them `bool`. At the call site it's easy to swap two by accident, and
the compiler can't help because the types match:

```csharp
ApplyGlide(throttle, forward, handbrake, wheelspin, onGround, dt);
ApplyGlide(throttle, forward, wheelspin, handbrake, onGround, dt); // Compiles. Wrong.
```

A good target is four or fewer. The usual fix, which you've already used for `DrivingInput`, is
to put values that travel together in a small `readonly record struct`:

```csharp
// What every helper needs to know about this physics tick.
private readonly record struct Tick(
    float Throttle,
    Vector3 Forward,
    bool Handbrake,
    bool Wheelspin,
    bool OnGround,
    float Delta);
```

Then the call is `ApplyGlide(tick);`, and inside the method you read `tick.Handbrake`, where the
name can't be mixed up. This is backlog item Q-4.

### Pure functions

A *pure* method uses only its inputs and changes nothing. `FollowCamera.Share` is a good
example from your own code:

```csharp
// The share of the remaining distance to cover this frame, at any frame rate.
private static float Share(float speed, float dt)
{
    return 1f - Mathf.Exp(-speed * dt);
}
```

The same input always gives the same output, and it can't break anything else. Pure methods are
the easiest code to read and to test, so pull the maths out of big methods into them whenever
you can.

## Try it yourself

1. Add `Flat` to `Vehicle.cs` and use it at lines 150, 151, 251 and 262. On line 262, replace
   the `new Vector3(...)` argument with `Flat(forward)`.
2. Build, then drive a lap. Nothing should feel different, because you changed the structure and
   not the behaviour. That kind of change is called a *refactor*.

(Physics in `Vehicle.cs` is normally agent work, but this one has no effect on physics, and it's
a good first refactor.)

## Check yourself

1. Two methods both contain `if (x < 0f) { x = 0f; }`, one for speed and one for health. Should
   you merge them?
2. Is the `2f` in `throttle * enginePush / 2f` a magic number?
3. Where does `///` show up that `//` doesn't?
4. Why can't the compiler catch swapped `bool` arguments?

<details>
<summary>Answers</summary>

1. No. They're different ideas that happen to look alike, and they're small.
2. Borderline. It means "split between the two rear wheels", and it never needs tuning, so it's
   fine. A short named `const` or comment would make it clearer.
3. In Rider's hover text, wherever that member is used.
4. Both have the type `bool`, so either order is valid. A struct with named fields avoids it.

</details>
