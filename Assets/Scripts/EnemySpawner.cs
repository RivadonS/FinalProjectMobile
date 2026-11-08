using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication; // 1. เพิ่ม using สำหรับระบบล็อกอิน
using Unity.Services.Core.Environments;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Dynamic Scaling Settings")]
    [SerializeField] private float timeToMaxDifficulty = 300f;

    [Header("1. Spawn Rate Scaling")]
    [SerializeField] private float initialSpawnInterval = 6f;
    [SerializeField] private float minSpawnInterval = 1.5f;

    [Header("2. Max Enemies Scaling")]
    [SerializeField] private int initialMaxEnemies = 5;
    [SerializeField] private int absoluteMaxEnemies = 30;

    [Header("3. Spawn Per Wave Scaling")]
    [SerializeField] private int initialSpawnPerWave = 1;
    [SerializeField] private int maxSpawnPerWave = 3;

    [Header("Distance Limits")]
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 12f;

    [Header("Map Boundaries")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private float elapsedTime = 0f;

    public struct userAttributes { }
    public struct appAttributes { }

    private async void Awake()
    {
        try
        {
            // 1. สร้างตัวแปรระบุ Environment และสั่งเปิดใช้งาน Unity Services
            var options = new InitializationOptions();
            options.SetEnvironmentName("development"); // ระบุชื่อ Environment ให้ตรงกับหน้าเว็บ

            await UnityServices.InitializeAsync(options);

            // 2. ล็อกอินแบบไม่ระบุตัวตน (ป้องกัน Error Auth Service not initialized)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"<color=green>[Auth]</color> ล็อกอินสำเร็จ PlayerID: {AuthenticationService.Instance.PlayerId}");
            }

            // 3. ผูกฟังก์ชันรอรับข้อมูล
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;

            // 4. สั่งเริ่มดึงข้อมูลจาก Cloud
            await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Remote Config Error] เชื่อมต่อ Cloud ไม่สำเร็จ (ใช้ค่า Default): {e.Message}");
        }
    }

    private void ApplyRemoteSettings(ConfigResponse response)
    {
        bool isChristmasEvent = RemoteConfigService.Instance.appConfig.GetBool("is_christmas_event", false);

        if (isChristmasEvent)
        {
            int christmasEnemies = RemoteConfigService.Instance.appConfig.GetInt("christmas_max_enemies", absoluteMaxEnemies);
            absoluteMaxEnemies = christmasEnemies;
            Debug.Log($"<color=cyan>[Remote Config]</color> 🎄 Dark Christmas Event Active! ปรับมอนสเตอร์สูงสุดเป็น: {absoluteMaxEnemies} ตัว");
        }
        else
        {
            Debug.Log($"<color=cyan>[Remote Config]</color> ⚔️ โหมดปกติ (เทศกาลปิดอยู่)");
        }
    }

    private void Start()
    {
        if (!playerTransform) playerTransform = FindAnyObjectByType<PlayerController>()?.transform;
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float difficultyFactor = Mathf.Clamp01(elapsedTime / timeToMaxDifficulty);

            float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, difficultyFactor);
            int currentMaxEnemies = Mathf.RoundToInt(Mathf.Lerp(initialMaxEnemies, absoluteMaxEnemies, difficultyFactor));
            int currentSpawnPerWave = Mathf.RoundToInt(Mathf.Lerp(initialSpawnPerWave, maxSpawnPerWave, difficultyFactor));

            for (int i = 0; i < currentSpawnPerWave; i++)
            {
                if (transform.childCount < currentMaxEnemies)
                {
                    SpawnSingle();
                }
            }

            yield return new WaitForSeconds(currentInterval);
        }
    }

    private void SpawnSingle()
    {
        if (!playerTransform || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        Vector2 spawnPos = Vector2.zero;
        bool validPosFound = false;
        int maxAttempts = 10; 

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(minDistance, maxDistance);
            Vector2 candidatePos = (Vector2)playerTransform.position + (randomDir * randomDist);

            bool inBounds = candidatePos.x >= minX && candidatePos.x <= maxX &&
                            candidatePos.y >= minY && candidatePos.y <= maxY;

            bool overlapObstacle = false;
            if (obstacleLayer != 0)
            {
                overlapObstacle = Physics2D.OverlapCircle(candidatePos, 0.5f, obstacleLayer) != null;
            }

            if (inBounds && !overlapObstacle)
            {
                spawnPos = candidatePos;
                validPosFound = true;
                break;
            }
        }

        if (!validPosFound)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(minDistance, maxDistance);
            Vector2 candidatePos = (Vector2)playerTransform.position + (randomDir * randomDist);

            spawnPos.x = Mathf.Clamp(candidatePos.x, minX, maxX);
            spawnPos.y = Mathf.Clamp(candidatePos.y, minY, maxY);
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, spawnPos, Quaternion.identity, transform);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, minDistance);
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(playerTransform.position, maxDistance);
        }

        Gizmos.color = Color.green;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}