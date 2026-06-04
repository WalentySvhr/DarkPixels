using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Додано для списків

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonAreaSpawner : MonoBehaviour
{
    [Header("Master Switch")]
    protected bool isSpawningActive = true;

    [Header("Enemy Settings")]
    [Tooltip("Додай сюди префаби ворогів. Порожні слоти будуть проігноровані.")]
    public GameObject[] enemyPrefabs;
    public int maxEnemies = 15;

    [Header("Regular Spawn Timing")]
    public float minSpawnDelay = 10f;
    public float maxSpawnDelay = 30f;

    // === НОВЕ: НАЛАШТУВАННЯ ЗБЕРЕЖЕННЯ ТАЙМЕРА ===
    [Header("Long Respawn Save Settings")]
    [Tooltip("Увімкни це для елітних мобів/босів із довгим респавном (наприклад, 5 годин), щоб час кулдауну не скидався при перезапуску гри.")]
    public bool persistSpawnTimer = false;

    [Header("Long Break Settings")]
    public int deathsBeforeLongBreak = 100;
    public float minLongBreakMinutes = 2f;
    public float maxLongBreakMinutes = 5f;

    [Header("Boss Settings (Optional)")]
    [Tooltip("Залиш порожнім для відкритого світу")]
    public BossTrigger bossTrigger;

    // === НАЛАШТУВАННЯ ПЕРЕВІРКИ ПЕРЕШКОД ===
    [Header("Obstacle Avoidance Settings")]
    [Tooltip("Вибери тут шар, на якому знаходяться твої дерева/каміння (наприклад, Obstacles)")]
    public LayerMask obstacleLayer;
    [Tooltip("Радіус фізичного кола для перевірки вільного місця навколо точки")]
    public float spawnCheckRadius = 0.4f;

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

    // Ключ для збереження часу в PlayerPrefs (унікальний для кожного спавнера завдяки імені об'єкта)
    private string SaveKey => "Spawner_" + gameObject.name + "_RespawnTime";

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

        // === ОНОВЛЕНО: Перевірка збереженого таймера при старті гри ===
        if (persistSpawnTimer)
        {
            StartCoroutine(CheckSavedSpawnAndFill());
        }
        else
        {
            InitialFill();
        }
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

    // Корутина перевірки збереженого часу кулдауну
    IEnumerator CheckSavedSpawnAndFill()
    {
        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady()) yield return null;

        if (PlayerPrefs.HasKey(SaveKey))
        {
            long savedUnlockTime = long.Parse(PlayerPrefs.GetString(SaveKey));
            long currentTime = TimeManager.Instance.GetCurrentUnixTime();

            if (currentTime < savedUnlockTime)
            {
                // Час респавну ще не настав, моб залишається мертвим.
                // Update() сам запустить кулдаун на залишок часу.
                yield break;
            }
        }

        InitialFill();
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

        // === ОНОВЛЕНО: Логіка збереження/зчитування майбутнього часу спавну ===
        if (persistSpawnTimer)
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                long savedUnlockTime = long.Parse(PlayerPrefs.GetString(SaveKey));
                if (savedUnlockTime > TimeManager.Instance.GetCurrentUnixTime())
                {
                    spawnTime = savedUnlockTime;
                }
            }
            else
            {
                PlayerPrefs.SetString(SaveKey, spawnTime.ToString());
                PlayerPrefs.Save();
            }
        }

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

            // Моб успішно з'явився, видаляємо запис кулдауну
            if (persistSpawnTimer)
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
        }
        isSpawning = false;
    }

    void SpawnInPolygon()
    {
        if (spawnArea == null) return;

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

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("Spawner " + gameObject.name + " не має префабів ворогів!");
            return;
        }

        GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];

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
                Collider2D hitObstacle = Physics2D.OverlapCircle(randomPoint, spawnCheckRadius, obstacleLayer);

                if (hitObstacle == null)
                {
                    spawnPos = randomPoint;
                    validPositionFound = true;
                    break;
                }
            }
        }

        if (!validPositionFound) return;

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
        isSpawningActive = true;
        isOnLongBreak = false;

        currentEnemyCount = 0;
        totalDeathsInSession = 0;
        isSpawning = false;

        // === ОНОВЛЕНО: Очищення сейву таймера при примусовому рестарті спавнера ===
        if (persistSpawnTimer)
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        StopAllCoroutines();
        InitialFill();

        Debug.Log($"<color=orange>Спавнер {gameObject.name} успішно перезавантажено для поверху {TowerManager.Instance.currentFloor}!</color>");
    }
}