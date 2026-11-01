# Documentation index

## Authoritative documents

| Document | Answers | Authoritative for |
|---|---|---|
| [VISION.md](VISION.md) | What are we ultimately making? | Design pillars, references, long-term scope, non-goals |
| [MVP.md](MVP.md) | What must milestone 1 do? | Prototype scope and acceptance criteria |
| [ROADMAP.md](ROADMAP.md) | In what order, by when? | Milestones and dates |
| [BACKLOG.md](BACKLOG.md) | What is the next concrete task? | Work items and their status |
| [IDEAS.md](IDEAS.md) | What might we build some day? | Unscheduled features and ideas |
| [BUGS.md](BUGS.md) | What is broken that we know about? | Known, unfixed defects |
| [ARCHITECTURE.md](ARCHITECTURE.md) | How is the code structured? | Layout, namespaces, boundaries, current vs. intended design |
| [DECISIONS.md](DECISIONS.md) | Why is it like this? | Decision records (context, decision, consequences) |
| [LEARNING_MAP.md](LEARNING_MAP.md) | Where does course concept X show up? | Mapping of C# concepts to game systems |

Outside `docs/`:

- [README.md](../README.md): setup, build/run, **verification commands**, workflow.
- [AGENTS.md](../AGENTS.md): rules for AI agents (imported by `CLAUDE.md`).
- [LICENSE](../LICENSE): MIT, covering code only.

## Directory map

The repository root is also the Godot project root (`res://`).

| Path | Status | Contents |
|---|---|---|
| `project.godot` | current | Godot project settings (input map, physics, rendering) |
| `docs/` | current | This documentation. Contains `.gdignore` so Godot skips it |
| `game/` | current | Our scenes, C# scripts and `.tres` resources, grouped by feature. Now: `spike/`, `vehicles/`, `cameras/`, `tracks/`. Planned: `race/`, `ui/`, `main/` |
| `tools/` | current | Development tools. `drift_track/` generates the drift track's terrain and rails |
| `assets/` | planned | Imported third-party art, audio and fonts, grouped by source pack; `assets/CREDITS.md` records licences |
| `core/` | planned | Engine-free C# library for testable rules (lap validation, timing). Has `.gdignore` |
| `tests/` | planned | xUnit tests for `core/`. Has `.gdignore` |
| `.godot/` | generated | Godot cache, never committed |

Folders are created when their first real content arrives, never as empty placeholders.
