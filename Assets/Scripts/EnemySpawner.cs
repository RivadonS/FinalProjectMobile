using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Wave Settings")]
    [SerializeField] private int initialCount = 5;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int spawnPerWave = 2;
    [SerializeField] private int maxEnemies = 30;

    [Header("Distance Limits")]
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 12f;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        if (!playerTransform) playerTransform = FindAnyObjectByType<PlayerController>()?.transform;

        for (int i = 0; i < initialCount; i++) SpawnSingle();
        InvokeRepeating(nameof(SpawnWave), spawnInterval, spawnInterval);
    }

    private void SpawnWave()
    {
        if (!playerTransform || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        for (int i = 0; i < spawnPerWave && transform.childCount < maxEnemies; i++)
            SpawnSingle();
    }

    private void SpawnSingle()
    {
        if (!playerTransform || enemyPrefabs == null || enemyPrefabs.Length == 0 || transform.childCount >= maxEnemies) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector2 spawnPos = (Vector2)playerTransform.position + (Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance));
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
