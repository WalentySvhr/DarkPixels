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

    // Щоб скрині не спавнилися занадто близько одна до одної
    [SerializeField] private float minDistanceBetweenChests = 3f;

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

            // 2. Додаткова перевірка (найнадійніша): чи є в цій точці тайл підлоги?
            // Якщо у вас є окремий FloorTilemap, використовуйте його:
            // Vector3Int floorCell = floorTilemap.WorldToCell(potentialPos);
            // if (!floorTilemap.HasTile(floorCell)) continue;

            // 3. Перевірка стін
            Vector3Int cellPos = wallTilemap.WorldToCell(potentialPos);
            if (wallTilemap.HasTile(cellPos) || IsWallNearby(cellPos)) continue;

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

            // Якщо пройшли всі перевірки:
            GameObject prefab = chestPrefabs[Random.Range(0, chestPrefabs.Length)];
            Instantiate(prefab, potentialPos, Quaternion.identity, transform);
            spawnedChestPositions.Add(potentialPos);
            return true;
        }

        return false;
    }

    private bool IsWallNearby(Vector3Int cell)
    {
        // Перевіряємо сусідні клітинки, щоб скриня не "врізалася" в стіну візуально
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

    public void ClearChests()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedChestPositions.Clear();
    }
}