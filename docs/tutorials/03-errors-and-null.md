# 3. Errors and null

Things go wrong in two very different ways, and they need opposite treatment. A *bug* should
be as loud as possible, so it gets fixed. An *expected situation* should be handled quietly as
part of normal code. Most error-handling mistakes come from mixing the two up.

## 1. Three kinds of problem

| Kind | Example | What to do |
|---|---|---|
| Expected condition | No driver assigned, car in the air | Check with `if`, `?.` or `??`. No exception |
| Programmer or setup mistake | A required child node is missing | Fail loudly |
| Outside world | A save file is missing or corrupt | Catch the specific exception, or use a `Try` method |

## 2. How Godot changes this

This matters more than anything else on the page. We tested each of these in Godot 4.7.2.

**A thrown exception doesn't stop the game.** Normally an unhandled exception crashes a .NET
program. Godot catches exceptions from callbacks like `_Ready`, logs them, and carries on.

Think about what that means for `Vehicle._Ready`:

```csharp
Mass = Tuning.Mass;
PhysicsMaterialOverride.Bounce = Tuning.Bounce;   // Line 49
GD.Print($"Car ready. Mass: {Mass} kg");
_rayFrontLeft = GetNode<RayCast3D>("RayFrontLeft");
// ...and nine more GetNode lines
```

If line 49 throws, because the scene has no physics material, every `GetNode` below it is
skipped. `_PhysicsProcess` still runs, though, and hits a null `_rayFrontLeft` on every tick. So
you get a stream of errors from the wrong place, sixty times a second. **Look at the first error
in the log, not the most frequent one.**

**`GetNode<T>` can return null, despite its signature.** With a wrong path, it logs an error and
returns `null`, even though the return type has no `?`. The compiler can't warn you:

| Call | Node missing | Node is the wrong type |
|---|---|---|
| `GetNode<RayCast3D>("Ray")` | Logs an error, returns `null` | Throws `InvalidCastException` |
| `GetNodeOrNull<RayCast3D>("Ray")` | Returns `null` quietly | Returns `null` quietly |

So use `GetNode<T>` when the node must exist, and `GetNodeOrNull<T>` only when it's genuinely
optional and you check the result.

**A freed node isn't null.** After `QueueFree()`, your C# variable still points at an object,
but the engine side is gone. To check one that may have been freed, use:

```csharp
if (GodotObject.IsInstanceValid(target))
```

## 3. Throwing exceptions

Throw when a method can't do its job because something is wrong. Use the existing .NET types:

```csharp
public LapTracker(int checkpointCount)
{
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(checkpointCount);
    _checkpointCount = checkpointCount;
}
```

(*Planned:* `LapTracker` doesn't exist yet.) That one line checks the value and throws with a
good message that names the parameter. It's the modern form of writing the `if` by hand.

| Situation | Use |
|---|---|
| A required argument is null | `ArgumentNullException.ThrowIfNull(argument);` |
| A number is out of range | `ArgumentOutOfRangeException.ThrowIfNegative(value);` and friends |
| An argument is invalid some other way | `throw new ArgumentException("message", nameof(argument));` |
| The object isn't in a usable state | `throw new InvalidOperationException("message");` |

Never `throw new Exception(...)`: nothing can catch it specifically.

Inside a Godot node, remember nothing stops. A setup check in `_Ready` needs to leave the node
in a safe state, so report it and turn the node off:

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

`GD.PushError` puts a red message in Godot's debugger with a stack trace, while `GD.Print` is
plain output. `SetPhysicsProcess(false)` stops `_PhysicsProcess` being called on this node.

## 4. Catching exceptions

**Only catch what you can do something about.** Catch the most specific type, and don't catch
and ignore:

```csharp
try
{
    string text = File.ReadAllText(path);
    return JsonSerializer.Deserialize<SaveData>(text);
}
catch (FileNotFoundException)
{
    return null;   // First run: no save yet. That's fine.
}
catch (JsonException exception)
{
    GD.PushError($"Save file is damaged: {exception.Message}");
    return null;
}
```

(*Planned:* we have no save files yet.)

Never write this. It hides every bug in the block, including ones you haven't thought of:

```csharp
catch (Exception) { }
```

To rethrow, write `throw;` and not `throw exception;`. The second one wipes out the stack trace
that tells you where the problem started.

**Don't use exceptions for normal flow.** If something routinely happens, check for it:

```csharp
// Exceptions are slow, and a typo in the input isn't exceptional
try { number = int.Parse(text); } catch (FormatException) { number = 0; }

// Better: a Try method returns a bool, and gives the result through `out`
if (!int.TryParse(text, out int number))
{
    number = 0;
}
```

## 5. Nullable reference types

We have `<Nullable>enable</Nullable>` on, so the compiler tracks whether a reference might be
null. It's a compile-time check only and adds nothing when the game runs.

```csharp
RigidBody3D target;    // Must never be null
RigidBody3D? target;   // Might be null: the compiler makes you check
```

The tools for handling a maybe-null value:

```csharp
if (Target is null) { return; }                       // Stop early
DrivingInput input = Driver?.Read() ?? DrivingInput.None;
```

That second line, from `Vehicle.cs`, has two operators:

- `Driver?.Read()`: if `Driver` is null, skip the call and produce null.
- `?? DrivingInput.None`: if the left side is null, use the right side instead.

### `= null!`, our one exception

`!` tells the compiler "trust me, this isn't null". Microsoft says to use it sparingly, because
you're switching off the check. We allow one case: node references filled in by `_Ready`.

```csharp
private RayCast3D _rayFrontLeft = null!;
```

C# wants a value when the object is created, but Godot can only supply the node later, in
`_Ready`. Everywhere else, use a real value or make the type nullable.

### Should an `[Export]` be nullable?

Say which you mean in the type:

```csharp
[Export] public PlayerDriver? Driver { get; set; }   // Optional: no driver is a valid state
[Export] public VehicleTuning Tuning { get; set; } = new();
```

`Driver` is honest: the car works without one, so `?` is right. `Tuning` has a quieter problem.
If you forget to assign it, the `= new()` means the car drives with default numbers and nothing
tells you. That could confuse you during tuning, so consider checking in `_Ready` once there
are several tuning files.

### Traps the compiler misses

- `default` on a struct with a reference field gives null in a non-nullable field.
- A new `string[10]` holds ten nulls, though the type says `string`.
- `GetNode<T>`, as above.

## Try it yourself

`FollowCamera` quietly does nothing when `Target` is unassigned:

```csharp
if (Target is null)
{
    return;
}
```

A camera with no target is a setup mistake, not a normal state, so make it visible. Add a
`_Ready` method that reports the problem with `GD.PushError` and turns off physics processing,
as in section 3. You can then remove the check from `_PhysicsProcess`, but the compiler will
still treat `Target` as nullable. So keep the check, or add `Target!` with a comment saying why.
Which do you prefer, and why?

To test it, clear the `Target` field in the Inspector and press F5.

## Check yourself

1. A method throws in `_Ready`. Does the game stop?
2. `GetNode<Label>("Hud")` has a typo in the path. What do you get?
3. Why is `catch (Exception) { }` dangerous?
4. When is `= null!` allowed in our code?

<details>
<summary>Answers</summary>

1. No. Godot logs it, the rest of `_Ready` is skipped, and the game keeps running in a half-set-up
   state.
2. A logged error and `null`, and later a `NullReferenceException` wherever you use it.
3. It hides every failure in the block, including bugs you didn't expect.
4. Only for node references assigned in `_Ready`.

</details>
