using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer masterMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("AI Weight Loading")]
    public Button loadAIButton;
    public TextMeshProUGUI aiStatusText;
    public Button clearAIButton;

    [Header("AIWeightLoader Reference (Optional)")]
    [Tooltip("Sahnede AIWeightLoader yoksa otomatik oluşturulur")]
    public AIWeightLoader aiWeightLoaderPrefab;

    private void Awake()
    {
        // AIWeightLoader yoksa oluştur
        EnsureAIWeightLoaderExists();

        // Event'e AWAKE'de abone ol (Start'tan önce)
        if (AIWeightLoader.Instance != null)
        {
            AIWeightLoader.Instance.OnLoadComplete += OnAILoadComplete;
        }
    }

    private void Start()
    {
        // RivalAI'ı resetle
        RivalAI.ResetWeights();

        // UI'ı güncelle
        UpdateAIStatusText();
    }

    private void OnDestroy()
    {
        // Event'ten çık
        if (AIWeightLoader.Instance != null)
        {
            AIWeightLoader.Instance.OnLoadComplete -= OnAILoadComplete;
        }
    }

    /// <summary>
    /// AIWeightLoader'ın var olduğundan emin ol
    /// </summary>
    private void EnsureAIWeightLoaderExists()
    {
        if (AIWeightLoader.Instance == null)
        {
            AIWeightLoader existingLoader = FindObjectOfType<AIWeightLoader>();

            if (existingLoader == null)
            {
                GameObject loaderObj = new GameObject("AIWeightLoader");
                existingLoader = loaderObj.AddComponent<AIWeightLoader>();
                DontDestroyOnLoad(loaderObj);
                Debug.Log("AIWeightLoader automatically created");
            }
        }
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene();
    }

    /// <summary>
    /// Butona bağla - AI ağırlıklarını yükle
    /// </summary>
    public void LoadAIWeights()
    {
        EnsureAIWeightLoaderExists();

        if (AIWeightLoader.Instance != null)
        {
            // Önce "Yükleniyor" göster
            if (aiStatusText != null)
            {
                aiStatusText.text = "AI:  YÜKLENİYOR... ";
                aiStatusText.color = Color.white;
            }

            // Yükle
            AIWeightLoader.Instance.LoadWeights();

            // Senkron olduğu için hemen sonucu kontrol et ve güncelle
            UpdateAIStatusText();
        }
        else
        {
            Debug.LogError("AIWeightLoader.Instance is null!");
        }
    }

    /// <summary>
    /// Butona bağla - Ağırlıkları temizle
    /// </summary>
    public void ClearAIWeights()
    {
        if (AIWeightLoader.Instance != null)
        {
            AIWeightLoader.Instance.ClearWeights();
        }
        UpdateAIStatusText();
    }

    /// <summary>
    /// Yükleme tamamlandığında çağrılır (event callback)
    /// </summary>
    private void OnAILoadComplete(bool success)
    {
        Debug.Log($"OnAILoadComplete called with success: {success}");
        UpdateAIStatusText();
    }

    private void UpdateAIStatusText()
    {
        if (aiStatusText == null)
        {
            Debug.LogWarning("aiStatusText is not assigned in Inspector!");
            return;
        }

        bool loaded = AIWeightLoader.Instance != null && AIWeightLoader.Instance.IsLoaded;

        Debug.Log($"UpdateAIStatusText - IsLoaded: {loaded}");

        if (loaded)
        {
            aiStatusText.text = "AI: EĞİTİLMİŞ ✓";
            aiStatusText.color = Color.green;
        }
        else
        {
            aiStatusText.text = "AI:  RASTGELE";
            aiStatusText.color = Color.yellow;
        }
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