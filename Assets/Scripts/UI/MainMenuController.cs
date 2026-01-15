using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject optionsCanvas;
    public GameObject creditsCanvas;
    public GameObject quitConfirmationCanvas;
    public GameObject levelSelectionCanvas;
    public Animator levelSelectionAnimator;

    [Header("Animator Controllers")]
    public Animator mainMenuAnimator;
    public Animator optionsAnimator;
    public Animator creditsAnimator;
    public Animator quitConfirmationAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Volume")]
    public Slider volumeSlider;

    [Header("FPS Settings")]
    [Tooltip("Toggle for 60 FPS mode (on = 60, off = 30)")]
    public Toggle fpsToggle;
    [Tooltip("Optional: Text to show current FPS")]
    public TextMeshProUGUI fpsValueText;

    [Header("Scene Name")]
    public string gameSceneName = "LV_Diaroma";

    private void Start()
    {
        ShowMainMenuInstant();

        // Load saved volume setting
        if (PlayerPrefs.HasKey("Volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            AudioListener.volume = savedVolume;
            if (volumeSlider != null)
            {
                volumeSlider.value = savedVolume;
            }
        }
        else
        {
            AudioListener.volume = 1.0f;
            if (volumeSlider != null)
            {
                volumeSlider.value = 1.0f;
            }
        }

        // Load saved FPS setting
        InitializeFPSToggle();
    }

    #region FPS Settings

    private void InitializeFPSToggle()
    {
        if (fpsToggle == null) return;

        // Load current setting from GameManager or PlayerPrefs
        bool isHighFPS = true;
        if (GameManager.Instance != null)
        {
            isHighFPS = GameManager.Instance.IsHighFPSEnabled();
        }
        else
        {
            isHighFPS = PlayerPrefs.GetInt("TargetFPS", 60) == 60;
        }

        // Set toggle without triggering the callback
        fpsToggle.SetIsOnWithoutNotify(isHighFPS);
        
        // Add listener
        fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);

        // Update text
        UpdateFPSValueText();
    }

    public void OnFPSToggleChanged(bool isHighFPS)
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetHighFPSMode(isHighFPS);
        }
        else
        {
            // Fallback if GameManager not available
            int fps = isHighFPS ? 60 : 30;
            Application.targetFrameRate = fps;
            PlayerPrefs.SetInt("TargetFPS", fps);
            PlayerPrefs.Save();
        }

        UpdateFPSValueText();
    }

    /// <summary>
    /// Alternative method for a button that toggles FPS.
    /// </summary>
    public void ToggleFPS()
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ToggleFPS();
        }
        else
        {
            int currentFPS = PlayerPrefs.GetInt("TargetFPS", 60);
            int newFPS = currentFPS == 30 ? 60 : 30;
            Application.targetFrameRate = newFPS;
            PlayerPrefs.SetInt("TargetFPS", newFPS);
            PlayerPrefs.Save();
        }

        // Update toggle if it exists
        if (fpsToggle != null)
        {
            bool isHighFPS = GameManager.Instance != null 
                ? GameManager.Instance.IsHighFPSEnabled() 
                : PlayerPrefs.GetInt("TargetFPS", 60) == 60;
            fpsToggle.SetIsOnWithoutNotify(isHighFPS);
        }

        UpdateFPSValueText();
    }

    private void UpdateFPSValueText()
    {
        if (fpsValueText == null) return;

        int fps;
        if (GameManager.Instance != null)
        {
            fps = GameManager.Instance.GetTargetFPS();
        }
        else
        {
            fps = PlayerPrefs.GetInt("TargetFPS", 60);
        }

        fpsValueText.text = $"{fps} FPS";
    }

    #endregion

    #region Menu Navigation

    public void StartGame()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            GameManager.Instance.ShowLevelSelection();
        }));
    }

    public void ShowOptions()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            ShowCanvasWithAnimation(optionsCanvas, optionsAnimator);
        }));
        AudioListener.volume = volumeSlider.value;
    }

    public void ShowCredits()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            ShowCanvasWithAnimation(creditsCanvas, creditsAnimator);
        }));
    }

    public void ShowQuitConfirmation()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            ShowCanvasWithAnimation(quitConfirmationCanvas, quitConfirmationAnimator);
        }));
    }

    public void ConfirmQuit()
    {
        PlayClickSound();
        GameManager.Instance.QuitGame();
    }

    public void CancelQuit()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(quitConfirmationCanvas, quitConfirmationAnimator, () =>
        {
            ShowCanvasWithAnimation(mainMenuCanvas, mainMenuAnimator);
        }));
    }

    public void BackToMainMenuFromOptions()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(optionsCanvas, optionsAnimator, () =>
        {
            ShowCanvasWithAnimation(mainMenuCanvas, mainMenuAnimator);
        }));
    }

    public void BackToMainMenuFromCredits()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(creditsCanvas, creditsAnimator, () =>
        {
            ShowCanvasWithAnimation(mainMenuCanvas, mainMenuAnimator);
        }));
    }

    #endregion

    #region Volume

    public void AdjustVolume(float volume)
    {
        Debug.Log($"AdjustVolume called with value: {volume}");
        volume = volumeSlider.value;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateVolume(volume);
        }
    }

    #endregion

    #region Canvas Helpers

    private void ShowCanvasWithAnimation(GameObject canvas, Animator animator)
    {
        canvas.SetActive(true);
        animator.SetTrigger("Appear");
    }

    private IEnumerator PlayDisappearAnimation(GameObject canvas, Animator animator, System.Action onComplete)
    {
        animator.SetTrigger("Disappear");
        float animationLength = GetAnimationClipLength(animator, "Disappear");
        yield return new WaitForSeconds(animationLength);

        canvas.SetActive(false);
        onComplete?.Invoke();
    }

    private float GetAnimationClipLength(Animator animator, string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        Debug.LogWarning($"Animation clip '{clipName}' not found in Animator.");
        return 0.0f;
    }

    private void ShowMainMenuInstant()
    {
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        quitConfirmationCanvas.SetActive(false);
    }

    #endregion

    #region Audio

    public void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    #endregion
}
