using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public Slider musicSlider;
    public Slider sfxSlider;

    AudioSettings audioSettings;

    void Start()
    {
        audioSettings = AudioSettings.Instance;

        if (musicSlider != null)
            musicSlider.value = audioSettings.musicSource.volume;

        if (sfxSlider != null)
            sfxSlider.value = audioSettings.sfxSource.volume;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        bool isActive = pauseMenu.activeSelf;

        pauseMenu.SetActive(!isActive);
        Time.timeScale = isActive ? 1f : 0f;
        PlayerController.isPaused = !isActive;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        PlayerController.isPaused = false;
    }

    public void SetMusic(float v)
    {
        if (audioSettings != null)
            audioSettings.SetMusic(v);
    }

    public void SetSFX(float v)
    {
        if (audioSettings != null)
            audioSettings.SetSFX(v);
    }
}
