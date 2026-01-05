using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            volumeSlider.value = savedVolume;
        }
        else
        {
            AudioListener.volume = 1.0f; // Default volume
            volumeSlider.value = 1.0f;
        }
    }

    public void StartGame()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            GameManager.Instance.ShowLevelSelection(); // Open Level Selection UI instead of loading a level
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
        GameManager.Instance.QuitGame(); // Use GameManager's quit method
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

    public void AdjustVolume(float volume)
    {
        Debug.Log($"AdjustVolume called with value: {volume}");
        volume = volumeSlider.value;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume setting

        // Inform GameManager about volume change
        GameManager.Instance.UpdateVolume(volume);
    }

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
}
