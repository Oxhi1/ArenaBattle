using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player; // YENİ PlayerController
    public RivalController rival;
    public string attackName = "Punch"; // "Punch", "Shot", "Poison", "Shockwave"

    [Header("UI Elements")]
    public Image cooldownFillImage;
    public TextMeshProUGUI cooldownText;
    public KeyCode displayKey = KeyCode.U;

    [Header("Visual Settings")]
    public Color readyColor = Color.green;
    public Color cooldownColor = Color.red;

    private void Update()
    {
        float progress = 0f;

        // Player varsa player'dan, rival varsa rival'dan al
        if (player != null)
        {
            progress = player.GetCooldownProgress(attackName);
        }
        else if (rival != null)
        {
            progress = rival.GetCooldownProgress(attackName);
        }
        else
        {
            return; // İkisi de yoksa çık
        }

        // Fill Image güncelle
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = progress;
            cooldownFillImage.color = progress >= 1f ? readyColor : cooldownColor;
        }

        // Text güncelle
        if (cooldownText != null)
        {
            if (progress >= 1f)
            {
                cooldownText.text = $"{displayKey}\nHAZIR";
            }
            else
            {
                float timeRemaining = GetCooldownRemaining();
                cooldownText.text = $"{displayKey}\n{timeRemaining:F1}s";
            }
        }
    }

    private float GetCooldownRemaining()
    {
        float progress;
        float maxCooldown;

        // Player mı Rival mı kontrol et
        if (player != null)
        {
            progress = player.GetCooldownProgress(attackName);
            maxCooldown = GetMaxCooldownForPlayer();
        }
        else if (rival != null)
        {
            progress = rival.GetCooldownProgress(attackName);
            maxCooldown = GetMaxCooldownForRival();
        }
        else
        {
            return 0f;
        }

        return maxCooldown * (1f - progress);
    }

    private float GetMaxCooldownForPlayer()
    {
        switch (attackName.ToLower())
        {
            case "punch": return player.punchCooldown;
            case "shot": return player.shotCooldown;
            case "poison": return player.poisonCooldown;
            case "shockwave": return player.shockwaveCooldown;
            default: return 1f;
        }
    }

    private float GetMaxCooldownForRival()
    {
        switch (attackName.ToLower())
        {
            case "punch": return rival.punchCooldown;
            case "heavy": return rival.heavyCooldown;
            case "dash": return rival.dashCooldown;
            case "pierce": return rival.pierceCooldown;
            default: return 1f;
        }
    }
}