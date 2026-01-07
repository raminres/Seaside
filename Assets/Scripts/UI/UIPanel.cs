using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for UI panels with show/hide functionality.
/// </summary>
public class UIPanel : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _showTrigger = "Appear";
    [SerializeField] private string _hideTrigger = "Disappear";
    [SerializeField] private bool _useAnimator = true;
    [SerializeField] private float _fadeSpeed = 5f;

    [Header("Canvas Group (for fade)")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Events")]
    [SerializeField] private UnityEvent _onShow;
    [SerializeField] private UnityEvent _onHide;
    [SerializeField] private UnityEvent _onShowComplete;
    [SerializeField] private UnityEvent _onHideComplete;

    public bool IsVisible { get; private set; }

    private bool _isFading;
    private float _targetAlpha;

    private void Awake()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (!_isFading || _canvasGroup == null) return;

        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.unscaledDeltaTime * _fadeSpeed);

        if (Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
        {
            _isFading = false;

            if (_targetAlpha == 0f)
            {
                gameObject.SetActive(false);
                _onHideComplete?.Invoke();
            }
            else
            {
                _onShowComplete?.Invoke();
            }
        }
    }

    public virtual void Show()
    {
        IsVisible = true;
        gameObject.SetActive(true);

        if (_useAnimator && _animator != null)
        {
            _animator.SetTrigger(_showTrigger);
        }
        else if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _targetAlpha = 1f;
            _isFading = true;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        _onShow?.Invoke();
    }

    public virtual void Hide()
    {
        IsVisible = false;

        if (_useAnimator && _animator != null)
        {
            _animator.SetTrigger(_hideTrigger);
        }
        else if (_canvasGroup != null)
        {
            _targetAlpha = 0f;
            _isFading = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        else
        {
            gameObject.SetActive(false);
            _onHideComplete?.Invoke();
        }

        _onHide?.Invoke();
    }

    public void ShowImmediate()
    {
        IsVisible = true;
        gameObject.SetActive(true);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        _isFading = false;
        _onShow?.Invoke();
        _onShowComplete?.Invoke();
    }

    public void HideImmediate()
    {
        IsVisible = false;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        _isFading = false;
        gameObject.SetActive(false);
        _onHide?.Invoke();
        _onHideComplete?.Invoke();
    }
}
