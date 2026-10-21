# Roadmap

Dates are targets, not promises. Concrete tasks live in [BACKLOG.md](BACKLOG.md).

## M0: Repository foundation (done)

Git, C# project, formatting, documentation. No gameplay.

## M1: Driving prototype (deadline 2026-11-14)

Scope and acceptance criteria: [MVP.md](MVP.md).

| Week | Dates | Focus |
|---|---|---|
| 1 | Oct 21 – Oct 27 | Foundation; physics spike; car drives on flat ground (throttle, brake, reverse, steer) with the input seam |
| 2 | Oct 28 – Nov 3 | Drift model and `VehicleTuning`; follow camera; graybox circuit with walls |
| 3 | Nov 4 – Nov 10 | Checkpoints, lap validation and timing (core library + tests); HUD; restart |
| 4 | Nov 11 – Nov 14 | Handling polish, bug fixing, submission build. **Buffer, so no new features** |

## Later milestones (unscheduled, order may change)

- **M2: Handling depth.** Jumps and ramps, surface types, a second car with different
  handling, juice (skid marks, smoke, sound).
- **M3: AI opponents.** Racing line following through the same `Vehicle`, race positions.
- **M4: Combat.** Weapons and ramming driven by orientation and momentum.
- **M5: Local multiplayer.** Split-screen or shared screen, multiple input devices.
- **M6: Online multiplayer.** Server-authoritative sessions.
- **M7: Release.** Menus, settings, content, desktop builds.
