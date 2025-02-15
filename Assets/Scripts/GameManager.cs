using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GameStateSo gameState;
    [SerializeField] private GameEventSo onGameStateChangeEvent;

    [Header("Volume Settings")]
    [SerializeField] private GameEventSo onVolumeChangeEvent;

    [Header("Scene Management")]
    public string mainMenuSceneName = "LV_MainMenu"; 
    public string gameSceneName = "LV_Diaroma";  

    [Header("Collectible Settings")]
    public GameObject collectiblePrefab;
    public int collectibleCount = 10;
    public float minX = -10f, maxX = 10f;
    public float minZ = -10f, maxZ = 10f;
    public float yPosition = 1f;
    public AudioClip collectibleAudio;

    [Header("UI Settings")]
    private TextMeshProUGUI counterText; // Collectible counter UI
    private int collectedCount = 0;

    [Header("Win State UI")]
    public GameObject winGameCanvas;  // Dynamically found Win Canvas
    public Animator winGameAnimator;  // Animator for WinGameCanvas

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            AssignCounterText(); 
            AssignWinGameCanvas();  // Find Win Canvas in the new scene
            ResetCollectibles();
        }
    }

    private void AssignCounterText()
    {
        GameObject counterObject = GameObject.Find("CollectibleCounter");
        if (counterObject != null)
        {
            counterText = counterObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("⚠️ CollectibleCounter UI not found in the scene!");
        }

        UpdateCounterUI(); 
    }

    private void AssignWinGameCanvas()
    {
        GameObject winCanvasObject = GameObject.FindWithTag("WinGame"); // Find by tag
        if (winCanvasObject != null)
        {
            winGameCanvas = winCanvasObject;
            winGameAnimator = winGameCanvas.GetComponent<Animator>();
            winGameCanvas.SetActive(false); // Hide it at start
        }
        else
        {
            Debug.LogWarning("⚠️ WinGameCanvas not found using tag! Make sure it's tagged correctly.");
        }
    }


    public void ChangeGameState(GameState newState)
    {
        gameState.CurrentState = newState;
        onGameStateChangeEvent.RaiseEvent();
    }

    public void ResetCollectibles()
    {
        collectedCount = 0;
        UpdateCounterUI();
        SpawnCollectibles();
    }

    public void SpawnCollectibles()
    {
        if (SceneManager.GetActiveScene().name == mainMenuSceneName) return; 

        for (int i = 0; i < collectibleCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                yPosition,
                Random.Range(minZ, maxZ)
            );

            GameObject collectible = Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);
            collectible.GetComponent<Collectible>().Initialize(collectibleAudio);
        }
    }

    public void CollectItem()
    {
        collectedCount++;
        UpdateCounterUI();

        if (collectedCount >= collectibleCount)
        {
            Debug.Log("🏆 All collectibles collected! Player Wins!");
            WinGame();
        }
    }

    private void WinGame()
    {
        if (winGameCanvas != null)
        {
            winGameCanvas.SetActive(true);
            //pauseMenuCanvas.SetActive(false); // Hide Pause Menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (winGameAnimator != null)
            {
                winGameAnimator.SetTrigger("Appear");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ WinGameCanvas not assigned!");
        }
    }

    private void UpdateCounterUI()
    {
        if (counterText != null)
        {
            counterText.text = collectedCount.ToString();
        }
        else
        {
            Debug.LogWarning("⚠️ Counter UI not assigned yet!");
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(gameSceneName); // Reload game scene
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName); // Load Main Menu
    }

    public void UpdateVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
        onVolumeChangeEvent?.RaiseEvent();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
