using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Long Respawn Save Settings")]
    [Tooltip("Увімкни це для елітних мобів/босів із довгим респавном (наприклад, 5 годин), щоб час кулдауну не скидався при перезапуску гри.")]
    public bool persistSpawnTimer = false;

    [Header("Long Break Settings")]
    public int deathsBeforeLongBreak = 100;
    public float minLongBreakMinutes = 2f;
    public float maxLongBreakMinutes = 5f;

    [Header("Boss Settings (Optional)")]
    public BossTrigger bossTrigger;

    [Header("Obstacle Avoidance Settings")]
    public LayerMask obstacleLayer;
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

    // === КЕШУВАННЯ ДЛЯ ОПТИМІЗАЦІЇ ===
    private string cachedSaveKey;
    private List<GameObject> validPrefabs = new List<GameObject>(); // Очищений список префабів у пам'яті
    private float activationDistanceSqr; // Квадрат дистанції активації
    private WaitForSeconds oneSecondWait;
    private WaitForSeconds twoSecondWait;
    private bool hasPlayer;
    private bool hasBossTrigger;
    private bool hasQuestManager;

    void Awake()
    {
        spawnArea = GetComponent<PolygonCollider2D>();
        spawnArea.isTrigger = true;

        // Кешуємо ключ збереження один раз, щоб не склеювати рядки в процесі гри
        cachedSaveKey = "Spawner_" + gameObject.name + "_RespawnTime";
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            hasPlayer = true;
        }

        hasBossTrigger = bossTrigger != null;
        hasQuestManager = questManager != null;

        if (hasQuestManager)
        {
            deathsBeforeLongBreak = questManager.killsRequired;
        }

        // Попередній підрахунок квадрата відстані
        activationDistanceSqr = activationDistance * activationDistance;

        // Кешуємо WaitForSeconds для корутин
        oneSecondWait = new WaitForSeconds(1f);
        twoSecondWait = new WaitForSeconds(2f);

        // Фільтруємо префаби ОДИН РАЗ на старті гри, замість того, щоб робити це при кожному спавні
        validPrefabs.Clear();
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            foreach (GameObject prefab in enemyPrefabs)
            {
                if (prefab != null) validPrefabs.Add(prefab);
            }
        }

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
        if (!isSpawningActive || !hasPlayer || isOnLongBreak) return;

        // Рахуємо квадрат відстані без квадратного кореня (заощаджує ресурси мобільного процесора)
        float sqrDistance = ((Vector2)transform.position - (Vector2)playerTransform.position).sqrMagnitude;

        if (sqrDistance > activationDistanceSqr || currentEnemyCount >= maxEnemies) return;

        if (!isSpawning)
        {
            StartCoroutine(SpawnWithDelay());
        }
    }

    IEnumerator CheckSavedSpawnAndFill()
    {
        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady()) yield return null;

        if (PlayerPrefs.HasKey(cachedSaveKey))
        {
            long savedUnlockTime = long.Parse(PlayerPrefs.GetString(cachedSaveKey));
            long currentTime = TimeManager.Instance.GetCurrentUnixTime();

            if (currentTime < savedUnlockTime)
            {
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

        if (persistSpawnTimer)
        {
            if (PlayerPrefs.HasKey(cachedSaveKey))
            {
                long savedUnlockTime = long.Parse(PlayerPrefs.GetString(cachedSaveKey));
                if (savedUnlockTime > TimeManager.Instance.GetCurrentUnixTime())
                {
                    spawnTime = savedUnlockTime;
                }
            }
            else
            {
                PlayerPrefs.SetString(cachedSaveKey, spawnTime.ToString());
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
            yield return oneSecondWait; // Оптимізовано: нуль сміття
        }

        if (currentEnemyCount < maxEnemies && !isOnLongBreak && isSpawningActive)
        {
            SpawnInPolygon();

            if (persistSpawnTimer)
            {
                PlayerPrefs.DeleteKey(cachedSaveKey);
                PlayerPrefs.Save();
            }
        }
        isSpawning = false;
    }

    void SpawnInPolygon()
    {
        if (spawnArea == null || validPrefabs.Count == 0) return;

        // Вибираємо закешований префаб
        GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];

        Bounds bounds = spawnArea.bounds;
        Vector2 spawnPos = Vector2.zero;
        bool validPositionFound = false;

        // Логіка оверлапу залишається математично правильною
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

        if (hasBossTrigger)
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

        if (hasQuestManager)
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
            yield return twoSecondWait; // Оптимізовано: нуль сміття
        }

        isOnLongBreak = false;
    }

    public void StopSpawningPermanently()
    {
        isSpawningActive = false;
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

        if (persistSpawnTimer)
        {
            PlayerPrefs.DeleteKey(cachedSaveKey);
            PlayerPrefs.Save();
        }

        // Безпечно зупиняємо корутини перед повторним запуском
        StopAllCoroutines();
        InitialFill();
    }
}