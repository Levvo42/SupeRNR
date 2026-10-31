using Godot;

namespace SupeRNR.Vehicles;

public partial class PlayerDriver : Node
{
    public DrivingInput Read()
    {
        return new DrivingInput(
            Throttle: Input.GetActionStrength("accelerate"),
            Brake: Input.GetActionStrength("brake"),
            Steer: Input.GetAxis("steer_right", "steer_left"),
            Handbrake: Input.IsActionPressed("handbrake"));
    }
}

