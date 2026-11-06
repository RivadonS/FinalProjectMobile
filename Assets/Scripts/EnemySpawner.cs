using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Dynamic Scaling Settings")]
    [Tooltip("เวลา (วินาที) ที่จะไต่ระดับความยากจากเริ่มต้นไปจนถึงจุดสูงสุด เช่น 300 = 5 นาที")]
    [SerializeField] private float timeToMaxDifficulty = 300f;

    [Header("1. Spawn Rate Scaling (ความถี่การเกิด)")]
    [SerializeField] private float initialSpawnInterval = 6f; // เริ่มต้นเกิด 1 ครั้งทุก 6 วิ (ชิลๆ)
    [SerializeField] private float minSpawnInterval = 1.5f;   // ท้ายเกมเกิดถี่สุดทุก 1.5 วิ

    [Header("2. Max Enemies Scaling (โควตามอนสเตอร์บนจอ)")]
    [SerializeField] private int initialMaxEnemies = 5;       // เริ่มต้นมีมอนสเตอร์บนจอได้ไม่เกิน 5 ตัว
    [SerializeField] private int absoluteMaxEnemies = 30;     // ท้ายเกมมีได้สูงสุด 30 ตัว

    [Header("3. Spawn Per Wave Scaling (จำนวนมอนสเตอร์ต่อรอบ)")]
    [SerializeField] private int initialSpawnPerWave = 1;     // เริ่มต้นเกิดทีละ 1 ตัว
    [SerializeField] private int maxSpawnPerWave = 3;         // ท้ายเกมเกิดพร้อมกันทีละ 3 ตัว

    [Header("Distance Limits")]
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 12f;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private float elapsedTime = 0f;

    private void Start()
    {
        if (!playerTransform) playerTransform = FindAnyObjectByType<PlayerController>()?.transform;

        // เริ่มลูปสปอว์นมอนสเตอร์แบบ Dynamic
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        // สะสมเวลาการเล่นนับตั้งแต่เริ่มด่าน
        elapsedTime += Time.deltaTime;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // คำนวณเปอร์เซ็นต์ความยากตั้งแต่ 0.0 (เริ่มต้น) ถึง 1.0 (ยากสุด) ตามเวลาที่ผ่านไป
            float difficultyFactor = Mathf.Clamp01(elapsedTime / timeToMaxDifficulty);

            // ใช้ Mathf.Lerp ไต่ระดับค่าต่าง ๆ ตามเปอร์เซ็นต์ความยาก
            float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, difficultyFactor);
            int currentMaxEnemies = Mathf.RoundToInt(Mathf.Lerp(initialMaxEnemies, absoluteMaxEnemies, difficultyFactor));
            int currentSpawnPerWave = Mathf.RoundToInt(Mathf.Lerp(initialSpawnPerWave, maxSpawnPerWave, difficultyFactor));

            // สปอว์นมอนสเตอร์ตามจำนวนต่อรอบ (หากยังไม่เกินโควตาบนจอปัจจุบัน)
            for (int i = 0; i < currentSpawnPerWave; i++)
            {
                if (transform.childCount < currentMaxEnemies)
                {
                    SpawnSingle();
                }
            }

            // หน่วงเวลาการเกิดในรอบถัดไป ตามค่า currentInterval ที่ถูกคำนวณใหม่
            yield return new WaitForSeconds(currentInterval);
        }
    }

    private void SpawnSingle()
    {
        if (!playerTransform || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // สุ่มทิศทางและระยะทางรอบตัว Player (2D Vector)
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minDistance, maxDistance);
        Vector2 spawnPos = (Vector2)playerTransform.position + (randomDir * randomDist);

        Instantiate(prefab, spawnPos, Quaternion.identity, transform);
    }

    private void OnDrawGizmosSelected()
    {
        if (!playerTransform) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTransform.position, minDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(playerTransform.position, maxDistance);
    }
}