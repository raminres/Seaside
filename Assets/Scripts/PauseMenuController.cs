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
        // Unsubscribe to the Pause action
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
        pauseMenuCanvas.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Pause the game
    }

    public void ResumeGame()
    {
        PlayClickSound();
        isPaused = false;
        pauseMenuCanvas.SetActive(false); // Hide the pause menu
        Time.timeScale = 1f; // Resume the game
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
