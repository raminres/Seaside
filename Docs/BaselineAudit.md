# Seaside — Baseline Audit

15 September 2026

## Evidence and limits

This audit reads the current repository, saved scene configuration, prior task history, and live Seaside play-mode results. Initially the reachable Editor was `unity-cli-mcp-test`. The CLI misleadingly reported Seaside as running; the owner clarified it was closed and opened it. Explicit project-targeted commands then verified Seaside 6000.6.0f1 as ready. See [Validation](Validation/README.md) for current results.

Changes in this pass: GameManager startup/loading corrections and removal of the redundant level-selection callback through the Editor. Unity also refreshed scene serialization on save. Existing modifications at the start included two TMP font assets, the package manifest/lock, build settings, ProBuilder settings, and Project Auditor settings; these were not reverted. Play-mode skybox exposure drift was restored to its pre-test value of 1.2, then fixed by giving DayNightCycle an isolated runtime skybox copy. The source material was rechecked after Play Mode and remained at 1.2.

## Current status

- Fixed and tested: duplicate level callback, overlapping scene requests, additive activation queue, explicit initial gameplay state.
- Menu visibility diagnosis closed: the initial capture was taken after only five simulation frames; focusing Game view and allowing background simulation completed the entrance animation. No visual redesign or animation rewrite was needed.
- Direct gameplay startup, visible pause/resume, menu Options/Back and the automated scene-flow checks passed. Console error count was zero; warnings remain.
- Scene screenshots confirm the settlement, docks and lighthouse already exist. Reuse their layout when developing the narrative route; a missing dedicated ending script does not mean missing lighthouse art.
- Remaining findings below describe the baseline and follow-up work; resolved findings are explicitly marked.

## Current inventory

| Area | Repository evidence | Assessment |
|---|---|---|
| Engine | ProjectVersion.txt: 6000.6.0f1 | README's 6000.5 setup instructions are stale |
| Rendering | URP / Shader Graph / VFX Graph 17.6.0 in manifest | Configured packages; current rendering requires live verification |
| Scenes | LV_MainMenu and LV_TestScene enabled in build settings | No authored Main / LV_Level1–3 scenes despite older architecture notes |
| Player | PlayerController, PlayerAnimation, PlayerAudioAndVfx | Movement, interaction and boat integration code exists |
| Arrival | BoatArrivalController, BoatInteractable, ScreenFade | Existing foundation; need full arrival/disembark regression |
| Water | GerstnerWaves.hlsl, SH_Water_Customizable, WaterController, WaterPreset | Custom system exists; visual/performance audit outstanding |
| Atmosphere | DayNightCycle / preset; wind, caustics, godray graphs | Existing components; verify actual scene wiring and night readability |
| VFX | Dust, leaves, butterfly; imported fire/smoke/sparks samples | Do not describe all fire/weather effects as original completed work |
| Story | Collectible IDs/events; no dedicated journal, narrative progression or checkpoint system found in Assets/Scripts | Essential narrative loop remains to build |
| Audio | Sea, nature, fire, engine, UI, doors and footsteps | Useful library; no narration found in audio inventory |
| Finale | No dedicated lighthouse progression/ending implementation found in Assets/Scripts | Needs design, scene staging and implementation |

## Findings, in priority order

### Resolved — Restore a connection explicitly bound to Seaside

Use the project path on every CLI command and verify it before edits. The owner opened Seaside and the project-targeted connection succeeded. No forced termination or discard of unsaved changes was needed.

### Fixed — Level selection contained two load callbacks

`Assets/Scenes/LV_MainMenu.unity` serialized both `GameManager.LoadLevel(0)` and `LevelSelectButton.SelectThisLevel()`. The latter calls SelectLevel → LoadLevel again. Removed the redundant direct callback through the live Editor and added an in-flight transition guard in GameManager. The validation harness confirmed two same-frame clicks produce one completed scene load.

### P1 — Scene manager configuration differs by entry point

The menu's GameManager has `persistentGameplayScene = LV_TestScene`, `useAdditiveLoading = false`, a 20-item collectible prefab, and shared state/pause/resume/volume event references. The test scene's manager has an empty persistent scene, additive loading enabled, no collectible prefab, a 10-item target, and distinct pause/resume channels.

Because the first manager persists and destroys duplicates, entering through the menu can behave differently from starting LV_TestScene directly. It also skips the test scene's collectible/UI initialization when that scene matches the surviving manager's persistentGameplayScene. Establish one bootstrap configuration and test both entry paths before migrating scenes.

### Fixed — Additive loading could stall when multiple operations were queued

Replaced the all-scenes-at-0.9 wait with sequential load/activation. Requests are deduplicated and validated before loading; concurrent transitions are rejected. The real two-scene validation completed successfully. The menu normally bypasses this path (`useAdditiveLoading = false`); it was not the cause of the previous menu visibility issue.

### Fixed — Direct gameplay startup did not establish Playing state

GameManager now initializes the first scene explicitly in Start and sets MainMenu or Playing from that scene, rather than inheriting the ScriptableObject's previous state. Direct test-scene startup and pause/resume checks passed. Full movement/input traversal remains a separate regression task.

### P1 — Matches gating has no implemented acquisition path in scripts

FireStarter uses static `HasMatches`; no assignment/acquisition path was found elsewhere in Assets/Scripts. `_requiredItemId` is serialized but not used by its interaction check. Connect item-collected events to an inventory state and clear that state for a new journey. Include the base interactable permission in CanInteract. Verify serialized event wiring live before concluding every placed fire is blocked.

### P1 — Prototype win condition conflicts with the intended ending

GameManager.CollectItem wins when its collectible count is reached. That does not represent discovery or lighthouse activation. Introduce a separate explicit narrative completion event and keep shells optional.

### Diagnosed — Menu visibility depended on simulation advancing

The previous task stopped while inspecting menu animation/rendering. Runtime inspection found only five frames had elapsed in the apparently blank menu. Focusing Game view and temporarily allowing background simulation completed the default Appear animation. MainMenuController still assumes several canvas/Animator references exist and lacks navigation-transition guards; those are follow-up robustness concerns.

### Fixed — Day/night no longer writes into its source skybox material

DayNightCycle now guards invalid day lengths and creates a runtime skybox clone before applying animated exposure. A live midnight check changed the clone to 0.2 while the source asset remained 1.2 after returning from Play Mode. Noon/midnight events still use narrow time windows and can be skipped by large time jumps.

### P2 — Day/night event timing still needs robustness

Exercise time jumps, zero/short durations, pause and repeated play sessions before using these events for critical progression.

### P2 — Documentation overstates completion

README lists weather, readable notes, terrain shading and a Main/additive scene structure more confidently than repository evidence supports. Use the GDD for intended scope and this audit for implementation status until those systems are built and verified.

## Required runtime checks once reconnected

- Confirm correct project, Unity version, clean compile and current dirty scene state.
- Capture menu after startup; navigate options/credits/back, level selection and gameplay.
- Run direct test-scene start and menu start; compare manager, state, event channels and collectible initialization.
- Board/arrive/disembark; walk/sprint/jump/swim; test doors, seats, pickups and fire.
- Pause/resume, restart, menu return, second journey; inspect console after each.
- Check water from shore and boat, full time-of-day range, VFX placement and build quality settings.
- Record PC build and iOS device results separately from Editor results.
