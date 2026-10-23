using Godot;

namespace SupeRNR.Spike;

// Spike car: a RigidBody3D pushed by one force per wheel, plus drift helpers.
public partial class SpikeCar : RigidBody3D
{
    // ---------------------------------------------------------------- Settings
    // [ExportGroup] starts a foldable section in the Inspector; every [Export]
    // after it belongs to that section until the next group starts.

    [ExportGroup("Engine and brakes")]
    // N, total, split over the rear wheels.
    [Export] public float EngineForce { get; set; } = 20000f;

    // N, total, split over all four wheels.
    [Export] public float BrakeForce { get; set; } = 24000f;

    // N per m/s of forward speed.
    [Export] public float RollingDrag { get; set; } = 200f;

    [ExportGroup("Grip")]
    // N, the most force one wheel can push with (its grip budget).
    [Export] public float MaxWheelForce { get; set; } = 10000f;

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

    // Steering share while reversing fast.
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

    [ExportGroup("Glide")]
    // Radians (~14°) of slide before the glide stabiliser works.
    [Export] public float GlideAngle { get; set; } = 0.25f;

    // Per second: how fast changes in the slide angle are calmed.
    [Export] public float GlideDamp { get; set; } = 4f;

    // Like DriftAssist, but while gliding without the handbrake.
    [Export(PropertyHint.Range, "0,1,0.05")] public float GlideAssist { get; set; } = 0.45f;

    [ExportGroup("Tire smoke")]
    // m/s of sideways slip where tire smoke starts.
    [Export] public float SmokeSlip { get; set; } = 3f;

    // m/s of sideways slip for full smoke.
    [Export] public float FullSmokeSlip { get; set; } = 10f;

    // --------------------------------------------------------------- Constants
    // Wheel contact points relative to the car's centre (x right, y up, -z forward).
    private static readonly Vector3 FrontLeftWheel = new(-0.8f, -0.4f, -1.3f);
    private static readonly Vector3 FrontRightWheel = new(0.8f, -0.4f, -1.3f);
    private static readonly Vector3 RearLeftWheel = new(-0.8f, -0.4f, 1.3f);
    private static readonly Vector3 RearRightWheel = new(0.8f, -0.4f, 1.3f);

    // ------------------------------------------------------------------- State
    // Values remembered from one physics frame to the next.
    private float _steerAngle;
    private float _previousHeading;
    private bool _pivoting;

    // Child nodes, found once in _Ready.
    private GpuParticles3D _smokeRearLeft;
    private GpuParticles3D _smokeRearRight;

    // --------------------------------------------------------- Godot callbacks
    public override void _Ready()
    {
        GD.Print($"Car ready. Mass: {Mass} kg");
        _smokeRearLeft = GetNode<GpuParticles3D>("SmokeRearLeft");
        _smokeRearRight = GetNode<GpuParticles3D>("SmokeRearRight");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Input
        float throttle = Input.GetAxis("ui_down", "ui_up");
        float steer = Input.GetAxis("ui_right", "ui_left");
        bool handbrake = Input.IsActionPressed("ui_accept");

        // Where the car is going
        Vector3 forward = -Basis.Z;
        float forwardSpeed = LinearVelocity.Dot(forward);
        bool reversing = forwardSpeed < -3f;

        // Engine and brakes
        bool braking = throttle < 0f && forwardSpeed > 1f;
        float rearDrive = braking ? 0f : throttle * EngineForce / 2f;
        float wheelBrake = braking ? BrakeForce / 4f : 0f;

        // Grip budgets: the handbrake weakens the trailing axle (the front when reversing)
        float leadMax = MaxWheelForce;
        float trailMax = MaxWheelForce * RearGripBias * (handbrake ? HandbrakeGrip : 1f);
        float frontMax = reversing ? trailMax : leadMax;
        float rearMax = reversing ? leadMax : trailMax;

        // Wheels
        Vector3 frontWheelRight = SteerFrontWheels(steer, forward, reversing, dt);
        ApplyWheel(FrontLeftWheel, frontWheelRight, 0f, wheelBrake, frontMax, 1f);
        ApplyWheel(FrontRightWheel, frontWheelRight, 0f, wheelBrake, frontMax, 1f);
        float rearLeftSlip = ApplyWheel(RearLeftWheel, Basis.X, rearDrive, wheelBrake, rearMax, SlideGrip);
        float rearRightSlip = ApplyWheel(RearRightWheel, Basis.X, rearDrive, wheelBrake, rearMax, SlideGrip);
        ApplyCentralForce(-forward * forwardSpeed * RollingDrag);

        // Drift helpers and effects
        ApplyGlide(throttle, forward, handbrake, dt);
        UpdateSmoke(rearLeftSlip, rearRightSlip);
    }

    // ---------------------------------------------------------------- Steering

    // Turns the front wheels towards the steering input, adds automatic
    // counter-steer while sliding, and returns the front wheels' right direction.
    private Vector3 SteerFrontWheels(float steer, Vector3 forward, bool reversing, float delta)
    {
        float targetAngle = steer * MaxSteerAngle * (reversing ? ReverseSteer : 1f);
        _steerAngle = Mathf.MoveToward(_steerAngle, targetAngle, SteerSpeed * delta);

        float wheelAngle = _steerAngle;
        Vector3 flatVelocity = new(LinearVelocity.X, 0f, LinearVelocity.Z);
        Vector3 flatForward = new(forward.X, 0f, forward.Z);
        if (!reversing && flatVelocity.Length() > 3f)
        {
            float bodySlide = flatVelocity.SignedAngleTo(flatForward, Vector3.Up);
            wheelAngle += Mathf.Clamp(-bodySlide * CounterSteer, -MaxCounterSteer, MaxCounterSteer);
        }

        return Basis.X.Rotated(Basis.Y, wheelAngle);
    }

    // ------------------------------------------------------------------ Wheels

    // Pushes the car at one wheel and returns the wheel's sideways slip (m/s).
    private float ApplyWheel(Vector3 localPosition, Vector3 wheelRight, float driveForce, float brakeForce, float maxForce, float slideGrip)
    {
        // How the wheel moves
        Vector3 offset = Basis * localPosition;
        Vector3 wheelVelocity = LinearVelocity + AngularVelocity.Cross(offset);
        Vector3 wheelForward = Basis.Y.Cross(wheelRight);
        float slip = wheelVelocity.Dot(wheelRight);
        float rollSpeed = wheelVelocity.Dot(wheelForward);

        // Sideways grip, with most of its speed-killing part removed
        Vector3 gripForce = -wheelRight * slip * WheelGrip;
        Vector3 travel = LinearVelocity.Normalized();
        float scrub = gripForce.Dot(travel);
        if (scrub < 0f)
        {
            gripForce -= travel * scrub * (1f - SlideScrub);
        }

        // Engine and brake
        Vector3 drive = wheelForward * driveForce;
        Vector3 brake = -wheelForward * Mathf.Sign(rollSpeed) * brakeForce;

        // Sliding tires: less grip budget (0 = gripping, 1 = fully sliding)
        float slipAngle = Mathf.Atan2(Mathf.Abs(slip), Mathf.Abs(rollSpeed));
        float sliding = Mathf.SmoothStep(SlideStart, SlideFull, slipAngle) * Mathf.SmoothStep(1f, 4f, Mathf.Abs(slip));
        float budget = maxForce * Mathf.Lerp(1f, slideGrip, sliding);

        // Gripping: everything shares the budget. Spinning: only the sideways grip does,
        // so the engine keeps pushing in a slide.
        Vector3 gripping = (gripForce + drive + brake).LimitLength(budget);
        Vector3 spinning = (gripForce.LimitLength(budget) + drive + brake).LimitLength(maxForce);
        ApplyForce(gripping.Lerp(spinning, sliding), offset);
        return slip;
    }

    // ----------------------------------------------------------- Drift helpers

    // Holds the slide angle steady while drifting without the handbrake. With the
    // handbrake held, or past PivotAngle, it hands over to the drift assist.
    private void ApplyGlide(float throttle, Vector3 forward, bool handbrake, float delta)
    {
        // How fast the direction of travel turns (radians per second)
        Vector3 flatVelocity = new(LinearVelocity.X, 0f, LinearVelocity.Z);
        float heading = Mathf.Atan2(-flatVelocity.X, -flatVelocity.Z);
        float travelTurnRate = Mathf.Wrap(heading - _previousHeading, -Mathf.Pi, Mathf.Pi) / delta;
        _previousHeading = heading;
        if (flatVelocity.Length() < 5f)
        {
            return;
        }

        // Pivot: stays on from PivotAngle until the slide is back under DriftAngle
        float slide = flatVelocity.SignedAngleTo(new Vector3(forward.X, 0f, forward.Z), Vector3.Up);
        if (Mathf.Abs(slide) > PivotAngle)
        {
            _pivoting = true;
        }
        else if (Mathf.Abs(slide) < DriftAngle)
        {
            _pivoting = false;
        }

        if (handbrake || _pivoting)
        {
            ApplyDriftAssist(throttle, forward, delta);
            return;
        }

        if (Mathf.Abs(slide) < GlideAngle)
        {
            return;
        }

        // On the gas and spinning further into the slide: momentum follows part of the spin
        float yawRate = AngularVelocity.Y;
        if (throttle > 0f && yawRate * slide > 0f)
        {
            LinearVelocity = LinearVelocity.Rotated(Vector3.Up, yawRate * GlideAssist * delta);
        }

        // Calm the change of slide angle, not the spin itself
        float slideRate = yawRate - travelTurnRate;
        float calmedYawRate = yawRate - slideRate * Mathf.Min(1f, GlideDamp * delta);
        AngularVelocity = new Vector3(AngularVelocity.X, calmedYawRate, AngularVelocity.Z);
    }

    // On the gas in a big slide: momentum follows part of the spin and the spin is damped.
    private void ApplyDriftAssist(float throttle, Vector3 forward, float delta)
    {
        if (throttle <= 0f || LinearVelocity.Length() < 1f)
        {
            return;
        }

        float slideAngle = LinearVelocity.AngleTo(forward);
        float yawRate = AngularVelocity.Dot(Basis.Y);
        float noseSide = LinearVelocity.Cross(forward).Dot(Basis.Y);
        bool spinningAway = yawRate * noseSide > 0f;

        if (slideAngle > DriftAngle && spinningAway)
        {
            float followAngle = yawRate * DriftAssist * delta;
            LinearVelocity = LinearVelocity.Rotated(Basis.Y, followAngle);
            AngularVelocity *= 1f - DriftYawDamp * delta;
        }
    }

    // ----------------------------------------------------------------- Effects

    private void UpdateSmoke(float rearLeftSlip, float rearRightSlip)
    {
        _smokeRearLeft.AmountRatio = SmokeAmount(rearLeftSlip);
        _smokeRearRight.AmountRatio = SmokeAmount(rearRightSlip);
    }

    // 0 below SmokeSlip, 1 from FullSmokeSlip, a straight ramp in between.
    private float SmokeAmount(float slip)
    {
        return Mathf.Clamp((Mathf.Abs(slip) - SmokeSlip) / (FullSmokeSlip - SmokeSlip), 0f, 1f);
    }
}
