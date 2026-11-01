# Add a car

A car with different handling is a new `.tres` file. It needs no new code and no subclass.

1. In the Godot editor's **FileSystem** dock, right-click `game/vehicles/default_tuning.tres` and
   choose **Duplicate**.
2. Name the copy `<car>_tuning.tres`, for example `heavy_tuning.tres`.
3. Double-click the copy and change its values in the Inspector.
4. Select the `Car` node in the track scene and drag the new `.tres` onto its **Tuning** field.
   That sets the car for this track only. To change it for every track, set it in `vehicle.tscn`.
5. [Check the handling](../standards/verification.md#handling-and-track-changes), then
   [finish the change](finish-a-change.md).

**Two cars in one scene:** they share one `PhysicsMaterial`, so whichever car loads last sets
`Bounce` for both (B-3). Fix that bug before relying on cars with different `Bounce` values.
