# Starting Boat Cutscene & Screen Fade Walkthrough

This walkthrough details the changes implemented to support the starting boat cutscene and screen fade transitions.

---

## 🛠️ Changes Implemented

### 1. Screen Fade UI Component
We created a new script, [ScreenFade.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/UI/ScreenFade.cs), to handle:
*   **Fade on Start:** Automatically fades the screen in from black on level load.
*   **Custom Fades:** Exposes helper methods `FadeIn` and `FadeOut` with duration and completion callbacks.
*   **Performance Optimization:** Automatically disables the Image object when fully transparent to prevent fillrate/drawcall overhead and block raycasts when fading.

### 2. Smooth Disembark Transition
We modified [BoatInteractable.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/Interactables/BoatInteractable.cs) to perform a fade-out/in sequence during disembarkation:
```diff
     protected override void OnInteractInternal(PlayerController player)
     {
         if (_disembarkPoint == null)
         {
             Debug.LogWarning("[BoatInteractable] No disembark point assigned!");
             return;
         }
 
-        // Trigger player disembark sequence
-        player.DisembarkBoat(_disembarkPoint);
-
-        // Tell the arrival controller we've disembarked (to stop boat logic if needed)
-        if (_arrivalController != null)
-        {
-            _arrivalController.OnPlayerDisembarked();
-        }
-
-        // Disable this interactable so we can't get back on (one-way trip)
-        SetInteractable(false);
+        // Disable this interactable so we can't trigger it again during transition
+        SetInteractable(false);
+
+        // If ScreenFade is available, perform transition fade
+        if (ScreenFade.Instance != null)
+        {
+            ScreenFade.Instance.FadeOut(0.5f, () =>
+            {
+                // Teleport the player
+                player.DisembarkBoat(_disembarkPoint);
+
+                // Notify arrival controller
+                if (_arrivalController != null)
+                {
+                    _arrivalController.OnPlayerDisembarked();
+                }
+
+                // Fade back in
+                ScreenFade.Instance.FadeIn(0.5f);
+            });
+        }
+        else
+        {
+            // Fallback (immediate disembark if screen fade is missing)
+            player.DisembarkBoat(_disembarkPoint);
+
+            if (_arrivalController != null)
+            {
+                _arrivalController.OnPlayerDisembarked();
+            }
+        }
     }
```

---

## 🖥️ Unity Editor Setup Instructions

To hook up the Screen Fade UI in your scene:

### 1. Create the UI Canvas & Image
1.  In the Unity hierarchy, select **Create > UI > Canvas** (name it `Canvas_ScreenFade`).
2.  Set the Canvas settings:
    *   **Render Mode:** `Screen Space - Overlay`
    *   **Sort Order:** `999` (Ensure it is higher than all other UI canvasses so it renders on top).
3.  Right-click on `Canvas_ScreenFade` and select **UI > Image** (name it `Panel_FadeImage`).
4.  Configure the `Panel_FadeImage`:
    *   **Anchor Presets:** Hold `Alt` and select the bottom-right option (**stretch-stretch**) to make it fill the screen completely.
    *   **Color:** Set it to Solid Black (`#000000`) with Alpha set to `255` (fully opaque).
    *   **Raycast Target:** Checked.

### 2. Attach the Script
1.  Drag the [ScreenFade.cs](file:///c:/Users/ramin/Desktop/Repos/Seaside/Assets/Scripts/UI/ScreenFade.cs) script onto the `Canvas_ScreenFade` object.
2.  Assign the `Fade Image` field in the inspector by dragging the `Panel_FadeImage` object into it.
3.  Ensure `Fade On Start` is checked.

### 3. Verification & Testing
1.  Press **Play** in the editor.
2.  The level will load, and the screen should smoothly fade in from black.
3.  Locomotion controls are fully enabled, allowing you to walk around inside the boat bounds.
4.  Once the boat docks, click the **Disembark** prompt.
5.  The screen will fade out to black, the player will teleport to the dock point, and the screen will fade back in smoothly.
