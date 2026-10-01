using System.Collections.Generic;
using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Arena Bounds")]
    [SerializeField] private Vector2 arenaSize = new Vector2(25f, 25f);
    [SerializeField] private Vector2 arenaCenter = Vector2.zero;

    [Header("Spawn Settings")]
    [SerializeField] private int minObstacleCount = 8;
    [SerializeField] private int maxObstacleCount = 15;
    [SerializeField] private float edgePadding = 1.2f;
    [SerializeField] private float safeRadiusFromPlayer = 3.5f;
    [SerializeField] private LayerMask avoidLayers;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private readonly List<(Vector2 pos, float radius)> spawnedList = new();

    private void Start()
    {
        if (!playerTransform) playerTransform = FindAnyObjectByType<PlayerController>()?.transform;
        SpawnObstacles();
    }

    public void SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        spawnedList.Clear();
        int targetCount = Random.Range(minObstacleCount, maxObstacleCount + 1);
        Vector2 playerPos = playerTransform ? (Vector2)playerTransform.position : Vector2.zero;
        float playerRadius = GetObjectRadius(playerTransform ? playerTransform.gameObject : null);

        int attempts = 0, maxAttempts = targetCount * 40;
        Vector2 half = arenaSize * 0.5f;

        while (spawnedList.Count < targetCount && attempts++ < maxAttempts)
        {
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            float radius = GetObjectRadius(prefab);

            Vector2 pos = new(
                Random.Range(arenaCenter.x - half.x + radius, arenaCenter.x + half.x - radius),
                Random.Range(arenaCenter.y - half.y + radius, arenaCenter.y + half.y - radius)
            );

            // DS&A Optimization: Squared distance checks (avoids square root operations in loop)
            float playerSafeDist = radius + playerRadius + safeRadiusFromPlayer;
            if (playerTransform && (pos - playerPos).sqrMagnitude < (playerSafeDist * playerSafeDist)) continue;
            if (IsOverlappingWithEdges(pos, radius)) continue;
            if (avoidLayers != 0 && Physics2D.OverlapCircle(pos, radius + (edgePadding * 0.5f), avoidLayers)) continue;

            Instantiate(prefab, pos, Quaternion.identity, transform);
            Physics2D.SyncTransforms();
            spawnedList.Add((pos, radius));
        }
    }

    private bool IsOverlappingWithEdges(Vector2 newPos, float newRadius)
    {
        // DS&A Optimization: Using sqrMagnitude for O(1) distance comparisons without Sqrt()
        foreach (var (pos, radius) in spawnedList)
        {
            float requiredClearance = newRadius + radius + edgePadding;
            if ((newPos - pos).sqrMagnitude < (requiredClearance * requiredClearance))
                return true;
        }
        return false;
    }

    private float GetObjectRadius(GameObject obj)
    {
        if (!obj) return 0.5f;
        var col = obj.GetComponentInChildren<Collider2D>();
        return col switch
        {
            CircleCollider2D c => c.radius * Mathf.Max(obj.transform.localScale.x, obj.transform.localScale.y),
            BoxCollider2D b => Vector2.Scale(b.size * 0.5f, obj.transform.localScale).magnitude,
            CapsuleCollider2D cp => Vector2.Scale(cp.size * 0.5f, obj.transform.localScale).magnitude,
            _ => col != null ? col.bounds.extents.magnitude : (obj.GetComponentInChildren<SpriteRenderer>()?.bounds.extents.magnitude ?? 0.75f)
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(arenaCenter, new Vector3(arenaSize.x, arenaSize.y, 0.1f));
        if (playerTransform)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, safeRadiusFromPlayer);
        }
    }
}
