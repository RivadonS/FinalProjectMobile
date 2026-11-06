using UnityEngine;

public class EnemyAttackHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    private float nextAttackTime;

    // ทำงานเมื่อมอนสเตอร์มี Collider 2D แบบกายภาพแข็ง (ไม่ได้ติ๊ก Is Trigger)
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckAndDealDamage(collision.gameObject);
    }

    // ทำงานเมื่อมอนสเตอร์ติ๊ก Is Trigger บน Collider 2D
    private void OnTriggerStay2D(Collider2D other)
    {
        CheckAndDealDamage(other.gameObject);
    }

    private void CheckAndDealDamage(GameObject targetObj)
    {
        if (Time.time >= nextAttackTime && targetObj.CompareTag("Player"))
        {
            // ดึง IDamageable จากตัว Player หรือ Parent
            IDamageable playerHp = targetObj.GetComponentInParent<IDamageable>();

            if (playerHp != null)
            {
                playerHp.TakeDamage(attackDamage);
                Debug.Log($"<color=red>[Enemy Attack 2D]</color> มอนสเตอร์โจมตี Player ดาเมจ: {attackDamage}");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }
}