using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TrapSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TrapSettings
    {
        public string trapName;
        public GameObject trapPrefab;
        public int minCount = 2;
        public int maxCount = 5;

        [Header("Відступи")]
        [Tooltip("На скільки клітинок пастка повинна відступати від стін")]
        public int wallBuffer = 1;
    }

    [Header("Налаштування")]
    [SerializeField] private CompositeCollider2D spawnArea;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Налаштування пасток")]
    [SerializeField] private TrapSettings[] trapsToSpawn;

    [Tooltip("Мінімальна дистанція між пастками")]
    [SerializeField] private float minDistanceBetweenTraps = 2.5f;

    [Header("Безпечна зона")]
    [SerializeField] private Transform safeZoneCenter;
    [SerializeField] private float safeZoneRadius = 3.5f;

    private List<Vector2> spawnedTrapPositions = new List<Vector2>();

    void Awake()
    {
        if (spawnArea == null) spawnArea = GetComponent<CompositeCollider2D>();
    }

    public void SpawnTrapsForFloor()
    {
        ClearTraps();

        foreach (TrapSettings trap in trapsToSpawn)
        {
            if (trap.trapPrefab == null) continue;

            int count = Random.Range(trap.minCount, trap.maxCount + 1);
            int spawnedCount = 0;
            int attempts = 0;

            while (spawnedCount < count && attempts < 150)
            {
                if (TrySpawnTrap(trap)) // Передаємо весь об'єкт налаштувань
                {
                    spawnedCount++;
                }
                attempts++;
            }
        }
    }
    public void SetSafeZone(Transform center, float radius)
    {
        safeZoneCenter = center;
        safeZoneRadius = radius;
    }

    private bool TrySpawnTrap(TrapSettings settings)
    {
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < 50; i++)
        {
            Vector2 potentialPos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (!spawnArea.OverlapPoint(potentialPos)) continue;

            // 1. Перевірка стін з урахуванням індивідуального відступу (buffer)
            Vector3Int cellPos = wallTilemap.WorldToCell(potentialPos);
            if (IsWallNearby(cellPos, settings.wallBuffer)) continue;

            // 2. Безпечна зона
            if (safeZoneCenter != null && Vector2.Distance(potentialPos, safeZoneCenter.position) < safeZoneRadius)
                continue;

            // 3. Дистанція між пастками
            bool tooClose = false;
            foreach (Vector2 existingPos in spawnedTrapPositions)
            {
                if (Vector2.Distance(potentialPos, existingPos) < minDistanceBetweenTraps)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            Instantiate(settings.trapPrefab, potentialPos, Quaternion.identity, transform);
            spawnedTrapPositions.Add(potentialPos);
            return true;
        }
        return false;
    }

    // Оновлений метод з перевіркою радіуса (buffer)
    private bool IsWallNearby(Vector3Int cell, int buffer)
    {
        for (int x = -buffer; x <= buffer; x++)
        {
            for (int y = -buffer; y <= buffer; y++)
            {
                if (wallTilemap.HasTile(new Vector3Int(cell.x + x, cell.y + y, cell.z)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ClearTraps()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        spawnedTrapPositions.Clear();
    }
}