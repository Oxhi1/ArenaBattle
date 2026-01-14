using UnityEngine;

/// <summary>
/// Karakterlerin baktığı yönü yöneten script.
/// Player için mouse pozisyonunu, Rival için Player pozisyonunu takip eder.
/// </summary>
public class DirectionController : MonoBehaviour
{
    [Header("Direction Settings")]
    public bool isPlayer = true; // Player mı yoksa Rival mı?
    public bool useAIControl = false; // AI tarafından kontrol edilecek mi?
    public Transform targetTransform; // Rival için Player'ın transform'u

    [Header("Visual Feedback")]
    public Transform directionIndicator; // Yön göstergesi (ok sprite'ı)
    public float indicatorDistance = 0.5f; // Karakterden ne kadar uzakta görünsün

    // Mevcut bakış yönü (normalized)
    private Vector2 currentDirection = Vector2.right;

    // Mouse pozisyonu için kamera referansı
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Eğer direction indicator yoksa uyarı ver
        if (directionIndicator == null)
        {
            Debug.LogWarning($"{gameObject.name}: Direction Indicator atanmadı!");
        }
    }

    private void Update()
    {
        UpdateDirection();
        UpdateVisualIndicator();
    }

    /// <summary>
    /// Karakterin bakış yönünü günceller
    /// </summary>
    private void UpdateDirection()
    {
        // AI kontrolündeyse, AI yönü set edecek - otomatik güncelleme yapma
        if (useAIControl)
        {
            return;
        }

        if (isPlayer)
        {
            // Player için: Mouse pozisyonuna bak
            UpdateDirectionToMouse();
        }
        else
        {
            // Rival için manuel kontrol: Player'a bak
            UpdateDirectionToTarget();
        }
    }

    /// <summary>
    /// Mouse pozisyonuna göre yön hesapla
    /// </summary>
    private void UpdateDirectionToMouse()
    {
        if (mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 directionToMouse = (mouseWorldPos - transform.position).normalized;

        if (directionToMouse.magnitude > 0.1f) // Çok küçük değerleri filtrele
        {
            currentDirection = directionToMouse;
        }
    }

    /// <summary>
    /// Target'a (Player) göre yön hesapla
    /// </summary>
    private void UpdateDirectionToTarget()
    {
        if (targetTransform == null) return;

        Vector2 directionToTarget = (targetTransform.position - transform.position).normalized;

        if (directionToTarget.magnitude > 0.1f)
        {
            currentDirection = directionToTarget;
        }
    }

    /// <summary>
    /// Görsel yön göstergesini güncelle
    /// </summary>
    private void UpdateVisualIndicator()
    {
        if (directionIndicator == null) return;

        // Göstergeyi karakterin etrafında konumlandır
        Vector3 indicatorPos = transform.position + (Vector3)currentDirection * indicatorDistance;
        directionIndicator.position = indicatorPos;

        // Göstergeyi yönüne göre döndür
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        directionIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Mevcut bakış yönünü döndürür (normalized)
    /// </summary>
    public Vector2 GetDirection()
    {
        return currentDirection;
    }

    /// <summary>
    /// AI tarafından yön set etmek için (Q-Learning için)
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.magnitude > 0.1f)
        {
            currentDirection = newDirection.normalized;
        }
    }

    /// <summary>
    /// Belirli bir pozisyona olan yönü döndürür
    /// </summary>
    public Vector2 GetDirectionToPosition(Vector3 targetPos)
    {
        return (targetPos - transform.position).normalized;
    }

    /// <summary>
    /// Belirli bir pozisyonun bakış yönünde olup olmadığını kontrol eder
    /// </summary>
    /// <param name="targetPos">Kontrol edilecek pozisyon</param>
    /// <param name="angleThreshold">Açı toleransı (derece)</param>
    public bool IsPositionInDirection(Vector3 targetPos, float angleThreshold = 45f)
    {
        Vector2 dirToTarget = GetDirectionToPosition(targetPos);
        float angle = Vector2.Angle(currentDirection, dirToTarget);
        return angle <= angleThreshold;
    }

    /// <summary>
    /// Debug için yön çizgisi çiz
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayer ? Color.blue : Color.red;
        Gizmos.DrawRay(transform.position, currentDirection * 2f);
    }
}