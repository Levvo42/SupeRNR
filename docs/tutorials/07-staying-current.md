# 7. Staying current

Every tool here changes on its own schedule. The goal isn't to have the newest version. It's to
stay on versions that are supported, and to know why we're on them.

Last checked: **2026-11-01**. Update this date and the table whenever you recheck.

## Where we are

| Tool | Our version | Status | Source of truth |
|---|---|---|---|
| Godot | 4.7.2 .NET | Latest stable release | [godotengine.org](https://godotengine.org/download) |
| `Godot.NET.Sdk` | 4.7.2 | Must match the editor | `SupeRNR.csproj` |
| .NET | 10 | LTS, supported until November 2028 | `TargetFramework` |
| C# | 14 | Comes with .NET 10 | The .NET SDK |

### How .NET releases work

A new .NET arrives every November. Even numbers are **LTS** (long-term support, three years), and
odd numbers are **STS** (standard-term support, shorter).

| Version | Status on 2026-11-01 |
|---|---|
| .NET 8 | LTS. Support ends 2026-11-10 |
| .NET 9 | STS. Support ends 2026-11-10 |
| **.NET 10** | **LTS. Ours** |
| .NET 11 | STS. Launches at .NET Conf, starting 2026-11-10 |

### Should we move to .NET 11?

**No.** It's a shorter-term release, and a [forum report](https://forum.godotengine.org/t/use-dotnet-11-rc-sdk/144192)
shows Godot failing to load a `net11.0` project with `FileNotFoundException`. .NET 10 is supported
for two more years, so we lose nothing by waiting.

Godot's own C# library, GodotSharp 4.7.2, targets `net8.0`. That doesn't tie us to .NET 8: a newer
project can use an older library, and our `net10.0` project does. So .NET 8 reaching its end of
support doesn't affect us.

### The analyser version is pinned

`AnalysisLevel` is `10-recommended`, not `latest-recommended`. With `latest`, installing a new
.NET SDK could add new warnings overnight, and since warnings fail our verification, a working
project would suddenly fail. With a pinned version, *we* choose when that happens.

## Newer C# features we can use

Each of these compiles here, against Godot's source generators. We checked that on the date above.
Use them when they make code clearer, not to show them off.

### From C# 14

**The `field` keyword.** A property can add logic to its setter without declaring a separate
private field. `field` means "the hidden storage for this property":

```csharp
// Before: a separate field just to clamp a value
private float _clamped = 1200f;
public float Clamped
{
    get => _clamped;
    set => _clamped = Mathf.Max(value, 1f);
}

// C# 14
public float Clamped { get; set => field = Mathf.Max(value, 1f); } = 1200f;
```

**Null-conditional assignment.** `?.` now works on the left of `=`:

```csharp
_smoke?.AmountRatio = 0f;     // Does nothing if _smoke is null
```

**Extension members.** You can add members to a type you don't own, including `Vector3`. The new
`extension` block groups them, and can add properties, which the old form couldn't:

```csharp
public static class VectorExtensions
{
    extension(Vector3 vector)
    {
        public Vector3 Flat => vector with { Y = 0f };
    }
}
```

After which you can write `LinearVelocity.Flat`. Use this sparingly. A private `Flat` helper in
one class is simpler, and an extension only earns its place once several classes need it.

### Already standard

| Feature | Example |
|---|---|
| Records | `readonly record struct DrivingInput(...)` |
| Target-typed `new` | `Vector3 offset = new(1f, 0f, 0f);` |
| Pattern matching | `if (Target is null)` |
| `with` on structs | `velocity with { Y = 0f }` |
| Switch expressions | `float grip = surface switch { Surface.Ice => 0.2f, _ => 1f };` |
| Collection expressions | `int[] laps = [1, 2, 3];` |
| `required` properties | `public required string Name { get; init; }` |
| Raw strings | `"""` for multi-line text |

### Things Godot limits

Being able to use a language feature doesn't mean Godot understands it:

- `[Export]` needs Godot-compatible types (error GD0102 otherwise).
- Classes extending Godot types must be `partial` and match their file name.
- Godot's style guide is written against an older C# version, so its examples can look dated. The
  rules still apply.

## How we update

Don't update in the middle of a feature. A good moment is **between milestones**, or when a
release fixes something that hurts.

### Each milestone

1. Check Godot's and .NET's latest releases.
2. Run these to look for outdated or vulnerable packages:

   ```sh
   dotnet list package --outdated
   dotnet list package --vulnerable
   ```

3. Skim [Microsoft's conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
   and [Godot's C# docs](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
   for anything new. Each Microsoft page shows when it was last updated.
4. Update the table and date at the top of this file.

### Upgrading Godot

1. Commit everything first, so you can get back.
2. Read the release notes and the migration guide.
3. Install the new editor and update `Godot.NET.Sdk` in `SupeRNR.csproj` to match.
4. Open the project, and let the editor reimport.
5. Run the verification commands.
6. **Drive a full lap.** Physics changes can alter handling while every check stays green.
7. Commit the upgrade on its own, and add a [decision](../DECISIONS.md).

### Upgrading .NET

1. Check that Godot supports the new version. That's the deciding factor.
2. Install the SDK, then change `TargetFramework`.
3. Change `AnalysisLevel` to match, and deal with any new warnings as separate work.
4. Verify, drive a lap, commit, and record the decision.

## Where to look

| For | Source |
|---|---|
| C# language | [What's new in C#](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14) |
| .NET support dates | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) |
| Godot releases | [Godot blog](https://godotengine.org/blog/) |
| Godot C# changes | [Godot C# docs](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html) |
| Code analysis rules | [Code analysis overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) |

Prefer official documentation. Blog posts and old answers often describe a C# from years ago, and
that's how advice like `ALL_CAPS` constants lives on.

## Try it yourself

1. Run `dotnet --list-sdks` and `dotnet --version`. Which SDK does this project use?
2. Find the support end date for .NET 10 on Microsoft's policy page, and compare it with the table.
3. Find the `field` example above. Which is easier to read, and why?

## Check yourself

1. Why don't we use `latest-recommended`?
2. Which fact decides whether we can move to a new .NET version?
3. What should you do after a Godot upgrade that no automated check does?
4. Why do upgrades get their own commit?

<details>
<summary>Answers</summary>

1. A new SDK could add warnings by itself. Pinning means we choose when.
2. Whether Godot supports it.
3. Drive a lap, because physics changes can alter handling without failing anything.
4. So you can undo the upgrade alone, and see exactly what it changed.

</details>
