using UnityEngine;

#if ! UNITY_WEBGL || UNITY_EDITOR
using System.IO;
#endif

public class AITrainer : MonoBehaviour
{
    [Header("References")]
    public RivalAI rivalAI;

    [Header("Training Settings")]
    public int saveIntervalSeconds = 60;
    public bool autoSave = true;

    [Header("Save Settings")]
    public string saveFileName = "rival_ai_weights.json";

    private float saveTimer;

#if UNITY_WEBGL && ! UNITY_EDITOR
    private void Start()
    {
        Debug.LogWarning("AITrainer is disabled on WebGL builds.");
        gameObject.SetActive(false);
    }
#else
    private void Start()
    {
        saveTimer = saveIntervalSeconds;

        if (rivalAI == null)
        {
            rivalAI = FindObjectOfType<RivalAI>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveWeights();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadWeights();
        }

        if (autoSave)
        {
            saveTimer -= Time.deltaTime;
            if (saveTimer <= 0f)
            {
                saveTimer = saveIntervalSeconds;
                SaveWeights();
            }
        }
    }

    public void SaveWeights()
    {
        if (rivalAI == null) return;

        string resourcesPath = Path.Combine(Application.dataPath, "Resources", "AI");

        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        string fullPath = Path.Combine(resourcesPath, saveFileName);
        rivalAI.SaveQTable(fullPath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void LoadWeights()
    {
        if (rivalAI == null) return;

        string resourcesPath = Path.Combine(Application.dataPath, "Resources", "AI", saveFileName);

        if (File.Exists(resourcesPath))
        {
            rivalAI.LoadQTable(resourcesPath);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Box("AI Training (Editor Only)");
        GUILayout.Label("F5 - Save | F9 - Load");
        GUILayout.EndArea();
    }
#endif
}