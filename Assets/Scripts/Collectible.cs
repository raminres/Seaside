using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip collectSound; // Sound to play when collected
    private AudioSource audioSource;

    private void Start()
    {
        // Add an AudioSource component to the object if not already present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play the collect sound
            if (collectSound != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            // Notify the manager about the collection
            FindFirstObjectByType<CollectibleManager>().CollectItem();

            // Destroy this collectible after the sound has played
            Destroy(gameObject, collectSound != null ? collectSound.length : 0f);
        }
    }
}