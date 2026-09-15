# 🤖 Seaside Agent Directives & Developer Guidelines

This file serves as a system instruction and developer guide for any AI coding agents (or developers) modifying this codebase. All new files, features, scripts, and shaders created for the **Seaside** project must adhere to these directives to maintain architectural integrity, performance, and clean decoupling.

---

## 🏛️ 1. Core Architecture Directives

### Decoupled Event-Driven Patterns
*   **Do not directly couple systems:** Avoid having systems call each other directly. Use the ScriptableObject event architecture.
*   **Create Event Channels:** Broadcast state changes or actions using [GameEventSo](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Events/GameEventSo.cs) or its typed equivalents ([FloatEventSo](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Events/FloatEventSo.cs), `IntEventSo`, `StringEventSo`).
*   **Subscription Pattern:** Have listening components subscribe to these ScriptableObject events via C# `event System.Action` delegates, ensuring proper registration/unregistration in Unity lifecycle methods (`OnEnable`/`OnDisable` or `Start`/`OnDestroy`).

### Persistent Singleton Pattern
*   For global manager singletons that coordinate runtime services, inherit from `Seaside.Core.Singleton<T>` (e.g., [AudioManager](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Core/AudioManager.cs)). Ensure `DontDestroyOnLoad` behavior is handled safely in `Awake()`.
*   Do not introduce another persistent manager to solve scene-entry issues. Reconcile the serialized `GameManager` settings already present in `LV_MainMenu` and `LV_TestScene`, then verify both entry paths before adding services.

### Scene Lifecycle
*   The current release entry is `LV_MainMenu`; `LV_TestScene` is the current gameplay bootstrap, housing the player, persistent managers, UI, and island foundation. Do not create or rename a `Main` scene merely to follow older notes.
*   Keep the menu and direct-`LV_TestScene` entry paths behaviorally equivalent. Before expanding scene flow, reconcile their `GameManager` configuration and verify the route in a normally advancing Game view.
*   Additive loading through [GameManager.Instance.LoadLevelAdditive()](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/GameManager.cs) is available for a later content-scene split; it is not a requirement for the first complete route.
*   Use the `onLoadProgress` float event to drive loading screens or progress bars.

---

## 🏃 2. Player Locomotion & State Machine

### Player State Machine
*   All player logic changes must integrate with the `PlayerState` enum in [PlayerController](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Player/PlayerController.cs) (e.g., `Idle`, `Walking`, `Running`, `Jumping`, `Falling`, `Swimming`, `Interacting`, `OnBoat`).
*   Implement state transitions in `UpdateState()` and keep the enter/exit logic cleanly partitioned inside `SetState(PlayerState newState)`.
*   Handle visual and audio cues through sub-components like `PlayerAnimation` and `PlayerAudioAndVfx`.

### Platform Kinematics & Traversal
*   When creating moving platforms, vehicles, or boats, do not parent the player statically if it interferes with character physics.
*   Use the kinematic delta application pattern: calculate the platform's exact transform delta (including yaw rotation) and apply it to the player via [PlayerController.ApplyExternalMovement(exactDelta, deltaYaw)](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Player/PlayerController.cs#L750-L764). See [BoatArrivalController](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Objects/BoatArrivalController.cs) for reference.

---

## 🤝 3. Interaction Subsystem

### Implementing New Interactables
*   All interactive elements in the game world must implement the `IInteractable` interface or inherit from [InteractableBase](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Interactables/InteractableBase.cs).
*   Implement custom triggers and payload logic by overriding the protected method `OnInteractInternal(PlayerController player)`.

### Interaction Rules
*   **Prompt Configuration:** Assign clear `_interactionPrompt` text and configure the correct `InteractionType` (`Instant`, `Hold`, `Toggle`).
*   **Focus Outline Feedback:** Utilize the built-in highlight triggers. When `OnFocused()` is called, assign `_outlineWidth` to the designated `_outlineRenderer`'s material property (default: `_OutlineWidth`). Reset it to `0` in `OnUnfocused()`.
*   **Hold Interactions:** For `InteractionType.Hold`, specify the target `HoldDuration`. The player's [PlayerInteraction](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Interactables/PlayerInteraction.cs) script will automatically handle hold-state timers and animate UI indicators.

---

## 🎮 4. Input Configuration & Mobile Support

*   The project uses the modern Unity **Input System**.
*   **Cross-Platform UI Adaptability:** Always query [MobileControlsManager.IsMobileControlsEnabled](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/UI/MobileControlsManager.cs) before enabling standard desktop inputs.
*   **Action Toggling:** When switching to a touch/mobile overlay layout, programmatically disable standard look/move input actions to prevent conflicting cursor delta capture (e.g., `lookAction.Disable()`). Gather input values directly from virtual joysticks through [MobileInputHandler](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/UI/MobileInputHandler.cs).

---

## 🌅 5. Technical Art & Shading Guidelines

### Environment & Day-Night cycle
*   Never modify ambient settings, fog parameters, or directional light settings dynamically inside separate custom scripts. Let [DayNightCycle](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Environment/DayNightCycle.cs) control them.
*   To create custom time profiles, instantiate a new [DayNightPreset](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Environment/DayNightPreset.cs) ScriptableObject asset and define appropriate color gradients and animation curves.
*   For scripts that need to respond to specific time phases, subscribe to the cycle events (`_onSunrise`, `_onSunset`, etc.) rather than polling the time float in `Update()`.

### Custom Shaders & Water Simulation
*   **Shading Language:** Write custom vertex or pixel math functions in `.hlsl` files (e.g., [GerstnerWaves.hlsl](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Shaders/Water/GerstnerWaves.hlsl)) and integrate them into URP Shader Graphs using Custom Function nodes.
*   **Property ID Caching:** When scripting materials (such as [WaterController](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Water/WaterController.cs)), always cache property names using `Shader.PropertyToID` in static read-only variables.
*   **Dynamic Material Instances:** Avoid calling `renderer.material` in update loops as it instantiates material duplicates. Use `MaterialPropertyBlock` or cache/modify shared materials appropriately.
*   **Water Presets:** When adding water styles, extend [WaterPreset](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Water/WaterPreset.cs) and configure default initialization presets inside [WaterPresetFactory](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Water/WaterPresetFactory.cs).

---

## 📝 6. Code Style & Quality Standards

1.  **Field Visibility:** Use `[SerializeField] private` for editor-exposed fields. Avoid using `public` fields unless they are properties with restricted setters.
2.  **Tooltips & Headers:** Document variables using `[Header("Name")]` and `[Tooltip("Info")]` for clean Inspector layouts.
3.  **Namespace Usage:** Organize core utilities under appropriate namespaces (e.g., `Seaside.Core`).
4.  **Performance Check:** Avoid `Find` or `GetComponent` inside `Update()` calls. Pre-cache reference components in `Awake()`, `Start()`, or `OnEnable()`.
5.  **Editor Operations:** Wrap editor-only APIs (like `AssetDatabase` or `EditorUtility`) inside `#if UNITY_EDITOR` blocks to avoid build failures.
6.  **Clean Log Outputs:** Prefix diagnostic messages with class names (e.g., `Debug.Log("[GameManager] Transitioning to Playing State")`).

---

## 🧭 7. Session Continuity Protocol (Required)

The repository, not a chat transcript, is the source of truth between agent or developer sessions. Treat [Docs/VSCodeHandoff.md](../Docs/VSCodeHandoff.md) as the canonical, durable handoff record.

### On every new session

1. Read this file, then `Docs/VSCodeHandoff.md`, `Docs/BaselineAudit.md`, `Docs/CompletionPlan.md`, and `Docs/GDD.md` before making changes.
2. Run `git status --short --branch`; never revert, reset, stage, or overwrite existing work unless the user explicitly requests it.
3. Confirm the connected Unity Editor and its compilation state using the commands documented in `Docs/VSCodeHandoff.md`.
4. Resume only the highest-priority unfinished item recorded in the handoff, unless the user gives a newer instruction.

### Narrative progression safeguards

* Keep route progress as stable, instance-owned and save-owned IDs (for example, notes discovered and actions completed). Never use static fields such as `FireStarter.HasMatches`, or the prototype collectible count, as narrative truth.
* Raise progress changes through ScriptableObject event channels. Interactions must be idempotent: restoring a save, revisiting an object, or receiving a repeated event must not duplicate rewards or replay the ending.
* Treat a clean compilation as insufficient. A gameplay change is complete only after its applicable live Unity route is verified in an advancing Game view and any remaining limitation is recorded in the handoff.

### Project decision register (maintain as decisions are made)

* Record every durable product or technical decision twice: keep the concise constraint here, and add dated context, evidence, and the current next action to `Docs/VSCodeHandoff.md`. Do not turn either file into a verbatim chat transcript.
* The first durable gameplay-systems milestone is a versioned local journey save with stable IDs for checkpoints, inventory, discovered notes, and completed one-shot actions. `arrival_complete` is one such completed action; New Journey clears it and a continuation skips the boat approach.
* The shared `GameManager` runtime contract for both entry scenes is: `persistentGameplayScene = LV_TestScene`, single-scene loading (`useAdditiveLoading = false`), `GameEvent.asset` for state and volume changes, `OnGamePaused.asset` / `OnGameResumed.asset` for pause transitions, and `OnLoadProgress.asset` for load progress. Keep menu-only UI references and prototype collectible settings scoped to their scene until that prototype is retired.
* `JourneyProgressService` is attached to the existing `GameManager` in both `LV_MainMenu` and `LV_TestScene`, with `OnJourneyProgressChanged.asset` as its channel. Do not turn it into a singleton or create a second persistent root.
* `LV_MainMenu` contains `ButtonContinue`, a clone of `ButtonStartGame` in the same vertical layout group. It appears only when `JourneyProgressService.HasCheckpoint` is true; New Journey clears progress and Continue loads level index `0`. Preserve that visual/template relationship when changing menu styling.
* The provisional arrival test wiring is concrete: `BoatArrivalController` lives on `PF_Boat_Parent`, `BoatInteractable` lives on its existing `BoatFloor` collider, and they reference the authored `BoatArrivalPoints` markers plus the existing `GameManager` progress service. A successful disembark records `arrival_complete` and `arrival_landing`.
* The arrival route remains provisional until its scene references and transforms are authored and a fresh journey plus a restored journey are exercised in an advancing Game view. Do not declare it shipped from unit tests or compilation alone.
* The prototype collectible counter is not an ending or narrative-progress system. Replace or isolate it before it can affect the authored route.
* `LV_TestScene` has an archived pre-whitebox reference at `Assets/Scenes/Archive/LV_TestScene_PreWhitebox.unity`. The active scene's `WhiteboxRoute` markers are non-colliding planning aids for arrival, reflection, campfire, fire-rain, and lighthouse beats; retain this separation until final level art replaces a beat.
* The first complete route is land-based: arrival, movement tutorial, optional sit/read reflection, persistent matches and campfire, fire-rain path, then the lighthouse upper-stairs ending. Swimming is currently disabled prototype code using a global Y-plane and is explicitly out of scope until a volume-based, mobile-tested design is approved.
* Every authored one-shot interaction needs a stable save-owned ID and restore behaviour. `FireStarter.HasMatches`, local `_isCollected`/`_isLit` fields, and the prototype collectible counter must not become narrative truth.
* Test on the owner's iPhone 15 and iPad mini (6th generation). The owner builds on a MacBook Air M4 and currently reports stable 60 FPS; treat that as a reported baseline, not a profiling result. Capture representative on-device evidence before final performance claims or quality cuts.
* iOS builds target both iPhone and iPad, with a minimum iOS version of 26.0. Keep device-family support and this deployment target aligned with the owner's iPhone 15 and iPad mini (6th generation) validation.

### Before ending a meaningful work session

Update `Docs/VSCodeHandoff.md` in the same change set. Record concise, verifiable facts:

* active branch and whether it tracks a remote;
* files/features changed, including key configuration choices;
* exact verification commands and their result;
* known errors, warnings, blockers, and work intentionally not performed;
* the single recommended next action.

Do not rely on memory, summaries, or assumptions. If any fact is unknown, mark it as unverified rather than guessing. Keep the handoff current even when work is uncommitted; Git history complements the handoff but does not replace it.
