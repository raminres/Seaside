# Seaside - Unity Technical Art Showcase

A seaside exploration game built in Unity, designed to showcase Technical Artist skills through custom shaders, VFX, and environmental systems. The focus is on clean, readable, and decoupled code that supports stunning visual work.

---

## 🏗️ Project Overview

You arrive by boat at an abandoned seaside village at dusk. A distant lighthouse—the only light on the horizon—beckons you. Your journey across the village and bay to reach and activate the lighthouse reveals traces of the people who once lived here.

---

## 🌟 Features

### Player System
*   **Third-Person Controller** — State-based movement machine (Idle, Walking, Running, Jumping, Swimming, Interacting, OnBoat).
*   **Starting Boat Cutscene** — Begins exploration on a moving boat (`PF_Boat_Parent.prefab`) where the player can walk on the deck. Upon arrival, triggers a transition and disembarks the player onto the dock.
*   **New Input System** — Full cross-platform support for Keyboard/Mouse, Gamepad, and Touch layouts.
*   **Interaction System** — Interface-based (`IInteractable`) with UnityEvents supporting Instant, Toggle, and Hold workflows.
*   **Cinemachine Camera** — Smooth third-person camera tracking.

### Interactive Elements
*   **Doors** — Toggle open/close with automatic closing on exit.
*   **Sitting** — Context-aware sitting mechanics with prompt feedback.
*   **Fire Pits** — Hold to ignite (requires matches).
*   **Collectibles** — Seashells and readable notes scattered across the island.
*   **Boat** — Board, disembark, and undock mechanics with platform motion tracking.

### Technical Art Showcases

| Feature | Techniques | Tools |
|---------|------------|-------|
| **Ocean System** | Gerstner waves (4 primary, 2 secondary), depth-based color fade, shore foam, crest foam, caustics, fresnel reflections, refraction offset | Shader Graph, HLSL |
| **Day/Night Cycle** | Animated sun/moon orbits, skybox exposure curves, light probes, dynamic ambient and fog coloring | C#, URP, ScriptableObjects |
| **Interactive Fire** | Flames, embers, smoke, heat distortion, flickering light | VFX Graph |
| **Weather Effects** | Rain particles, wet surfaces, puddles, fog, global wind | VFX Graph, Shaders |
| **Terrain Materials** | Triplanar mapping, height blending, slope detection, shoreline wetness | Shader Graph |
| **Post-Processing** | Time-of-day LUTs, bloom, depth of field, vignette | URP Volume |

### Architecture & Scripting
*   **Singleton Managers** — `GameManager`, `AudioManager`, `MobileControlsManager`, and `ScreenFade` orchestrators.
*   **ScriptableObject Event Channels** — Decoupled communication channels (`GameEventSo`, `FloatEventSo`, `IntEventSo`, etc.) avoiding tight system coupling.
*   **Water Preset System** — Swappable scriptable presets (Ocean, River, Lake, Pond) managing wave parameters and coloring.

### UI Systems
*   **Main Menu** — Level Select, Options, and Quit workflows.
*   **Screen Fade** — Reusable overlay shader panel managing start-up fade-ins and transition fades.
*   **Loading Screen** — Additive scene progress tracker displaying level-loading tips.
*   **Options** — Audio sliders, graphics presets, and frame rate settings (30 vs 60 FPS toggles).
*   **Mobile Controls Canvas** — Adaptive On-Screen Joysticks and Touch Zone Look panels that activate on mobile platforms.

---

## ⚙️ Installation & Setup

### Requirements
*   **Unity** 6000.5.0f1 (Unity 6) or higher.

### Required Packages
1.  Input System
2.  TextMesh Pro
3.  Universal Render Pipeline (URP)
4.  Shader Graph
5.  Visual Effect Graph
6.  Addressables
7.  AI Navigation (NavMesh)

### Steps to Run
1.  Clone the repository:
    ```bash
    git clone https://github.com/raminres/Seaside.git
    ```
2.  Open the project in Unity 6000.5.0f1.
3.  Open **Assets > Scenes > LV_MainMenu.unity** (or **LV_TestScene.unity** to test sandbox mechanics directly).
4.  Hit Play to explore.

---

## 🎮 Controls

| Action | Keyboard/Mouse | Gamepad | Touch |
|--------|---------------|---------|-------|
| Move | WASD / Arrow Keys | Left Stick | Virtual Joystick |
| Camera | Mouse | Right Stick | Drag on Screen Touch Zone |
| Interact / Sit | E | A | Interact Button |
| Sprint | Left Shift | Left Trigger / Button | Sprint Toggle Button |
| Jump | Space | South Button | Jump Button |

---

## 📂 Folder Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Managers, singletons, utilities
│   ├── Player/         # Controller, animation, states
│   ├── Interactables/  # Door, Collectible, FireStarter, BoatInteractable
│   ├── Environment/    # WeatherController, DayNightCycle, DayNightPreset
│   ├── UI/             # Menus, HUD, prompts, ScreenFade
│   └── Water/          # WaterController, WaterPreset, WaterPresetFactory
├── Shaders/            # Shader Graphs + GerstnerWaves HLSL includes
├── VFX/                # VFX Graph assets
├── Data/               # ScriptableObjects (Events, GameState)
├── Settings/           # URP render pipeline settings and Input Actions
└── Art/                # Models, textures, audio clips, and animations
```

---

## 📄 License

This project is open-source under the MIT License. Feel free to use, modify, and distribute it as needed.
