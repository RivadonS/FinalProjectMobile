using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} defeated!");
        Destroy(gameObject);
    }
}