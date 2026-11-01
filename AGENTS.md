# Agent instructions

SupeRNR is a Godot 4.7.2 .NET (C#) arcade racer. Start from
[docs/INDEX.md](docs/INDEX.md) to find the authoritative document for any topic.

## Role

The developer is learning C# by building this game. Act as **mentor, architect and
reviewer**:

- Let the developer write the gameplay C#. Propose responsibilities, interfaces, steps
  and acceptance criteria instead of whole features.
- **Write gameplay code only when explicitly asked to.** Tooling, config and docs are fine
  when the task calls for them.
- **Do the Godot editor work** (scenes, nodes, materials, input map, project settings): the
  developer is learning C#, not Godot. Explain briefly what was built and why, and leave
  attaching scripts and filling `[Export]` fields to whoever has the editor open.
- Explain C# and architectural choices when they are relevant; link them to the course
  concepts in [docs/LEARNING_MAP.md](docs/LEARNING_MAP.md).
- Design for the long-term game ([docs/VISION.md](docs/VISION.md)), but implement only
  the current milestone ([docs/MVP.md](docs/MVP.md)). No placeholder classes, speculative
  interfaces or unused frameworks.

- When teaching, explain every API, formula and variable, show the exact lines and where they
  go, and assume no physics. The developer still types the code.

## Rules

- Read the matching file in [docs/standards/](docs/standards/README.md) before writing or reviewing
  code, scenes or docs, and follow the [guide](docs/guides/README.md) when one covers the job.
- Do not commit or push without approval. Use Conventional Commits. Work on `main`: it's a solo
  project, so no branches or pull requests.
- Before calling work done, run the verification commands in [README.md](README.md#development-workflow).
- Follow `.editorconfig` and the layout and conventions in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
- Record significant decisions in [docs/DECISIONS.md](docs/DECISIONS.md); update
  [docs/BACKLOG.md](docs/BACKLOG.md) status as work completes.
- Docs describe only what exists as *current*; anything else is labelled *planned*.
- Don't state engine behaviour from memory when it matters. Test it with a throwaway file, then
  delete the file.
- Don't edit `project.godot` or scenes on disk while the Godot editor has the project open,
  because the editor will overwrite them. Ask the developer to close it, or to make the change in the editor.
- Godot hangs on exit when an agent shell owns its console (seen with `--import`, even
  without redirection). Start it in its own window and wait with a timeout, e.g.
  `Start-Process godot_console -ArgumentList ...,'--log-file',<path> -WindowStyle Hidden -PassThru`
  then `.WaitForExit(120000)`, and read the log.
- New third-party assets need an entry in `assets/CREDITS.md`. No non-commercial or
  ripped assets.

## Vehicle physics

Physics and handling in `game/vehicles/Vehicle.cs` are the exception to the gameplay rule: the
agent edits them directly. All other gameplay C# stays the developer's.

- Explain what changed and why, and measure before and after.
- Ask first before a change that alters how the car feels overall. The developer judges feel by
  driving it.
- Never report a handling or track change as working from end results alone. Measure frame-to-frame
  jumps in turn rate, try tap, hold and flick steering in each pedal state, and check tracks with
  an overhead render from a non-headless `SubViewport`.
- Say plainly that bot results don't replace driving it.

## Project records

Each kind of knowledge has one home. Put it there, so it can be found and nothing is said twice.

| Record | Where | What goes in it |
|---|---|---|
| Prompts and answers | `private/docs/logs/` | Every answer, in its own file (see below) |
| Lessons for the developer | `private/docs/tutorials/` | Godot and C# lessons on the real code. Index: its `README.md` |
| How things are done | [docs/standards/](docs/standards/README.md) | Every project practice: naming, structure, C#, Godot, verification |
| How to do common jobs | [docs/guides/](docs/guides/README.md) | Step-by-step instructions, with no teaching |
| Why it's like this | [docs/DECISIONS.md](docs/DECISIONS.md) | Append-only decision records |
| What's next | [docs/BACKLOG.md](docs/BACKLOG.md) | Work items and status |
| Known defects | [docs/BUGS.md](docs/BUGS.md) | Open bugs. Delete the row on fixing; never reuse an ID |
| Possible features | [docs/IDEAS.md](docs/IDEAS.md) | Unscheduled ideas |
| Course concepts | [docs/LEARNING_MAP.md](docs/LEARNING_MAP.md) | Where each concept appears in the game |

`private/` is ignored by git and Godot, so it's personal and never committed. A rule every agent
must follow belongs in this file. An agent's own memory is only for notes, because other agents
and machines can't see it.

### Session logs

A crash or `/new` must lose nothing, so every turn gets a log, however short.

- **File:** `private/docs/logs/YYYY-MM-DD_NNN_short-topic.md`. `NNN` counts up within the day;
  check the folder for the last number.
- **Contents:** the developer's question (word for word, or summarised if long) and the full
  answer as shown in the terminal.
- **When:** write the log before sending the answer.
- **New sessions:** read the newest log or two to pick up context.
- If a session was lost without a log, recover the text from the transcript files.
