# C# conventions and good practice

How we write C# in SupeRNR, and why. These tutorials are the long version;
[CODING_RULES.md](../CODING_RULES.md) is the short checklist that points back here.

Each tutorial uses real code from this repository. Anything not built yet is labelled
*planned*. Sources were checked on **2026-11-01** against Godot 4.7.2, .NET 10 and C# 14.

## Reading order

| # | Tutorial | Covers | Course concepts ([map](../LEARNING_MAP.md)) |
|---|---|---|---|
| 1 | [Naming and layout](01-naming-and-layout.md) | Naming, meaningful names, organising a file, formatting, comments | Classes and objects, Properties |
| 2 | [Clean code](02-clean-code.md) | DRY, magic numbers, XML documentation, small methods | Static members, Value types / records |
| 3 | [Errors and null](03-errors-and-null.md) | Exceptions, nullable references, how Godot handles failure | Constructors, Encapsulation |
| 4 | [Design](04-design.md) | SOLID, dependency injection the Godot way, events | Interfaces, Polymorphism, Events, Inheritance |
| 5 | [Testing and review](05-testing-and-review.md) | Unit tests, source control, code review, what the tools catch | Unit testing |
| 6 | [Performance and security](06-performance-and-security.md) | Measuring, allocations, Godot interop, safe data | Collections, LINQ |
| 7 | [Staying current](07-staying-current.md) | Versions, what's new in C# 14, how we update | |

## Which tutorial has which practice

| Practice | Tutorial |
|---|---|
| 1. Naming conventions | [1](01-naming-and-layout.md#1-naming-conventions) |
| 2. Meaningful names | [1](01-naming-and-layout.md#2-meaningful-names) |
| 3. Organise your code | [1](01-naming-and-layout.md#3-organising-a-file) |
| 4. Don't repeat yourself | [2](02-clean-code.md#1-dont-repeat-yourself) |
| 5. Exceptions | [3](03-errors-and-null.md) |
| 6. Code documentation | [2](02-clean-code.md#3-documentation) |
| 7. Small, focused methods | [2](02-clean-code.md#4-small-focused-methods) |
| 8. Dependency injection | [4](04-design.md#2-dependency-injection-the-godot-way) |
| 9. Unit tests | [5](05-testing-and-review.md#1-unit-tests) |
| 10. Source control | [5](05-testing-and-review.md#2-source-control) |
| 11. Performance | [6](06-performance-and-security.md#part-1-performance) |
| 12. Security | [6](06-performance-and-security.md#part-2-security) |
| 13. SOLID | [4](04-design.md#1-solid) |
| 14. Code review | [5](05-testing-and-review.md#3-code-review) |
| 15. Stay up to date | [7](07-staying-current.md) |

## Where we differ from common advice

General C# checklists often say a few things that are dated or don't fit a Godot game.

| Common advice | What we do | Why |
|---|---|---|
| `ALL_CAPS` for constants | `PascalCase` | Microsoft, the .NET runtime and Godot all use PascalCase for constants |
| Use `#region` to organise | No regions | A class that needs regions usually needs splitting |
| `s_` prefix on private statics | `PascalCase` for `static readonly` | They read as constants; see [decision 21](../DECISIONS.md) |
| Use a DI container | The scene tree is our container | Godot creates our nodes, and `[Export]` hands them their dependencies |
| Make an interface for everything | An interface arrives with its second implementation | One implementation means the interface is guesswork |
| Parameterised SQL queries | Doesn't apply | We have no database; the game's risks are saves, mods and network input |

## Sources

Microsoft and .NET:

- [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [C# identifier naming](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- [Best practices for exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Unit testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Code analysis overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- [What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [Secure coding guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)

Godot:

- [C# style guide](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_style_guide.html)
- [C# API differences](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_differences.html)
- [C# collections](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_collections.html)
- [C# signals](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html)
- [C# exported properties](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_exports.html)
- [Scene organisation](https://docs.godotengine.org/en/stable/tutorials/best_practices/scene_organization.html)
- [`FileAccess`](https://docs.godotengine.org/en/stable/classes/class_fileaccess.html)
