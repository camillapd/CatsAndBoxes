using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    public AudioSource audioSource;

    public AudioClip waterSplash;
    public AudioClip footsteps;
    public AudioClip catOnBox;
    public AudioClip buttonClick;
    public AudioClip winLevel;
    public AudioClip loseLevel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClick);
    }
}
