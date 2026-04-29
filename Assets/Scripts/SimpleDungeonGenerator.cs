using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FloorGenerator : MonoBehaviour
{
    [Header("Налаштування Tilemap")]
    public Tilemap floorTilemap;

    // Тепер це масив. В інспекторі можна вказати розмір і перетягнути кілька тайлів
    public TileBase[] floorTiles;

    [Header("Параметри форми")]
    [Range(10, 100)]
    public int iterations = 30;
    [Range(10, 200)]
    public int walkLength = 50;

    void Start()
    {
        GenerateFloor();
    }

    [ContextMenu("Generate New Floor")]
    public void GenerateFloor()
    {
        // Перевіряємо, чи масив не порожній
        if (floorTilemap == null || floorTiles == null || floorTiles.Length == 0)
        {
            Debug.LogError("Заповни посилання на Tilemap та хоча б один Тайл у масиві!");
            return;
        }

        floorTilemap.ClearAllTiles();

        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
        Vector2Int currentPos = Vector2Int.zero;

        // 1. Генеруємо форму (логіка Random Walk залишається такою ж)
        for (int i = 0; i < iterations; i++)
        {
            Vector2Int pathPos = currentPos;
            for (int j = 0; j < walkLength; j++)
            {
                pathPos += GetRandomDirection();
                positions.Add(pathPos);
            }

            List<Vector2Int> list = new List<Vector2Int>(positions);
            currentPos = list[Random.Range(0, list.Count)];
        }

        // 2. Малюємо тайли, вибираючи випадковий для кожної клітинки
        foreach (var pos in positions)
        {
            // Вибираємо випадковий індекс з масиву floorTiles
            TileBase randomTile = floorTiles[Random.Range(0, floorTiles.Length)];
            floorTilemap.SetTile((Vector3Int)pos, randomTile);
        }

        Debug.Log($"<color=yellow>Генерація завершена! Використано {positions.Count} тайлів.</color>");
    }

    private Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        return directions[Random.Range(0, directions.Length)];
    }
}