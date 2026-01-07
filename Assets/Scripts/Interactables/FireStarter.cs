using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fire pit, torch, or campfire that can be lit by the player.
/// </summary>
public class FireStarter : InteractableBase
{
    [Header("Fire Settings")]
    [SerializeField] private GameObject _fireVFX;
    [SerializeField] private Light _fireLight;
    [SerializeField] private float _lightIntensity = 2f;
    [SerializeField] private bool _requiresItem = true;
    [SerializeField] private string _requiredItemId = "Matches";

    [Header("Light Flicker")]
    [SerializeField] private bool _flickerLight = true;
    [SerializeField] private float _flickerSpeed = 10f;
    [SerializeField] private float _flickerIntensityRange = 0.3f;

    [Header("Fire Prompts")]
    [SerializeField] private string _lightPrompt = "Light Fire";
    [SerializeField] private string _noItemPrompt = "Need Matches";
    [SerializeField] private string _litPrompt = "";

    [Header("Events")]
    [SerializeField] private GameEventSo _onFireLit;
    [SerializeField] private UnityEvent _onLit;

    private bool _isLit;
    private float _baseIntensity;

    // Simple inventory check - replace with your inventory system
    public static bool HasMatches { get; set; } = false;

    public bool IsLit => _isLit;

    private void Awake()
    {
        _interactionType = InteractionType.Hold;
        _holdDuration = 2f;

        if (_fireVFX != null) _fireVFX.SetActive(false);
        if (_fireLight != null)
        {
            _baseIntensity = _lightIntensity;
            _fireLight.intensity = 0f;
        }
    }

    public override string InteractionPrompt
    {
        get
        {
            if (_isLit) return _litPrompt;
            if (_requiresItem && !HasMatches) return _noItemPrompt;
            return _lightPrompt;
        }
    }

    public override bool CanInteract => !_isLit && (!_requiresItem || HasMatches);

    protected override void OnInteractInternal(PlayerController player)
    {
        if (_isLit) return;
        LightFire();
    }

    private void Update()
    {
        if (_isLit && _flickerLight && _fireLight != null)
        {
            float flicker = Mathf.PerlinNoise(Time.time * _flickerSpeed, 0f);
            flicker = (flicker - 0.5f) * 2f * _flickerIntensityRange;
            _fireLight.intensity = _baseIntensity + flicker;
        }
    }

    public void LightFire()
    {
        if (_isLit) return;

        _isLit = true;

        if (_fireVFX != null)
        {
            _fireVFX.SetActive(true);
        }

        if (_fireLight != null)
        {
            _fireLight.intensity = _lightIntensity;
        }

        _onFireLit?.RaiseEvent();
        _onLit?.Invoke();
    }

    public void ExtinguishFire()
    {
        if (!_isLit) return;

        _isLit = false;

        if (_fireVFX != null)
        {
            _fireVFX.SetActive(false);
        }

        if (_fireLight != null)
        {
            _fireLight.intensity = 0f;
        }
    }
}
