using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 5;
    public Animator anim;

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (anim != null)
            anim.SetTrigger("Stun");
        if (health <= 0)
            Destroy(gameObject);
    }

    // Placeholder actions (no AI)
    public void PlayRoar() { if (anim != null) anim.SetTrigger("Roar"); }
    public void PlayShield() { if (anim != null) anim.SetTrigger("Shield"); }
}