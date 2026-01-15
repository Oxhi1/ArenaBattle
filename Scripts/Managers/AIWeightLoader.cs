using UnityEngine;

public class AIWeightLoader : MonoBehaviour
{
    public static AIWeightLoader Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Resources klasörü içindeki yol (uzantısız)")]
    public string weightFilePath = "AI/rival_ai_weights";

    public bool IsLoaded { get; private set; } = false;

    // Event - yükleme tamamlandığında tetiklenir
    public System.Action<bool> OnLoadComplete;

    private void Awake()
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

    /// <summary>
    /// AI ağırlıklarını Resources klasöründen yükle (JSON)
    /// WebGL dahil tüm platformlarda çalışır
    /// </summary>
    public void LoadWeights(string resourcePath = null)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            resourcePath = weightFilePath;
        }

        try
        {
            // Resources. Load WebGL'de senkron çalışır ve sorunsuz çalışır
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

            if (jsonFile != null)
            {
                string jsonContent = jsonFile.text;

                if (string.IsNullOrEmpty(jsonContent))
                {
                    Debug.LogError("AI weight file is empty!");
                    IsLoaded = false;
                    OnLoadComplete?.Invoke(false);
                    return;
                }

                // RivalAI'a JSON string olarak gönder
                bool success = RivalAI.LoadWeightsFromJson(jsonContent);
                IsLoaded = success;

                Debug.Log(success
                    ? $"✓ AI weights loaded from Resources/{resourcePath}. json"
                    : "✗ AI weight parse failed!");
            }
            else
            {
                Debug.LogWarning($"✗ AI weight file not found at: Resources/{resourcePath}. json\n" +
                                 "Make sure the file exists at:  Assets/Resources/AI/rival_ai_weights.json");
                IsLoaded = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ Error loading AI weights: {e.Message}");
            IsLoaded = false;
        }

        OnLoadComplete?.Invoke(IsLoaded);
    }

    /// <summary>
    /// Ağırlıkları sıfırla
    /// </summary>
    public void ClearWeights()
    {
        RivalAI.ResetWeights();
        IsLoaded = false;
        OnLoadComplete?.Invoke(false);
        Debug.Log("AI weights cleared");
    }
}