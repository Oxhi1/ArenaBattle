using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Rival References")]
    public Transform rivalTransform;
    public CharacterHealth rivalHealth;
    public SlowEffect rivalSlowEffect;

    [Header("Direction")]
    public DirectionController directionController;

    private CharacterHealth myHealth;

    [Header("Attack Ranges")]
    public float punchRange = 1.5f;
    public float punchAngle = 60f; // Punch için açı toleransı
    public float shotMaxRange = 10f;
    public float poisonRange = 3f;
    public float shockwaveRange = 6f;

    [Header("Damages")]
    public int punchDamage = 15;
    public int shotDamage = 10;
    public int poisonDamage = 8;
    public int shockwaveDamage = 20;

    [Header("Cooldowns")]
    public float punchCooldown = 0.7f;
    public float shotCooldown = 1f;
    public float poisonCooldown = 1.2f;
    public float shockwaveCooldown = 3f;

    private float punchTimer;
    private float shotTimer;
    private float poisonTimer;
    private float shockwaveTimer;

    [Header("Shot Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    [Header("Poison Effect")]
    public float poisonSlowPercentage = 0.3f; // %30 yavaşlatma
    public float poisonSlowDuration = 3f; // 3 saniye

    [Header("Audio")]
    public AudioClip punchSound;
    public AudioClip shotSound;
    public AudioClip poisonSound;
    public AudioClip shockwaveSound;

    private AudioSource audioSource;

    private void Awake()
    {
        myHealth = GetComponent<CharacterHealth>();
        myHealth.characterName = "Player";
        audioSource = GetComponent<AudioSource>();
        directionController = GetComponent<DirectionController>();

        if (directionController == null)
        {
            Debug.LogError("PlayerController: DirectionController bulunamadı!");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleCooldowns();
        HandleAttacks();
    }

    public float GetCooldownProgress(string attackName)
    {
        switch (attackName.ToLower())
        {
            case "punch":
                return Mathf.Clamp01(1f - (punchTimer / punchCooldown));
            case "shot":
                return Mathf.Clamp01(1f - (shotTimer / shotCooldown));
            case "poison":
                return Mathf.Clamp01(1f - (poisonTimer / poisonCooldown));
            case "shockwave":
                return Mathf.Clamp01(1f - (shockwaveTimer / shockwaveCooldown));
            default:
                return 1f;
        }
    }

    void HandleMovement()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;

        Vector2 dir = new Vector2(h, v).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    void HandleCooldowns()
    {
        punchTimer -= Time.deltaTime;
        shotTimer -= Time.deltaTime;
        poisonTimer -= Time.deltaTime;
        shockwaveTimer -= Time.deltaTime;
    }
     void HandleAttacks()
    {
        // PUNCH - Sol Tık (Mouse 0) - Yakın mesafe, yönlü
        if (Input.GetMouseButtonDown(0) && punchTimer <= 0)
        {
            PerformPunch();
        }

        // SHOT - Sağ Tık (Mouse 1) - Mermi fırlatma
        if (Input.GetMouseButtonDown(1) && shotTimer <= 0)
        {
            PerformShot();
        }

        // POISON - E tuşu - Yavaşlatma + hasar
        if (Input.GetKeyDown(KeyCode.E) && poisonTimer <= 0)
        {
            PerformPoison();
        }

        // SHOCKWAVE - Q tuşu - 360° alan hasarı
        if (Input.GetKeyDown(KeyCode.Q) && shockwaveTimer <= 0)
        {
            PerformShockwave();
        }
    }

    /// <summary>
    /// PUNCH: Mouse yönüne göre yakın mesafe saldırısı
    /// </summary>
    void PerformPunch()
    {
        punchTimer = punchCooldown;
        PlaySound(punchSound);

        if (rivalTransform == null || rivalHealth == null) return;

        // Rival menzilde mi?
        float distance = Vector2.Distance(transform.position, rivalTransform.position);
        if (distance > punchRange)
        {
            Debug.Log("Punch: Rival menzil dışında!");
            return;
        }

        // Rival bakış yönünde mi?
        Vector2 currentDirection = directionController.GetDirection();
        Vector2 dirToRival = (rivalTransform.position - transform.position).normalized;
        float angle = Vector2.Angle(currentDirection, dirToRival);

        if (angle <= punchAngle / 2f)
        {
            rivalHealth.TakeDamage(punchDamage);
            Debug.Log("Punch HIT!");
        }
        else
        {
            Debug.Log($"Punch MISS! Açı: {angle:F1}° (Max: {punchAngle / 2f}°)");
        }
    }

    /// <summary>
    /// SHOT: Mermi fırlatma
    /// </summary>
    void PerformShot()
    {
        shotTimer = shotCooldown;
        PlaySound(shotSound);

        if (bulletPrefab == null)
        {
            Debug.LogError("Shot: Bullet Prefab atanmamış!");
            return;
        }

        // Mermiyi spawn et
        Vector3 spawnPos = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            Vector2 shootDirection = directionController.GetDirection();
            bullet.Initialize(shootDirection, shotDamage, "Rival");
        }

        Debug.Log("Shot fired!");
    }

    /// <summary>
    /// POISON: Yavaşlatma efekti + hasar
    /// </summary>
    void PerformPoison()
    {
        poisonTimer = poisonCooldown;
        PlaySound(poisonSound);

        if (rivalTransform == null || rivalHealth == null) return;

        float distance = Vector2.Distance(transform.position, rivalTransform.position);
        if (distance > poisonRange)
        {
            Debug.Log("Poison: Rival menzil dışında!");
            return;
        }

        // Hasar ver
        rivalHealth.TakeDamage(poisonDamage);

        // Yavaşlatma efekti uygula
        if (rivalSlowEffect != null)
        {
            rivalSlowEffect.ApplySlow(poisonSlowPercentage, poisonSlowDuration);
        }

        Debug.Log("Poison HIT!");
    }

    /// <summary>
    /// SHOCKWAVE: 360° alan hasarı
    /// </summary>
    void PerformShockwave()
    {
        shockwaveTimer = shockwaveCooldown;
        PlaySound(shockwaveSound);

        if (rivalTransform == null || rivalHealth == null) return;

        float distance = Vector2.Distance(transform.position, rivalTransform.position);
        if (distance <= shockwaveRange)
        {
            rivalHealth.TakeDamage(shockwaveDamage);
            Debug.Log("Shockwave HIT!");
        }
        else
        {
            Debug.Log("Shockwave MISS! Rival çok uzak.");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Debug için saldırı menzillerini göster
    private void OnDrawGizmosSelected()
    {
        // Punch range (cone)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, punchRange);

        // Poison range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, poisonRange);

        // Shockwave range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shockwaveRange);
    }
}