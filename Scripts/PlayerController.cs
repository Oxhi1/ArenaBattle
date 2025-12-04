using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static bool isPaused = false;

    public float speed = 5f;
    public float dashBoost = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    public AudioClip shootClip;

    Rigidbody2D rb;
    Vector2 move;
    int health = 5;

    AudioSettings audioSettings;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSettings = AudioSettings.Instance;
    }

    void Update()
    {
        if (isPaused) return;

        move.x = Input.GetAxis("Horizontal");
        move.y = Input.GetAxis("Vertical");

        if (Input.GetMouseButtonDown(0))
            Shoot();

        if (Input.GetKeyDown(KeyCode.Space))
            Dash();

        if (Input.GetKeyDown(KeyCode.Q))
            Heal();
    }

    void FixedUpdate()
    {
        if (isPaused) return;
        rb.linearVelocity = move * speed;
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            var b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Vector3 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - firePoint.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            b.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        
        if (AudioSettings.Instance != null && shootClip != null)
        {
            AudioSource sfx = AudioSettings.Instance.sfxSource;

            if (!sfx.enabled)
                sfx.enabled = true;  

            sfx.PlayOneShot(shootClip);
        }
    }

    void Dash()
    {
        rb.linearVelocity = rb.linearVelocity * dashBoost;
        Debug.Log("Dash atıldı!");
    }

    void Heal()
    {
        health++;
        Debug.Log("Heal basıldı! Yeni Can: " + health); 
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Hasar alındı! Yeni Can: " + health);
    }
}
