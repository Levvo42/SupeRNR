using Godot;

namespace SupeRNR.Vehicles;

// The numbers that make one car feel the way it does.
[GlobalClass]
public partial class VehicleTuning : Resource
{
    // [ExportGroup] starts a foldable section in the Inspector; every [Export]
    // after it belongs to that section until the next group starts.

    [ExportGroup("Body")]
    // kg.
    [Export] public float Mass { get; set; } = 1200f;

    // Share of its speed into a wall the car keeps coming back off it. Hard landings bounce too.
    [Export(PropertyHint.Range, "0,1,0.01")] public float Bounce { get; set; } = 0.3f;

    [ExportGroup("Engine and brakes")]
    // N, total, split over the rear wheels.
    [Export] public float EngineForce { get; set; } = 20000f;

    // N, total, split over all four wheels.
    [Export] public float BrakeForce { get; set; } = 24000f;

    // N, total, split over the rear wheels, while reversing.
    [Export] public float ReverseForce { get; set; } = 14000f;

    // m/s, reverse gear's top speed: the engine adds no speed above this.
    [Export] public float MaxReverseSpeed { get; set; } = 20f;

    // Share of its speed the car must carry along its nose to count as rolling straight.
    [Export(PropertyHint.Range, "0,1,0.01")] public float StraightShare { get; set; } = 0.99f;

    // Share of EngineForce that reaches the road while the gas fights backwards momentum.
    [Export(PropertyHint.Range, "0,1,0.05")] public float WheelspinPush { get; set; } = 0.35f;

    // N per m/s of forward speed.
    [Export] public float RollingDrag { get; set; } = 200f;

    [ExportGroup("Grip")]
    // N, the most force one wheel can push with (its grip budget).
    [Export] public float MaxWheelForce { get; set; } = 9000f;

    // N per m/s of sideways slide.
    [Export] public float WheelGrip { get; set; } = 6000f;

    // Rear budget relative to the front (higher = more stable).
    [Export] public float RearGripBias { get; set; } = 1.2f;

    // Share of the speed-killing grip that is kept (1 = realistic).
    [Export(PropertyHint.Range, "0,1,0.05")] public float SlideScrub { get; set; } = 0.15f;

    // Rear grip share once a wheel fully slides.
    [Export(PropertyHint.Range, "0,1,0.05")] public float SlideGrip { get; set; } = 0.5f;

    // Radians of wheel slip angle where grip starts to fall.
    [Export] public float SlideStart { get; set; } = 0.15f;

    // Radians of wheel slip angle where grip reaches SlideGrip.
    [Export] public float SlideFull { get; set; } = 0.5f;

    [ExportGroup("Steering")]
    // Radians, full steering lock.
    [Export] public float MaxSteerAngle { get; set; } = 0.6f;

    // Radians per second the front wheels turn.
    [Export] public float SteerSpeed { get; set; } = 4f;

    // Steering share left at SteerFadeSpeed and above.
    [Export(PropertyHint.Range, "0,1,0.05")] public float HighSpeedSteer { get; set; } = 0.4f;

    // m/s where the steering has finished shrinking.
    [Export] public float SteerFadeSpeed { get; set; } = 40f;

    // Steering share left at MaxReverseSpeed. The steered wheels trail going backwards,
    // so the same lock turns the car much harder than going forwards.
    [Export(PropertyHint.Range, "0,1,0.05")] public float ReverseSteer { get; set; } = 0.3f;

    // Share of the slide the front wheels self-align (automatic counter-steer).
    [Export(PropertyHint.Range, "0,1,0.05")] public float CounterSteer { get; set; } = 1f;

    // Radians, the most self-alignment added to the steering.
    [Export] public float MaxCounterSteer { get; set; } = 1.4f;

    [ExportGroup("Handbrake and pivot")]
    // Rear budget share while the handbrake is held.
    [Export(PropertyHint.Range, "0,1,0.05")] public float HandbrakeGrip { get; set; } = 0.25f;

    // Share of the car's spin that momentum follows on the gas.
    [Export(PropertyHint.Range, "0,1,0.05")] public float DriftAssist { get; set; } = 0.6f;

    // Radians (~25°) of slide before the drift assist starts.
    [Export] public float DriftAngle { get; set; } = 0.45f;

    // Share of the spin removed per second while the drift assist works.
    [Export] public float DriftYawDamp { get; set; } = 3f;

    // Radians (~92°): past this the drift assist takes over and finishes the turn.
    [Export] public float PivotAngle { get; set; } = 1.6f;

    // Radians (~6°) off exactly backwards before the gas starts swinging the car round.
    [Export] public float BreakoutAngle { get; set; } = 0.1f;

    // Radians per second the nose swings round while the gas fights backwards momentum.
    [Export] public float BreakoutYaw { get; set; } = 3f;

    // Radians per second, per second: how quickly that swing builds up.
    [Export] public float BreakoutRamp { get; set; } = 14f;

    // Radians (~17°) off exactly backwards where the swing reaches full strength.
    [Export] public float BreakoutFullAngle { get; set; } = 0.3f;

    [ExportGroup("Glide")]
    // Radians (~14°) of slide before the glide stabiliser works.
    [Export] public float GlideAngle { get; set; } = 0.25f;

    // Per second: how fast changes in the slide angle are calmed.
    [Export] public float GlideDamp { get; set; } = 4f;

    // Like DriftAssist, but while gliding without the handbrake.
    [Export(PropertyHint.Range, "0,1,0.05")] public float GlideAssist { get; set; } = 0.45f;

    // Per second: like GlideDamp, but while sliding with no pedal held.
    [Export] public float CoastCalm { get; set; } = 12f;

    [ExportGroup("Tire smoke")]
    // m/s of sideways slip where tire smoke starts.
    [Export] public float SmokeSlip { get; set; } = 3f;

    // m/s of sideways slip for full smoke.
    [Export] public float FullSmokeSlip { get; set; } = 10f;

    [ExportGroup("Suspension")]
    // N per metre a spring is squeezed. The spring length is the ray's length.
    [Export] public float SpringStrength { get; set; } = 30000f;

    // N per m/s a spring is squeezed or stretched (the shock absorber).
    [Export] public float SpringDamping { get; set; } = 3000f;

    // Metres from the centre of a wheel mesh to the bottom of its tire.
    [Export] public float WheelRadius { get; set; } = 0.3f;

    // m/s a wheel mesh can drop when the road falls away.
    [Export] public float WheelDropSpeed { get; set; } = 3f;

    [ExportGroup("Recovery")]
    // In the air: how hard the car turns towards upright, per radian of tilt.
    [Export] public float AirLevel { get; set; } = 6f;

    // In the air: share of the tumbling (pitch and roll spin) removed per second.
    [Export] public float AirDamp { get; set; } = 2f;

    // Seconds stuck on the side or roof before the car rolls itself back over.
    [Export] public float RightDelay { get; set; } = 1f;

    // Radians per second the car rolls while righting itself.
    [Export] public float RightSpeed { get; set; } = 3f;

    // m/s upward hop when righting starts, so the roof does not scrape.
    [Export] public float RightHop { get; set; } = 2f;
}
