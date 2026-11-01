# Add an asset

For third-party art, audio and fonts. `assets/` is created by the first one.

1. **Check the licence** before downloading. Commercial use must be allowed. Never use
   non-commercial or ripped assets.
2. **Copy it** to `assets/<source_pack>/`, in `snake_case`, with the pack's licence file if it has
   one.
3. **Add an entry** to `assets/CREDITS.md`:

   ```markdown
   | Asset | Author | Licence | Source |
   |---|---|---|---|
   | `assets/source_pack/engine_loop.ogg` | Author name | CC0 | https://example.com/pack |
   ```

4. **Open the Godot editor** so it imports the file. This creates a `.import` file beside it.
5. **Use it** from the feature that needs it. Assets stay in `assets/`, not in `game/`.
6. **[Finish the change](finish-a-change.md).** Commit the asset, its `.import` file and the credit
   together.
