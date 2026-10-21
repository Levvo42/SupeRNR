# Decisions

Lightweight decision records. Append new entries; don't rewrite old ones. If a decision is
reversed, add a new entry and mark the old one **Superseded by #N**.

Format: **Context** (why it came up) · **Decision** · **Consequences**.

---

## 1. Engine, language and repository: 2026-10-21, Accepted

- **Context:** Long-term C# learning project with a desktop target.
- **Decision:** Godot 4.7.2 .NET, C#, Jolt Physics, a single Git repository whose root is
  the Godot project. Minimal dependencies: no third-party packages beyond the test
  framework.
- **Consequences:** One place for everything; Godot-specific build via Godot.NET.Sdk.

## 2. Project name SupeRNR: 2026-10-21, Accepted

- **Context:** "schmovement_racer" was long; the game draws mostly on *Rock n' Roll Racing*
  and *Super Cars II*; "SupeRNR" = **Supe**r Cars + **R**ock **N** **R**oll Racing.
- **Decision:** Display name **SupeRNR**, assembly/root namespace `SupeRNR`. The repository
  folder name is irrelevant to the build and can be renamed freely.
- **Consequences:** Renamed before the C# project existed, so no rename churn.

## 3. Target framework net10.0: 2026-10-21, Accepted

- **Context:** Godot's template targets `net8.0`, whose support ends 2026-11-10.
- **Decision:** Target `net10.0` (LTS to 2028, C# 14). Verified 2026-10-21: builds with no
  warnings, and Godot 4.7.2 loads it (hostfxr 10.0.13) and runs a C# node script headless.
- **Consequences:** Exporting a build is not verified yet (no export templates installed);
  check it at the first export. Fall back to `net8.0` if Godot export or editor issues appear.

## 4. Nullable reference types enabled: 2026-10-21, Accepted

- **Decision:** `<Nullable>enable</Nullable>`. Nodes assigned in `_Ready` use `= null!`.
- **Consequences:** Null mistakes surface at compile time; one documented idiom for nodes.

## 5. Feature-based layout and naming: 2026-10-21, Accepted

- **Decision:** Feature folders under `game/`, `snake_case` folders and Godot files,
  `PascalCase` C# files, namespaces `SupeRNR.<Feature>`. Details in ARCHITECTURE.md.
- **Consequences:** Rider's namespace/folder check (IDE0130) is disabled in `.editorconfig`.

## 6. Vehicle physics: RigidBody3D with custom arcade forces: 2026-10-21, Accepted

- **Context:** Options were `VehicleBody3D` (hard to tune for expressive drift),
  `RigidBody3D` + custom forces, a kinematic `CharacterBody3D` simulation (deterministic but
  hand-written collisions), and a rolling sphere (loses real orientation and shape).
- **Decision:** `RigidBody3D` with our own engine, brake and lateral-grip forces, validated by
  a timeboxed spike (backlog D-1).
- **Consequences:** Expressive control with free collisions, ramps and impacts. Jolt is not
  deterministic, so online play will be server-authoritative with snapshots and
  interpolation, not rollback. Revisit only if the spike fails.

## 7. Input seam via a DrivingInput value: 2026-10-21, Accepted

- **Decision:** The vehicle consumes a `DrivingInput` value each physics tick and never reads
  `Input`. No input-source interface until a second source (AI or ghost) exists.
- **Consequences:** Bots and network commands reuse the vehicle unchanged; no speculative
  abstraction now.

## 8. Engine-free core library and xUnit tests: 2026-10-21, Accepted

- **Decision:** Introduce `core/` (no Godot reference) and `tests/` (xUnit) when lap timing
  is built (backlog D-8). Both get a `.gdignore` and are excluded from the Godot project.
- **Consequences:** The compiler enforces the rules/presentation boundary; lap rules are
  testable with `dotnet test`. Two extra project files.

## 9. Licensing: 2026-10-21, Accepted

- **Decision:** Code under MIT. Third-party assets keep their own licences, recorded in
  `assets/CREDITS.md`. Prefer CC0, accept CC-BY with attribution, reject non-commercial
  and ripped assets. Release builds include Godot's MIT notice.
- **Consequences:** The project can be published openly without asset legal debt.

## 10. No Git LFS for now: 2026-10-21, Accepted

- **Decision:** Commit binary assets directly; source art files (`.blend`) are not committed
  unless needed.
- **Consequences:** Revisit if binary assets exceed ~100 MB.

## 11. Camera direction: 2026-10-21, Accepted

- **Decision:** Fixed 3/4 isometric-style view like *Rock n' Roll Racing*, slightly more
  top-down, never rotating with the car. Orthographic vs. perspective is decided in
  backlog D-6.
- **Consequences:** Track art and scale are designed for one fixed viewing direction.

## 12. No autoloads for game state: 2026-10-21, Accepted

- **Decision:** Race and session state live in a recreatable `RaceSession`, not in global
  singletons.
- **Consequences:** Clean restarts, and a path to multiple sessions or a headless server.
