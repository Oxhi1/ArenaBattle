using UnityEngine;

public class RivalController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Control Mode")]
    public bool useManualControl = false;

    [Header("Player References")]
    public Transform playerTransform;
    public CharacterHealth playerHealth;

    [Header("Direction")]
    public DirectionController directionController;

    private CharacterHealth myHealth;

    [Header("Attack Ranges")]
    public float punchRange = 1.5f;
    public float punchAngle = 60f; // Punch için açı toleransı
    public float heavyRange = 2f;
    public float heavyAngle = 60f;
    public float dashDistance = 3f; // Dash ile ne kadar ışınlanacak
    public float pierceRange = 10f; // Pierce maksimum menzil

    [Header("Damages")]
    public int punchDamage = 12;
    public int heavyDamage = 22;
    public int dashDamage = 14;
    public int pierceDamage = 9;

    [Header("Cooldowns")]
    public float punchCooldown = 1f;
    public float heavyCooldown = 3f;
    public float dashCooldown = 2f;
    public float pierceCooldown = 2.5f;

    public float punchTimer;
    public float heavyTimer;
    public float dashTimer;
    public float pierceTimer;

    [Header("Pierce Laser")]
    public LaserBeam laserBeam;

    [Header("Audio")]
    public AudioClip punchSound;
    public AudioClip heavySound;
    public AudioClip dashSound;
    public AudioClip pierceSound;

    private AudioSource audioSource;

    private void Awake()
    {
        punchTimer = 0f;
        heavyTimer = 0f;
        dashTimer = 0f;
        pierceTimer = 0f;

        myHealth = GetComponent<CharacterHealth>();
        myHealth.characterName = "Rival";

        audioSource = GetComponent<AudioSource>();
        directionController = GetComponent<DirectionController>();

        if (directionController == null)
        {
            Debug.LogError("RivalController: DirectionController bulunamadı!");
        }
    }

    void Update()
    {
        HandleCooldowns();

        if (!useManualControl) return;

        HandleMovement();
        HandleAttacks();
    }

    void HandleMovement()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1f;

        Vector2 dir = new Vector2(h, v).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    void HandleCooldowns()
    {
        punchTimer -= Time.deltaTime;
        heavyTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        pierceTimer -= Time.deltaTime;
    }

    void HandleAttacks()
    {
        // PUNCH - Numpad 1
        if (Input.GetKeyDown(KeyCode.Keypad1) && punchTimer <= 0)
        {
            PerformPunch();
        }

        // HEAVY - Numpad 2
        if (Input.GetKeyDown(KeyCode.Keypad2) && heavyTimer <= 0)
        {
            PerformHeavy();
        }

        // DASH - Numpad 3
        if (Input.GetKeyDown(KeyCode.Keypad3) && dashTimer <= 0)
        {
            PerformDash();
        }

        // PIERCE - Numpad 4
        if (Input.GetKeyDown(KeyCode.Keypad4) && pierceTimer <= 0)
        {
            PerformPierce();
        }
    }

    /// <summary>
    /// PUNCH: Player yönüne göre yakın mesafe saldırısı
    /// </summary>
    public void PerformPunch()
    {
        punchTimer = punchCooldown;
        PlaySound(punchSound);

        if (playerTransform == null || playerHealth == null) return;

        // Player menzilde mi?
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance > punchRange)
        {
            Debug.Log("Rival Punch: Player menzil dışında!");
            return;
        }

        // Player bakış yönünde mi?
        Vector2 currentDirection = directionController.GetDirection();
        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector2.Angle(currentDirection, dirToPlayer);

        if (angle <= punchAngle / 2f)
        {
            playerHealth.TakeDamage(punchDamage);
            Debug.Log("Rival Punch HIT!");
        }
        else
        {
            Debug.Log($"Rival Punch MISS! Açı: {angle:F1}° (Max: {punchAngle / 2f}°)");
        }
    }

    /// <summary>
    /// HEAVY: Güçlü yakın saldırı, player yönünde
    /// </summary>
    public void PerformHeavy()
    {
        heavyTimer = heavyCooldown;
        PlaySound(heavySound);

        if (playerTransform == null || playerHealth == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance > heavyRange)
        {
            Debug.Log("Rival Heavy: Player menzil dışında!");
            return;
        }

        Vector2 currentDirection = directionController.GetDirection();
        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector2.Angle(currentDirection, dirToPlayer);

        if (angle <= heavyAngle / 2f)
        {
            playerHealth.TakeDamage(heavyDamage);
            Debug.Log("Rival Heavy HIT!");
        }
        else
        {
            Debug.Log($"Rival Heavy MISS! Açı: {angle:F1}° (Max: {heavyAngle / 2f}°)");
        }
    }

    /// <summary>
    /// DASH: Rival'i 3 unit ışınlar (BAKIŞ YÖNÜNE - Player yönüne DEĞİL!)
    /// AI veya manuel kontrol hangi yöne bakarsa oraya dash atar
    /// </summary>
    public void PerformDash()
    {
        dashTimer = dashCooldown;
        PlaySound(dashSound);

        Vector2 dashDirection = directionController.GetDirection();
        Vector3 newPosition = transform.position + (Vector3)dashDirection * dashDistance;

        transform.position = newPosition;

        Debug.Log($"Rival Dashed! Direction: {dashDirection}, New Position: {newPosition}");
    }

    /// <summary>
    /// PIERCE: LineRenderer laser beam saldırısı
    /// </summary>
    public void PerformPierce()
    {
        pierceTimer = pierceCooldown;
        PlaySound(pierceSound);

        if (laserBeam == null)
        {
            Debug.LogError("Rival Pierce: LaserBeam component bulunamadı!");
            return;
        }

        // Player yönüne doğru laser ateşle
        Vector2 shootDirection = directionController.GetDirection();
        laserBeam.FireLaser(transform.position, shootDirection, "Player");

        Debug.Log("Rival Pierce fired!");
    }

    /// <summary>
    /// Mevcut cooldown durumunu döndürür
    /// </summary>
    public float GetCooldownProgress(string attackName)
    {
        switch (attackName.ToLower())
        {
            case "punch":
                return punchTimer <= 0 ? 1f : Mathf.Clamp01(1f - (punchTimer / punchCooldown));

            case "heavy":
                return heavyTimer <= 0 ? 1f : Mathf.Clamp01(1f - (heavyTimer / heavyCooldown));

            case "dash":
                return dashTimer <= 0 ? 1f : Mathf.Clamp01(1f - (dashTimer / dashCooldown));

            case "pierce":
                return pierceTimer <= 0 ? 1f : Mathf.Clamp01(1f - (pierceTimer / pierceCooldown));

            default:
                return 1f;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Debug için menzilleri göster
    private void OnDrawGizmosSelected()
    {
        // Punch range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, punchRange);

        // Heavy range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, heavyRange);

        // Dash direction
        if (directionController != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 dashDir = directionController.GetDirection();
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)dashDir * dashDistance);
        }
    }
}