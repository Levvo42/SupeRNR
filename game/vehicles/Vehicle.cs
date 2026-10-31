using Godot;

namespace SupeRNR.Vehicles;

// A RigidBody3D pushed by one force per wheel, plus drift helpers.
public partial class Vehicle : RigidBody3D
{
    // Who drives this car. Without a driver it just rolls.
    [Export] public PlayerDriver? Driver { get; set; }

    // ---------------------------------------------------------------- Settings
    // [ExportGroup] starts a foldable section in the Inspector; every [Export]
    // after it belongs to that section until the next group starts.

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
    [Export] public float MaxWheelForce { get; set; } = 8500f;

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
    [Export(PropertyHint.Range, "0,1,0.05")] public float HighSpeedSteer { get; set; } = 0.35f;

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

    // --------------------------------------------------------------- Constants
    // Where the tires push, relative to the car's centre (x right, y up, -z forward).
    // Kept fixed instead of using the suspension's ground point, which changed the tuned handling.
    private static readonly Vector3 FrontLeftWheel = new(-0.8f, -0.4f, -1.3f);
    private static readonly Vector3 FrontRightWheel = new(0.8f, -0.4f, -1.3f);
    private static readonly Vector3 RearLeftWheel = new(-0.8f, -0.4f, 1.3f);
    private static readonly Vector3 RearRightWheel = new(0.8f, -0.4f, 1.3f);

    // ------------------------------------------------------------------- State
    // Values remembered from one physics frame to the next.
    private float _steerAngle;
    private float _previousHeading;
    private bool _pivoting;
    private bool _breakingOut;
    private float _tippedTime;
    private bool _righting;

    // Child nodes, found once in _Ready. "= null!" tells the compiler they are
    // never null when used, because _Ready runs before _PhysicsProcess.
    private RayCast3D _rayFrontLeft = null!;
    private RayCast3D _rayFrontRight = null!;
    private RayCast3D _rayRearLeft = null!;
    private RayCast3D _rayRearRight = null!;
    private MeshInstance3D _wheelFrontLeft = null!;
    private MeshInstance3D _wheelFrontRight = null!;
    private MeshInstance3D _wheelRearLeft = null!;
    private MeshInstance3D _wheelRearRight = null!;
    private GpuParticles3D _smokeRearLeft = null!;
    private GpuParticles3D _smokeRearRight = null!;

    // --------------------------------------------------------- Godot callbacks
    public override void _Ready()
    {
        GD.Print($"Car ready. Mass: {Mass} kg");
        _rayFrontLeft = GetNode<RayCast3D>("RayFrontLeft");
        _rayFrontRight = GetNode<RayCast3D>("RayFrontRight");
        _rayRearLeft = GetNode<RayCast3D>("RayRearLeft");
        _rayRearRight = GetNode<RayCast3D>("RayRearRight");
        _wheelFrontLeft = GetNode<MeshInstance3D>("WheelFrontLeft");
        _wheelFrontRight = GetNode<MeshInstance3D>("WheelFrontRight");
        _wheelRearLeft = GetNode<MeshInstance3D>("WheelRearLeft");
        _wheelRearRight = GetNode<MeshInstance3D>("WheelRearRight");
        _smokeRearLeft = GetNode<GpuParticles3D>("SmokeRearLeft");
        _smokeRearRight = GetNode<GpuParticles3D>("SmokeRearRight");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Input
        DrivingInput input = Driver?.Read() ?? DrivingInput.None;
        float throttle = input.Throttle - input.Brake;
        float steer = input.Steer;
        bool handbrake = input.Handbrake;

        // Where the car is going
        Vector3 forward = -Basis.Z;
        float forwardSpeed = LinearVelocity.Dot(forward);
        bool reversing = forwardSpeed < -3f;

        // Engine and brakes
        bool braking = input.Brake > 0f && forwardSpeed > 1f;
        bool reverseLimit = throttle < 0f && forwardSpeed < -MaxReverseSpeed;
        bool wheelspin = throttle > 0f && reversing;
        float enginePush = throttle < 0f ? ReverseForce : EngineForce;
        if (wheelspin)
        {
            enginePush *= WheelspinPush;
        }

        float rearDrive = braking || reverseLimit ? 0f : throttle * enginePush / 2f;
        float wheelBrake = braking ? input.Brake * BrakeForce / 4f : 0f;

        // Free-rolling keeps its speed: above reverse speed, or spinning on the handbrake off the gas
        bool sliding = Mathf.Abs(forwardSpeed) < LinearVelocity.Length() * StraightShare;
        bool freeRolling = reverseLimit || (handbrake && sliding && throttle <= 0f);
        LinearDampMode = freeRolling ? DampMode.Replace : DampMode.Combine;

        // Grip budgets: the handbrake weakens the trailing axle (the front when reversing)
        float leadMax = MaxWheelForce;
        float trailMax = MaxWheelForce * RearGripBias * (handbrake ? HandbrakeGrip : 1f);
        float frontMax = reversing ? trailMax : leadMax;
        float rearMax = reversing ? leadMax : trailMax;

        // Suspension: first, because it tells which wheels touch the ground
        bool frontLeftDown = ApplySuspension(_rayFrontLeft, _wheelFrontLeft, dt);
        bool frontRightDown = ApplySuspension(_rayFrontRight, _wheelFrontRight, dt);
        bool rearLeftDown = ApplySuspension(_rayRearLeft, _wheelRearLeft, dt);
        bool rearRightDown = ApplySuspension(_rayRearRight, _wheelRearRight, dt);
        int wheelsDown = (frontLeftDown ? 1 : 0) + (frontRightDown ? 1 : 0) + (rearLeftDown ? 1 : 0) + (rearRightDown ? 1 : 0);
        bool onGround = wheelsDown >= 2;

        // Wheels: only the ones on the ground grip, drive and brake (0 slip in the air)
        Vector3 frontWheelRight = SteerFrontWheels(steer, forward, reversing, wheelspin, handbrake, dt);
        if (frontLeftDown)
        {
            ApplyWheel(FrontLeftWheel, frontWheelRight, 0f, wheelBrake, frontMax, 1f);
        }

        if (frontRightDown)
        {
            ApplyWheel(FrontRightWheel, frontWheelRight, 0f, wheelBrake, frontMax, 1f);
        }

        float rearLeftSlip = rearLeftDown ? ApplyWheel(RearLeftWheel, Basis.X, rearDrive, wheelBrake, rearMax, SlideGrip) : 0f;
        float rearRightSlip = rearRightDown ? ApplyWheel(RearRightWheel, Basis.X, rearDrive, wheelBrake, rearMax, SlideGrip) : 0f;
        if (onGround && !freeRolling)
        {
            ApplyCentralForce(-forward * forwardSpeed * RollingDrag);
        }

        // Drift helpers, recovery and effects
        ApplyGlide(throttle, forward, handbrake, wheelspin, onGround, dt);
        ApplyRecovery(wheelsDown, forward, dt);
        UpdateSmoke(rearLeftSlip, rearRightSlip, wheelspin && onGround);
    }

    // ---------------------------------------------------------------- Steering

    // Turns the front wheels towards the steering input, adds automatic
    // counter-steer while sliding, and returns the front wheels' right direction.
    private Vector3 SteerFrontWheels(float steer, Vector3 forward, bool reversing, bool wheelspin, bool handbrake, float delta)
    {
        float speed = Mathf.Abs(LinearVelocity.Dot(forward));
        float fade = Mathf.Clamp(speed / (reversing ? MaxReverseSpeed : SteerFadeSpeed), 0f, 1f);
        float speedSteer = Mathf.Lerp(1f, reversing ? ReverseSteer : HighSpeedSteer, fade);
        // A handbrake spin back round needs the full lock
        speedSteer = reversing && handbrake ? 1f : speedSteer;
        float targetAngle = steer * MaxSteerAngle * speedSteer;
        _steerAngle = Mathf.MoveToward(_steerAngle, targetAngle, SteerSpeed * delta);

        float wheelAngle = _steerAngle;
        Vector3 flatVelocity = new(LinearVelocity.X, 0f, LinearVelocity.Z);
        Vector3 flatForward = new(forward.X, 0f, forward.Z);
        if ((!reversing || wheelspin) && flatVelocity.Length() > 3f)
        {
            // Rolling backwards, the wheels line up with the reverse direction instead
            float bodySlide = flatVelocity.SignedAngleTo(flatForward, Vector3.Up);
            bodySlide = reversing ? Mathf.Wrap(bodySlide, -Mathf.Pi / 2f, Mathf.Pi / 2f) : bodySlide;
            wheelAngle += Mathf.Clamp(-bodySlide * CounterSteer, -MaxCounterSteer, MaxCounterSteer);
        }

        return Basis.X.Rotated(Basis.Y, wheelAngle);
    }

    // -------------------------------------------------------------- Suspension

    // One spring per wheel: the ray looks down from the wheel's mount. Pushes the
    // car up at the ground point, moves the wheel mesh and returns whether the wheel
    // touches the ground.
    private bool ApplySuspension(RayCast3D ray, MeshInstance3D wheel, float delta)
    {
        // Cast now: the ray's own update would give last frame's result
        ray.ForceRaycastUpdate();
        if (!ray.IsColliding())
        {
            PlaceWheel(wheel, ray, ray.TargetPosition.Length(), delta);
            return false;
        }

        // How far the spring is squeezed (0 = fully stretched)
        Vector3 hitPoint = ray.GetCollisionPoint();
        float distance = ray.GlobalPosition.DistanceTo(hitPoint);
        float compression = ray.TargetPosition.Length() - distance;

        // How fast it is being squeezed (negative = stretching)
        Vector3 offset = hitPoint - GlobalPosition;
        Vector3 pointVelocity = LinearVelocity + AngularVelocity.Cross(offset);
        float compressionSpeed = -pointVelocity.Dot(Basis.Y);

        // Spring plus shock absorber. Max: a tire can push on the road, never pull.
        float springForce = compression * SpringStrength + compressionSpeed * SpringDamping;
        ApplyForce(Basis.Y * Mathf.Max(0f, springForce), offset);
        PlaceWheel(wheel, ray, distance, delta);
        return true;
    }

    // Puts the bottom of the wheel mesh at the end of the spring. Only the picture moves.
    // A wheel rises at once, so it never sinks into the road, but drops at WheelDropSpeed,
    // so it doesn't snap down when the car leaves a ramp.
    private void PlaceWheel(MeshInstance3D wheel, RayCast3D ray, float distance, float delta)
    {
        float tireHeight = ray.Position.Y - distance + WheelRadius;
        float height = Mathf.Max(tireHeight, wheel.Position.Y - WheelDropSpeed * delta);
        wheel.Position = new Vector3(wheel.Position.X, height, wheel.Position.Z);
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
    private void ApplyGlide(float throttle, Vector3 forward, bool handbrake, bool wheelspin, bool onGround, float delta)
    {
        // How fast the direction of travel turns (radians per second). The heading is
        // saved every frame, also in the air, so the first frame after landing is right.
        Vector3 flatVelocity = new(LinearVelocity.X, 0f, LinearVelocity.Z);
        float heading = Mathf.Atan2(-flatVelocity.X, -flatVelocity.Z);
        float travelTurnRate = Mathf.Wrap(heading - _previousHeading, -Mathf.Pi, Mathf.Pi) / delta;
        _previousHeading = heading;
        if (!onGround || flatVelocity.Length() < 5f)
        {
            _breakingOut = false;
            return;
        }

        // Pivot: stays on from PivotAngle until the slide is back under DriftAngle
        float slide = flatVelocity.SignedAngleTo(new Vector3(forward.X, 0f, forward.Z), Vector3.Up);
        if (Mathf.Abs(slide) > PivotAngle)
        {
            _pivoting = true;
        }
        else if (Mathf.Abs(slide) < DriftAngle || (!handbrake && throttle > 0f))
        {
            _pivoting = false;
        }

        // Gas against backwards momentum: stays on until the nose has come round
        float backwardsOffset = Mathf.Pi - Mathf.Abs(slide);
        if (wheelspin && !handbrake && backwardsOffset > BreakoutAngle)
        {
            _breakingOut = true;
        }
        else if (handbrake || throttle <= 0f || Mathf.Abs(slide) < DriftAngle)
        {
            _breakingOut = false;
        }

        if (_breakingOut)
        {
            ApplyBreakout(slide, backwardsOffset, delta);
            return;
        }

        // No pedal: hold the angle and let the tyres scrub the speed off
        if (!handbrake && Mathf.IsZeroApprox(throttle) && Mathf.Abs(slide) > GlideAngle)
        {
            CalmSlide(travelTurnRate, CoastCalm, delta);
            return;
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

        CalmSlide(travelTurnRate, GlideDamp, delta);
    }

    // The spinning rear swings the nose round: gently near exactly backwards, hardest
    // around sideways, easing off as the car lines up. Builds up instead of snapping.
    private void ApplyBreakout(float slide, float backwardsOffset, float delta)
    {
        float leaving = Mathf.SmoothStep(0f, BreakoutFullAngle, backwardsOffset);
        float arriving = Mathf.Min(1f, Mathf.Abs(slide) / PivotAngle);
        float targetYaw = -Mathf.Sign(slide) * BreakoutYaw * leaving * arriving;
        float yawRate = Mathf.MoveToward(AngularVelocity.Y, targetYaw, BreakoutRamp * delta);
        AngularVelocity = new Vector3(AngularVelocity.X, yawRate, AngularVelocity.Z);
    }

    // Calms the change of slide angle, not the spin itself.
    private void CalmSlide(float travelTurnRate, float strength, float delta)
    {
        float yawRate = AngularVelocity.Y;
        float slideRate = yawRate - travelTurnRate;
        float calmedYawRate = yawRate - slideRate * Mathf.Min(1f, strength * delta);
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

    // ---------------------------------------------------------------- Recovery

    // Keeps the car on its wheels: levels it in the air, and rolls it back over
    // when it is stuck on its side or roof.
    private void ApplyRecovery(int wheelsDown, Vector3 forward, float delta)
    {
        // upDot: 1 upright, 0 on its side, -1 on the roof. Turning around tiltAxis
        // brings the roof back up; its length is 0 upright and exactly upside down.
        float upDot = Basis.Y.Dot(Vector3.Up);
        Vector3 tiltAxis = Basis.Y.Cross(Vector3.Up);

        // In the air: a spring towards upright plus a damper on tumbling. The spin
        // around world up (yaw) is left alone.
        if (wheelsDown == 0)
        {
            float tilt = Mathf.Acos(Mathf.Clamp(upDot, -1f, 1f));
            Vector3 axis = tiltAxis.Length() > 0.01f ? tiltAxis.Normalized() : forward;
            Vector3 tumble = AngularVelocity - Vector3.Up * AngularVelocity.Y;
            AngularVelocity += (axis * tilt * AirLevel - tumble * AirDamp) * delta;
        }

        // Tipped over 60° and not standing on its wheels for RightDelay seconds: hop
        bool stuck = upDot < 0.5f && wheelsDown < 2;
        _tippedTime = stuck ? _tippedTime + delta : 0f;
        if (_tippedTime > RightDelay && !_righting)
        {
            _righting = true;
            LinearVelocity += Vector3.Up * RightHop;
        }

        if (!_righting)
        {
            return;
        }

        // Nearly upright again: the springs take over
        if (upDot > 0.8f)
        {
            _righting = false;
            _tippedTime = 0f;
            return;
        }

        // Roll the shorter way at exactly RightSpeed (either way when on the roof)
        float side = tiltAxis.Dot(forward);
        float rollDirection = Mathf.Abs(side) < 0.05f ? 1f : Mathf.Sign(side);
        float rollNow = AngularVelocity.Dot(forward);
        AngularVelocity += forward * (rollDirection * RightSpeed - rollNow);
    }

    // ----------------------------------------------------------------- Effects

    private void UpdateSmoke(float rearLeftSlip, float rearRightSlip, bool wheelspin)
    {
        float spinSmoke = wheelspin ? 1f : 0f;
        _smokeRearLeft.AmountRatio = Mathf.Max(SmokeAmount(rearLeftSlip), spinSmoke);
        _smokeRearRight.AmountRatio = Mathf.Max(SmokeAmount(rearRightSlip), spinSmoke);
    }

    // 0 below SmokeSlip, 1 from FullSmokeSlip, a straight ramp in between.
    private float SmokeAmount(float slip)
    {
        return Mathf.Clamp((Mathf.Abs(slip) - SmokeSlip) / (FullSmokeSlip - SmokeSlip), 0f, 1f);
    }
}
