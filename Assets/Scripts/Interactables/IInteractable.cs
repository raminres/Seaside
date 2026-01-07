/// <summary>
/// Interface for all interactive objects in the game.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Text shown in the interaction prompt.
    /// </summary>
    string InteractionPrompt { get; }

    /// <summary>
    /// Whether the player can currently interact with this object.
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// Type of interaction (affects UI and input handling).
    /// </summary>
    InteractionType InteractionType { get; }

    /// <summary>
    /// For Hold interactions, how long the player must hold the button.
    /// </summary>
    float HoldDuration { get; }

    /// <summary>
    /// Called when interaction completes.
    /// </summary>
    void Interact(PlayerController player);

    /// <summary>
    /// Called when player starts looking at this interactable.
    /// </summary>
    void OnFocused();

    /// <summary>
    /// Called when player stops looking at this interactable.
    /// </summary>
    void OnUnfocused();
}

public enum InteractionType
{
    Instant,    // Pickup, toggle switch
    Hold,       // Start fire, undock boat
    Toggle      // Doors, levers
}
