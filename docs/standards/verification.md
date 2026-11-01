# Verification

## Before every commit

All three must pass with no warnings or changes:

```sh
dotnet build SupeRNR.sln --no-incremental -warnaserror
dotnet format SupeRNR.sln --verify-no-changes
godot_console --headless --path . --import
```

- `--no-incremental` is required: a normal build skips unchanged code, and with it that code's
  warnings.
- Ordinary builds only warn, so experiments stay easy. `-warnaserror` applies at verification.
- `dotnet format SupeRNR.sln`, without the flag, applies formatting fixes.

Agents run Godot in its own window because of the exit hang described in
[AGENTS.md](../../AGENTS.md#rules).

## What the tools check

| Setting | Effect |
|---|---|
| `AnalysisLevel` = `10-recommended` | Microsoft's recommended rules, pinned to .NET 10 |
| `EnforceCodeStyleInBuild` | `.editorconfig` rules run in the build |
| `GenerateDocumentationFile` | Required to detect unused `using`s |
| `NoWarn` `CS1591` | Doesn't require `///` on every public member |
| `IDE1006` = warning | Naming rules reach the build |
| `CA1822` = suggestion | A node's method needn't be `static` |

| Problem | Caught by |
|---|---|
| Type errors, possible null use | Compiler |
| Naming style, unused `using`, missing access modifier | Build and `dotnet format` |
| Risky API use such as `throw exception;` | Build and `dotnet format` |
| Indentation and spacing | `dotnet format` |
| `ALL_CAPS`, blank lines, comment style, long lines | Review only |
| Bad design, wrong `GetNode` path, bad handling | Review, running and driving |

## How each kind of code is checked

| Code | Check |
|---|---|
| Engine-free rules | *planned:* unit tests in `tests/`, from D-8 |
| Node wiring | Run the game |
| Vehicle physics and feel | Measure, then drive |

## Unit tests (planned)

The test project arrives with D-8.

- xUnit, in `tests/`, for code in `core/`. Test through the public API.
- Each test has Arrange, Act and Assert sections, with one Act.
- No `if` or `for` in a test. Use `[Theory]` for several inputs.
- No real clock, files or randomness. Pass time in as a value.
- Name tests `Method_Scenario_Expected`.

## Handling and track changes

Whoever makes the change follows the measurement rules in
[AGENTS.md](../../AGENTS.md#vehicle-physics), then drives it.

## Review

Read your own diff before committing, with `git diff --staged`.

- **Correct:** Does it do what the task asks, and only that? What happens with `null`, zero or an
  empty list? Is `_PhysicsProcess` code scaled by the time step where it should be?
- **Clear:** Are the names still clear out of context? Is every comment true? Is any `GD.Print` left
  behind?
- **Well placed:** Does it belong in this class? Are new numbers in `VehicleTuning`? Is every event
  unsubscribed?
- **Recorded:** Are the records up to date? See
  [Finish a change](../guides/finish-a-change.md).
