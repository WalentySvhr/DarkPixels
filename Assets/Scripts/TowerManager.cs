using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EnemyTier
{
    public string tierName;
    public int minFloor;
    public int maxFloor;
    public GameObject[] prefabs;
}

[System.Serializable]
public struct EnvironmentTier
{
    public string themeName;
    public int minFloor;
    public int maxFloor;
    public Transform floorEntryPoint;
    public Color skyColor;
    public FloorData floorData; // Посилання на дані конкретного данжу (Spawner/BossTrigger)
}

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance;

    [Header("Chest Settings")]
    public ChestSpawner chestSpawner;

    [Header("Tower Stats")]
    public int currentFloor = 1;
    public int bossEveryXFloors = 5;

    [Header("Difficulty Scaling")]
    public float enemyMultiplierPerFloor = 0.1f;
    public float bossMultiplierPerFloor = 0.2f;

    [Space(5)]
    public int baseEnemyCount = 5;
    public int enemiesIncrementPerFloor = 2;

    [Header("Enemy & Visual Progression")]
    public EnemyTier[] enemyTiers;
    public EnvironmentTier[] environmentTiers;

    [Header("UI Elements")]
    public GameObject floorUIContainer;
    public TextMeshProUGUI floorText;

    [Header("Notifications")]
    [TextArea(1, 3)] public string startRunMessage = "БАШТА РОЗПОЧАТА";
    [TextArea(1, 3)] public string floorClearedMessage = "ПОВЕРХ ЗАЧИЩЕНО! ШУКАЙ ВИХІД";
    [TextArea(1, 3)] public string normalFloorStartMessage = "ПОВЕРХ РОЗПОЧАТО";
    [TextArea(1, 3)] public string bossFloorStartMessage = "УВАГА! ЛІГВО БОСА";

    [Header("References (Dynamic)")]
    public BossTrigger bossTrigger; // Це посилання тепер оновлюється автоматично
    public GameObject player;
    public Camera mainCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (floorUIContainer != null) floorUIContainer.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;
        if (chestSpawner == null) chestSpawner = FindObjectOfType<ChestSpawner>();
    }

    // --- Логіка перемикання данжів ---

    private void ApplyFloorEnvironment()
    {
        foreach (var tier in environmentTiers)
        {
            if (currentFloor >= tier.minFloor && currentFloor <= tier.maxFloor)
            {
                // 1. Оновлюємо візуал
                if (mainCamera != null) mainCamera.backgroundColor = tier.skyColor;

                // 2. ОНОВЛЮЄМО ПОСИЛАННЯ НА СПАВНЕР ТА БОСА
                if (tier.floorData != null)
                {
                    bossTrigger = tier.floorData.floorBossTrigger;
                }
                else
                {
                    Debug.LogError($"TowerManager: Об'єкт FloorData не призначений для {tier.themeName}!");
                }

                // 3. Телепортуємо
                TeleportPlayerToPoint(tier.floorEntryPoint);

                Debug.Log($"<color=cyan>TowerManager:</color> Данж оновлено: {tier.themeName}");
                return;
            }
        }
    }

    private void TeleportPlayerToPoint(Transform targetPoint)
    {
        if (player == null || targetPoint == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        player.transform.position = targetPoint.position;

        if (mainCamera != null)
        {
            Vector3 camPos = targetPoint.position;
            camPos.z = mainCamera.transform.position.z;
            mainCamera.transform.position = camPos;
        }
    }

    // --- Основний ігровий цикл ---

    public void StartTowerRun()
    {
        currentFloor = 1;
        PrepareLevel();
        ShowTowerUI();
        if (TowerUIManager.Instance != null)
            TowerUIManager.Instance.ShowNotification(startRunMessage);
        StartSpawners();
    }

    public void GoToNextFloor()
    {
        currentFloor++;
        PrepareLevel();
        UpdateFloorText();
        StartSpawners();

        if (TowerUIManager.Instance != null)
        {
            string msg = IsBossFloor() ? bossFloorStartMessage : normalFloorStartMessage;
            TowerUIManager.Instance.ShowNotification(msg);
        }
    }

    private void PrepareLevel()
    {
        // ВАЖЛИВО: Спочатку оновлюємо посилання (bossTrigger), а потім чистимо
        ApplyFloorEnvironment();

        StopSpawners();
        ClearEnemies();
        ClearLoot();

        if (chestSpawner != null) chestSpawner.ClearChests();
        if (bossTrigger != null) bossTrigger.ResetTrigger();
    }

    private void StartSpawners()
    {
        if (bossTrigger == null) return;

        // Отримуємо спавнер через BossTrigger (або напряму з FloorData)
        TowerSpawner ts = null;
        if (bossTrigger.linkedSpawner != null)
            ts = bossTrigger.linkedSpawner.GetComponent<TowerSpawner>();

        if (IsBossFloor())
        {
            if (ts != null) ts.isSpawningActive = false;
            bossTrigger.SpawnBoss();
        }
        else
        {
            if (chestSpawner != null) chestSpawner.SpawnChestsForFloor();

            if (ts != null)
            {
                ts.enabled = true;
                ts.isSpawningActive = true;
                ts.RestartSpawner();
            }
        }
    }

    // --- Методи очищення та UI ---

    public void OnFloorCleared()
    {
        if (TowerUIManager.Instance != null)
            TowerUIManager.Instance.ShowNotification(floorClearedMessage);
        if (bossTrigger != null) bossTrigger.ActivateExitDoor();
    }

    public void ResetTowerProgress()
    {
        currentFloor = 1;
        UpdateFloorText();
        HideTowerUI();
        StopSpawners();
        ClearLoot();
        if (chestSpawner != null) chestSpawner.ClearChests();
    }

    private void StopSpawners()
    {
        if (bossTrigger != null && bossTrigger.linkedSpawner != null)
        {
            var ts = bossTrigger.linkedSpawner.GetComponent<TowerSpawner>();
            if (ts != null) ts.enabled = false;
        }
    }

    private void ClearEnemies()
    {
        if (bossTrigger != null && bossTrigger.linkedSpawner != null)
        {
            var ts = bossTrigger.linkedSpawner.GetComponent<TowerSpawner>();
            if (ts != null) ts.ClearTowerEnemies();
        }
    }

    private void ClearLoot()
    {
        GameObject[] loot = GameObject.FindGameObjectsWithTag("Loot");
        foreach (GameObject l in loot) Destroy(l);
    }

    private void UpdateFloorText() { if (floorText != null) floorText.text = "FLOOR: " + currentFloor; }
    public void ShowTowerUI() { if (floorUIContainer != null) { floorUIContainer.SetActive(true); UpdateFloorText(); } }
    public void HideTowerUI() { if (floorUIContainer != null) floorUIContainer.SetActive(false); }

    public GameObject[] GetAvailablePrefabs()
    {
        List<GameObject> available = new List<GameObject>();
        foreach (var tier in enemyTiers)
        {
            if (currentFloor >= tier.minFloor && currentFloor <= tier.maxFloor)
                available.AddRange(tier.prefabs);
        }
        return (available.Count > 0) ? available.ToArray() : (enemyTiers.Length > 0 ? enemyTiers[0].prefabs : null);
    }

    public int GetEnemiesCountForCurrentFloor() => IsBossFloor() ? 0 : baseEnemyCount + (currentFloor - 1) * enemiesIncrementPerFloor;
    public float GetDifficultyMultiplier() => 1f + ((currentFloor - 1) * enemyMultiplierPerFloor);
    public float GetBossDifficultyMultiplier() => 1f + ((currentFloor - 1) * bossMultiplierPerFloor);
    public bool IsBossFloor() => currentFloor % bossEveryXFloors == 0;
}