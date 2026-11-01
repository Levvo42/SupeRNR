# Repository

## Commits

[Conventional Commits](https://www.conventionalcommits.org/): `type(scope): summary`.

| Type | Means |
|---|---|
| `feat` | New behaviour |
| `fix` | Bug fix, with its ID: `fix(vehicle): wheels follow suspension (B-1)` |
| `refactor` | Structure changed, behaviour didn't |
| `docs` | Documentation only |
| `test` | Tests only |
| `chore` | Tooling and configuration |

- The scope is the feature: `vehicle`, `track`, `camera`.
- One idea per commit. An "and" in the summary usually means two commits.
- Every commit passes [verification](verification.md).
- Work on `main`. It's a solo project, so there are no branches or pull requests.
- Agents need approval to commit or push.

## Project records

The full table of what's recorded where is in
[AGENTS.md](../../AGENTS.md#project-records). Each kind of knowledge has one home.

- Docs describe only what exists as *current*. Anything else is labelled *planned*.
- `DECISIONS.md` is append-only. To reverse a decision, add a new one and mark the old one
  **Superseded by #N**.
- Delete a row from `BUGS.md` when the bug is fixed, and never reuse its ID.
- Keep ideas in `IDEAS.md` until they're scheduled. They then move to `BACKLOG.md`.

## Versions

- Stay on the .NET LTS that Godot supports. Today that's `net10.0` with C# 14.
- `Godot.NET.Sdk` always matches the editor version.
- Keep `AnalysisLevel` pinned to a version number, never `latest`.
- Upgrades happen between milestones, in their own commit, with a decision record. See
  [Upgrade the tools](../guides/upgrade-the-tools.md).

## Dependencies and assets

- No third-party packages beyond the test framework, unless a decision says otherwise.
- Every third-party asset has an entry in `assets/CREDITS.md`.
- No non-commercial or ripped assets.

## Data safety

- No secrets in git, even in a private repository: no API keys, tokens or passwords.
- *Planned:* save files, mods and network data are untrusted. Validate every value after loading.
- Save data is plain JSON through `System.Text.Json`.
- Never use `BinaryFormatter`, or `allowObjects: true` in `FileAccess.GetVar`, on outside data.
- Load `.tres` and `.tscn` files only from the game itself, because they can carry scripts.
- File paths from outside stay inside `user://`.
- In multiplayer the server decides. Clients send only `DrivingInput`, and the server clamps it.
