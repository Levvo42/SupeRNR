# Standards

How things are done in SupeRNR. Every practice the project follows is written here once, for
people and agents alike, and the steps for common jobs are in the [guides](../guides/README.md).

**What's current:** a rule applies now, even if it's for something the game doesn't have yet.
Examples of code that doesn't exist yet are marked *planned*.

| Standard | Covers |
|---|---|
| [Naming](naming.md) | The name of everything: C#, files, scenes, nodes, inputs, docs, IDs |
| [Project structure](project-structure.md) | Where each kind of file and code belongs |
| [C#](csharp.md) | File layout, formatting, comments, methods, null, errors, performance |
| [Godot](godot.md) | Scenes, nodes, dependencies, signals, resources, input, physics |
| [Verification](verification.md) | The checks, tests, handling measurements and review |
| [Repository](repository.md) | Commits, project records, versions, dependencies, assets, data safety |

## Keeping them true

- A new practice goes in the matching standard in the same commit that starts using it.
- A job done the same way twice gets a guide.
- Changing a standard is a decision: add it to [DECISIONS.md](../DECISIONS.md).
- If code and a standard disagree and the code isn't listed below, the code is wrong.

## Known exceptions

Existing code that doesn't meet a standard yet. Don't copy these, and don't add more.

| Where | Standard it breaks | Fixed by |
|---|---|---|
| `Vehicle` | Bare handling numbers | Q-2 |
| `Vehicle` | `Basis` instead of `GlobalBasis`, and a `GD.Print` in `_Ready` | Q-3 |
| `Vehicle` | Methods with more than four parameters | Q-4 |
| `Vehicle` | Repeated code, long methods, banner comments, lines over 120 characters | Q-5 |
| `FollowCamera`, `PlayerDriver` | Formatting and class comments | Q-6 |
| `Vehicle` | A missing `Tuning` isn't reported | Q-7 |
| `Vehicle` | `dt` and `delta` for the same value, and one `tyres` in a comment | Q-5 |
| `VehicleTuning` | `//` on tuning values instead of `///` | When the file is next changed |
