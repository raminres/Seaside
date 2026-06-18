using UnityEngine;

/// <summary>
/// Handles the player disembarking from the boat. 
/// Inherits from InteractableBase so it works natively with Keyboard/Mouse, Gamepad, and Mobile input.
/// </summary>
public class BoatInteractable : InteractableBase
{
    [Header("Boat Settings")]
    [SerializeField] private Transform _disembarkPoint;
    [SerializeField] private BoatArrivalController _arrivalController;

    private void Awake()
    {
        _interactionType = InteractionType.Instant;
        _interactionPrompt = "Disembark";
    }

    protected override void OnInteractInternal(PlayerController player)
    {
        if (_disembarkPoint == null)
        {
            Debug.LogWarning("[BoatInteractable] No disembark point assigned!");
            return;
        }

        // Disable this interactable so we can't trigger it again during transition
        SetInteractable(false);

        // If ScreenFade is available, perform transition fade
        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.FadeOut(0.5f, () =>
            {
                // Teleport the player
                player.DisembarkBoat(_disembarkPoint);

                // Notify arrival controller
                if (_arrivalController != null)
                {
                    _arrivalController.OnPlayerDisembarked();
                }

                // Fade back in
                ScreenFade.Instance.FadeIn(0.5f);
            });
        }
        else
        {
            // Fallback (immediate disembark if screen fade is missing)
            player.DisembarkBoat(_disembarkPoint);

            if (_arrivalController != null)
            {
                _arrivalController.OnPlayerDisembarked();
            }
        }
    }
}

