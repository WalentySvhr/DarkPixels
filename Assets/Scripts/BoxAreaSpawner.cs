using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxAreaSpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 15;

    [Header("Regular Spawn Timing")]
    [Tooltip("Мінімальна затримка між спавном окремих мобів")]
    public float minSpawnDelay = 10f;
    [Tooltip("Максимальна затримка між спавном окремих мобів")]
    public float maxSpawnDelay = 30f;

    [Header("Long Break Settings")]
    [Tooltip("Після якої кількості вбитих ворогів настане довга пауза?")]
    public int deathsBeforeLongBreak = 100;
    [Tooltip("Мінімальний час довгої паузи (у секундах)")]
    public float minLongBreakMinutes = 2f;
    [Tooltip("Максимальний час довгої паузи (у секундах)")]
    public float maxLongBreakMinutes = 5f;

    [Header("Optimization")]
    public float activationDistance = 30f;

    private BoxCollider2D spawnArea;
    private int currentEnemyCount = 0;
    private int totalDeathsInSession = 0; // Лічильник смертей
    private Transform playerTransform;
    private bool isSpawning = false;
    private bool isOnLongBreak = false;
    public AreaQuestManager questManager;

    void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        spawnArea.isTrigger = true;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        InitialFill();
    }

    void Update()
    {
        if (playerTransform == null || isOnLongBreak) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > activationDistance || currentEnemyCount >= maxEnemies) return;

        if (!isSpawning)
        {
            StartCoroutine(SpawnWithDelay());
        }
    }

    void InitialFill()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnInBox();
        }
    }

    IEnumerator SpawnWithDelay()
    {
        isSpawning = true;
        float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
        yield return new WaitForSeconds(randomDelay);

        if (currentEnemyCount < maxEnemies && !isOnLongBreak)
        {
            SpawnInBox();
        }
        isSpawning = false;
    }

    void SpawnInBox()
    {
        if (spawnArea == null) return;

        Bounds bounds = spawnArea.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        Vector2 spawnPos = new Vector2(randomX, randomY);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemyCount++;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null) health.mySpawner = this;
    }

    public void EnemyDied()
    {
        currentEnemyCount--;
        totalDeathsInSession++;
        if (questManager != null)
        {
            questManager.OnEnemyKilled();
        }

        Debug.Log($"Ворог убитий у зоні {gameObject.name}. Усього вбито: {totalDeathsInSession}/{deathsBeforeLongBreak}");

        // Перевірка на довгу паузу
        if (totalDeathsInSession >= deathsBeforeLongBreak)
        {
            StartCoroutine(LongBreakRoutine());
        }
    }

    IEnumerator LongBreakRoutine()
    {
        isOnLongBreak = true;
        totalDeathsInSession = 0; // Скидаємо лічильник для наступного циклу

        float breakDuration = Random.Range(minLongBreakMinutes * 60f, maxLongBreakMinutes * 60f);
        Debug.Log($"<color=yellow>Зона {gameObject.name} зачищена! Довга пауза на {breakDuration / 60f:F1} хв.</color>");

        yield return new WaitForSeconds(breakDuration);

        isOnLongBreak = false;
        Debug.Log($"<color=green>Зона {gameObject.name} знову активна!</color>");
    }

    private void OnDrawGizmos()
    {
        if (spawnArea == null) spawnArea = GetComponent<BoxCollider2D>();
        if (spawnArea != null)
        {
            // Якщо пауза — малюємо червоним, якщо активна — зеленим
            Gizmos.color = isOnLongBreak ? new Color(1, 0, 0, 0.2f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(spawnArea.bounds.center, spawnArea.bounds.size);
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }
}