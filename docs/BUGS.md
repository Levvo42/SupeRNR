# Known bugs

Defects we know about but haven't fixed yet. When a bug is fixed, delete its row and
mention the ID in the commit message (e.g. `fix(vehicle): wheels follow suspension (B-1)`).
Never reuse an ID. Feature wishes go in [IDEAS.md](IDEAS.md).

| ID | Bug | Where | How to see it | Fix planned in |
|---|---|---|---|---|
| B-2 | No way out when the car gets wedged (e.g. against a wall). It only rolls itself back over when it's on its side or roof | `game/vehicles/Vehicle.cs` | Drive into a tight corner of the walls at an angle | D-12 (session restart) |
| B-3 | All cars share one `PhysicsMaterial`, so each car's `_Ready` overwrites the `Bounce` of every other car. It can't show with one car or one tuning. `_Ready` also fails if the material is missing, which skips all the `GetNode` calls after it | `game/vehicles/Vehicle.cs:49`, `vehicle.tscn` | Two cars with different `Bounce` values: both use the last one | M3, or the first second car |
| B-4 | Sliding round past 180° into travelling backwards feels odd and is hard to stop. S only brakes while rolling forwards (`braking` needs `forwardSpeed > 1`), so rolling backwards it pushes the car further backwards. W while rolling backwards starts the wheelspin breakout. Not yet confirmed as the cause by measurement | `game/vehicles/Vehicle.cs:79`, `ApplyGlide` | Slide round until the car travels backwards, hold W a little too long, then press S | D-13 |
