using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject pauseMenuCanvas;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Scene Name")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    [Header("Input Action Asset")]
    public InputActionReference pauseActionReference; // Reference to the Pause action

    private ThirdPersonController thirdPersonController; // Reference to the third-person controller

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
        // Subscribe to the Pause action
        if (pauseActionReference != null)
        {
            pauseActionReference.action.performed += OnPauseAction;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the Pause action
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

        // Show the pause menu
        pauseMenuCanvas.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;

        // Disable player input
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        // Show the mouse pointer
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        PlayClickSound();
        isPaused = false;

        // Hide the pause menu
        pauseMenuCanvas.SetActive(false);

        // Resume the game
        Time.timeScale = 1f;

        // Enable player input
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }

        // Hide the mouse pointer
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMainMenu()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Ensure time resumes before switching scenes
        SceneManager.LoadScene(mainMenuSceneName);
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
