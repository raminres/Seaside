using System.Collections;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;
    private bool isCollected = false;

    public void Initialize(AudioClip clip)
    {
        animator = GetComponent<Animator>();

        // Add an AudioSource if not already attached
        audioSource = gameObject.AddComponent<AudioSource>();
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
        GameManager.Instance.CollectItem(); // Update counter
        animator.SetTrigger("Collect"); // Play collect animation
        audioSource.Play(); // Play sound

        StartCoroutine(WaitAndDestroy(audioSource.clip.length));
    }

    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}