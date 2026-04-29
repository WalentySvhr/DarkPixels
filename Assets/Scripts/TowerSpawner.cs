using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class TowerSpawner : MonoBehaviour
{
    [Header("Master Switch")]
    public bool isSpawningActive = true;

    [Header("Spawn Area Settings")]
    [SerializeField] private CompositeCollider2D spawnArea;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Enemy Settings")]
    [Tooltip("Префаби для тестів, якщо немає TowerManager")]
    public GameObject[] defaultEnemyPrefabs;
    private int dynamicMaxEnemies = 10;

    [Header("Tower References")]
    public BossTrigger bossTrigger;
    public float activationDistance = 50f;

    [Header("Spawn Timing")]
    public float minSpawnDelay = 2f;
    public float maxSpawnDelay = 5f;

    private int currentEnemyCount = 0;
    private Transform playerTransform;
    private bool isSpawning = false;

    private List<GameObject> towerEnemies = new List<GameObject>();

    void Awake()
    {
        if (spawnArea == null) spawnArea = GetComponent<CompositeCollider2D>();
        if (bossTrigger == null) bossTrigger = GetComponent<BossTrigger>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // КРИТИЧНО: Якщо це поверх боса, звичайний спавн не працює
        if (IsBossFloorActive()) return;

        if (!isSpawningActive || playerTransform == null || currentEnemyCount >= dynamicMaxEnemies) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= activationDistance && !isSpawning)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    public void InitialFill()
    {
        if (!isSpawningActive) return;

        UpdateMaxEnemies();

        // Якщо ліміт ворогів 0 (поверх боса), InitialFill нічого не робить
        if (dynamicMaxEnemies <= 0) return;

        for (int i = 0; i < dynamicMaxEnemies; i++)
        {
            SpawnInPolygon();
        }
    }

    private void UpdateMaxEnemies()
    {
        if (TowerManager.Instance != null)
        {
            dynamicMaxEnemies = TowerManager.Instance.GetEnemiesCountForCurrentFloor();
            Debug.Log($"<color=cyan>Спавнер: ліміт ворогів = {dynamicMaxEnemies}</color>");
        }
    }

    private bool IsBossFloorActive()
    {
        return TowerManager.Instance != null && TowerManager.Instance.IsBossFloor();
    }

    IEnumerator SpawnRoutine()
    {
        isSpawning = true;
        yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

        if (isSpawningActive && currentEnemyCount < dynamicMaxEnemies && !IsBossFloorActive())
        {
            SpawnInPolygon();
        }
        isSpawning = false;
    }

    public void SpawnInPolygon()
    {
        if (spawnArea == null || wallTilemap == null) return;

        GameObject[] availablePrefabs;
        if (TowerManager.Instance != null)
        {
            availablePrefabs = TowerManager.Instance.GetAvailablePrefabs();
        }
        else
        {
            availablePrefabs = defaultEnemyPrefabs;
        }

        if (availablePrefabs == null || availablePrefabs.Length == 0) return;

        Bounds bounds = spawnArea.bounds;
        Vector2 spawnPos = Vector2.zero;
        bool found = false;

        for (int i = 0; i < 100; i++)
        {
            Vector2 p = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));

            if (spawnArea.OverlapPoint(p))
            {
                Vector3Int cellPos = wallTilemap.WorldToCell(p);
                if (!wallTilemap.HasTile(cellPos) && !IsWallNearby(cellPos))
                {
                    spawnPos = p;
                    found = true;
                    break;
                }
            }
        }

        if (!found) return;

        GameObject prefabToSpawn = availablePrefabs[Random.Range(0, availablePrefabs.Length)];
        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        currentEnemyCount++;
        towerEnemies.Add(enemy);

        if (bossTrigger != null) bossTrigger.RegisterEnemy(enemy);

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.towerSpawner = this;
            if (TowerManager.Instance != null)
            {
                float multiplier = TowerManager.Instance.GetDifficultyMultiplier();
                health.maxHealth *= multiplier;
                health.currentHealth = health.maxHealth;
            }
        }

        float randomFlip = (Random.value > 0.5f) ? 1f : -1f;
        enemy.transform.localScale = new Vector3(randomFlip, 1, 1);
    }

    private bool IsWallNearby(Vector3Int cell)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (wallTilemap.HasTile(new Vector3Int(cell.x + x, cell.y + y, cell.z)))
                    return true;
            }
        }
        return false;
    }

    public void EnemyDied(GameObject enemy)
    {
        currentEnemyCount--;
        if (towerEnemies.Contains(enemy)) towerEnemies.Remove(enemy);
    }

    public void ClearTowerEnemies()
    {
        foreach (GameObject enemy in towerEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        towerEnemies.Clear();
        currentEnemyCount = 0;
    }

    public void RestartSpawner()
    {
        isSpawningActive = true;
        StopAllCoroutines();
        isSpawning = false;
        ClearTowerEnemies();

        UpdateMaxEnemies();

        // Викликаємо наповнення тільки якщо це НЕ поверх боса
        if (!IsBossFloorActive())
        {
            InitialFill();
        }
    }

    public void StopSpawningPermanently()
    {
        isSpawningActive = false;
        StopAllCoroutines();
    }
}