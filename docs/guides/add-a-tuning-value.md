# Add a tuning value

Any number that affects how a car feels goes in `VehicleTuning`, not in `Vehicle.cs`.

1. **Add the property** to `game/vehicles/VehicleTuning.cs`, in the `[ExportGroup]` section it
   belongs to. Start the comment with its unit:

   ```csharp
   [ExportGroup("Engine and brakes")]
   /// <summary>m/s backwards before the car counts as reversing.</summary>
   [Export] public float ReverseStartSpeed { get; set; } = 3f;
   ```

2. **Add a range** if the value has hard limits:

   ```csharp
   [Export(PropertyHint.Range, "0,1,0.01")] public float Bounce { get; set; } = 0.3f;
   ```

3. **Use it** in `Vehicle.cs` through the `Tuning` property:

   ```csharp
   bool reversing = forwardSpeed < -Tuning.ReverseStartSpeed;
   ```

4. **If it sets a property of the car's body,** copy it across in `_Ready`, as `Mass` and `Bounce`
   do.
5. **[Check the handling](../standards/verification.md#handling-and-track-changes).** Replacing a
   bare number with the same value shouldn't change anything, but measure it anyway.
6. **[Finish the change](finish-a-change.md).**

## Where a car's values come from

`default_tuning.tres` currently stores no numbers, so the default car uses the values written in
`VehicleTuning.cs`. Changing a value in the Inspector saves it in the `.tres`, and that saved value
then takes priority over the `.cs` default for that car.

Physics in `Vehicle.cs` is edited by the agent, not by hand: see
[AGENTS.md](../../AGENTS.md#vehicle-physics).
