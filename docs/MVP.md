# MVP: Milestone 1, driving prototype

**Target date:** 2026-11-14 (academic deadline).
**Status:** not started. Progress is tracked in [BACKLOG.md](BACKLOG.md).

## Goal

A single car on a simple closed circuit, fun to drive on its own, with timed and
validated laps and a quick restart.

## Features and acceptance criteria

### 1. Controllable vehicle
- Throttle accelerates forward; releasing it lets the car coast and slow down.
- Brake slows the car; holding brake at (near) standstill reverses.
- Steering turns the car, with steering response depending on speed.
- Keyboard and gamepad both work through Godot input actions (no hard-coded keys).
- The vehicle does not read input directly; it receives a driving-input value from a
  separate player-driver component.

### 2. Momentum-based drifting
- The car can slide sideways: lateral grip is finite and can be overcome.
- A handbrake (or equivalent) breaks rear grip to start a drift on purpose.
- Countersteering controls and recovers a drift; releasing input returns to grip.
- Handling values (grip, power, mass, steering) live in a tunable resource,
  not in hard-coded constants.

### 3. Closed circuit with collisions
- One graybox circuit with walls/barriers the car collides with and bounces off.
- The car cannot leave the playable area.

### 4. Isometric-style follow camera
- Fixed 3/4 elevated angle, slightly more top-down than *Rock n' Roll Racing*.
- Does not rotate with the car; follows smoothly without jitter.

### 5. Lap validation and timing
- A lap only counts when all checkpoints are passed in order (no shortcuts, no
  reversing over the finish line).
- Shows the current lap time, last lap and best lap.
- Lap rules are covered by automated tests.

### 6. Restartable session
- One input restarts the session: car back at the start, timers and laps reset.
- Restarting does not leak state from the previous session.

## Out of scope

AI opponents, weapons, multiple cars, multiplayer/networking, menus beyond the bare
minimum, progression, final art and audio.

## Definition of done

All acceptance criteria above pass in a playtest, the verification commands in
[README.md](../README.md#development-workflow) pass, and [ARCHITECTURE.md](ARCHITECTURE.md)
describes what was built.
