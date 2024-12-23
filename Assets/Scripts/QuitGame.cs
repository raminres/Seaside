using UnityEngine;
using UnityEngine.SceneManagement;
public class QuitGame : MonoBehaviour
{
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public void Quit()
    {
        Debug.Log("Game is quitting...");
        PlayClickSound();

        SceneManager.LoadScene("LV_MainMenu");
        
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