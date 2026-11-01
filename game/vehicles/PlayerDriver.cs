using Godot;

namespace SupeRNR.Vehicles;

public partial class PlayerDriver : Node
{
    private static readonly StringName AccelerateAction = "accelerate";
    private static readonly StringName BrakeAction = "brake";
    private static readonly StringName SteerRightAction = "steer_right";
    private static readonly StringName SteerLeftAction = "steer_left";
    private static readonly StringName HandbrakeAction = "handbrake";
    public DrivingInput Read()
    {
        return new DrivingInput(
            Throttle: Input.GetActionStrength(AccelerateAction),
            Brake: Input.GetActionStrength(BrakeAction),
            Steer: Input.GetAxis(SteerRightAction, SteerLeftAction),
            Handbrake: Input.IsActionPressed(HandbrakeAction));
    }



}

