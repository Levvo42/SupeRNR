namespace SupeRNR.Vehicles;

public readonly record struct DrivingInput(float Throttle, float Brake, float Steer, bool Handbrake)
{
    public static DrivingInput None { get; } = new(0f, 0f, 0f, false);
}
