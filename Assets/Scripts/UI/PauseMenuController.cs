using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject pauseMenuCanvas;
    public GameObject winGameCanvas; // 🎯 Added Win Game Canvas Reference

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Scene Name")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "LV_Diaroma"; // 🎯 Added Game Scene Name for Restart

    private bool isPaused = false;

    [Header("Input Action Asset")]
    public InputActionReference pauseActionReference;

    private ThirdPersonController thirdPersonController;

    private void Awake()
    {
        thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        if (thirdPersonController == null)
        {
            Debug.LogError("ThirdPersonController not found. Make sure it's in the scene.");
        }
    }

    private void OnEnable()
    {
        if (pauseActionReference != null)
        {
            pauseActionReference.action.performed += OnPauseAction;
        }
    }

    private void OnDisable()
    {
        if (pauseActionReference != null)
        {
            pauseActionReference.action.performed -= OnPauseAction;
        }
    }

    private void OnPauseAction(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        PlayClickSound();
        isPaused = true;

        pauseMenuCanvas.SetActive(true);
        GameManager.Instance.ChangeGameState(GameState.Paused);

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
    }

    public void ResumeGame()
    {
        PlayClickSound();
        isPaused = false;

        pauseMenuCanvas.SetActive(false);
        GameManager.Instance.ChangeGameState(GameState.Playing);

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }
    }

    public void QuitToMainMenu()
    {
        PlayClickSound();
        GameManager.Instance.ReturnToMainMenu();
    }

    // 🎯 **New Methods for WinGameCanvas Buttons**
    public void RestartGame()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        GameManager.Instance.ChangeGameState(GameState.Playing);
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMainMenuFromWin()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        GameManager.Instance.ReturnToMainMenu();
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