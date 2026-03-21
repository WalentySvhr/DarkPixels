using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;    // Сюди перетягни свій префаб ворога
    public int enemyCount = 5;        // Скільки мобів створити
    public float spawnDelay = 1f;     // Затримка між спавном (опціонально)

    private BoxCollider2D spawnArea;

    void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < enemyCount; i++)
        {
            // Вибираємо випадкову точку всередині меж колайдера
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 spawnPosition = new Vector3(x, y, 0);

            // Створюємо ворога
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}