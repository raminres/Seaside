using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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
    public string[] levelScenes = { "LV_Level1", "LV_Level2", "LV_Level3" }; // Array to store level names

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
    private GameObject winGameCanvas;
    private Animator winGameAnimator;

    [Header("Level Selection UI")]
    public GameObject levelSelectionCanvas;
    public Animator levelSelectionAnimator;

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
        AssignCounterText(); 
        AssignWinGameCanvas();
        ResetCollectibles();
    }
    public void ChangeGameState(GameState newState)
    {
        if (gameState != null)
        {
            gameState.CurrentState = newState;
            onGameStateChangeEvent?.RaiseEvent();
        }
        else
        {
            Debug.LogWarning("⚠️ GameStateSO reference is missing in GameManager!");
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
        winGameCanvas = GameObject.Find("CanvasWinGame");
        if (winGameCanvas != null)
        {
            winGameAnimator = winGameCanvas.GetComponent<Animator>();
            winGameCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ WinGameCanvas not found in the scene!");
        }
    }

    public void ResetCollectibles()
    {
        collectedCount = 0;
        UpdateCounterUI();
        SpawnCollectibles();
    }

    public void UpdateCounterUI()
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

    public void SpawnCollectibles()
    {
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
            if (winGameAnimator != null)
            {
                winGameAnimator.SetTrigger("Appear");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ WinGameCanvas is not assigned in the scene!");
        }
    }

    public void ShowLevelSelection()
    {
        if (levelSelectionCanvas != null)
        {
            levelSelectionCanvas.SetActive(true);
            if (levelSelectionAnimator != null)
            {
                levelSelectionAnimator.SetTrigger("Appear");
            }
        }
    }

    public void HideLevelSelection()
    {
        if (levelSelectionCanvas != null)
        {
            if (levelSelectionAnimator != null)
            {
                levelSelectionAnimator.SetTrigger("Disappear");
                StartCoroutine(DisableCanvasAfterAnimation(levelSelectionCanvas, levelSelectionAnimator, "Disappear"));
            }
            else
            {
                levelSelectionCanvas.SetActive(false);
            }
        }
    }

    public void SelectLevel(int levelIndex)
    {
        HideLevelSelection();
        LoadLevel(levelIndex);
    }

    private IEnumerator DisableCanvasAfterAnimation(GameObject canvas, Animator animator, string animationName)
    {
        float animationLength = GetAnimationClipLength(animator, animationName);
        yield return new WaitForSeconds(animationLength);
        canvas.SetActive(false);
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
        return 0.5f;
    }
    public void UpdateVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
        onVolumeChangeEvent?.RaiseEvent();
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            SceneManager.LoadScene(levelScenes[levelIndex]);
        }
        else
        {
            Debug.LogError("Invalid level index!");
        }
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

