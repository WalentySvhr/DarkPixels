using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ChestSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CompositeCollider2D spawnArea;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private GameObject[] chestPrefabs;

    [Header("Randomization")]
    [SerializeField] private int minChests = 1;
    [SerializeField] private int maxChests = 3;

    [Header("Distance Settings")]
    [SerializeField] private float minDistanceBetweenChests = 3f;

    [Header("Trap Settings (Пошук за назвою об'єкта)")]
    [Tooltip("Жорстка дистанція від пасток, яка ніколи не зменшується")]
    [SerializeField] private float safeDistanceFromTraps = 2.5f;

    [Header("Safe Zone Settings")]
    [Tooltip("Перетягни сюди об'єкт стартової точки з ієрархії (Spawn Point)")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Мінімальна відстань від цієї точки, ближче якої скрині НЕ з'являться")]
    [SerializeField] private float safeZoneRadius = 4.0f;

    private List<Vector2> spawnedChestPositions = new List<Vector2>();

    void Awake()
    {
        if (spawnArea == null) spawnArea = GetComponent<CompositeCollider2D>();
    }

    /// <summary>
    /// Викликайте цей метод з вашого LevelGenerator або TowerManager, коли створюється поверх
    /// </summary>
    public void SpawnChestsForFloor()
    {
        ClearChests();

        int chestsToSpawn = Random.Range(minChests, maxChests + 1);
        int attempts = 0;
        int spawnedCount = 0;

        // Обмежуємо спроби, щоб уникнути нескінченного циклу, якщо мало місця
        while (spawnedCount < chestsToSpawn && attempts < 100)
        {
            if (TrySpawnChest())
            {
                spawnedCount++;
            }
            attempts++;
        }

        Debug.Log($"<color=yellow>ChestSpawner: Розміщено {spawnedCount} скринь</color>");
    }

    private bool TrySpawnChest()
    {
        if (spawnArea == null || wallTilemap == null || chestPrefabs.Length == 0) return false;

        Bounds bounds = spawnArea.bounds;

        // Спробуємо знайти точку
        for (int i = 0; i < 30; i++) // Внутрішній цикл спроб для однієї скрині
        {
            Vector2 potentialPos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            // 1. Перевірка: чи точка ВЗАГАЛІ всередині фізичного колайдера підлоги?
            if (!spawnArea.OverlapPoint(potentialPos)) continue;

            // 2. Перевірка стін
            Vector3Int cellPos = wallTilemap.WorldToCell(potentialPos);
            if (wallTilemap.HasTile(cellPos) || IsWallNearby(cellPos)) continue;

            // 3. ЗМІНЕНО: Перевірка безпечної зони навколо заданої точки в інспекторі
            if (spawnPoint != null)
            {
                float distanceToSpawn = Vector2.Distance(potentialPos, spawnPoint.position);
                if (distanceToSpawn < safeZoneRadius)
                {
                    // Точка занадто близько до старту, пропускаємо
                    continue;
                }
            }

            // 4. Перевірка дистанції до інших скринь
            bool tooClose = false;
            foreach (Vector2 existingPos in spawnedChestPositions)
            {
                if (Vector2.Distance(potentialPos, existingPos) < minDistanceBetweenChests)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // 5. ПЕРЕВІРКА ЗА НАЗВОЮ: Чи є поруч пастки?
            if (IsTrapNearby(potentialPos, safeDistanceFromTraps))
            {
                continue; // Якщо поруч виявлено пастку, пропускаємо цю точку
            }

            // Якщо пройшли всі перевірки:
            GameObject prefab = chestPrefabs[Random.Range(0, chestPrefabs.Length)];
            Instantiate(prefab, potentialPos, Quaternion.identity, transform);
            spawnedChestPositions.Add(potentialPos);
            return true;
        }

        return false;
    }

    // Метод перевіряє відстань до всіх пасток на сцені, орієнтуючись на їхнє ім'я
    private bool IsTrapNearby(Vector2 targetPos, float radius)
    {
        Transform[] allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (Transform obj in allObjects)
        {
            string name = obj.name.ToLower();

            // Перевіряємо, чи містить назва об'єкта ключові слова пасток
            if (name.Contains("trap") || name.Contains("spike"))
            {
                float distance = Vector2.Distance(targetPos, obj.position);

                if (distance < radius)
                {
                    Debug.Log($"<color=orange>Спавн скрині скасовано: поруч пастка '{obj.name}' (Відстань: {distance:F2}м)</color>");
                    return true; // Пастка занадто близько
                }
            }
        }

        return false; // Поруч немає пасток
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

    public void ClearChests()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedChestPositions.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        // Малюємо зони безпеки від пасток червоним
        Gizmos.color = Color.red;
        foreach (Vector2 pos in spawnedChestPositions)
        {
            Gizmos.DrawWireSphere(pos, safeDistanceFromTraps);
        }

        // Візуалізуємо бірюзовим колом безпечну зону навколо нашої точки
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, safeZoneRadius);
        }
    }
}