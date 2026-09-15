# Seaside — VS Code Handoff

Updated 15 September 2026

## Open the correct workspace

Open `C:\Users\ramin\Desktop\Repos\Seaside` as the VS Code folder. The Unity project is rooted there; do not open `Assets` as a standalone folder.

Use Unity **6000.6.0f1**. Before editing scenes or assets, make sure the Seaside Editor is open and its Pipeline connection is ready.

## Read before changing code

1. [Agent directives](../agents/agents.md) — project architecture and Unity editing rules.
2. [Baseline audit](BaselineAudit.md) — verified state, known gaps, and evidence.
3. [Completion plan](CompletionPlan.md) — ordered work remaining.

## Current handoff state

- Active branch: `gdd-gameplay-bootstrap-alignment`, tracking `origin/gdd-gameplay-bootstrap-alignment`. Preserve all existing uncommitted work.
- The Editor was left on `LV_TestScene`, stopped, with no compilation errors.
- `GameManager` scene-loading and menu-button fixes were completed earlier in this workspace.
- The technical-debt pass replaced deprecated Unity object lookups, removed obsolete/unused fields, and added null/empty-input guards to the small material-switching components.
- `DayNightCycle` now clones its skybox material in Play Mode. The source material was verified at exposure `1.200` before and after a midnight runtime test; the runtime clone reached `0.200` independently.
- `Tools > Build iOS` is provided by `Assets/Editor/IOSBuildTool.cs`. It exports a Swift Xcode project to the next ignored folder under `Builds/` (`_build1`, `_build2`, ...), increments `PlayerSettings.bundleVersion`'s patch number before each build, and builds the enabled Build Settings scenes. It compiled successfully in Unity 6000.6.0f1; an actual iOS export has not been run from this Windows workstation.
- Latest verification: `unity command recompile_status --project-path 'C:/Users/ramin/Desktop/Repos/Seaside'` returned `completed` with `compilationFailed: false` after adding the iOS tool. `git diff --check` passed.
- Live check on this branch: Unity 6000.6.0f1 is connected, stopped, and open on `LV_TestScene` with no compilation in progress. `LV_TestScene` is the gameplay bootstrap; it is not a disposable placeholder for a future `Main` scene.
- Agent directives now explicitly prohibit introducing a second persistent manager, require instance/save-owned narrative progress rather than static flags, and require live-route verification before a gameplay task is marked complete.
- Arrival-sequence foundation added: `ArrivalSequenceStateMachine`, `BoatArrivalController` docking/disembark guards, `PlayerController.ReleaseBoatMovement`, and event-channel-based `ArrivalTutorialUI`. The three `ArrivalSequenceStateMachineTests` passed in EditMode. Scene integration is intentionally pending: `LV_TestScene` contains `PF_Boat_Parent` but it has no `BoatArrivalController`/`BoatInteractable` and no authored approach, stand, disembark, or land-checkpoint transforms. Do not guess those placements from the existing artwork.
- A CLI-driven menu → `LV_TestScene` load reached the gameplay scene but could not complete while the Game view was not advancing in the background: the persistent menu manager remained `IsLoading=true` and its duplicate was still pending deferred destruction. This is not a confirmed gameplay-shell regression; re-run it with the Game view focused/advancing before changing manager serialization.
- Recommended next action: author and wire the boat's approach, stand, disembark, and land-checkpoint transforms in `LV_TestScene`, then run the arrival route in a normally advancing Game view. Follow with the menu/direct-scene comparison and the remaining boat, interaction, pause, return-to-menu, and replay regression.

## Safe working conventions

- Keep Inspector-facing fields as `[SerializeField] private`.
- Prefer ScriptableObject event channels over new direct system-to-system calls.
- Use `FindAnyObjectByType` only for one-time service/reference discovery; do not call object searches from `Update`.
- Use `MaterialPropertyBlock` for per-renderer visual changes. Do not add `renderer.material` calls in frame loops.
- Change Unity scenes through the Editor or Unity CLI, never by hand-editing `.unity` YAML.
- Preserve existing uncommitted work. The repository already has user changes in font assets, menu scene, packages, ProjectSettings, GameManager, and `Docs`/`Tools`.

## Quick verification

From the repository root in PowerShell:

```powershell
unity command editor_status --project-path 'C:/Users/ramin/Desktop/Repos/Seaside'
unity command recompile --project-path 'C:/Users/ramin/Desktop/Repos/Seaside'
unity command recompile_status --project-path 'C:/Users/ramin/Desktop/Repos/Seaside'
unity command console --tail 50 --level error --project-path 'C:/Users/ramin/Desktop/Repos/Seaside'
```

The expected compile result is `completed` with `compilationFailed: false`.

## Known follow-up items

- `PF_KB3D_ENC_Grass` reports a shader warning: it requires `Nature/Soft Occlusion` for correct billboarding and lighting.
- The Editor reports Graphics Ring Buffer exhaustion during rendering. Profile before changing command-line buffer settings.
- Fire-item acquisition/reset is still not wired to a persistent inventory system.
- The menu and direct gameplay scenes retain different manager configurations. Validate both entry paths in an advancing Game view, then make the smallest configuration/code change that gives them equivalent gameplay behavior.
- Run the boat, movement, interaction, pause, and replay regression route before claiming the existing game shell is complete.

## Suggested first prompt in VS Code

> Read `agents/agents.md`, `Docs/VSCodeHandoff.md`, `Docs/BaselineAudit.md`, and `Docs/CompletionPlan.md`. Confirm the Unity Editor is connected to `C:/Users/ramin/Desktop/Repos/Seaside`, inspect the current git status without reverting anything, update the handoff after meaningful work, then continue the highest-priority unfinished item using the project directives.
