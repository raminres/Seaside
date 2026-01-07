using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Loading screen with progress bar.
/// </summary>
public class LoadingScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private Image _progressFill;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _loadingTipText;

    [Header("Loading Tips")]
    [SerializeField] private string[] _loadingTips;
    [SerializeField] private float _tipChangeInterval = 3f;

    [Header("Events")]
    [SerializeField] private FloatEventSo _onLoadProgress;

    private float _tipTimer;
    private int _currentTipIndex;

    private void OnEnable()
    {
        if (_onLoadProgress != null)
        {
            _onLoadProgress.OnEventRaised += UpdateProgress;
        }

        ShowRandomTip();
    }

    private void OnDisable()
    {
        if (_onLoadProgress != null)
        {
            _onLoadProgress.OnEventRaised -= UpdateProgress;
        }
    }

    private void Update()
    {
        if (_loadingTips != null && _loadingTips.Length > 0)
        {
            _tipTimer += Time.unscaledDeltaTime;
            if (_tipTimer >= _tipChangeInterval)
            {
                _tipTimer = 0f;
                ShowRandomTip();
            }
        }
    }

    private void UpdateProgress(float progress)
    {
        if (_progressBar != null)
        {
            _progressBar.value = progress;
        }

        if (_progressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100f);
            _progressText.text = $"{percentage}%";
        }
    }

    private void ShowRandomTip()
    {
        if (_loadingTipText == null || _loadingTips == null || _loadingTips.Length == 0)
            return;

        _currentTipIndex = (_currentTipIndex + 1) % _loadingTips.Length;
        _loadingTipText.text = _loadingTips[_currentTipIndex];
    }
}
