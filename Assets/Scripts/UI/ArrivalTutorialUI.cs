using TMPro;
using UnityEngine;

/// <summary>
/// Shows the first movement hint after the boat docks. It listens to event
/// channels so the arrival sequence remains independent of the UI hierarchy.
/// </summary>
public class ArrivalTutorialUI : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEventSo _onArrivalDocked;
    [SerializeField] private GameEventSo _onArrivalCompleted;

    [Header("UI")]
    [SerializeField] private GameObject _tutorialContainer;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private string _keyboardMessage = "Use WASD to move";
    [SerializeField] private string _gamepadMessage = "Use the left stick to move";
    [SerializeField] private string _touchMessage = "Use the left joystick to move";

    private void Awake()
    {
        SetVisible(false);
    }

    private void OnEnable()
    {
        if (_onArrivalDocked != null) _onArrivalDocked.OnEventRaised += ShowMovementTutorial;
        if (_onArrivalCompleted != null) _onArrivalCompleted.OnEventRaised += HideTutorial;
    }

    private void OnDisable()
    {
        if (_onArrivalDocked != null) _onArrivalDocked.OnEventRaised -= ShowMovementTutorial;
        if (_onArrivalCompleted != null) _onArrivalCompleted.OnEventRaised -= HideTutorial;
    }

    public void ShowMovementTutorial()
    {
        if (_tutorialText != null)
        {
            string scheme = UnityEngine.InputSystem.PlayerInput.all.Count > 0
                ? UnityEngine.InputSystem.PlayerInput.all[0].currentControlScheme
                : "Keyboard&Mouse";
            _tutorialText.text = scheme switch
            {
                "Gamepad" => _gamepadMessage,
                "Touch" => _touchMessage,
                _ => _keyboardMessage
            };
        }

        SetVisible(true);
    }

    public void HideTutorial()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (_tutorialContainer != null) _tutorialContainer.SetActive(visible);
    }
}
