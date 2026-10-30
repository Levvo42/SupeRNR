# Learning map

Where each course concept shows up naturally in SupeRNR. Status is `planned` until
the code exists, then `implemented` with a link to the file. Don't add features just to
show off a concept; if a concept has no natural home yet, leave it waiting for its milestone.

| Concept | Where it fits | Milestone | Status |
|---|---|---|---|
| Classes and objects | `Vehicle`, `RaceSession`, lap tracker: each owns one responsibility | M1 | planned |
| Constructors | Lap tracker built with its checkpoint count; validates arguments | M1 | planned |
| Properties | Read-only state such as `CurrentLap`, `BestLapTime`, `Speed` | M1 | planned |
| Encapsulation | Lap tracker exposes results but only changes through `RegisterCheckpoint(...)` | M1 | planned |
| Value types / records | `DrivingInput` as an immutable `readonly record struct` ([DrivingInput.cs](../game/vehicles/DrivingInput.cs)) | M1 | implemented |
| Static members | `DrivingInput.None` ([DrivingInput.cs](../game/vehicles/DrivingInput.cs)); pure static math helpers (e.g. grip curve) | M1 | implemented |
| Collections | `List<TimeSpan>` of lap times; ordered checkpoint array | M1 | planned |
| LINQ | Best lap and total time from the lap list; later, race standings ordering | M1 / M3 | planned |
| Algorithms | Ordered checkpoint validation; slip-angle and lateral-velocity projection; camera smoothing | M1 | planned |
| Inheritance | `Vehicle : RigidBody3D`, `VehicleTuning : Resource` (extending engine types) | M1 | planned |
| Events | Lap completed, checkpoint crossed (C# `event`) | M1 | planned |
| Unit testing | xUnit tests for lap rules in `tests/` | M1 | planned |
| Dictionaries | Race progress per vehicle, e.g. `Dictionary<Vehicle, RaceProgress>` | M3 | planned |
| Interfaces | Driving-input source shared by player, AI and ghost/network drivers | M3 (or ghost car) | planned |
| Polymorphism | `Vehicle` driven by any input source without knowing which | M3 | planned |
| Abstract classes | Shared base for weapons or pickups with common behaviour | M4 | planned |
