using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AmbushArena : MonoBehaviour
{
    private enum ArenaState { Idle, MobsPhase, BossPhase, RewardPhase, Cooldown, Finished }
    private ArenaState currentState = ArenaState.Idle;

    [Header("Ідентифікація та Режим")]
    [Tooltip("Унікальний ID цієї арени (наприклад: 'orc_ambush_dungeon_1')")]
    [SerializeField] private string uniqueID;

    [Tooltip("Якщо УВІМКНЕНО — арена пройдеться 1 раз на все життя профілю. Якщо ВИМКНЕНО — вона буде відновлюватися через КД.")]
    [SerializeField] private bool isPermanent = true;

    [Tooltip("Час відновлення арени у секундах (якщо вимкнено перманентність). 300 секунд = 5 хвилин.")]
    [SerializeField] private int cooldownSeconds = 300;

    [Header("ФАЗА 1: Звичайні моби")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] mobSpawnPoints;
    [SerializeField] private int totalMobsToSpawn = 9;
    [SerializeField] private float delayBetweenSpawns = 3.0f;

    [Header("ФАЗА 2: Бос (Залиши порожнім, якщо не треба)")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("ФАЗА 3: Нагорода")]
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private Transform rewardSpawnPoint;

    [Header("Ефекти")]
    [SerializeField] private GameObject spawnVFX;

    // --- НОВИЙ БЛОК: НАЛАШТУВАННЯ АГРУ ДЛЯ АРЕНИ ---
    [Header("Налаштування ШІ Мобів в Арені")]
    [Tooltip("Чи потрібно збільшувати радіус пошуку гравця для мобів цієї арени?")]
    [SerializeField] private bool overrideAggroRadius = true;

    [Tooltip("Новий радіус агру для мобів (наприклад, 15 чи 20, щоб вони бачили гравця здалеку)")]
    [SerializeField] private float ambushAggroRadius = 15f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Collider2D arenaTrigger;

    private IEnumerator Start()
    {
        arenaTrigger = GetComponent<Collider2D>();

        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError($"<color=red>[AmbushArena] На об'єкті '{gameObject.name}' НЕ вказано Unique ID!</color>");
            yield break;
        }

        if (isPermanent)
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                if (SaveManager.Instance.CurrentData.unlockedTrueObjects.Contains(uniqueID))
                {
                    SpawnReward(isAlreadyCleared: true);
                    yield break;
                }
            }
        }
        else
        {
            if (TimeManager.Instance != null)
            {
                yield return new WaitUntil(() => TimeManager.Instance.IsReady());
            }

            if (PlayerPrefs.HasKey(uniqueID + "_CooldownUnixEnd"))
            {
                string savedUnixStr = PlayerPrefs.GetString(uniqueID + "_CooldownUnixEnd");
                long cooldownEndTime = long.Parse(savedUnixStr);

                long currentUnixTime = TimeManager.Instance != null
                    ? TimeManager.Instance.GetCurrentUnixTime()
                    : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                long remainingSeconds = cooldownEndTime - currentUnixTime;

                if (remainingSeconds > 0)
                {
                    StartCoroutine(CooldownRoutine((float)remainingSeconds));
                    yield break;
                }
                else
                {
                    PlayerPrefs.DeleteKey(uniqueID + "_CooldownUnixEnd");
                    PlayerPrefs.Save();
                }
            }
        }
    }

    private void Update()
    {
        if (currentState == ArenaState.MobsPhase || currentState == ArenaState.BossPhase)
        {
            activeEnemies.RemoveAll(enemy => enemy == null);

            if (activeEnemies.Count == 0)
            {
                AdvanceArena();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == ArenaState.Idle && collision.CompareTag("Player"))
        {
            if (arenaTrigger != null) arenaTrigger.enabled = false;
            StartCoroutine(StartMobsPhase());
        }
    }

    private IEnumerator StartMobsPhase()
    {
        currentState = ArenaState.MobsPhase;
        int mobsSpawnedSoFar = 0;

        while (mobsSpawnedSoFar < totalMobsToSpawn)
        {
            for (int i = 0; i < mobSpawnPoints.Length; i++)
            {
                if (mobsSpawnedSoFar >= totalMobsToSpawn) break;

                if (mobSpawnPoints[i] != null)
                {
                    GameObject randomEnemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
                    SpawnUnit(randomEnemyPrefab, mobSpawnPoints[i].position);
                    mobsSpawnedSoFar++;
                }
            }

            if (delayBetweenSpawns > 0 && mobsSpawnedSoFar < totalMobsToSpawn)
            {
                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }
    }

    private void AdvanceArena()
    {
        if (currentState == ArenaState.MobsPhase)
        {
            if (bossPrefab != null)
            {
                currentState = ArenaState.BossPhase;
                Vector3 bossPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
                SpawnUnit(bossPrefab, bossPos);
            }
            else
            {
                SpawnReward(isAlreadyCleared: false);
            }
        }
        else if (currentState == ArenaState.BossPhase)
        {
            SpawnReward(isAlreadyCleared: false);
        }
    }

    private void SpawnReward(bool isAlreadyCleared)
    {
        currentState = ArenaState.RewardPhase;

        if (rewardPrefab != null)
        {
            Vector3 rewardPos = rewardSpawnPoint != null ? rewardSpawnPoint.position : transform.position;
            if (spawnVFX != null && !isAlreadyCleared) Instantiate(spawnVFX, rewardPos, Quaternion.identity);
            Instantiate(rewardPrefab, rewardPos, Quaternion.identity);
        }

        if (!isAlreadyCleared)
        {
            if (isPermanent)
            {
                if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
                {
                    if (!SaveManager.Instance.CurrentData.unlockedTrueObjects.Contains(uniqueID))
                    {
                        SaveManager.Instance.CurrentData.unlockedTrueObjects.Add(uniqueID);
                        SaveManager.Instance.SaveGame();
                        Debug.Log($"<color=gold>[AmbushArena] Зачищено назавжди! ID: '{uniqueID}' збережено.</color>");
                    }
                }
                currentState = ArenaState.Finished;
                Destroy(gameObject, 0.1f);
            }
            else
            {
                long currentUnixTime = TimeManager.Instance != null
                    ? TimeManager.Instance.GetCurrentUnixTime()
                    : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                long cooldownEndTime = currentUnixTime + cooldownSeconds;

                PlayerPrefs.SetString(uniqueID + "_CooldownUnixEnd", cooldownEndTime.ToString());
                PlayerPrefs.Save();

                StartCoroutine(CooldownRoutine(cooldownSeconds));
            }
        }
        else
        {
            currentState = ArenaState.Finished;
            Destroy(gameObject, 0.1f);
        }
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        currentState = ArenaState.Cooldown;
        if (arenaTrigger != null) arenaTrigger.enabled = false;

        Debug.Log($"<color=cyan>[AmbushArena] '{uniqueID}' на КД. Відновлення через {duration} сек.</color>");
        yield return new WaitForSeconds(duration);

        PlayerPrefs.DeleteKey(uniqueID + "_CooldownUnixEnd");
        PlayerPrefs.Save();

        ResetArena();
    }

    private void ResetArena()
    {
        currentState = ArenaState.Idle;
        if (arenaTrigger != null) arenaTrigger.enabled = true;
        activeEnemies.Clear();
        Debug.Log($"<color=green>[AmbushArena] '{uniqueID}' відновлено за інтернет-часом!</color>");
    }

    private void SpawnUnit(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        if (spawnVFX != null) Instantiate(spawnVFX, position, Quaternion.identity);
        GameObject spawned = Instantiate(prefab, position, Quaternion.identity);
        activeEnemies.Add(spawned);

        // --- ЗМУШУЄМО МОБА ОДРАЗУ АГРИТИСЯ НА ГРАВЦЯ ---
        EnemyAgro agroScript = spawned.GetComponent<EnemyAgro>();
        if (agroScript != null)
        {
            agroScript.StartAggro(); // Викликаємо твій метод! Моб відразу отримує статус агресії
            Debug.Log($"<color=orange>[AmbushArena] '{spawned.name}' успішно розлючений при спавні!</color>");
        }
    }
}