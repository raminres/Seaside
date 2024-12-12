using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject optionsCanvas;
    public GameObject creditsCanvas;

    [Header("Volume")]
    public Slider volumeSlider;

    private void Start()
    {
        // Initialize main menu to be active
        ShowMainMenu();

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
        // Replace "YourSceneName" with the actual name of the game scene
        SceneManager.LoadScene("LV_Diaroma");
    }

    public void OpenOptions()
    {
        mainMenuCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenuCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
    }

    public void BackToMainMenu()
    {
        ShowMainMenu();
    }
    public void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Exit play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application
        Application.Quit();
#endif
    }

    public void AdjustVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume setting
    }
}