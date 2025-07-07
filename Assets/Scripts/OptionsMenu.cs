using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        ApplyVolume();
    }

    public void ApplyVolume()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.audioSource.volume = musicSlider.value;

        if (SFXManager.Instance != null)
            SFXManager.Instance.audioSource.volume = sfxSlider.value;

        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }
}