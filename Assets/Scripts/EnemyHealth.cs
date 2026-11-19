using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Score Settings")]
    [SerializeField] private int scoreReward = 100; // คะแนนที่จะได้เมื่อฆ่าตัวนี้ (ปรับแยกตาม Prefab ได้)

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; // ถ้าตายแล้ว ไม่รับดาเมจซ้ำ

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} defeated!");

        // ส่งคะแนนและนับจำนวน Kill เข้า ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddKill(scoreReward);
        }

        Destroy(gameObject);
    }
}