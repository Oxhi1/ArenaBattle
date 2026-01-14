using UnityEngine;

/// <summary>
/// Shot saldırısı için mermi objesi.
/// Belirli yönde hareket eder ve hedefe çarptığında hasar verir.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public int damage = 10;
    public float lifetime = 3f; // Maksimum yaşam süresi

    [Header("Target")]
    public string targetTag = "Rival"; // Hangi tag'e hasar verecek
    public LayerMask targetLayers; // Hangi layer'lara çarpabilir (opsiyonel)

    private Vector2 direction;
    private float timer;

    /// <summary>
    /// Mermiyi başlatır
    /// </summary>
    public void Initialize(Vector2 shootDirection, int bulletDamage, string target)
    {
        direction = shootDirection.normalized;
        damage = bulletDamage;
        targetTag = target;
        timer = lifetime;

        // Merminin görsel yönünü ayarla
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        // Mermiyi hareket ettir
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Yaşam süresini azalt
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hedefe çarptı mı?
        if (collision.CompareTag(targetTag))
        {
            CharacterHealth health = collision.GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }

        // Duvara veya engele çarptı mı?
        if (collision.CompareTag("Wall") )
        {
            Destroy(gameObject);
        }
    }
}