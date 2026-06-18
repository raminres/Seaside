# Starting Boat Cutscene and Docking System

This implementation plan details the setup and integration of a starting boat cutscene. The player will begin on a boat (`PF_Boat.prefab`), move slowly to the port, be restricted to the boat's interior boundaries, and then disembark onto the dock to explore the island.

---

## User Review Required

> [!NOTE]
> We already have a robust kinematic attachment and movement system built in [BoatArrivalController.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Objects/BoatArrivalController.cs) and [PlayerController.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Player/PlayerController.cs#L692-L766).
>
> We will utilize these existing systems to avoid redundant work and prevent bugs.

> [!IMPORTANT]
> The boundaries to restrict the player inside the boat should be implemented in the Unity Editor using static colliders on the boat prefab. This plan includes specific step-by-step instructions for editor setup.

---

## Open Questions

> [!IMPORTANT]
> **1. Input Restrictions:** During the boat's approach, should the player be allowed to walk around the deck immediately, or should they be locked in place (unable to move, only look) for a cinematic intro before movement is enabled? We have proposed an `IntroCutsceneController` that can handle both paths.
>
> **2. Screen Fading:** Do you want a camera black fade-in effect when the level starts? We can implement a quick UI canvas fade script if one does not exist.

---

## Proposed Changes

### Core Systems

#### [NEW] [IntroCutsceneController.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Objects/IntroCutsceneController.cs)
A script to orchestrate the cutscene lifecycle:
*   Controls the screen fade-in on level load.
*   Optionally locks player locomotion input (allowing camera rotation) during the initial approach.
*   Enables player locomotion after a set time or immediately.
*   Triggers the [BoatArrivalController](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Objects/BoatArrivalController.cs) sequence.

#### [MODIFY] [PlayerController.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Player/PlayerController.cs)
Ensure locomotion input can be toggled by the `IntroCutsceneController`. We can add a simple boolean flag `_disableLocomotion` to prevent walking while on the boat, keeping only camera rotation active.

---

## Editor Setup Instructions

To implement this cutscene in your level scene (e.g., `LV_TestScene` or a new scene):

### 1. Prefab & Target Configuration
1.  Open your gameplay scene in the Unity Editor.
2.  Drag the **`PF_Boat_Parent.prefab`** (located at [Assets/Art/Props/Boat/](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Art/Props/Boat/)) into the hierarchy.
3.  Create three empty game objects in the scene:
    *   `Boat_StartPoint`: Position it out at sea where the cutscene starts.
    *   `Boat_EndPoint`: Position it alongside the port's dock where the boat will stop.
    *   `Dock_DisembarkPoint`: Position it on the dock's surface where the player will stand after disembarking.

### 2. Controller Configuration
1.  Select the **`PF_Boat_Parent`** object.
2.  Ensure it has the [BoatArrivalController](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Objects/BoatArrivalController.cs) component.
3.  Assign the fields on `BoatArrivalController` in the inspector:
    *   `Start Point` = `Boat_StartPoint`
    *   `End Point` = `Boat_EndPoint`
    *   `Player Stand Point` = (Assign the child transform of the boat prefab where the player is positioned, e.g., `PlayerStandPoint`)
    *   `Approach Duration` = e.g., `15` seconds (or your choice)
    *   `Boat Interactable` = (Assign the child object holding the `BoatInteractable` script)
    *   `Start Automatically` = `true` (or link it to the new `IntroCutsceneController` script)
4.  Select the child object containing the **`BoatInteractable`** component and assign:
    *   `Disembark Point` = `Dock_DisembarkPoint`

### 3. Collision Bounds (Restricting Player to Boat)
1.  Open the **`PF_Boat`** prefab (or override instances).
2.  Add empty game objects with **Box Colliders** or **Mesh Colliders** to represent the walls/railings of the boat.
3.  Configure these colliders:
    *   Create a border on the left, right, front, and back sides of the deck.
    *   Ensure they are high enough (at least 2 units) so the player's `CharacterController` cannot jump or push over them.
    *   Set their layer to `Default` or `Ground` so that the player's physics bounds will collide with them.
4.  Make sure the deck itself has a collider on the `Ground` layer so the player remains grounded during the ride.

---

## Verification Plan

### Automated Tests
- None. Verification is done through editor simulation.

### Manual Verification
1.  Load the game scene in the editor.
2.  Verify the player spawns correctly at the `PlayerStandPoint` on the boat.
3.  Verify the boat slowly moves from the `StartPoint` to the `EndPoint`.
4.  Attempt to walk/sprint off the boat's edges and verify that the border colliders restrict you to the deck.
5.  Verify that upon arriving at the dock, the "Disembark" prompt appears.
6.  Interact with the boat and verify that you are teleported onto the dock (`Dock_DisembarkPoint`) and player controls/states revert to normal `Idle` / `Walking` locomotion.
