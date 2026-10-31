using Godot;

namespace SupeRNR.Cameras;

public partial class FollowCamera : Node3D
{

    //The car to follow
    [Export] public RigidBody3D? Target { get; set; }
    // Per second: how quickly the camera catches up with where it wants to be.
    [Export] public float FollowSpeed { get; set; } = 5f;

    // Seconds of travel the camera looks ahead of the car.
    [Export] public float LookAhead { get; set; } = 0.5f;

    // Per second: how quickly the look-ahead turns to a new direction of travel.
    [Export] public float LookAheadTurn { get; set; } = 3f;

    private Vector3 _lead;

    public override void _PhysicsProcess(double delta)
    {
        if (Target is null)
        {
            return;
        }

        float dt = (float)delta;

        _lead = _lead.Lerp(Target.LinearVelocity * LookAhead, Share(LookAheadTurn, dt));

        Vector3 goal = Target.GlobalPosition + _lead;
        goal.Y = GlobalPosition.Y;
        GlobalPosition = GlobalPosition.Lerp(goal, Share(FollowSpeed, dt));
    }
    // The share of the remaining  distance to cover this frame, at any frame rate.
    private static float Share(float speed, float dt)
    {
        return 1f - Mathf.Exp(-speed * dt);
    }
}
