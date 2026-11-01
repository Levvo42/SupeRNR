# Finish a change

1. **Read your diff:** `git diff`, against the [review list](../standards/verification.md#review).
2. **Run the [verification commands](../standards/verification.md#before-every-commit).** After a
   handling change, also do the [handling checks](../standards/verification.md#handling-and-track-changes).
3. **Update the records** the change affects:

   | If you... | Update |
   |---|---|
   | Finished a backlog task | Its status in `docs/BACKLOG.md` |
   | Fixed a bug | Delete its row from `docs/BUGS.md` |
   | Found a bug | Add a row to `docs/BUGS.md` with a new ID |
   | Made a significant choice | Add an entry to `docs/DECISIONS.md` |
   | Changed what exists | `docs/ARCHITECTURE.md`, moving *planned* text to current |
   | Started a new practice | The matching [standard](../standards/README.md) |
   | Met a known exception | Remove its row from the [standards list](../standards/README.md#known-exceptions) |
   | Added a course concept to the game | `docs/LEARNING_MAP.md` |

4. **Commit** with a [Conventional Commit](../standards/repository.md#commits) message:

   ```sh
   git add <files>
   git commit -m "fix(vehicle): wheels follow suspension (B-1)"
   ```

Agents also write the [session log](../../AGENTS.md#session-logs), and need approval to commit.
