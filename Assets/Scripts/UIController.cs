using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject optionsCanvas;
    public GameObject creditsCanvas;

    [Header("Animator Controllers")]
    public Animator mainMenuAnimator;
    public Animator optionsAnimator;
    public Animator creditsAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Volume")]
    public Slider volumeSlider;
    
    [Header("Level Name")]
    public string levelName = "LV_Diaroma"; 

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

    public void StartGame()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            SceneManager.LoadScene(levelName);
        }));
    }

    public void OpenOptions()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            ShowCanvasWithAnimation(optionsCanvas, optionsAnimator);
        }));
    }

    public void OpenCredits()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
            ShowCanvasWithAnimation(creditsCanvas, creditsAnimator);
        }));
    }

    public void BackToMainMenu()
    {
        PlayClickSound();
        if (optionsCanvas.activeSelf)
        {
            StartCoroutine(PlayDisappearAnimation(optionsCanvas, optionsAnimator, () =>
            {
                ShowCanvasWithAnimation(mainMenuCanvas, mainMenuAnimator);
            }));
        }
        else if (creditsCanvas.activeSelf)
        {
            StartCoroutine(PlayDisappearAnimation(creditsCanvas, creditsAnimator, () =>
            {
                ShowCanvasWithAnimation(mainMenuCanvas, mainMenuAnimator);
            }));
        }
    }

    public void QuitGame()
    {
        PlayClickSound();
        StartCoroutine(PlayDisappearAnimation(mainMenuCanvas, mainMenuAnimator, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }));
    }

    public void AdjustVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume setting
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
    }
}
