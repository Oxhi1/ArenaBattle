using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    public float maxDistance = 10f;
    public float beamDuration = 0.2f;
    public int damage = 9;
    public LayerMask hitLayers;
    public bool isPiercing = true; // Delip geçsin mi?

    private LineRenderer lineRenderer;
    private float timer;
    private bool isActive = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true;

        // Sorting Layer ayarı
        lineRenderer.sortingLayerName = "VFX";
        lineRenderer.sortingOrder = 100;

        // Materyal kontrolü (Pembe görünmemesi için)
        if (lineRenderer.material == null || lineRenderer.material.shader.name == "Sprites/Default")
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.darkRed;
        }
    }

    private void Update()
    {
        if (isActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) DeactivateBeam();
        }
    }

    public void FireLaser(Vector3 origin, Vector2 direction, string targetTag)
    {
        isActive = true;
        timer = beamDuration;
        lineRenderer.enabled = true;

        Vector3 endPoint = origin + (Vector3)direction * maxDistance;

        if (isPiercing)
        {
            // DELİP GEÇME: Yol üzerindeki her şeyi bulur
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance, hitLayers);

            foreach (var hit in hits)
            {
                ApplyDamage(hit.collider, targetTag);
            }
        }
        else
        {
            // TEK HEDEF: İlk çarptığı yerde durur
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, hitLayers);
            if (hit.collider != null)
            {
                endPoint = hit.point;
                ApplyDamage(hit.collider, targetTag);
            }
        }

        // Görsel Ayarlar (Z ekseni objeyle aynı seviyeye çekildi)
        float zPos = origin.z;
        lineRenderer.SetPosition(0, new Vector3(origin.x, origin.y, zPos));
        lineRenderer.SetPosition(1, new Vector3(endPoint.x, endPoint.y, zPos));
    }

    private void ApplyDamage(Collider2D collider, string targetTag)
    {
        if (collider.CompareTag(targetTag))
        {
            CharacterHealth health = collider.GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Hit {collider.name} for {damage} damage.");
            }
        }
    }

    private void DeactivateBeam()
    {
        isActive = false;
        lineRenderer.enabled = false;
    }
}