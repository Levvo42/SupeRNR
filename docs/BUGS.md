# Known bugs

Defects we know about but haven't fixed yet. When a bug is fixed, delete its row and
mention the ID in the commit message (e.g. `fix(vehicle): wheels follow suspension (B-1)`).
Never reuse an ID. Feature wishes go in [IDEAS.md](IDEAS.md).

| ID | Bug | Where | How to see it | Fix planned in |
|---|---|---|---|---|
| B-2 | No way out when the car gets wedged (e.g. against a wall). It only rolls itself back over when it's on its side or roof | `game/vehicles/Vehicle.cs` | Drive into a tight corner of the walls at an angle | D-12 (session restart) |
