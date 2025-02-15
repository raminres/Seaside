using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip collectSound;
    private AudioSource audioSource;
    private Animator animator;
    private bool isCollected = false;

    public void Initialize(AudioClip clip)
    {
        animator = GetComponent<Animator>();

        // Add an AudioSource if not already attached
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = clip;
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            isCollected = true;
            Collect();
        }
    }

    public void Collect()
    {
        GameManager.Instance.CollectItem(); // Notify GameManager
        
        // ✅ **Trigger the shrink animation**
        if (animator != null)
        {
            Debug.Log("Collectible Animation Triggered"); // Debugging
            animator.SetTrigger("Collect");
        }
        else
        {
            Debug.LogWarning("Animator not found on collectible!");
        }

        // ✅ **Play sound**
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // ✅ **Destroy the object after animation & sound complete**
        float destroyDelay = collectSound != null ? collectSound.length : 0.25f; // Default 1s if no sound
        StartCoroutine(WaitAndDestroy(destroyDelay));
    }

    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
