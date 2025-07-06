using UnityEngine;

public class CatSFX : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip onPickSound;
    public AudioClip onScaredSound;
    public AudioClip onSeeMouseSound;

    public void PlayPickSound()
    {
        if (onPickSound != null)
            audioSource.PlayOneShot(onPickSound);
    }

    public void PlayScaredSound()
    {
        if (onScaredSound != null)
            audioSource.PlayOneShot(onScaredSound);
    }

    public void PlaySeeMouseSound()
    {
        if (onSeeMouseSound != null)
            audioSource.PlayOneShot(onSeeMouseSound);
    }
}
