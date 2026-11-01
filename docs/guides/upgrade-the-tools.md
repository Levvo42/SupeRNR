# Upgrade the tools

Last checked: **2026-11-01**. Update this date and the table after each check.

| Tool | Version | Status |
|---|---|---|
| Godot | 4.7.2 .NET | Latest stable release |
| `Godot.NET.Sdk` | 4.7.2 | Matches the editor |
| .NET | 10 | LTS, supported until November 2028 |
| C# | 14 | Comes with .NET 10 |

Decision 21 explains why we aren't on .NET 11.

## Each milestone

1. Check the latest [Godot](https://godotengine.org/download) release and the
   [.NET support dates](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
2. Look for outdated or vulnerable packages:

   ```sh
   dotnet list package --outdated
   dotnet list package --vulnerable
   ```

3. Check [Microsoft's conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
   and [Godot's C# docs](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
   for changes that affect the [standards](../standards/README.md).
4. Update the table above.

## Upgrade Godot

1. Commit everything first.
2. Read the release notes and migration guide.
3. Install the new editor, and update `Godot.NET.Sdk` in `SupeRNR.csproj` to match.
4. Open the project and let the editor reimport.
5. Run the [verification commands](../standards/verification.md#before-every-commit).
6. Drive a full lap, because physics changes can alter handling while every check passes.
7. Update the version in `README.md`, `AGENTS.md` and `docs/ARCHITECTURE.md`.
8. Commit the upgrade by itself, with a [decision record](../DECISIONS.md).

## Upgrade .NET

1. Check that Godot supports the new version. This decides whether you can upgrade at all.
2. Install the SDK, then change `TargetFramework` in `SupeRNR.csproj`.
3. Change `AnalysisLevel` to match. Fix any new warnings in a separate commit.
4. Verify, drive a lap and update the docs, as for Godot.
5. Commit the upgrade by itself, with a decision record.
