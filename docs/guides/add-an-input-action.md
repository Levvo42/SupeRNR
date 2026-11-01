# Add an input action

This example adds a `horn` action.

## 1. Create the action

1. In the Godot editor, open **Project → Project Settings → Input Map**.
2. Type `horn` in **Add New Action** and press **Add**. Name it for what the player wants, not for
   the key.
3. Set its **Deadzone** to `0.2`.
4. Press **+** on the action, press the key, and choose **Physical Keycode** in the key's dialog.
5. Press **+** again and press the gamepad button or stick.

## 2. Read it

Only `PlayerDriver` reads input.

1. Add the value to `DrivingInput` in `game/vehicles/DrivingInput.cs`:

   ```csharp
   public readonly record struct DrivingInput(
       float Throttle,
       float Brake,
       float Steer,
       bool Handbrake,
       bool Horn)
   {
       public static DrivingInput None { get; } = new(0f, 0f, 0f, false, false);
   }
   ```

2. Add a cached name to `game/vehicles/PlayerDriver.cs`:

   ```csharp
   private static readonly StringName HornAction = "horn";
   ```

3. Read it in `PlayerDriver.Read`:

   ```csharp
   Horn: Input.IsActionPressed(HornAction));
   ```

| Input type | Method | Result |
|---|---|---|
| Button | `Input.IsActionPressed` | `bool` |
| Pedal or trigger | `Input.GetActionStrength` | `float` from 0 to 1 |
| Two opposite directions | `Input.GetAxis(negative, positive)` | `float` from -1 to 1 |

## 3. Finish

1. Add the action to **Input actions** in [ARCHITECTURE.md](../ARCHITECTURE.md#current).
2. Press F5 and test it on the keyboard and a gamepad.
3. [Finish the change](finish-a-change.md).
