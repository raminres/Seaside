using UnityEngine;
using UnityEngine.SceneManagement;
public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Game is quitting...");

        SceneManager.LoadScene("LV_MainMenu");
        
    }
}