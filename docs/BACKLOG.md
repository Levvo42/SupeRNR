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
| D-4 | `VehicleTuning` resource; move all handling numbers into it | todo |
| D-5 | Lateral grip and handbrake drift with countersteer recovery | done |
| D-6 | Isometric follow camera (pick orthographic vs. perspective) | todo |
| D-7 | Graybox circuit with walls and start/finish | todo |
| D-8 | `core/` library + xUnit test project | todo |
| D-9 | Lap tracker (ordered checkpoints, lap times, best lap) with tests | todo |
| D-10 | Checkpoint `Area3D`s and `RaceSession` wiring | todo |
| D-11 | HUD: current, last and best lap | todo |
| D-12 | Session restart | todo |
| D-13 | Handling polish pass and submission build | todo |

## Not scheduled

Unscheduled features and ideas are in [IDEAS.md](IDEAS.md); known bugs are in
[BUGS.md](BUGS.md).
