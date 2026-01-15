using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    [Header("Audio (Optional)")]
    public AudioMixer masterMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isPaused = false;

    private void Update()
    {
        // ESC tuşuna basınca pause aç/kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Oyunu durdur
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Oyunu devam ettir
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // MainMenu
    }

    // Audio Sliderlar için (varsa)
    public void SetMusicVolume(float value)
    {
        if (masterMixer != null)
        {
            // Minimum değer kontrolü
            if (value <= 0.0001f)
            {
                masterMixer.SetFloat("MusicVolume", -80f); // Tam sessiz
            }
            else
            {
                float dB = Mathf.Log10(value) * 20f;
                masterMixer.SetFloat("MusicVolume", dB);
            }

            Debug.Log("Music Volume set to: " + value);
        }
    }

    public void SetSFXVolume(float value)
    {
        if (masterMixer != null)
        {
            // Minimum değer kontrolü
            if (value <= 0.0001f)
            {
                masterMixer.SetFloat("SFXVolume", -80f); // Tam sessiz
            }
            else
            {
                float dB = Mathf.Log10(value) * 20f;
                masterMixer.SetFloat("SFXVolume", dB);
            }

            Debug.Log("SFX Volume set to: " + value);
        }
    }
}