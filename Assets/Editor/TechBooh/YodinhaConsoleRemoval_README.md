# Yodinha removal - publisher instructions

## Delivery and scope

Import only `YodinhaConsoleRemovalInstaller.cs` into `Assets/Editor/TechBooh/` in the
CONSOLE project's dedicated branch/copy. Keep the file under an Editor folder.
The installer is self-contained and does not depend on other TechBooh editor tools.
Importing the script does not apply any changes.

Running Apply modifies that project permanently: Yodinha will also be disabled if
you subsequently build a PC player from that same copy. It does not use a DLC flag
or conditional platform symbols. Do not run it in the shipping PC branch.

## Steps

1. Commit or back up the console branch. Save all scenes and close Prefab Mode.
2. Wait for Unity to compile, then exit Play Mode.
3. Run `TechBooh > Console > Remove Yodinha > 1 - Preview`.
4. If preview passes, run `2 - Apply to CONSOLE copy` and confirm the console warning.
5. Wait for compilation, then run `3 - Audit build references`.
6. Resolve any audit findings before publishing. Inspect the final Build Report too.

## Changes made

- `DlcManager.CheckDlc()` always returns false and clears `hasDlc`.
- `HubManager` never unlocks the Yodinha NPC (character enum value 6).
- `ChangeCharacter` rejects missing/Yodinha character data before effects,
  movement or equipment changes, and never reactivates the previous Yodinha NPC.
- `CharacterDataManager` rejects Yodinha in both selection entry points. Startup
  replaces unsupported/missing character data with Azaus before applying its Animator.
- `HighScoreTableRow` renders historical Yodinha entries with the Azaus portrait.
- Yodinha NPC instances are removed from inspected scenes/prefabs. The shared
  Hanba NPC prefab is not deleted. Character lists and other exclusive asset
  references are cleared; current-character fields use Azaus as fallback.
- Exclusive sprite references use the Azaus portrait; exclusive Animator references
  use the Azaus controller. Existing enum values, score history and other characters
  are preserved.

The original Yodinha source assets are NOT deleted. Cleaning references is not
proof that custom bundle/Addressables pipelines exclude those files. The audit
checks enabled build scenes, Resources, sprite atlases and possible Addressables
GUID references. Custom build scripts, external bundles, newly added content,
unnamed/repacked sprites or SDK-specific build profiles still require inspection.

## Backups and failures

Files are backed up before mutation under the project root:
`TechBoohBackups/Yodinha/<timestamp-id>/` (outside Assets).
The folder contains original files, their metadata, `manifest.txt` and a report.
Protect this folder from source-control cleanup until verification is complete.

Source transformations require known signatures. If publisher code differs, the
preview stops instead of guessing. On an application exception, backed-up files
are restored. Reopen affected scenes after rollback to discard stale in-memory
changes. A successful Apply is not a successful compilation or console build.

To revert, close Unity and restore the files listed in the manifest from the
backup to the same relative paths, then reopen Unity. Reverting the console branch
commit is preferable when using source control. Do not restore over newer work
without comparing the changes first.

## Required QA

- Force the DLC state to true: the character must remain unavailable.
- Start with `RunData.currentCharacter` pointing to Yodinha: Azaus must be used.
- Try both character selection APIs with Yodinha: it must not be loaded.
- Switch among supported characters repeatedly: no Yodinha NPC should appear.
- Check old leaderboard records, pause menus, portraits and death/corpse handling.
- Verify no Yodinha exclusive textures, animation clips, audio or character data
  are included in the actual console player/bundles.

Technical markers make source changes repeatable without duplicating injections.
Reimporting the installer does not rebuild or undo previously removed content.
