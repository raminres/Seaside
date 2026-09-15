# Interaction audit — whitebox route

Updated 15 September 2026

## First-route contract

The whitebox in `LV_TestScene` is a narrative and traversal map, not final environment art. It marks this route:

1. Boat arrival and disembark.
2. Short movement tutorial from the landing to the settlement/campfire.
3. Optional reflective beats: sit and read a CV-style file.
4. Acquire a way to light the campfire, then light it.
5. Traverse the fire-rain technical-art path to the lighthouse.
6. Enter the lighthouse and end the journey on its upper stairs.

`WhiteboxRoute` contains non-colliding visual markers for these beats. It is deliberately separate from the shipped art and can be removed or repositioned without changing gameplay physics.

## Existing interaction assessment

| Interaction | Current behaviour | Change needed for the route |
|---|---|---|
| Boat arrival | `BoatArrivalController` gates movement, enables disembark, and records `arrival_complete` / `arrival_landing`. | Focused Game-view route-test; confirm platform movement does not double-apply while the player is parented to the boat. |
| Generic focus and prompts | `PlayerInteraction` discovers `IInteractable` components and `InteractableBase` provides prompt, outline, hold, and UnityEvent hooks. | Retain as the presentation layer; add stable progress IDs to authored one-shot route interactions. |
| Collectible | `CollectibleItem` uses local `_isCollected`, destroys itself, and can increment the prototype `GameManager` counter. | Replace/extend with save-owned inventory acquisition. A matches item must grant a stable inventory ID, hide itself after restore, and be idempotent. |
| Campfire | `FireStarter` uses local `_isLit` and static `FireStarter.HasMatches`. | Replace the static flag with journey inventory; persist a `campfire_lit` action; restore fire VFX/light safely; raise an event channel to open the night/path beat. |
| Seating | `SeatInteractable` owns only a runtime occupied state and handles mobile/desktop stand input. | Good optional reflective interaction. Add a one-time `seat_reflection_seen` action only if it gates narration; otherwise keep it non-persistent ambience. |
| Doors / lighthouse entry | `Door` is a local animated state. | Add a stable action or prerequisite only if the lighthouse door is intentionally locked. Otherwise keep it a simple idempotent door and add a separate upper-stairs ending trigger. |
| Readable CV/file | No dedicated readable-document interaction has been identified. | Add a `ReadableDocumentInteractable`: modal/read-only input state, close affordance for touch and desktop, stable `cv_read` discovery action, and a small event payload for UI. |
| Ending | No route-end trigger has been identified. | Add a trigger on the upper stairs that records `lighthouse_reached`, disables repeat ending playback, and raises the completion/event channel. |
| Swimming | `PlayerController` contains a disabled `_enableSwimming` prototype. It switches by global player Y versus `_waterSurfaceY`; `Water_01` has no collider/trigger. | Do not enable for the first route. If promoted later, implement local `WaterVolume` trigger(s), surface-height provider, deterministic entry/exit, a shore/respawn decision, mobile controls, audio/VFX, and iPhone/iPad profiling. |

## Recommended implementation order

1. Route-test boat arrival in an advancing Game view and resolve the parent-plus-external-delta concern if reproduced.
2. Implement the persistent matches/campfire vertical slice, including restored visuals and event channel.
3. Add the readable CV/file and optional seat beat.
4. Add fire-rain path gating and the upper-stairs ending trigger.
5. Decide whether swimming serves the authored route; prototype it only after the land route is complete.
