# Seaside — Completion Plan

15 September 2026 · Ordered by dependencies, not calendar estimates

## Milestone 0 — Reproducible baseline

- [x] Recover previous request and interruption point.
- [x] Read existing project directives and historical notes.
- [x] Inventory source, scenes, packages and visual assets.
- [x] Write a working GDD and evidence-based audit.
- [x] Restore Seaside's live Pipeline connection and verify the project path.
- [x] Capture menu/gameplay baseline and current console output.

Exit: a recorded, repeatable menu → gameplay → menu test in the correct Editor.

## Milestone 1 — Stable existing game shell

- [x] Resolve duplicate level-button callbacks and transition re-entry.
- [ ] Unify persistent manager settings and initialize direct gameplay startup.
- [x] Repair and test additive loading with two distinct scenes (actual menu and gameplay scenes used as fixtures).
- [x] Diagnose the actual menu visibility/animation issue in play mode.
- [ ] Verify boat arrival/disembark, screen fade, movement, interactions, pause and replay.
- [ ] Wire item acquisition/reset and fire requirements through event channels.

Exit: two consecutive sessions through all existing mechanics, without exceptions, duplicate managers, stuck fades or stale inventory. Use targeted automated tests for state/loader behavior plus live interaction checks.

## Milestone 2 — Complete greybox journey

- [ ] Extend `LV_TestScene` into the complete journey, retaining it as the gameplay bootstrap and mechanics sandbox. Revisit an additive content-scene split only when the route proves it necessary.
- [ ] Block out dock → settlement → coast → lighthouse with clear landmarks and collision.
- [ ] Implement stable narrative IDs, note reading/journal and explicit finale progression.
- [ ] Stage three essential clues and the two lighthouse actions.
- [ ] Produce a complete ending with return-to-menu/replay.

Exit: a first-time tester completes the route without developer guidance. The game is finishable before expanding visual polish.

## Milestone 3 — Persistence and cross-platform interaction

- [ ] Add versioned checkpoint saves, Continue and New Journey reset.
- [ ] Restore collected items, narrative actions and time before accepting input.
- [ ] Handle missing/corrupt save and iOS pause/resume.
- [ ] Validate touch safe areas, look-region exclusion, readable notes and tap/hold options.

Exit: quit/relaunch at each checkpoint and reach the same ending on PC and iOS input layouts.

## Milestone 4 — Three polished technical art moments

- [ ] Arrival water: shore/depth/foam, boat motion and dusk composition.
- [ ] Coast: fire ignition, wind/ambient VFX and legible evening lighting.
- [ ] Finale: beacon beam, glass/light response and synchronized sound.
- [ ] Verify day/night boundaries and material lifecycle.
- [ ] Attribute imported assets and sample-derived effects.

Exit: each moment works during the journey and has a clear portfolio breakdown.

## Milestone 5 — Builds, performance and handoff

- [ ] Name reference PC and baseline iPhone; record build settings and hardware.
- [ ] Profile the same full route; tune PC/mobile quality profiles against frame budgets.
- [ ] Test Windows standalone and signed iOS build on device, including sustained play/backgrounding.
- [ ] Complete a regression route from fresh save and existing checkpoints.
- [ ] Update README, controls, credits and known limitations.
- [ ] Capture a short gameplay reel and technical breakdowns with measured costs.

Exit: meet the GDD definition of complete. Editor compilation alone does not close this milestone.

## Resume checkpoint

Connection restored after the owner opened Seaside. GameManager startup/loading fixes and the level-button callback correction are saved and tested. A technical-debt pass also removed obsolete object lookups, shared-material mutation in interaction effects, and runtime skybox asset mutation. The Editor was left stopped on `LV_TestScene`. See [validation results](Validation/README.md) and the [VS Code handoff](VSCodeHandoff.md).

Next action: run the menu-to-`LV_TestScene` route in a normally advancing Game view, then compare it with a direct `LV_TestScene` start before changing serialized manager settings. Build the narrative route on the existing settlement and lighthouse. Read [BaselineAudit.md](BaselineAudit.md), [VSCodeHandoff.md](VSCodeHandoff.md), and [GDD.md](GDD.md) before changing settings.
