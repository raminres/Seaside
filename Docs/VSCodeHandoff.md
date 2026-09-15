# Seaside — VS Code Handoff

Updated 15 September 2026

## Open the correct workspace

Open `C:\Users\ramin\Desktop\Repos\Seaside` as the VS Code folder. The Unity project is rooted there; do not open `Assets` as a standalone folder.

Use Unity **6000.6.0f1**. Before editing scenes or assets, make sure the Seaside Editor is open and its Pipeline connection is ready.

## Read before changing code

1. [Agent directives](../agents/AGENTS.md) — project architecture and Unity editing rules.
2. [Baseline audit](BaselineAudit.md) — verified state, known gaps, and evidence.
3. [Completion plan](CompletionPlan.md) — ordered work remaining.

## Current handoff state

- The Editor was left on `LV_TestScene`, stopped, with no compilation errors.
- `GameManager` scene-loading and menu-button fixes were completed earlier in this workspace.
- The technical-debt pass replaced deprecated Unity object lookups, removed obsolete/unused fields, and added null/empty-input guards to the small material-switching components.
- `DayNightCycle` now clones its skybox material in Play Mode. The source material was verified at exposure `1.200` before and after a midnight runtime test; the runtime clone reached `0.200` independently.

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
- The menu and direct gameplay scenes retain different manager configurations; unify them before expanding the scene flow.
- Run the boat, movement, interaction, pause, and replay regression route before claiming the existing game shell is complete.

## Suggested first prompt in VS Code

> Read `agents/AGENTS.md`, `Docs/VSCodeHandoff.md`, `Docs/BaselineAudit.md`, and `Docs/CompletionPlan.md`. Confirm the Unity Editor is connected to `C:/Users/ramin/Desktop/Repos/Seaside`, inspect the current git status without reverting anything, then continue the highest-priority unfinished item using the project directives.
