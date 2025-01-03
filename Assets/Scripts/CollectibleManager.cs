using UnityEngine;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    [Header("Collectible Settings")]
    public GameObject collectiblePrefab; // Prefab of the collectible object
    public int collectibleCount = 10; // Number of collectibles to instantiate
    public float minX = -10f; // Minimum X position
    public float maxX = 10f; // Maximum X position
    public float minZ = -10f; // Minimum Z position
    public float maxZ = 10f; // Maximum Z position
    public float yPosition = 1f; // Static Y position
    public AudioClip collectibleAudio;  

    [Header("UI Settings")]
    public TextMeshProUGUI counterText; // Reference to the TextMeshPro counter
    private int collectedCount = 0; // Current collected count

    private void Start()
    {
        InstantiateCollectibles();
        UpdateCounterUI();
    }

    // Instantiate collectibles at random positions
    private void InstantiateCollectibles()
    {
        for (int i = 0; i < collectibleCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                yPosition,
                Random.Range(minZ, maxZ)
            );

            Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);
        }
    }

    // Call this method when a collectible is collected
    public void CollectItem()
    {
        collectedCount++;
        UpdateCounterUI();
    }

    // Update the counter in the UI
    private void UpdateCounterUI()
    {
        counterText.text = collectedCount.ToString();
    }

    
}