# Coding rules

The checklist for writing and reviewing C# in SupeRNR, for people and agents alike. Each rule is
short on purpose; the reasons and examples are in the [tutorials](tutorials/README.md).

**(T)** marks a rule that tools enforce. Everything else is checked in review.

## Naming and layout ([tutorial 1](tutorials/01-naming-and-layout.md))

- **(T)** `PascalCase` for types, methods, properties, events, `const` and `static readonly`.
- **(T)** `_camelCase` for private fields; `camelCase` for locals and parameters.
- **(T)** `I` prefix on interfaces. No `s_` prefix on statics.
- **(T)** Always write access modifiers, and never `this.`.
- Never `ALL_CAPS`, even for constants. Tools don't catch this.
- `snake_case` for Godot files and folders. C# files match the class name.
- Namespaces are `SupeRNR.<Feature>`, file-scoped, with usings outside. **(T)**
- One type per file. Small, closely related types are the exception.
- Members go in this order: signals and events, exports, public properties, constants, fields,
  Godot callbacks, public methods, private methods.
- No `#region`. Split the class instead.
- **(T)** Braces always, on their own line.
- Keep lines under about 100 characters; 120 is the limit.
- `var` only when the type is visible on the same line.
- Write float literals with a suffix: `1f`.
- Use full words. `dt` and loop counters are the accepted short forms.
- Angles are radians unless the name says otherwise. Say the unit when it isn't obvious.

## Clean code ([tutorial 2](tutorials/02-clean-code.md))

- Comments explain why, on their own line, starting with a capital. Keep them true.
- New public types and tuning values get `///`. Existing `//` can convert as files are touched.
- Don't repeat knowledge. Make a helper at the third copy, if the copies mean the same thing.
- A number that affects handling goes in `VehicleTuning`, with a comment and units.
- Other unexplained numbers get a named `const`.
- A method does one job. Aim for four parameters or fewer.
- Prefer `readonly record struct` for values that travel together.
- Prefer `private static` pure methods for maths.

## Errors and null ([tutorial 3](tutorials/03-errors-and-null.md))

- Never leave a nullable warning unresolved.
- `= null!` only for node references assigned in `_Ready`. No other use of `!` without a comment.
- Mark an optional `[Export]` with `?`.
- `GetNode<T>` for nodes that must exist; `GetNodeOrNull<T>` for optional ones, with a check.
- Use `GodotObject.IsInstanceValid` for a node that may have been freed.
- Godot keeps running after a thrown exception. A failed setup check must leave the node safe:
  `GD.PushError`, turn processing off, return.
- Throw specific types using helpers such as `ArgumentNullException.ThrowIfNull`.
- Never `throw new Exception`, an empty `catch`, or `catch (Exception)` without a very good reason.
- Rethrow with `throw;`. **(T)**
- Don't use exceptions for normal flow. Use checks and `Try` methods.

## Design ([tutorial 4](tutorials/04-design.md))

- One reason to change per class.
- Different cars are different data, not subclasses.
- Implement only the current milestone: no placeholder classes or speculative interfaces.
- Add an interface with its second implementation. `[Export]` can't hold a C# interface.
- A node gets dependencies through `[Export]`, and a plain class through its constructor.
- No `GetParent().GetParent()`, no `/root/...` paths, no autoloads for game state.
- Call down, signal up. Siblings go through their parent.
- C# `event`s between C# classes. `[Signal]`s for editor wiring and engine nodes.
- Every `+=` on a C# event has a matching `-=`, usually in `_ExitTree`. No lambdas on long-lived
  events.
- Only `PlayerDriver` reads `Input`.
- Rules that need no engine go in `core/`. It never references Godot.
- Use `{ get; private set; }` for state others may read. New classes are `sealed` unless designed
  for inheritance.

## Tests and review ([tutorial 5](tutorials/05-testing-and-review.md))

- Test engine-free rules with xUnit in `tests/`.
- Name tests `Method_Scenario_Expected`, in Arrange-Act-Assert form, with no logic.
- Tests use no real clock, files or randomness. Pass the time in as a value.
- Never call a handling change working from end results alone. See
  [AGENTS.md](../AGENTS.md#vehicle-physics).
- Leave no debug `GD.Print` in committed code.
- Conventional Commits, with one idea per commit. A bug fix mentions its ID.

## Performance and security ([tutorial 6](tutorials/06-performance-and-security.md))

- Clear code first. Optimise what the profiler shows.
- No allocations in per-tick code: no new classes or lists, string building, LINQ or capturing
  lambdas. Structs are fine.
- Cache `StringName`s used per tick in `static readonly` fields.
- Use `SignalName`, `MethodName` and `PropertyName` instead of strings.
- Read an engine property once per method when it's used repeatedly, unless it's assigned too.
- .NET collections by default. Godot collections only when calling the engine.
- Movement code goes in `_PhysicsProcess`.
- Treat save files, mods and network data as untrusted. Validate every value.
- Never use `BinaryFormatter`, or `allowObjects: true` on outside data.
- No secrets in the repository.

## Versions ([tutorial 7](tutorials/07-staying-current.md))

- Stay on the .NET LTS that Godot supports. Today that's `net10.0` with C# 14.
- Keep `AnalysisLevel` pinned, and update it deliberately.
- Upgrades are between milestones, in their own commit, with a decision record.
- Prefer modern C# when it's clearer. Check it compiles against Godot's source generators.
