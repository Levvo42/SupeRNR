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

## 13. Agent builds scenes, developer writes C#: 2026-10-21, Accepted

- **Context:** The learning goal is C#, not the Godot editor, which is new to the developer.
- **Decision:** The agent builds scenes, materials, input map and project settings, and
  explains what it built. The developer writes the gameplay C# and attaches scripts.
- **Consequences:** Faster progress on what the course grades. Scene files are written
  on disk, so the editor must be closed (or the scene not yet open) when the agent edits them.

## 14. Ray suspension with fixed tyre points: 2026-10-26, Accepted

- **Context:** The spike car's box slid on the ground and its wheel forces also worked in
  the air, so it flew off hills, stopped dead at kerbs and could not recover from its roof.
- **Decision:** One `RayCast3D` spring and damper per wheel (30000 N/m, 3000 N·s/m,
  0.5 m) holds the car up. A wheel only grips, drives and brakes while its ray hits the
  ground, but its tyre force stays at the fixed body point (y -0.4) rather than at the ray's
  hit point, which cut grip cornering by about 20%. Gravity scale 2. Recovery: the car
  levels itself in the air and rolls itself back over after 1 s on its side or roof.
- **Consequences:** Measured on a 14° ramp: 1.4 s airtime instead of 3–5 s, and it lands on
  its wheels. The drift tune is unchanged. The ray must call `ForceRaycastUpdate()` or the
  spring reacts one frame late. The wheel meshes follow the springs, but only the picture
  moves: the tyre force points stay fixed.

## 15. Ramps are CSG bodies on top of the terrain: 2026-10-26, Accepted

- **Context:** The spike drift track is a 1 m heightmap. A ramp needs a sharp lip and a
  4 m drop, which a 1 m grid cannot hold without steep, broken triangles.
- **Decision:** Each ramp is a `CSGPolygon3D` with collision: its side profile (smooth
  transition, straight face, lip) is extruded across the road and sunk 0.3 m into the
  ground. Shapes are sized with a point-mass flight model at the speeds the car really
  reaches there, then checked by driving the car over them.
- **Consequences:** Ramps can be moved and reshaped without touching the terrain. The
  flight model matched the car to within about 1 m of landing point. The overpass lands at
  every speed the car reaches after corner 7 (36–41 m/s at the lip, 0.8–1.1 s airtime).

## 16. Reverse is a low gear, and free-rolling keeps momentum: 2026-10-30, Accepted

- **Context:** The car should be able to spin 180°, travel backwards at speed (to shoot
  behind it later) and spin back, without the move killing its speed. A plain cap on
  reverse speed bled that speed away. Rolling drag, Godot's default linear damping (0.1
  per second) and the spins themselves were all taking speed.
- **Decision:** Reverse works like first gear: strong pull (14000 N), but the engine adds
  no speed above `MaxReverseSpeed` (20 m/s). Above that speed, with the reverse pedal held,
  the car free-rolls: no rolling drag and no linear damping, so it keeps its momentum. A
  handbrake slide off the gas free-rolls too, but only while the car is actually sliding, so
  a straight-line handbrake still slows it. Backwards, the steering fades harder with speed
  (ReverseSteer, 30% at reverse top speed) because the steered wheels trail, but the
  handbrake keeps the full lock for a fast spin back.
- **Consequences:** On flat ground, spinning from 30 m/s gives about 27 m/s backwards and
  about 24 m/s out of the second spin. Holding reverse keeps speed indefinitely on flat
  ground, which is deliberate; releasing every pedal coasts as normal. Pressing reverse
  before the car has turned past sideways still brakes it. Stronger reverse makes a tap roll
  further (9 m from 0.2 s), so precise parking needs a tap of gas to stop.

## 17. Grip is 2.8 g, so tight turns reward a slide: 2026-10-30, Accepted

- **Context:** The car should feel arcadey but allow real drifting techniques, and a drift
  must be faster than braking through a very tight turn. At 3.9 g the car steered through
  most turns, and off the gas a slide either spun round or snapped straight, with no skid
  in between.
- **Decision:** `MaxWheelForce` drops from 12000 N to 8500 N (2.8 g, 23 m radius at 25 m/s).
  With no pedal held, `CoastCalm` holds the slide angle while the tyres scrub the speed off.
  Gas against backwards momentum spins the wheels: `WheelspinPush` lets 35% of the engine
  through, with smoke. More than about 6° off straight backwards, the nose swings round. The
  swing is a helper, not a tyre force, and its turn rate builds up at `BreakoutRamp` and eases
  off as the car lines up, because setting it directly felt like a jerk.
- **Consequences:** `TightTurn` in `test_track.tscn` is the benchmark: an 8 m gate into a bowl
  around a 4 m inner radius, with a 20 m wide exit. A scripted driver reached the end of the
  exit walls in 4.42 s drifting and 4.87 s braking, wall-free, and was 0.35 s ahead 100 m
  later. An earlier 8 m wide hairpin was rejected: it left no room to exit on full throttle.
  The gas catch from a 70° slide still peaks near 8 g, from the velocity-rotating drift
  helpers. The car is rear-wheel drive only.
