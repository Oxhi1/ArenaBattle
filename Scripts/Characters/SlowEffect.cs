using UnityEngine;

/// <summary>
/// Karakterlere yavaşlatma efekti uygular.
/// Poison saldırısı için kullanılır.
/// </summary>
public class SlowEffect : MonoBehaviour
{
    [Header("Current Slow")]
    public bool isSlowed = false;
    public float slowPercentage = 0f; // 0 ile 1 arası (0.3 = %30 yavaşlama)

    private float slowTimer = 0f;
    private float originalMoveSpeed;
    private bool hasOriginalSpeed = false;

    /// <summary>
    /// Yavaşlatma efektini uygular
    /// </summary>
    /// <param name="percentage">Yavaşlama yüzdesi (0-1 arası, örn: 0.3 = %30)</param>
    /// <param name="duration">Efekt süresi (saniye)</param>
    public void ApplySlow(float percentage, float duration)
    {
        // İlk kez yavaşlatma uygulanıyorsa orijinal hızı kaydet
        if (!hasOriginalSpeed)
        {
            SaveOriginalSpeed();
        }

        slowPercentage = Mathf.Clamp01(percentage);
        slowTimer = duration;
        isSlowed = true;

        ApplySlowToSpeed();

        Debug.Log($"{gameObject.name} yavaşlatıldı! %{slowPercentage * 100} - {duration} saniye");
    }

    private void Update()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0f)
            {
                RemoveSlow();
            }
        }
    }

    /// <summary>
    /// Yavaşlatma efektini kaldırır
    /// </summary>
    private void RemoveSlow()
    {
        isSlowed = false;
        slowPercentage = 0f;
        slowTimer = 0f;

        RestoreOriginalSpeed();

        Debug.Log($"{gameObject.name} yavaşlatma efekti sona erdi!");
    }

    /// <summary>
    /// Orijinal hızı kaydeder
    /// </summary>
    private void SaveOriginalSpeed()
    {
        // Player veya Rival Controller'dan hızı al
        PlayerController player = GetComponent<PlayerController>();
        RivalController rival = GetComponent<RivalController>();

        if (player != null)
        {
            originalMoveSpeed = player.moveSpeed;
            hasOriginalSpeed = true;
        }
        else if (rival != null)
        {
            originalMoveSpeed = rival.moveSpeed;
            hasOriginalSpeed = true;
        }
    }

    /// <summary>
    /// Yavaşlatmayı hıza uygular
    /// </summary>
    private void ApplySlowToSpeed()
    {
        if (!hasOriginalSpeed) return;

        float newSpeed = originalMoveSpeed * (1f - slowPercentage);

        PlayerController player = GetComponent<PlayerController>();
        RivalController rival = GetComponent<RivalController>();

        if (player != null)
        {
            player.moveSpeed = newSpeed;
        }
        else if (rival != null)
        {
            rival.moveSpeed = newSpeed;
        }
    }

    /// <summary>
    /// Orijinal hızı geri yükler
    /// </summary>
    private void RestoreOriginalSpeed()
    {
        if (!hasOriginalSpeed) return;

        PlayerController player = GetComponent<PlayerController>();
        RivalController rival = GetComponent<RivalController>();

        if (player != null)
        {
            player.moveSpeed = originalMoveSpeed;
        }
        else if (rival != null)
        {
            rival.moveSpeed = originalMoveSpeed;
        }
    }

    /// <summary>
    /// Mevcut yavaşlatma çarpanını döndürür (1 = normal, 0.7 = %30 yavaş)
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return isSlowed ? (1f - slowPercentage) : 1f;
    }
}