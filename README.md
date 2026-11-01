# SupeRNR

A 3D isometric arcade racer inspired by *Rock n' Roll Racing* and *Super Cars II*
(with a nod to *Skidmarks* and *Micro Machines*). Its defining feature is
**schmovement**: expressive, momentum-based car control (drifting, countersteering,
reverse manoeuvres and jumps) that is fun even with nobody else on the track.

## Status

**Milestone 1 in progress.** The car, its drifting, the camera and the circuit are playable;
lap timing and the HUD come next. See [docs/MVP.md](docs/MVP.md),
[docs/ROADMAP.md](docs/ROADMAP.md) and [docs/BACKLOG.md](docs/BACKLOG.md).

## Requirements

- [Godot 4.7.2, .NET edition](https://godotengine.org/download)
- .NET 10 SDK
- C# IDE: the project is developed with [JetBrains Rider](https://www.jetbrains.com/rider/),
  which has Godot support built in and applies `.editorconfig` as you type. Any
  `.editorconfig`-aware editor works.

## Getting started

1. Open `project.godot` in the Godot editor (or run `godot --path . --editor`).
   To open scripts in Rider, set **Editor Settings → Dotnet → Editor → External Editor**
   to *JetBrains Rider* (a per-machine setting).
2. Build C# with the editor's **Build** button, in Rider, or `dotnet build SupeRNR.sln`.
3. Press **F5** in the editor, or run `godot --path .`. The main scene is the drift track,
   `game/tracks/drift_track.tscn`.

## Development workflow

Run these before every commit. All must pass with no warnings or changes:

```sh
dotnet build SupeRNR.sln --no-incremental -warnaserror   # compile, with analysers
dotnet format SupeRNR.sln --verify-no-changes            # style, driven by .editorconfig
godot_console --headless --path . --import               # Godot import/resource sanity check
```

`--no-incremental` matters: a normal build skips unchanged code, and with it that code's warnings.

- Every practice is in [docs/standards/](docs/standards/README.md), and the steps for common jobs
  are in [docs/guides/](docs/guides/README.md).
- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/),
  e.g. `feat(vehicle): add handbrake drift`, `docs: update roadmap`.
- Work on `main` and keep changes small.
- Move or rename Godot files **inside the Godot editor** so `res://` references and
  `.uid` files stay intact. Commit the `.uid` files.

## Exporting a test build

`export_presets.cfg` has two presets, **Windows Desktop** and **Linux**, which write to
`builds/windows/` and `builds/linux/` (ignored by git). Create those folders first. They need the
Godot 4.7.2 .NET export templates (**Editor → Manage Export Templates**). With the editor closed:

```sh
godot_console --headless --path . --export-release "Windows Desktop" builds/windows/SupeRNR.exe
godot_console --headless --path . --export-release "Linux" builds/linux/SupeRNR.x86_64
```

Pack the whole folder for each platform: the program needs the `.pck` and the `data_SupeRNR_*`
folder next to it. Use a `.zip` for Windows and a `.tar.gz` for Linux, with `SupeRNR.x86_64` marked
executable. Testers need nothing installed.

## Documentation

Start at [docs/INDEX.md](docs/INDEX.md).

## Licence

Code is licensed under the [MIT License](LICENSE). Third-party assets keep their own
licences and will be recorded in `assets/CREDITS.md` as they are added.
