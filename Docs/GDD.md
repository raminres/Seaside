# Seaside — Game Design Document

Version 0.1 · 15 September 2026 · Working design

## Purpose

Finish a short island exploration game that demonstrates technical art through a coherent playable journey. The player arrives by boat, explores an abandoned coastal settlement, pieces together clues, and reaches and activates a lighthouse. Target platforms are PC and iOS.

The island, lighthouse destination, narrative exploration, shaders, VFX, interactions, and dynamic day/night are the owner's brief. The story, timing, puzzle details, and scope below are proposed defaults, not previously approved creative decisions. See [Baseline Audit](BaselineAudit.md) for what actually exists and [Completion Plan](CompletionPlan.md) for delivery gates.

## Experience pillars

1. **Follow the coast.** Landmarks, light, sound, and sightlines guide the player without a constant objective arrow.
2. **Read the traces.** Small environmental details and short written clues explain what happened to the settlement.
3. **Make the world respond.** Lighting a fire and restoring the beacon produce clear audiovisual changes.
4. **Show the craft.** Water, atmosphere, and the beacon each get a deliberate moment where their technical work is visible in normal play.

## Scope

- One continuous journey, approximately 20–30 minutes on a first playthrough.
- Retain the existing third-person controller for the first complete version. The narrative inspiration does not require rebuilding the camera as first-person.
- Four compact connected areas: arrival dock, fishing settlement, coastal ascent, lighthouse.
- Three essential clues, two simple environmental interactions, one definitive ending.
- Optional scenic stops and a small number of optional notes; no collectible quota for completion.
- No combat, crafting tree, procedural island, multiplayer, branching endings, or survival simulation in the first release.
- Weather changes beyond the existing atmosphere are a later polish option; the complete route comes first.

## Narrative proposal: the last watch

The player returns to a recently abandoned island to finish the lighthouse keeper's final watch. The settlement was evacuated before a severe storm. Evidence initially suggests the keeper abandoned the beacon; later clues reveal that the keeper left to help the last residents reach the evacuation boat. Restoring the light becomes an act of remembrance rather than a rescue deadline.

Use a few concrete objects and short texts rather than exposition-heavy dialogue. Final character names, relationship to the keeper, and ending text remain open. Voice-over is optional; every essential fact must work through readable text and environmental staging.

## Journey and pacing

| Area | Approx. time | Player action | Information / payoff | Technical art focus |
|---|---:|---|---|---|
| Arrival dock | 2–3 min | Look around on the approaching boat; disembark | Establish destination through the lighthouse silhouette | Water displacement, foam, boat movement, dusk reflections |
| Fishing settlement | 6–8 min | Explore a few accessible buildings; read evacuation notice; find matches | Residents left together; a sheltered work area points toward the keeper's route | Material variation, wind, ambient particles, warm/cool contrast |
| Coastal ascent | 6–8 min | Light a sheltered fire; read keeper's log; follow the service path | Explain the keeper's choice; reveal how to restore the beacon | Fire VFX, embers, responsive light, changing atmosphere |
| Lighthouse | 5–7 min | Restore the power mechanism; align/activate the lamp; read final message | Light sweeps across the places already visited; ending and credits | Beacon beam, glass, bloom, timed audio and light transition |

The route must remain navigable at night. Advance the day/night system gently between authored mood targets; do not force players to wait for a particular hour or punish slow exploration. Retain a separate full-cycle demonstration for the portfolio.

## Core loop

Observe a landmark → explore a small area → inspect a clue → perform a readable interaction → see the world change → discover the next landmark.

Required progression is based on stable clue/action IDs, not random pickups. Notes may be found out of order and can be reread. Revisiting an interaction cannot duplicate a reward or replay the ending.

### Interaction design

- Keep existing Instant, Toggle, and Hold interaction modes.
- Notes open a readable panel and record their ID in a journal. Closing returns the player to their previous control state.
- Matches are a reusable inventory flag. A fire that requires them explains why it cannot be lit.
- The lighthouse uses two visible stages: restore power, then activate the lamp. A short nearby clue explains both. No obscure numeric code or precision platforming gate.
- Keep the fire optional to route completion unless testing shows its clue can be made reliably discoverable. Essential lighthouse instructions remain available independently.
- Collectible shells are optional atmosphere. The existing count-based win condition does not define the narrative ending.

## Controls and presentation

PC: movement, camera look, interact, pause, optional sprint. Retain jumping/swimming where the existing route needs them; do not introduce jumps as essential puzzles. Verify the actual Input Actions before documenting final bindings.

iOS: left movement control, right look region, contextual interact, pause, optional sprint toggle. Prevent UI touches from moving the camera. Support safe areas and landscape layouts. Provide a tap alternative to long holds, adjustable look sensitivity, readable text size, and reduced camera motion.

Main menu: New Journey, Continue when a checkpoint exists, Options, Credits. Keep a development-only scene selector if useful. Pause: Resume, Options, Restart Checkpoint, Main Menu. Ending: credits with replay/menu access. The release UI should not expose development scene names.

## State and scene architecture

Follow `agents/agents.md`: ScriptableObject event channels connect independent systems; interactables derive from the existing interface/base class; DayNightCycle owns ambient light, fog, and sun/moon settings.

Proposed scene structure: a persistent `Main` scene for player, camera, services, and UI, with island content loaded additively through GameManager. This structure is documented in the older notes but does not yet exist in the repository. Implement it only after the current menu-to-test-scene baseline works, preserving LV_TestScene as a mechanics sandbox.

Separate narrative progress from GameManager's prototype collectible counter. Store discovered note IDs, inventory flags, completed action IDs, checkpoint ID, and time-of-day in a versioned local save. Restore progress before enabling interactions. A new journey explicitly clears runtime progression, including static item flags.

Checkpoint locations: after disembarking, after the settlement, at the lighthouse entrance, and after the finale. Save on milestone completion and application pause; never depend solely on application quit. Recover safely from an absent or unreadable save.

## Technical art deliverables

| Feature | Reuse | Finish criteria |
|---|---|---|
| Ocean | Custom water Shader Graph, Gerstner HLSL, WaterController / presets | Correct shore/depth behavior, stable normals, deliberate waves, no obvious shoreline gaps; PC and iOS captures |
| Day/night | DayNightCycle and DayNightPreset | Readable dusk/night route, consistent fog/sky/light, predictable milestone events, no unwanted asset changes after play mode |
| Interactive fire | FireStarter and available fire assets | Reliable matches gating, one-shot ignition, coordinated light/audio/embers; identify any sample-derived work in credits |
| Coastal atmosphere | Wind, leaves, butterfly, dust graphs | Intentional placement and bounded particle counts; avoid filling every view with effects |
| Lighthouse | New beacon presentation | Beam, lamp/glass response, audio and ending synchronized; reduced-cost mobile version preserves the same visual cue |

Create portfolio breakdowns showing inputs, Shader Graph/HLSL structure, effect layers, and measured cost. Distinguish authored work from imported models and Unity samples.

## Performance targets and testing

Provisional targets: 60 FPS at 1080p on a named PC test configuration, and sustained 30 FPS on a named baseline iPhone, with 60 FPS optional on suitable devices. These are targets, not measurements. Select the baseline devices before final quality tuning.

Budget around 16.7 ms per PC frame and 33.3 ms per baseline mobile frame. Record CPU/GPU time, memory, transparent overdraw, and thermal behavior along the same route. Scale shadows, render scale, water detail and VFX density using quality profiles. Verify effects on Metal hardware; package presence alone does not prove device support or acceptable performance.

An iOS release requires a Mac/Xcode signing and device-validation stage. The current Windows audit does not establish an iOS build result.

## Definition of complete

1. A new player can go from menu through arrival, clues and lighthouse ending without Editor intervention.
2. Required interactions cannot soft-lock the journey, including when notes are found out of order.
3. Pause, restart, return-to-menu and a second journey work without stale state or duplicate managers.
4. Checkpoint continuation survives application restart and mobile background/resume.
5. PC and a named iPhone meet the agreed sustained performance target on a complete route.
6. No unhandled exceptions, missing scripts, or broken materials during that route.
7. Build instructions, controls, asset credits and a short technical art breakdown accompany the deliverable.

## Creative decisions to revisit after the first playable route

Confirm the protagonist's relationship to the keeper, final narrative tone, whether to retain third-person for release, baseline iPhone/PC hardware, and whether narration will be recorded. None prevents the audit or a reversible route prototype.
