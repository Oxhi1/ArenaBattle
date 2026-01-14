using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class MainMenuUI : MonoBehaviour
{
    
    public AudioMixer masterMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    public void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene();
    }

    public void SetMusicVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
        masterMixer.SetFloat("MusicVolume", dB);
    }

    public void SetSFXVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
        masterMixer.SetFloat("SFXVolume", dB);
    }
}
