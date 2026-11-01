# Ideas

Features and ideas that are **not scheduled**. Writing an idea down is not a commitment to
build it. When one is picked up, it moves to [BACKLOG.md](BACKLOG.md) as a task in a
milestone, and its line here links to that task. Known defects go in [BUGS.md](BUGS.md).

## Cars and handling

- **Car classes.** The spike car (`MaxWheelForce` 12000) is the baseline: mid-range in top
  speed and cornering. Other cars trade speed against grip and weight. They are different
  `VehicleTuning` data, not subclasses (VISION pillar 4).
- **Tyre compounds:** slick, wet, maybe off-road. Each is a grip multiplier, and it can
  depend on the surface or weather. Pairs naturally with pit stops.
- **Skid marks and engine sound.** Tyre smoke exists in the spike.
- **Debug overlay** for velocity, slip angle and grip.

## Racing

- **Pit stops:** repair, refuel, change tyres, rearm (once M4 combat exists). Driving
  through the pit lane costs time, so it's a strategy choice.
- **Damage and wrecks.** Damage builds up. A full wreck puts you back in the race,
  perhaps in the pit lane with a fresh car, at the cost of time.
- **Ghost car** replaying your best lap. It would be a natural second `DrivingInput`
  source.

## Tracks

- **Tracks in the air, over a void.** Falling off counts as crashing, as in Mario Kart,
  rather than being stopped by arena walls. Needs a way to put the car back on the track.
- **Surfaces:** grass, glass, ice and water, from shallow pools to deep water with its own
  physics. Grip and drag ignore the surface today, so leaving the road costs nothing.
- **Path choices:** routes that split and rejoin.
- **Obstacles and hazards:** things to jump over, and things that damage the car.
- **Jumps and ramps on real tracks.** The drift track already has test ramps: an
  overpass jump, three tabletops and a jump test pad with four kicker heights.

## Progression

- **Garage/shop:** buy cars and upgrades with credits. Cars are `VehicleTuning` data, so a
  new car is a new `.tres`.
- **Player profile, saved locally:** credits, best lap and race time per track. A JSON
  file (`System.Text.Json`) is enough long before a database is.
- **Achievements and online leaderboards,** built on the profile. Leaderboards need a
  server, so they belong with the multiplayer milestones.
