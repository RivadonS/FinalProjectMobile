using UnityEngine;

public class PlayerAttackHandler : MonoBehaviour
{
    [Header("Targeting Settings")]
    public float detectionRadius = 10f;
    public LayerMask enemyLayer;

    [Header("1. Unarmed Attack (โจมตีมือเปล่า)")]
    public float unarmedDamage = 10f;
    public float unarmedAttackRate = 1f; // จำนวนครั้งต่อวินาที
    public float unarmedRange = 3f;

    [Header("2. Weapon Attack (โจมตีด้วยอาวุธ)")]
    public bool hasWeapon = false;
    public float weaponDamage = 25f;
    public float weaponAttackRate = 1.5f;
    public float weaponRange = 5f;

    [Header("3. Skill Attack (สกิล AoE รอบตัว)")]
    public bool hasSkill = false;
    public float skillDamage = 40f;
    public float skillCooldown = 3f;

    private float nextWeaponAttackTime;
    private float nextSkillTime;
    private Transform currentTarget;

    private void Update()
    {
        FindNearestEnemy();

        // System Loop 1: อาวุธหลัก (ถ้าไม่มีอาวุธ จะใช้ Unarmed อัตโนมัติ)
        if (Time.time >= nextWeaponAttackTime)
        {
            if (hasWeapon)
            {
                ExecuteWeaponAttack();
                nextWeaponAttackTime = Time.time + (1f / weaponAttackRate);
            }
            else
            {
                ExecuteUnarmedAttack();
                nextWeaponAttackTime = Time.time + (1f / unarmedAttackRate);
            }
        }

        // System Loop 2: สกิลเสริม (ทำงานขนานกันทันทีถ้าติ๊ก hasSkill = true)
        if (hasSkill && Time.time >= nextSkillTime)
        {
            ExecuteSkillAttack();
            nextSkillTime = Time.time + skillCooldown;
        }
    }

    // ระบบสแกนหาศัตรู 2D ที่ใกล้ที่สุด
    private void FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D col in hits)
        {
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = col.transform;
            }
        }

        currentTarget = closestEnemy;
    }

    private void ExecuteUnarmedAttack()
    {
        if (currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= unarmedRange)
        {
            IDamageable damageable = currentTarget.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(unarmedDamage);
                Debug.Log($"<color=cyan>[Unarmed Punch]</color> ต่อย {currentTarget.name} ดาเมจ: {unarmedDamage}");
            }
        }
    }

    private void ExecuteWeaponAttack()
    {
        if (currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= weaponRange)
        {
            IDamageable damageable = currentTarget.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponDamage);
                Debug.Log($"<color=green>[Weapon Slash]</color> ฟัน {currentTarget.name} ดาเมจ: {weaponDamage}");
            }
        }
    }

    private void ExecuteSkillAttack()
    {
        // สกิลปล่อยคลื่นพลังระเบิด (AoE 2D) ใส่ศัตรูทั้งหมดในระยะตรวจจับ
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        int hitCount = 0;

        foreach (Collider2D col in hits)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(skillDamage);
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            Debug.Log($"<color=yellow>[Skill Nova 2D]</color> ระเบิดสกิลใส่ศัตรู {hitCount} ตัว ดาเมจตัวละ: {skillDamage}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // วาดวงกลมรัศมีตรวจจับ 2D ในหน้า Scene
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}