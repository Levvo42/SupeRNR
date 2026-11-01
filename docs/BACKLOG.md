# Backlog

Status: `todo` · `doing` · `done`. Ordered by priority within each section.
Acceptance criteria for M1 items are in [MVP.md](MVP.md).

## M0: Foundation

| ID | Task | Status |
|---|---|---|
| F-1 | Git repository, ignore rules, `.gitattributes` | done |
| F-2 | Rename project to SupeRNR; generate C# solution (`net10.0`, nullable) | done |
| F-3 | `.editorconfig` C# conventions; `dotnet build` and `dotnet format` pass | done |
| F-4 | Documentation set (README, AGENTS, docs/) | done |
| F-5 | First commit | done |

## M1: Driving prototype

| ID | Task | Status |
|---|---|---|
| D-1 | Physics spike: `RigidBody3D` car on a flat plane, check feel and stability (timeboxed: 1 day) | done |
| D-2 | Input actions in the Input Map (keyboard + gamepad) and `DrivingInput` + `PlayerDriver` | done |
| D-3 | `Vehicle`: throttle, brake, reverse, speed-dependent steering | done |
| D-4 | `VehicleTuning` resource; move all handling numbers into it | done |
| D-5 | Lateral grip and handbrake drift with countersteer recovery | done |
| D-6 | Isometric follow camera (pick orthographic vs. perspective) | done |
| D-7 | Graybox circuit with walls and start/finish | done |
| D-8 | `core/` library + xUnit test project | todo |
| D-9 | Lap tracker (ordered checkpoints, lap times, best lap) with tests | todo |
| D-10 | Checkpoint `Area3D`s and `RaceSession` wiring | todo |
| D-11 | HUD: current, last and best lap | todo |
| D-12 | Session restart | todo |
| D-13 | Handling polish pass and submission build | todo |

## Code quality

From the code review of 2026-11-01. None of these change how the car drives, so each counts as
a refactor. Who does it: **you** for your files, **agent** for `Vehicle.cs` physics.

| ID | Task | Who | Status |
|---|---|---|---|
| Q-1 | `PlayerDriver`: cache the five input action names as `static readonly StringName` | you | todo |
| Q-2 | `Vehicle`: name the bare thresholds. Handling values (lines 76, 79, 152, 232, 255, 340) go in `VehicleTuning`; the rest (374, 380, 394, 403) become named `const`s | agent | todo |
| Q-3 | `Vehicle`: use `GlobalBasis` instead of `Basis`, and remove the `GD.Print` in `_Ready` | agent | todo |
| Q-4 | `Vehicle`: replace the long parameter lists (`SteerFrontWheels`, `ApplyWheel`, `ApplyGlide`) with a per-tick `readonly record struct` | agent | todo |
| Q-5 | `Vehicle`: remove duplication (`Flat` helper, a yaw-rate setter, one loop over four wheels), then split it where the banner comments are | agent | todo |
| Q-6 | `FollowCamera` and `PlayerDriver`: formatting slips, and a comment on each class ([exercise](tutorials/01-naming-and-layout.md#try-it-yourself)) | you | todo |
| Q-7 | Report unassigned required exports (`FollowCamera.Target`, `Vehicle.Tuning`) instead of carrying on silently ([exercise](tutorials/03-errors-and-null.md#try-it-yourself)) | you | todo |

## Not scheduled

Unscheduled features and ideas are in [IDEAS.md](IDEAS.md); known bugs are in
[BUGS.md](BUGS.md).
