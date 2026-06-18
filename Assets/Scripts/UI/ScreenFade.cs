using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A screen fade utility for level load fade-in and transition fade-out/in sequences.
/// </summary>
public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image _fadeImage;

    [Header("Settings")]
    [SerializeField] private bool _fadeOnStart = true;
    [SerializeField] private float _startFadeDuration = 1.5f;
    [SerializeField] private Color _fadeColor = Color.black;

    private Coroutine _currentFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_fadeImage == null)
        {
            _fadeImage = GetComponentInChildren<Image>();
        }

        if (_fadeImage != null)
        {
            // Block raycasts while loading or transitioning to avoid player input issues
            _fadeImage.raycastTarget = true;
            
            // Start at full opacity
            Color initialColor = _fadeColor;
            initialColor.a = 1f;
            _fadeImage.color = initialColor;
        }
    }

    private void Start()
    {
        if (_fadeOnStart)
        {
            FadeIn(_startFadeDuration);
        }
    }

    /// <summary>
    /// Smoothly fades the screen in (from black to transparent).
    /// </summary>
    public void FadeIn(float duration, System.Action onComplete = null)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }
        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration, onComplete));
    }

    /// <summary>
    /// Smoothly fades the screen out (from transparent to black).
    /// </summary>
    public void FadeOut(float duration, System.Action onComplete = null)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }
        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        if (_fadeImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        _fadeImage.gameObject.SetActive(true);
        // Block raycasts if we are fading or solid
        _fadeImage.raycastTarget = (endAlpha > 0.01f || startAlpha > 0.01f);

        float elapsed = 0f;
        Color c = _fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            _fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        _fadeImage.color = c;

        // If screen is transparent, disable object to save fillrate and drawcalls
        if (endAlpha < 0.01f)
        {
            _fadeImage.gameObject.SetActive(false);
            _fadeImage.raycastTarget = false;
        }
        else
        {
            _fadeImage.raycastTarget = true;
        }

        _currentFadeCoroutine = null;
        onComplete?.Invoke();
    }
}
