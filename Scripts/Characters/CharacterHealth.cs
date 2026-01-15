using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    public string characterName = "Character";
    public int maxHealth = 100;
    public int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(characterName + " took " + amount + " damage! HP = " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log(characterName + " DIED!");
        }
    }
}
