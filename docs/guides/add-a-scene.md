# Add a scene

Done in the Godot editor. Node names follow [Naming](../standards/naming.md#scenes-and-nodes).

1. **Create it:** **Scene → New Scene**, then pick the root node type. A driven car's root is a
   `RigidBody3D`, for example, and a holder is a `Node3D`.
2. **Name the root** in `PascalCase`.
3. **Add the children,** named for their job: `RayFrontLeft`, not `RayCast3D2`.
4. **Save it** as `game/<feature>/<name>.tscn` in `snake_case`, so the root `FollowCamera` becomes
   `follow_camera.tscn`.
5. **[Add its script](add-a-script.md),** if it has one, and attach it to the root.
6. **Check it stands alone:** nothing in the scene should reach outside its own root.
7. **Place it:** open the scene that uses it and drag the `.tscn` from the **FileSystem** dock onto
   the parent node. Rename the instance for its role there, as `vehicle.tscn` became `Car`.
8. **Connect it:** select the instance and assign its `[Export]` fields in the Inspector. The
   track scene does this for `Car` and `FollowCamera`.
9. **Press F5** and check it works, then [finish the change](finish-a-change.md).

To make a scene the one the game starts with, set **Project Settings → Application → Run → Main
Scene**.
