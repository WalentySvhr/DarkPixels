using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Використовуємо TextMeshPro для кращої продуктивності та чіткості

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonAreaSpawner : MonoBehaviour
{
    [Header("Master Switch")]
    protected bool isSpawningActive = true;

    [Header("Enemy Settings")]
    [Tooltip("Додай сюди префаби елітних ворогів. Порожні слоти будуть проігноровані.")]
    public GameObject[] enemyPrefabs;
    [Tooltip("Для поодиноких еліток зазвичай ставлять 1.")]
    public int maxEnemies = 1;

    [Header("Respawn Delay (Cooldown)")]
    [Tooltip("Час кулдауну після смерті мобів (в секундах).")]
    public float minSpawnDelay = 18000f; // 5 годин = 18000 сек
    public float maxSpawnDelay = 18000f;

    [Header("Long Respawn Save Settings")]
    [Tooltip("Увімкни це, щоб час кулдауну не скидався при перезапуску гри.")]
    public bool persistSpawnTimer = true;

    [Header("Boss Settings (Optional)")]
    public BossTrigger bossTrigger;

    [Header("Obstacle Avoidance Settings")]
    public LayerMask obstacleLayer;
    public float spawnCheckRadius = 0.4f;

    [Header("Optimization")]
    public float activationDistance = 30f;

    [Header("UI Timer Settings (Optional)")]
    [Tooltip("Об'єкт панелі або World Space Canvas з таймером, який буде вмикатися під час кулдауну.")]
    public GameObject timerPanel;
    [Tooltip("Текстовий компонент TextMeshPro для відображення зворотного відліку.")]
    public TextMeshProUGUI timerText;

    private PolygonCollider2D spawnArea;
    private int currentEnemyCount = 0;
    private Transform playerTransform;
    private bool isSpawning = false;
    private bool isCooldownActive = false;

    public AreaQuestManager questManager;

    // === КЕШУВАННЯ ДЛЯ ОПТИМІЗАЦІЇ ===
    private string cachedSaveKey;
    private List<GameObject> validPrefabs = new List<GameObject>();
    private float activationDistanceSqr;
    private WaitForSeconds oneSecondWait;
    private bool hasPlayer;
    private bool hasBossTrigger;
    private bool hasQuestManager;

    // Властивість для повної сумісності з AreaQuestManager
    public bool isOnLongBreak => isCooldownActive;

    void Awake()
    {
        spawnArea = GetComponent<PolygonCollider2D>();
        spawnArea.isTrigger = true;
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

        activationDistanceSqr = activationDistance * activationDistance;
        oneSecondWait = new WaitForSeconds(1f);

        // Ховаємо таймер на старті гри, якщо моб ще живий
        if (timerPanel != null) timerPanel.SetActive(false);

        // Фільтруємо префаби від порожніх слотів
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
        if (!isSpawningActive || !hasPlayer || isCooldownActive || currentEnemyCount >= maxEnemies) return;

        float sqrDistance = ((Vector2)transform.position - (Vector2)playerTransform.position).sqrMagnitude;
        if (sqrDistance > activationDistanceSqr) return;

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
                // Ми ще на кулдауні, запускаємо очікування залишку часу
                StartCoroutine(CooldownRoutine(savedUnlockTime));
                yield break;
            }
            else
            {
                // Час кулдауну вже минув, видаляємо ключ
                PlayerPrefs.DeleteKey(cachedSaveKey);
                PlayerPrefs.Save();
            }
        }

        InitialFill();
    }

    protected void InitialFill()
    {
        if (!isSpawningActive) return;

        while (currentEnemyCount < maxEnemies)
        {
            if (!SpawnInPolygon()) break;
        }
    }

    IEnumerator SpawnWithDelay()
    {
        isSpawning = true;

        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady()) yield return null;

        if (currentEnemyCount < maxEnemies && isSpawningActive)
        {
            SpawnInPolygon();
        }

        isSpawning = false;
    }

    bool SpawnInPolygon()
    {
        if (spawnArea == null || validPrefabs.Count == 0) return false;

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

        if (!validPositionFound) return false;

        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        currentEnemyCount++;

        if (hasBossTrigger) bossTrigger.RegisterEnemy(enemy);

        float randomFlip = (Random.value > 0.5f) ? 1f : -1f;
        enemy.transform.localScale = new Vector3(randomFlip, 1, 1);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null && ai.hpBarTransform != null)
        {
            ai.hpBarTransform.localScale = new Vector3(randomFlip, 1, 1);
        }

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null) health.mySpawner = this;

        return true;
    }

    public void EnemyDied()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0) currentEnemyCount = 0;

        if (hasQuestManager) questManager.OnEnemyKilled();

        // Якщо всі елітки в цьому спавнері померли — запускаємо кулдаун респавну
        if (currentEnemyCount == 0 && isSpawningActive)
        {
            float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            long unlockTime = TimeManager.Instance.GetCurrentUnixTime() + (long)randomDelay;

            if (persistSpawnTimer)
            {
                PlayerPrefs.SetString(cachedSaveKey, unlockTime.ToString());
                PlayerPrefs.Save();
            }

            StartCoroutine(CooldownRoutine(unlockTime));
        }
    }

    IEnumerator CooldownRoutine(long unlockTime)
    {
        isCooldownActive = true;

        // Вмикаємо панель таймера, коли починається відлік кулдауну
        if (timerPanel != null) timerPanel.SetActive(true);

        while (TimeManager.Instance == null || !TimeManager.Instance.IsReady()) yield return null;

        while (TimeManager.Instance.GetCurrentUnixTime() < unlockTime)
        {
            if (!isSpawningActive)
            {
                isCooldownActive = false;
                if (timerPanel != null) timerPanel.SetActive(false);
                yield break;
            }

            // Оновлюємо UI текст раз на секунду (оптимізовано)
            if (timerText != null)
            {
                long secondsLeft = unlockTime - TimeManager.Instance.GetCurrentUnixTime();
                if (secondsLeft < 0) secondsLeft = 0;

                System.TimeSpan time = System.TimeSpan.FromSeconds(secondsLeft);
                // Форматує час у вигляд "ГГ:ММ:СС" (наприклад, 04:59:42)
                timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
            }

            yield return oneSecondWait;
        }

        if (persistSpawnTimer)
        {
            PlayerPrefs.DeleteKey(cachedSaveKey);
            PlayerPrefs.Save();
        }

        isCooldownActive = false;

        // Ховаємо панель таймера, бо моб готовий до появи
        if (timerPanel != null) timerPanel.SetActive(false);
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
            Gizmos.color = isCooldownActive ? new Color(1, 0, 0, 0.2f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }

    public void RestartSpawner()
    {
        isSpawningActive = true;
        isCooldownActive = false;
        currentEnemyCount = 0;
        isSpawning = false;

        if (persistSpawnTimer)
        {
            PlayerPrefs.DeleteKey(cachedSaveKey);
            PlayerPrefs.Save();
        }

        if (timerPanel != null) timerPanel.SetActive(false);

        StopAllCoroutines();
        InitialFill();
    }
}