using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Додано для списків

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonAreaSpawner : MonoBehaviour
{
    [Header("Master Switch")]
    protected bool isSpawningActive = true;

    [Header("Enemy Settings")]
    // НОВЕ: Замість одного префаба, тепер тут масив.
    [Tooltip("Додай сюди префаби ворогів. Порожні слоти будуть проігноровані.")]
    public GameObject[] enemyPrefabs;
    public int maxEnemies = 15;

    [Header("Regular Spawn Timing")]
    public float minSpawnDelay = 10f;
    public float maxSpawnDelay = 30f;

    [Header("Long Break Settings")]
    public int deathsBeforeLongBreak = 100;
    public float minLongBreakMinutes = 2f;
    public float maxLongBreakMinutes = 5f;

    [Header("Boss Settings (Optional)")]
    [Tooltip("Залиш порожнім для відкритого світу")]
    public BossTrigger bossTrigger;

    [Header("Optimization")]
    public float activationDistance = 30f;

    private PolygonCollider2D spawnArea;
    private int currentEnemyCount = 0;
    private int totalDeathsInSession = 0;
    private Transform playerTransform;
    private bool isSpawning = false;

    [HideInInspector]
    public bool isOnLongBreak = false;

    public AreaQuestManager questManager;

    void Awake()
    {
        spawnArea = GetComponent<PolygonCollider2D>();
        spawnArea.isTrigger = true;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (questManager != null)
        {
            deathsBeforeLongBreak = questManager.killsRequired;
        }

        InitialFill();
    }

    void Update()
    {
        if (!isSpawningActive || playerTransform == null || isOnLongBreak) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > activationDistance || currentEnemyCount >= maxEnemies) return;

        if (!isSpawning)
        {
            StartCoroutine(SpawnWithDelay());
        }
    }

    protected void InitialFill()
    {
        if (!isSpawningActive) return;

        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnInPolygon();
        }
    }

    IEnumerator SpawnWithDelay()
    {
        isSpawning = true;
        float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);

        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady()) yield return null;

        long spawnTime = TimeManager.Instance.GetCurrentUnixTime() + (long)randomDelay;

        while (TimeManager.Instance.GetCurrentUnixTime() < spawnTime)
        {
            if (!isSpawningActive)
            {
                isSpawning = false;
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }

        if (currentEnemyCount < maxEnemies && !isOnLongBreak && isSpawningActive)
        {
            SpawnInPolygon();
        }
        isSpawning = false;
    }

    void SpawnInPolygon()
    {
        if (spawnArea == null) return;

        // --- НОВЕ: Логіка вибору випадкового ворога ---
        // Спочатку збираємо всі НЕПОРОЖНІ префаби з масиву
        List<GameObject> validPrefabs = new List<GameObject>();
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            foreach (GameObject prefab in enemyPrefabs)
            {
                if (prefab != null)
                {
                    validPrefabs.Add(prefab);
                }
            }
        }

        // Якщо масив порожній або всі слоти пусті - відміняємо спавн
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("Spawner " + gameObject.name + " не має префабів ворогів!");
            return;
        }

        // Вибираємо випадковий префаб з валідних
        GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];
        // ----------------------------------------------

        Bounds bounds = spawnArea.bounds;
        Vector2 spawnPos = Vector2.zero;
        bool validPositionFound = false;

        for (int i = 0; i < 30; i++)
        {
            float randomXPos = Random.Range(bounds.min.x, bounds.max.x);
            float randomYPos = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPoint = new Vector2(randomXPos, randomYPos);

            if (spawnArea.OverlapPoint(randomPoint))
            {
                spawnPos = randomPoint;
                validPositionFound = true;
                break;
            }
        }

        if (!validPositionFound) return;

        // Інстанціюємо обраний префаб замість старого enemyPrefab
        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        currentEnemyCount++;

        if (bossTrigger != null)
        {
            bossTrigger.RegisterEnemy(enemy);
        }

        float randomFlip = (Random.value > 0.5f) ? 1f : -1f;
        enemy.transform.localScale = new Vector3(randomFlip, 1, 1);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null && ai.hpBarTransform != null)
        {
            ai.hpBarTransform.localScale = new Vector3(randomFlip, 1, 1);
        }

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

        if (totalDeathsInSession >= deathsBeforeLongBreak && !isOnLongBreak)
        {
            StartCoroutine(LongBreakRoutine());
        }
    }

    IEnumerator LongBreakRoutine()
    {
        isOnLongBreak = true;
        totalDeathsInSession = 0;

        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady()) yield return null;

        float breakDuration = Random.Range(minLongBreakMinutes * 60f, maxLongBreakMinutes * 60f);
        long unlockTime = TimeManager.Instance.GetCurrentUnixTime() + (long)breakDuration;

        while (TimeManager.Instance.GetCurrentUnixTime() < unlockTime)
        {
            yield return new WaitForSeconds(2f);
        }

        isOnLongBreak = false;
    }

    public void StopSpawningPermanently()
    {
        isSpawningActive = false;
        Debug.Log($"<color=red>Спавнер {gameObject.name} назавжди вимкнено (прийшов бос)!</color>");
    }

    private void OnDrawGizmos()
    {
        if (spawnArea == null) spawnArea = GetComponent<PolygonCollider2D>();
        if (spawnArea != null)
        {
            Gizmos.color = isOnLongBreak ? new Color(1, 0, 0, 0.2f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }

    public void RestartSpawner()
    {
        // 1. Вмикаємо можливість спавну (якщо вона була вимкнена босом)
        isSpawningActive = true;
        isOnLongBreak = false;

        // 2. Скидаємо лічильники
        currentEnemyCount = 0;
        totalDeathsInSession = 0;
        isSpawning = false;

        // 3. Зупиняємо всі поточні затримки, щоб вони не накладалися
        StopAllCoroutines();

        // 4. Заповнюємо рівень мобами знову
        InitialFill();

        Debug.Log($"<color=orange>Спавнер {gameObject.name} успішно перезавантажено для поверху {TowerManager.Instance.currentFloor}!</color>");
    }

}