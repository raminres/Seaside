using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls the boat's approach to the shore and manages player attachment.
/// Designed to work with the PlayerController's new OnBoat state.
/// </summary>
public class BoatArrivalController : MonoBehaviour
{
    [Header("Arrival Settings")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private float _approachDuration = 15f;
    [SerializeField] private AnimationCurve _approachCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool _startAutomatically = true;

    [Header("Player Settings")]
    [SerializeField] private Transform _playerStandPoint;
    [SerializeField] private BoatInteractable _boatInteractable;

    [Header("Events")]
    [SerializeField] private UnityEvent _onArrived;

    private PlayerController _player;
    private bool _isApproaching;
    private float _approachTimer;

    // We track the boat's previous position to apply delta movement to the player
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;

    private void Start()
    {
        if (_boatInteractable != null)
        {
            // Disable disembarking while moving
            _boatInteractable.SetInteractable(false);
        }

        if (_startAutomatically)
        {
            StartArrival();
        }
    }

    public void StartArrival()
    {
        if (_startPoint == null || _endPoint == null)
        {
            Debug.LogError("[BoatArrivalController] Start or End point missing!");
            return;
        }

        // Snap boat to start
        transform.position = _startPoint.position;
        transform.rotation = _startPoint.rotation;

        _previousPosition = transform.position;
        _previousRotation = transform.rotation;

        // Find and attach player
        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null && _playerStandPoint != null)
        {
            _player.BoardBoat(_playerStandPoint, this.transform);
        }

        _isApproaching = true;
        _approachTimer = 0f;
    }

    private void Update()
    {
        if (!_isApproaching) return;

        _approachTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_approachTimer / _approachDuration);
        float curveT = _approachCurve.Evaluate(t);

        // Calculate new boat position
        Vector3 newPos = Vector3.Lerp(_startPoint.position, _endPoint.position, curveT);
        Quaternion newRot = Quaternion.Slerp(_startPoint.rotation, _endPoint.rotation, curveT);

        // Apply delta to player BEFORE moving the boat, so the player stays synchronized
        if (_player != null && _player.CurrentState == PlayerState.OnBoat)
        {
            Vector3 deltaPos = newPos - _previousPosition;
            
            // Calculate rotational delta
            Quaternion deltaRot = newRot * Quaternion.Inverse(_previousRotation);
            
            // If the boat rotates, the player needs to be swept along an arc relative to the boat's pivot
            Vector3 playerOffsetFromBoat = _player.transform.position - _previousPosition;
            Vector3 expectedNewPlayerPos = newPos + (deltaRot * playerOffsetFromBoat);
            Vector3 exactDelta = expectedNewPlayerPos - _player.transform.position;

            // Apply movement ignoring Y for the boat's forward motion (player gravity handles Y)
            // But if the boat rotates, we apply the full exact Delta
            _player.ApplyExternalMovement(exactDelta, deltaRot.eulerAngles.y);
        }

        // Move the boat
        transform.position = newPos;
        transform.rotation = newRot;

        _previousPosition = transform.position;
        _previousRotation = transform.rotation;

        // Check for arrival
        if (t >= 1f)
        {
            CompleteArrival();
        }
    }

    private void CompleteArrival()
    {
        _isApproaching = false;

        // Snap exactly to end
        transform.position = _endPoint.position;
        transform.rotation = _endPoint.rotation;

        if (_boatInteractable != null)
        {
            _boatInteractable.SetInteractable(true);
        }

        _onArrived?.Invoke();
    }

    public void OnPlayerDisembarked()
    {
        // Player has left the boat, we can clear the reference so we stop applying delta updates
        _player = null;
    }
}
