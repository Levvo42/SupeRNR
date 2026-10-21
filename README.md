# SupeRNR

A 3D isometric arcade racer inspired by *Rock n' Roll Racing* and *Super Cars II*
(with a nod to *Skidmarks* and *Micro Machines*). Its defining feature is
**schmovement**: expressive, momentum-based car control (drifting, countersteering,
reverse manoeuvres and jumps) that is fun even with nobody else on the track.

## Status

**Pre-prototype.** The repository foundation and documentation exist; no gameplay
is implemented yet. The current milestone is a playable driving prototype; see
[docs/MVP.md](docs/MVP.md) and [docs/ROADMAP.md](docs/ROADMAP.md).

## Requirements

- [Godot 4.7.2, .NET edition](https://godotengine.org/download)
- .NET 10 SDK

## Getting started

1. Open `project.godot` in the Godot editor (or run `godot --path . --editor`).
2. Build C# with the editor's **Build** button, or `dotnet build SupeRNR.sln`.
3. Press **F5** in the editor, or run `godot --path .` (there is no main scene yet).

## Development workflow

Run these before every commit. All must pass with no warnings or changes:

```sh
dotnet build SupeRNR.sln                              # compile
dotnet format SupeRNR.sln --verify-no-changes         # style, driven by .editorconfig
godot_console --headless --path . --import            # Godot import/resource sanity check
```

- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/),
  e.g. `feat(vehicle): add handbrake drift`, `docs: update roadmap`.
- Work on short-lived branches off `main`; keep changes small.
- Move or rename Godot files **inside the Godot editor** so `res://` references and
  `.uid` files stay intact. Commit the `.uid` files.

## Documentation

Start at [docs/INDEX.md](docs/INDEX.md).

## Licence

Code is licensed under the [MIT License](LICENSE). Third-party assets keep their own
licences and will be recorded in `assets/CREDITS.md` as they are added.
