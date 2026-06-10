using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TrapSpawner : MonoBehaviour
{
    // Створюємо структуру, яка об'єднає префаб та його особисті ліміти
    [System.Serializable]
    public class TrapSettings
    {
        public string trapName; // Для зручного підпису елемента в Інспекторі
        public GameObject trapPrefab;
        public int minCount = 2;
        public int maxCount = 5;
    }

    [Header("Налаштування")]
    [SerializeField] private CompositeCollider2D spawnArea;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Налаштування пасток")]
    [Tooltip("Додавайте сюди різні типи пасток та налаштовуйте межі кількості для кожної з них")]
    [SerializeField] private TrapSettings[] trapsToSpawn;

    [Tooltip("Щоб пастки не спавнилися одна на одній")]
    [SerializeField] private float minDistanceBetweenTraps = 2.5f;

    [Header("Безпечна зона (Щоб не спавнити під ногами гравця)")]
    [Tooltip("Сюди можна перетягнути точку FloorEntryPoint з вашого EnvironmentTier")]
    [SerializeField] private Transform safeZoneCenter;
    [SerializeField] private float safeZoneRadius = 3.5f;

    private List<Vector2> spawnedTrapPositions = new List<Vector2>();

    void Awake()
    {
        if (spawnArea == null) spawnArea = GetComponent<CompositeCollider2D>();
    }

    /// <summary>
    /// Викликайте цей метод з TowerManager при генерації/оновленні поверху
    /// </summary>
    public void SpawnTrapsForFloor()
    {
        ClearTraps();

        int totalSpawnedCount = 0;

        // По черзі проходимо по кожному налаштованому типу пастки
        foreach (TrapSettings trap in trapsToSpawn)
        {
            if (trap.trapPrefab == null) continue;

            // Вираховуємо рандомну кількість саме для цього префабу
            int trapsToSpawnForThisType = Random.Range(trap.minCount, trap.maxCount + 1);
            int attempts = 0;
            int spawnedCountForThisType = 0;

            // Намагаємося заспавнити задану кількість пасток цього типу
            while (spawnedCountForThisType < trapsToSpawnForThisType && attempts < 100)
            {
                if (TrySpawnTrap(trap.trapPrefab))
                {
                    spawnedCountForThisType++;
                    totalSpawnedCount++;
                }
                attempts++;
            }

            Debug.Log($"<color=yellow>TrapSpawner: Розміщено {spawnedCountForThisType}/{trapsToSpawnForThisType} пасток типу [{trap.trapName}].</color>");
        }

        Debug.Log($"<color=red>TrapSpawner: Всього згенеровано {totalSpawnedCount} пасток на поверсі.</color>");
    }

    // Метод для динамічного встановлення безпечної зони під час зміни данжу
    public void SetSafeZone(Transform center, float radius)
    {
        safeZoneCenter = center;
        safeZoneRadius = radius;
    }

    private bool TrySpawnTrap(GameObject prefab)
    {
        if (spawnArea == null || wallTilemap == null || prefab == null) return false;

        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < 30; i++)
        {
            Vector2 potentialPos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            // 1. Чи точка взагалі на підлозі?
            if (!spawnArea.OverlapPoint(potentialPos)) continue;

            // 2. Перевірка стін поруч
            Vector3Int cellPos = wallTilemap.WorldToCell(potentialPos);
            if (wallTilemap.HasTile(cellPos) || IsWallNearby(cellPos)) continue;

            // 3. Перевірка безпечної зони гравця
            if (safeZoneCenter != null)
            {
                if (Vector2.Distance(potentialPos, safeZoneCenter.position) < safeZoneRadius)
                {
                    continue; // Занадто близько до спавну гравця, шукаємо інше місце
                }
            }

            // 4. Перевірка дистанції до вже створених пасток (всіх типів разом)
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

            // Якщо все ок — створюємо саме ту пастку, яку передали в метод
            Instantiate(prefab, potentialPos, Quaternion.identity, transform);
            spawnedTrapPositions.Add(potentialPos);
            return true;
        }

        return false;
    }

    private bool IsWallNearby(Vector3Int cell)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkPos = new Vector3Int(cell.x + x, cell.y + y, cell.z);
                if (wallTilemap.HasTile(checkPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ClearTraps()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedTrapPositions.Clear();
    }
}