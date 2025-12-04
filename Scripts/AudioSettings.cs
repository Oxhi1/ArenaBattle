using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
        SetMusic(musicSource.volume);
        SetSFX(sfxSource.volume);
    }

    public void SetMusic(float vol)
    {
        if (musicSource != null)
            musicSource.volume = vol <= 0.01f ? 0f : vol;
    }

    public void SetSFX(float vol)
    {
        if (sfxSource != null)
            sfxSource.volume = vol <= 0.01f ? 0f : vol;
    }
}
