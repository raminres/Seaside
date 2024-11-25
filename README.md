# Seaside - Unity Project
This is a personal Unity project that features a seaside scene with a basic third-person character controller, various interactive elements, and custom shaders. The project integrates models and textures generated from Blender using geometry nodes and employs various animations to enhance the experience.

## Features
1. Third-Person Character Controller
  - The character is controlled using Unity's new Input System.
  - Basic movement (walking/running) and camera control are integrated using Unity's own Starter Assets.
  - Interaction with objects such as doors and sitting is implemented.
  - Imported animations for walking, sitting, and interacting with the environment.
  - Remote Addressable Loading. The Boat prefab is an asset that is being loaded from Firebase database. The remote loading is handled by a script. The addressable is built locally and uploaded to firebase.
2. Interactive Elements
  - Door Interaction: Doors can be opened and closed using keyboard interaction (E key). When the character exits the trigger zone, the door automatically closes.
  - Sitting Interaction: The character can sit on specific objects. If the player attempts to sit in an invalid area, a UI prompt will notify them.
  - Change the color of the Sofa: The color of the sofa can be changed with pressing the C key on the keyboard. When you are near the sofa try it.
  - Change the Texture of the Carpet: The texture of the carpet could be toggled with T key.
3. Seaside Environment
  - Custom water shader created using Unity's Shader Graph for dynamic and stylized water effects.
  - Sand, rocks, and other seaside elements to create a stylized environment.
  - Falling leaves, fire and dust VFX to enhance the feeling of the environment.
4. House Models
  - House models with textures and materials used to add variety.
  - Models are created in Blender using geometry nodes and imported as .fbx files into Unity.
5. Animations
  - Character animations for walking, sitting, interacting with doors, and other actions are imported and integrated with Unity's Animator.

# Installation & Setup
Requirements
Unity: This project is built using Unity 2021.3 or higher.
Blender (Optional): Models are generated using Blender Geometry Nodes, although Blender is not required to run the project.
TextMesh Pro: Ensure you have TextMesh Pro installed via the Unity Package Manager for UI-related functionality.
Steps to Run the Project:
## Clone the repository:
```git clone https://github.com/RasulzadeRamin/Seaside.git```

Open the project in Unity 2023.2.20f1.
Ensure that the following packages are installed:
1. Input System
2. TextMesh Pro
3. Open the **Assets > Scenes > Seaside** Unity Scene
3. Hit Play in the Unity Editor to explore the scene.
## Controls
Movement: WASD or arrow keys to move the character.
Camera Control: Use the mouse to rotate the camera.
Interact with Doors: Press E when near a door to open or close it.
Sit: Press Q near a sitting area. If you’re not in a sitting area, a UI message will display "Not a sitting place!"
Models and Assets
## Assets 
1. Character: The basic character model is from the Unity Asset Store or customized for this project.

2. Houses: Blender-generated .fbx models with procedural textures applied in Unity.

3. Terrain and Water

4. Boat on the water surface (Addressable)

## License
This project is open-source under the MIT License. Feel free to use, modify, and distribute it as needed.
