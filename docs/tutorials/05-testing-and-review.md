# 5. Testing and review

Every check in this tutorial exists to find a mistake earlier. A typo the compiler catches costs
seconds. The same bug found by a playtester costs an evening.

## 1. Unit tests

A unit test is a small method that runs a piece of your code and checks the result. It runs in
milliseconds, with no game window, every time you ask.

*Planned:* the test project arrives with D-8. Nothing in this section exists in the repository
yet, and the `LapTracker` API is only an illustration.

### What a test looks like

```csharp
public class LapTrackerTests
{
    [Fact]
    public void RegisterCheckpoint_OutOfOrder_DoesNotCount()
    {
        // Arrange: set up the situation
        LapTracker tracker = new(checkpointCount: 3);

        // Act: do the one thing being tested
        tracker.RegisterCheckpoint(2);

        // Assert: check the result
        Assert.Equal(0, tracker.NextCheckpoint);
    }
}
```

- `[Fact]` marks a method as a test.
- `Assert.Equal(expected, actual)` fails the test if they differ. Expected comes first.
- The name has three parts: **Method_Scenario_Expected**. When it fails, the name alone tells you
  what broke.
- **Arrange, Act, Assert** is the standard shape, with one Act per test.

To run one test with several inputs, use `[Theory]` rather than a loop:

```csharp
[Theory]
[InlineData(-1)]
[InlineData(0)]
public void Constructor_NoCheckpoints_Throws(int count)
{
    Assert.Throws<ArgumentOutOfRangeException>(() => new LapTracker(count));
}
```

`() => new LapTracker(count)` is a small unnamed function. `Assert.Throws` runs it and passes only
if that exception comes out.

### What makes a test good

Microsoft's list is **Fast, Isolated, Repeatable, Self-checking, Timely**.

- **Isolated and repeatable:** no real clock, no files, no randomness. Pass the time in as a value,
  so a test can say "this lap took 62.5 seconds" without waiting for it.
- **No logic in tests:** no `if` or `for`. Otherwise you'd need a test for your test.
- **Test through the public API.** Testing private details means the tests break when you
  refactor, even though nothing is wrong.
- **Coverage isn't quality.** A high percentage only proves the lines ran.

### What we test, and what we don't

| Code | How we check it |
|---|---|
| Rules with no engine (lap validation, timing) | Unit tests in `tests/` (*planned*) |
| Node wiring | Run the game |
| Vehicle physics and feel | Measurement and driving |

Jolt isn't something a unit test can usefully check. For handling, final numbers aren't enough,
because a good lap time can hide horrible frame-by-frame jerks. So we check per-frame jerk, use
real input sequences, look at an overhead render, and then drive it.

### Test project setup (*planned for D-8*)

Use xUnit v3 (`xunit.v3`) on Microsoft.Testing.Platform, which the .NET 10 SDK selects through
`test.runner` in `global.json`. Check the current [xUnit setup page](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
when building it, because this area has changed quickly.

## 2. Source control

### Commits

We use [Conventional Commits](https://www.conventionalcommits.org/): `type(scope): summary`.

```text
feat(vehicle): add handbrake drift
fix(vehicle): wheels follow suspension (B-1)
docs: update roadmap
refactor(vehicle): extract Flat helper
```

| Type | Means |
|---|---|
| `feat` | New behaviour |
| `fix` | Bug fix. Mention the ID from [BUGS.md](../BUGS.md) |
| `refactor` | Structure changed, behaviour didn't |
| `docs`, `test`, `chore` | Docs, tests, tooling |

### Habits

- **One idea per commit.** If you need "and" in the summary, it's probably two commits.
  It keeps `git revert` and `git log` useful.
- **Commit working code.** Run the verification commands first.
- **Read your diff before committing.** `git diff --staged` finds debug prints and accidents.
- **Work on `main`.** It's a solo project, so we skip branches and pull requests. This replaces the
  branch workflow we started with.

### Godot specifics

- Move or rename Godot files **inside the Godot editor**, so `res://` paths stay correct.
- Commit `.uid` files. They're how Godot tracks a file when it moves.
- Never commit `.godot/`. It's a generated cache.
- Don't edit `.tscn` files or `project.godot` by hand while the editor has the project open. The
  editor writes its own copy back over yours.
- `private/` is git-ignored and stays that way.

## 3. Code review

A review answers: *is this correct, and will it be easy to change?* In a solo project you review
your own diff, and an agent can review it too.

### Checklist

**Correct**
- Does it do what the backlog item asks, and only that?
- What happens with `null`, zero, or an empty list?
- If it's `_PhysicsProcess` code, is everything scaled by `delta` where it should be?

**Clear**
- Would you understand the names in six months?
- Does any comment repeat the code, or explain something that's no longer true?
- Did you leave a `GD.Print` behind?

**Well placed**
- Does this belong in this class?
- Are there new bare numbers that should be in `VehicleTuning`?
- Is an event subscribed without being unsubscribed?

**Paperwork**
- Did [BACKLOG.md](../BACKLOG.md) or [BUGS.md](../BUGS.md) change status?
- Is a significant choice in [DECISIONS.md](../DECISIONS.md)?
- Do the docs still describe what's true?

Be kind to your past self, and be specific. "`maxForce` should be checked for zero here" is
useful. "This is messy" isn't.

## 4. What the tools catch

No single tool catches everything. Knowing where the gaps are tells you what to look at yourself.

| Problem | Compiler | `dotnet build` analysers | `dotnet format` | You |
|---|---|---|---|---|
| Type errors | ✓ | | | |
| Possible null use | ✓ | | | |
| Wrong naming style | | ✓ | ✓ | |
| Unused `using` | | ✓ | ✓ | |
| Missing `private`/`public` | | ✓ | ✓ | |
| Risky API use (`throw exception;`) | | ✓ | ✓ | |
| Indentation and spacing | | | ✓ | |
| `ALL_CAPS` constants | | | | ✓ |
| Blank lines, comment style | | | | ✓ |
| Long lines | | | | ✓ |
| `GetNode<T>` with a wrong path | | | | ✓ (at run time) |
| Bad design, bad handling | | | | ✓ |

### The verification commands

```sh
dotnet build SupeRNR.sln --no-incremental -warnaserror
dotnet format SupeRNR.sln --verify-no-changes
godot_console --headless --path . --import
```

- `--no-incremental` compiles everything again. A normal build skips unchanged code, so it would
  also skip that code's warnings, and a clean-looking result could be false.
- `-warnaserror` makes a warning fail the command, so you can't scroll past one.
- Warnings don't fail ordinary builds, so experimenting while you work stays easy.
- `--verify-no-changes` reports formatting problems rather than fixing them. Run `dotnet format
  SupeRNR.sln` without it to apply the fixes.

### What's turned on

| Setting in `SupeRNR.csproj` | Effect |
|---|---|
| `AnalysisLevel` = `10-recommended` | Microsoft's recommended rules, pinned to .NET 10 |
| `EnforceCodeStyleInBuild` | The `.editorconfig` style rules run in the build |
| `GenerateDocumentationFile` | Required to detect unused `using`s |
| `NoWarn` `CS1591` | Doesn't require `///` on every public member |

## Try it yourself

1. Add `int unusedValue = 1;` inside `FollowCamera._PhysicsProcess`.
2. Run `dotnet build SupeRNR.sln`, and see that it only warns.
3. Run the first verification command, and see that it fails.
4. Delete the line.
5. Write down a test name for this rule: "a lap time of zero is rejected".

## Check yourself

1. What's wrong with using the real clock in a unit test?
2. Why does the verification build use `--no-incremental`?
3. Which belongs in a `core/` test: "the car drifts well" or "the best lap is the lowest time"?
4. Would a tool catch `MAX_SPEED`?

<details>
<summary>Answers</summary>

1. The test becomes slow and unrepeatable. Pass the time in as a value.
2. An incremental build skips unchanged code, along with its warnings.
3. The best lap. It's a pure rule with no engine. Drift feel is checked by measuring and driving.
4. No. Our tools accept it, so it's a review item.

</details>
