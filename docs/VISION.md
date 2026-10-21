# Vision

> Everything in this document is a **goal**, not a description of what exists.
> For what is built, see [ARCHITECTURE.md](ARCHITECTURE.md); for the current milestone, see [MVP.md](MVP.md).

## Pitch

SupeRNR is a 3D isometric arcade racer in the spirit of *Rock n' Roll Racing* and
*Super Cars II*, with influences from *Skidmarks* and *Micro Machines*. Short, chaotic
races on tight circuits, where how you move the car matters as much as how fast it is.

## Pillars

1. **Schmovement first.** Momentum-based handling with drifting, countersteering,
   handbrake turns, reverse manoeuvres and jumps. Driving alone on an empty track must
   already be fun and reward skill.
2. **Readable at a glance.** A fixed, elevated camera shows the car's orientation and
   slide clearly, so players can read and correct a drift.
3. **Orientation and momentum matter in combat.** Where your car points and how it moves
   determines what you can hit and how hard, so combat comes out of the driving itself.
4. **Distinct vehicles.** Cars differ in handling character (grip, weight, drift
   behaviour), defined as data rather than code.

## Camera direction

Like *Rock n' Roll Racing*: a fixed 3/4 isometric-style view that does **not** rotate
with the car. Slightly more top-down than the original, so drift angles and the space
around the car stay readable. Projection (orthographic vs. narrow-FOV perspective) is
decided when the camera is built (see [DECISIONS.md](DECISIONS.md)).

## Long-term scope

- Solo races against AI opponents.
- Vehicle combat (weapons, ramming) shaped by orientation and momentum.
- Multiple vehicles with different handling.
- Local multiplayer.
- Online multiplayer with authoritative game-state management.
- A possible public desktop release (open source, not monetised).

## Non-goals

- Realistic driving simulation.
- Open-world or free-roam driving.
- Monetisation, live-service features.
