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
    [Tooltip("Мінімальний час довгої паузи (у хвилинах)")]
    public float minLongBreakMinutes = 2f;
    [Tooltip("Максимальний час довгої паузи (у хвилинах)")]
    public float maxLongBreakMinutes = 5f;

    [Header("Optimization")]
    public float activationDistance = 30f;

    private BoxCollider2D spawnArea;
    private int currentEnemyCount = 0;
    private int totalDeathsInSession = 0;
    private Transform playerTransform;
    private bool isSpawning = false;

    // ЗМІНЕНО НА PUBLIC, щоб AreaQuestManager міг бачити цей стан
    [HideInInspector] // Це сховає її в інспекторі, щоб не заважала, але вона буде доступна коду
    public bool isOnLongBreak = false;

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
        // Синхронізуємо спавнер із квестом автоматично
        if (questManager != null)
        {
            deathsBeforeLongBreak = questManager.killsRequired;
        }

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

        // Перевіряємо ще раз перед самим спавном
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
        float randomXPos = Random.Range(bounds.min.x, bounds.max.x);
        float randomYPos = Random.Range(bounds.min.y, bounds.max.y);

        Vector2 spawnPos = new Vector2(randomXPos, randomYPos);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemyCount++;

        // Рандомний поворот
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

        Debug.Log($"Ворог убитий у зоні {gameObject.name}. Усього вбито для паузи: {totalDeathsInSession}/{deathsBeforeLongBreak}");

        if (totalDeathsInSession >= deathsBeforeLongBreak && !isOnLongBreak)
        {
            StartCoroutine(LongBreakRoutine());
        }
    }

    IEnumerator LongBreakRoutine()
    {
        isOnLongBreak = true;
        totalDeathsInSession = 0;

        float breakDuration = Random.Range(minLongBreakMinutes * 60f, maxLongBreakMinutes * 60f);
        Debug.Log($"<color=yellow>Зона {gameObject.name} зачищена! Пауза на {breakDuration / 60f:F1} хв.</color>");

        yield return new WaitForSeconds(breakDuration);

        isOnLongBreak = false;
        Debug.Log($"<color=green>Зона {gameObject.name} знову активна!</color>");
    }

    private void OnDrawGizmos()
    {
        if (spawnArea == null) spawnArea = GetComponent<BoxCollider2D>();
        if (spawnArea != null)
        {
            Gizmos.color = isOnLongBreak ? new Color(1, 0, 0, 0.2f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(spawnArea.bounds.center, spawnArea.bounds.size);
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }
}