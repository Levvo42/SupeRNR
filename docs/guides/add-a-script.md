# Add a script

1. **Choose the kind of class.** [Where code belongs](../standards/project-structure.md#where-code-belongs)
   decides between a node, a resource, a value and engine-free code.
2. **Create the file** in `game/<feature>/`, named exactly after the class. Create the folder if
   this is the feature's first file.
3. **Start from the matching template** below. The namespace matches the folder.
4. **Build** in Rider or with the editor's **Build** button. Godot can't see a new class until then.
5. **Attach it,** for a node: select the node in the Godot editor and drag the `.cs` file from the
   **FileSystem** dock onto it.
6. **Fill in the `[Export]` fields** in the Inspector.
7. **[Finish the change](finish-a-change.md).** Godot creates a `.cs.uid` file next to the script,
   and it's committed with it.

## Templates

A node:

```csharp
using Godot;

namespace SupeRNR.Cameras;

// One line saying what this node is for.
public sealed partial class Example : Node3D
{
    [Export] public RigidBody3D? Target { get; set; }

    public override void _PhysicsProcess(double delta)
    {
    }
}
```

A resource, which holds data in a `.tres` file:

```csharp
using Godot;

namespace SupeRNR.Vehicles;

// One line saying what this data describes.
[GlobalClass]
public sealed partial class ExampleData : Resource
{
    // Unit, then what it does.
    [Export] public float Value { get; set; } = 1f;
}
```

A value passed between classes, which needs no `using Godot;` unless it holds Godot types:

```csharp
namespace SupeRNR.Vehicles;

// One line saying what this value means.
public readonly record struct ExampleValue(float First, bool Second);
```
