# Raining Blood: Hellfire — console port technical guide

Starting point for programmers assessing the project and estimating the porting effort.

**Analyzed baseline:** local project on September 14, 2026, commit `de0b42a8`, Unity **6000.3.23f1**. This review covered code, settings, and serialized scene/prefab references; no build or console test was performed. Outstanding work is listed in [PORT_CONSOLES_CHECKLIST.md](PORT_CONSOLES_CHECKLIST.md).

Paths below are relative to the project root. Search for class names to quickly locate their code. The managers folder is literally named `Assets/Scripts/Managers and Handllers/`.

## 1. Main game flow

### Where to start

1. Open `Assets/Scenes/Main menu.unity`, the first enabled scene in `ProjectSettings/EditorBuildSettings.asset`.
2. Inspect `Assets/Prefabs/Managers/Game Session.prefab`, `Assets/Prefabs/Managers/MANAGER & CANVAS.prefab`, and `Assets/Prefabs/Managers/Canvas.prefab`.
3. For gameplay, open `Assets/Prefabs/Players/Player_PREFAB.prefab` and a combat scene such as `Assets/Scenes/Cemetery gates.unity`.
4. Read the classes below and inspect their events in the Inspector. Some connections use UnityEvents and `MMF_Player` feedbacks rather than direct C# calls.

### Overall sequence

`Main menu` → load progression → `HandleGameStart.StartGame()` → `Intro - Hell 2.0` if the tutorial is incomplete, or `The Void - HUB` if complete → select/start a run → combat, waves, bosses, and scene transitions → death/end of attempt → score display and return/restart.

| Class | Role and entry point |
| --- | --- |
| `GameDataAndSettings` | In `Data &  SO/`. Loads progression in `Awake()` through `LoadPlayerProgressOnce()`, applies settings, and persists across scenes through `DontDestroyOnLoad`. |
| `HandleGameStart` | Chooses the tutorial or HUB using `PlayerData.isTutorialFinished`. |
| `HubManager` | Updates NPCs, unlocks, and blocked routes; marks the tutorial complete and saves progression in `Start()`. |
| `HandleStartNewRun` | `StartNewRun()` finalizes the previous attempt when applicable, registers the new one, resets stats/run/health/runes, and applies permanent skills. The tutorial has a separate reset path. |
| `RunDataManager` / `RunData` | Maintain attempt state: health, weapons, points, character, wave, and statistics. `RunData` is a ScriptableObject used to carry state between scenes. |
| `PlayerHolder` / `PlayerInputHandler` | In `Player/`. Provide access to player components and distribute input commands. |
| `EnemyWaveManager` | Controls wave start/progression, spawn frequency, and enemy counts. Also see `EnemyWave`, `ProceduralEnemyWave`, `EnemyAI`, and the scripts in `Boss/`. |
| `GunManager`, `MeleeManager`, `HandleSpecialAttack` | Combat; depend on inventories, weapon data, stats, and feedbacks. |
| `Health`, `PlayerDeath`, `HandleGameOver` | `Health.OnZeroHealth` triggers death, screens, statistics, and score recording. |
| `ScoreManager` | `FinalizeCurrentRunScore()` prevents duplicate attempt records, updates progression, saves, and requests submission to the Steam leaderboard. Ignores tutorial/HUB attempts without combat progress. |
| `SceneLoader` | Changes scenes by name or index. `FinalizeRunAndLoadSpecificScene()` ends the attempt; `LoadSpecificScene()` also handles transitions that must preserve it. |

**Organization:** the core combines MonoBehaviours, shared ScriptableObjects (`PlayerData`, `RunData`, `GlobalStats`, weapon/rune data), C# events, and Inspector callbacks. No single class controls the entire flow. Changes to pause, death, and transitions must also account for prefabs and their overrides.

## 2. Save data

### File count and contents

**The game's progression system writes one file: `player.blood`.** There is one slot per directory, overwritten on each save. Preferences are stored separately in `PlayerPrefs`, so this file does not contain all persistent state.

| Storage | Contents / owner |
| --- | --- |
| `player.blood` | `SaveSystem` serializes `SavedData`, built from `PlayerData`: level, XP, skill points, horns, permanent upgrades, unlocked characters/NPCs, tutorial state, flags, and score history. |
| Game `PlayerPrefs` | `SettingsMenu`, `GameDataAndSettings`, `LanguageData`, and first-run scripts. Audio, language, effects, options, and first-use markers. The code does not define a custom file for this storage. |
| Kamgam `PlayerPrefs` | JSON under **`Settings.RainingBloodHellfire`**, defined in `Assets/Settings/GameSettings/SettingsProvider (Raining Blood Hellfire).asset`. `AutoSave=1`, with a configured one-second delay after changes. |

`RunData` preserves the attempt in memory between scenes; it is not part of the `SavedData` payload. No complete run save was identified for resuming the same attempt after closing the game.

Feel also contains an optional routine that writes `MMSoundManager/mmsound.settings`. The default asset examined has `AutoSave=0`, and writing this file through the game flow was not confirmed. It was not counted as an active progression file. CSV exporters and data tools are also separate from player saves.

### Paths and classes

- `Assets/Scripts/Data &  SO/SaveSystem.cs`: `SaveData()`, `LoadData()`, and `GetDirectoryPath()`.
- `Assets/Scripts/Data &  SO/SavedData.cs`: persisted data structure.
- `Assets/Scripts/Player/PlayerData.cs`: `LoadPlayerData()` applies data and handles read failures.
- `Assets/Scripts/Data &  SO/SaveGame.cs`: `SaveGameToDisk()`, also used by UI callbacks.
- `GameDataAndSettings.Awake()` is the loader identified in the startup flow. `LoadGame.Start()` → `LoadSave()` also exists, but `LoadGame` did not appear in the GUID dependency survey of enabled scenes.

Under `UNITY_STANDALONE_WIN`, the path is the user's Documents folder + `Raining Blood Hellfire/player.blood`, or `Raining Blood Hellfire Beta Test/player.blood` when `isBeta=true`. The other branch uses `Application.persistentDataPath/player.blood`, with no separate beta directory.

### Save triggers and validation

- `HubManager.Start()` saves when entering the HUB.
- `ScoreManager.FinalizeCurrentRunScore()` saves when finalizing a valid attempt. Death, restart, and specific exits use this logic.
- UI/manager prefabs contain `SaveGameToDisk` callbacks, including serialized overrides. Check buttons/events when changing menus.
- `SaveSystem.LoadData()` checks `File.Exists`, opens the file, and deserializes it with `BinaryFormatter`.
- `PlayerData.LoadPlayerData()` catches exceptions. A null result triggers `ResetSavedData()` in memory. It normalizes null upgrade/score lists, removes invalid wave-zero scores, and sets legacy attempt counts to at least 1.

**Porting work required:** no checksum, explicit format version, backup, temporary-write-and-replace operation, or player-facing recovery was found. Writes use `FileMode.Create` without local failure handling. A read error can leave reset data in memory that is later written over the existing save. The reset also does not reset every `SavedData` field and needs review. Define storage per user/platform and test migration, corruption, and interruptions before delivery.

## 3. Input

**Main system:** Unity Input System **1.20.0**, using `PlayerInput` and `Assets/Input System/PlayerInputActions.inputactions`, with **Player** and **UI** maps. The asset declares no control schemes. `activeInputHandler: 2` enables both the new and legacy systems.

**Partially centralized:** `PlayerInputHandler` receives callbacks and publishes events for shooting, melee, reload, interaction, dash, pause, inventory, and navigation. It still uses `UnityEngine.Input` for number keys, scrolling, mouse input, and legacy detection. Input is also read outside this class in `SettingsMenu`, `UiZoom`, `SkillTreeZoom`, `CameraController`, `CameraFollow`, `FollowMouse`, and debug scripts.

| Command | Main asset bindings |
| --- | --- |
| Movement / aim | Left stick or WASD / right stick; mouse aiming is handled in code |
| Shoot / melee | Left mouse button or RT/RB / right mouse button or LT |
| Interact / reload | E or south button / R or west button |
| Dash / special | Shift or LB/east button / space or either stick click |
| Switch weapon | D-pad, north button, and up/down arrows; scroll/1–4 also handled in code |
| Pause / inventory | Esc/Start and I/Tab/Select; the current `PauseMenuManager` routes Escape and inventory to `ShowinventoryMenu()` |
| UI | UI map, EventSystem, and `InputSystemUIInputModule`; `PressBackButton` also uses `OnMenuB` |

South/east/west/north refer to physical button positions. `ButtonsData.cs` includes `InputPromptDeviceType` and `InputPromptDeviceDetector_TechBooh`, with keyboard/mouse, Xbox, PlayStation, and Switch glyphs. This does not establish native console support.

`PlayerInputHandler.IsConsoleOnlyInputBuild()` already has a console branch for PS4/PS5/Switch/GameCore/XboxOne: it disables available mouse/keyboard devices, forces controller mode, and prevents automatic switching. Verify symbols and behavior on the chosen target.

**Controller disconnection:** `OnControllerChange` reports changes to the device used for input/prompts. `EnableObjectOnControllerConnection` reads this state despite its name suggesting physical connection. No game-owned handling of `InputSystem.onDeviceChange`/`PlayerInput.onDeviceLost` was identified. Do not use prompt switching to detect disconnection.

## 4. Cheats and development tools

**F9** is bound to the `Cheats` action. `PlayerInputHandler.ReadCheatInput()` only invokes `OnCheat` inside `#if UNITY_EDITOR`. `PauseMenuManager` uses this event to open the old pause/cheats menu. `ShowHideCheats` also controls visibility using `PlayerData.showCheats`.

`Assets/Scripts/Misc/CheatsHandler.cs` provides:

- Toggle invulnerability and hide the HUD.
- Add 10 horns, 1 health point, or 5 skill points.
- Add **10,000 points** (`InfinitePoints`, despite its name) or reset points to zero.
- Advance, go back, or select a wave.
- Switch special abilities: dash, decoy, arrow, trap, Azaus dash, and glass sword.
- Clear upgrades, set level to 1 and skill points to 0: `ResetSkillAndLevel()` **writes the result to disk**.

| Shortcut / tool | Class | Effect |
| --- | --- | --- |
| J / K | `ChangeGun_Debug` | Previous/next weapon in the test list |
| O / P | `ChangeSpecialAttack_Debug` | Previous/next special ability |
| - / = | `ChangeScene_Debug` | Previous/next scene in build order |
| Backspace | `ActivateOrbitalUpgrade_Debug` | Toggles an orbital upgrade object |
| U / I | `AutoUpgradeGun` | Increases/decreases projectile size and speed; also changes weapon data in `Awake()` |
| No dedicated shortcut | `ToggleEnemySpawn_TestScene`, `PushEnemiesDebug` | Toggle spawning and test enemy impulses |
| Public methods | `AchievementManager.ClearAllAchievmentStatus()` / `ClearAchievementStatus()` | Clear Steam achievements; testing only |

The five shortcut scripts above and `CheatsHandler` appear in serialized references reachable from enabled scenes. This does not prove their objects are active at runtime, but their components and overrides need review. The F9 guard does not automatically protect these other scripts, which lack the same Editor restriction.

## 5. Plugins and assets in use

**Evidence basis:** usage observed in code or serialized references from enabled scenes. This list guides the assessment; confirm the definitive included-content inventory in each target's Build Report. GUID references do not cover every serialized type, dynamic load, stripping decision, or library dependency.

| Dependency | Evidence / consideration |
| --- | --- |
| **URP 17.3.0** | Pipeline configured in `GraphicsSettings.asset`; effect scripts use URP. Check shaders, post-processing, and graphics profiles. |
| **Input System 1.20.0** | Action asset, `PlayerInputHandler`, and UI. |
| **Cinemachine 3.1.5** | `ManageCameraShake`, `ManageLensDistortion`, and feedback/camera integrations. |
| **uGUI 2.0.0 / TextMesh Pro** | Canvas, EventSystem, buttons, and text throughout the UI; TMP assets in `Assets/Packages B/TextMesh Pro/`. |
| **Feel / More Mountains** | `Assets/Packages B/Feel/`; `MMF_Player` calls and serialization, MMTools, audio, UI animations, and timescale control. Do not treat this as removable visual effects alone. |
| **Nice Vibrations, bundled with Feel** | Module present, integration symbol in Standalone, and referenced audio samples. Confirm actual haptics activation and required binaries separately. The game's `Vibrate`/`SmoothVibration` move transforms; they do not establish controller rumble usage. |
| **Kamgam Settings Generator and supporting components** | `Assets/Packages B/Kamgam/`; referenced game provider, options menu, UGUI components, and localization. Uses PlayerPrefs. |
| **VariaBULLET** | `Assets/Packages B/ND_VariaBULLET/`; `VariaShootingManager`, `ProjectileWithVaria`, `VariaAutomatorSetter`, `HandleAttackOnEnemy`, and referenced assets. |
| **A* Pathfinding Project 5.4.6** | Embedded package `Packages/com.arongranberg.astar/`; `EnemyAI`, `ScanPathFinder`, and pathfinding prefab. Include the embedded package in the handoff, alongside the manifest/lock files. |
| **Facepunch.Steamworks** | `Assets/Plugins/FacePunch.Steamworks/`; `SteamIntegration`, `Aux`, `ScoreManager`, `SteamLeaderBoardSystem`, `AchievementManager`, and `DlcManager`. Contains Win32/Win64/Posix DLLs and native Steam redistributables. Calls need platform isolation/replacement. |
| **Sprite Shaders Ultimate** | Package shaders and textures are reachable through scene references. Test compilation and appearance on the target. |
| **Unity Visual Scripting 1.9.12** | Present in the manifest and imports in game-owned scripts. Graph usage was not confirmed; check actual type usage before removal. |
| **Game content assets** | References in `Assets/Sprites/`, `Animations/`, `Tiles/`, `SFX/`, `Fonts/`, `Shaders & materials/`, `Prefabs/`, and `Scriptable Objects/`. Do not remove art folders because they contain no code. |

### Potential leftovers and review items

- **Kamgam:** the game provider references game settings → `QualityConnection.asset` → `Examples/FromAsset/Resources/SettingsProvider (FromAsset).asset`. Correct this link before deleting examples.
- **Third-party Resources:** these also exist in Kamgam, Feel, and VariaBULLET example folders. An asset is not necessarily excluded from a build just because no scene directly references it.
- **Scenes:** the global list contains **19 enabled scenes**, including `Test scene` and `_Blessings scene`. `Story Trailer` and `_loadout` are disabled. Review index-based calls before changing the order.
- **Legacy settings:** `EditorBuildSettings.asset` still references Addressables and Unity Localization, but neither package appears in the examined manifest or lock file. Data exists in `Assets/Settings/AddressableAssetsData/`. Confirm whether these are leftovers or a dependency that needs restoring.
- **Packages without confirmed gameplay usage in this review:** AI Inference 2.6.1, AI Navigation 2.0.14, Multiplayer Center 1.0.1, and Timeline 1.8.13. Evaluate the 2D packages (`feature.2d` 2.0.2 and `2d.enhancers` 1.0.0) against the assets in use.
- **Editor tools:** vHierarchy declares an Editor-only assembly. Rider/Visual Studio/VS Code, Collab Proxy, and Test Framework appear in the manifest; their presence does not imply player usage.

Package versions are recorded in `Packages/manifest.json`, `Packages/packages-lock.json`, and A*'s `package.json`. Unconfirmed commercial package versions were not assumed.

## 6. Windows and pop-ups

**Current pattern:** uGUI layouts in scenes/prefabs, panels toggled with `SetActive`, TMP text, button callbacks, and `MMF_Player` animations/effects. No single window-stack manager was identified.

| Entry point | Responsibility |
| --- | --- |
| `PauseMenuManager` | Inventory/pause, options, and restart/return confirmations. Blocks movement and changes time through `MMF_TimescaleModifier`. |
| `PopupTutorialManager.ShowPopup(PopupTutorialData)` | Populates the title, localized description, and image; displays the panel, selects the close button, blocks movement/pause, and sets timescale to zero. `ClosePopup()` releases input and plays closing feedback. |
| `SelectEventSystemButton`, `SelectButtonOnEnable`, `ForceButtonSelection` | Selection/focus for controller navigation. |
| `PressBackButton` | Routes back input to the configured button, respecting blocking children and hierarchy-depth priority. |
| `GeneralTextBoxUI`, `PopUpTextHandler` | Interaction text and messages; do not replace a modal that blocks gameplay. |

**Adding a window:** start from an existing UI panel, wire its references in the Inspector, populate text through `LanguageData`/localized data, define the initial button and back behavior, and configure feedbacks that work while paused. See `Assets/Prefabs/Managers/Canvas.prefab` and `Assets/Prefabs/UI/New inventory/Options Menu canvas.prefab` for composition examples.

**Controller-disconnected window (not yet implemented):** detect loss/return of the player's assigned device, open a priority modal, and block gameplay. The window must survive or be restored across relevant transitions, manage focus, and release control only after valid reconnection/assignment. Preserve the previous pause/input state: existing close methods often restore `timeScale=1` and movement, which could resume gameplay underneath another menu. Implement this coordination before reusing an existing popup's close behavior.

## Scope of this assessment

This guide describes the implementation found, not a completed port or a time estimate. Outstanding work and the validation plan are in the [preparation checklist](PORT_CONSOLES_CHECKLIST.md). Agree on platforms, generations, SDKs, performance targets, and specific requirements with the porting team.
