# Seaside - Unity Technical Art Showcase

A seaside exploration game built in Unity, designed to showcase Technical Artist skills through custom shaders, VFX, and environmental systems. The focus is on clean, readable code that supports stunning visual work.

## Project Overview

You arrive by boat at an abandoned seaside village at dusk. A distant lighthouse—the only light on the horizon—beckons you. Your journey across the village and bay to reach and activate the lighthouse reveals traces of the people who once lived here.

## Features

### Player System
- **Third-Person Controller** — State-based movement (Idle, Walking, Running, Jumping, Swimming, Interacting)
- **New Input System** — Full support for Keyboard/Mouse, Gamepad, and Touch controls
- **Interaction System** — Interface-based (`IInteractable`) with UnityEvents for Inspector-driven workflows
- **Cinemachine Camera** — Smooth third-person camera follow

### Interactive Elements
- **Doors** — Toggle open/close with automatic closing on exit
- **Sitting** — Context-aware sitting with UI feedback
- **Fire Pits** — Hold to ignite (requires matches)
- **Collectibles** — Seashells and readable notes
- **Boat** — Board, disembark, and undock mechanics
- **Lighthouse Mechanism** — Final objective interaction

### Technical Art Showcases

| Feature | Techniques | Tools |
|---------|------------|-------|
| **Ocean System** | Gerstner waves, depth-based color, shore foam, caustics, fresnel reflections, refraction | Shader Graph |
| **Day/Night Cycle** | Animated sun/moon, sky gradient, window lights, light probes | C#, URP |
| **Interactive Fire** | Flames, embers, smoke, heat distortion, flickering light | VFX Graph |
| **Weather Effects** | Rain particles, wet surfaces, puddles, fog, global wind | VFX Graph, Shaders |
| **Terrain Materials** | Triplanar mapping, height blending, slope detection, shoreline wetness | Shader Graph |
| **Post-Processing** | Time-of-day LUTs, bloom, depth of field, vignette | URP Volume |

### Architecture
- **Singleton Managers** — GameManager, AudioManager, InputManager, UIManager
- **ScriptableObject Event Channels** — Decoupled communication without complex event buses

### UI Systems
- **Main Menu** — New Game, Continue, Level Select, Options, Quit
- **Pause Menu** — Resume, Map, Options, Main Menu
- **Options** — Audio sliders, graphics presets, control sensitivity
- **HUD** — Interaction prompts, objectives, collectible counter
- **Mobile Controls** — On-Screen Stick and Button components

## Installation & Setup

### Requirements
- **Unity** 6.3 or higher

### Required Packages
1. Input System
2. TextMesh Pro
3. Universal Render Pipeline (URP)

### Steps to Run
1. Clone the repository:
   ```
   git clone https://github.com/raminres/Seaside.git
   ```
2. Open the project in Unity 6.3
3. Open **Assets > Scenes > Seaside**
4. Hit Play to explore

## Controls

| Action | Keyboard/Mouse | Gamepad | Touch |
|--------|---------------|---------|-------|
| Move | WASD / Arrow Keys | Left Stick | Virtual Joystick |
| Camera | Mouse | Right Stick | Drag |
| Interact | E | A | Interact Button |
| Sit | E | A | Interact Button |

## Folder Structure
```
Assets/
├── Scripts/
│   ├── Core/           # Managers, utilities, extensions
│   ├── Player/         # Controller, interaction, animation
│   ├── Interactables/  # Door, Collectible, FireStarter, etc.
│   ├── Environment/    # WeatherController, DayNightCycle
│   └── UI/             # Menus, HUD, prompts
├── Shaders/            # Shader Graph + HLSL includes
├── VFX/                # VFX Graph assets
├── Data/               # ScriptableObjects
│   ├── Events/         # Event channels
│   └── Settings/       # Game settings
└── Art/
    ├── Models/
    ├── Textures/
    └── Animations/
```

## License

This project is open-source under the MIT License. Feel free to use, modify, and distribute it as needed.
