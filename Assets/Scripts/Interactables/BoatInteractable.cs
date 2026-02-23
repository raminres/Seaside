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

        // Trigger player disembark sequence
        player.DisembarkBoat(_disembarkPoint);

        // Tell the arrival controller we've disembarked (to stop boat logic if needed)
        if (_arrivalController != null)
        {
            _arrivalController.OnPlayerDisembarked();
        }

        // Disable this interactable so we can't get back on (one-way trip)
        SetInteractable(false);
    }
}
