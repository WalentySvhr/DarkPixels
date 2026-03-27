using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float spawnDelay = 1f;

    // --- НОВІ ЗМІННІ ---
    [Header("Boss Settings")]
    public BossTrigger bossTrigger;   // Посилання на скрипт тригера боса
    // -------------------

    private BoxCollider2D spawnArea;

    void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();

        // Якщо забув перетягнути BossTrigger в інспекторі, спробуємо знайти його самі
        if (bossTrigger == null) bossTrigger = GetComponent<BossTrigger>();

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < enemyCount; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 spawnPosition = new Vector3(x, y, 0);

            // Створюємо ворога
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // --- НОВИЙ РЯДОК ---
            // Реєструємо кожного ворога в тригері боса
            if (bossTrigger != null)
            {
                bossTrigger.RegisterEnemy(enemy);
            }
            // -------------------
        }
    }
}