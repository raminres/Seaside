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

### Scene Lifecycle
*   Maintain the persistent gameplay scene structure. The `Main` scene houses the player, persistent managers, and UI.
*   Always load and unload game levels additively using [GameManager.Instance.LoadLevelAdditive()](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/GameManager.cs).
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
