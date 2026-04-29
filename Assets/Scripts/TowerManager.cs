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
    public GameObject environmentPrefab;
    public Color skyColor;
}

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance;

    [Header("Chest Settings")]
    public ChestSpawner chestSpawner; // Посилання на наш новий спавнер

    [Header("Tower Stats")]
    public int currentFloor = 1;
    public int bossEveryXFloors = 5;

    [Header("Difficulty Scaling")]
    public float enemyMultiplierPerFloor = 0.1f;
    public float bossMultiplierPerFloor = 0.2f;

    [Space(5)]
    [Tooltip("Скільки мобів буде на 1-му поверсі")]
    public int baseEnemyCount = 5;
    [Tooltip("На скільки більше мобів стає з кожним новим поверхом")]
    public int enemiesIncrementPerFloor = 2;

    [Header("Enemy & Visual Progression")]
    public EnemyTier[] enemyTiers;
    public EnvironmentTier[] environmentTiers;

    [Header("UI Elements (Permanent)")]
    public GameObject floorUIContainer;
    public TextMeshProUGUI floorText;

    [Header("Custom Notifications")]
    [TextArea(1, 3)] public string startRunMessage = "БАШТА РОЗПОЧАТА";
    [TextArea(1, 3)] public string floorClearedMessage = "ПОВЕРХ ЗАЧИЩЕНО! ШУКАЙ ВИХІД";
    [TextArea(1, 3)] public string normalFloorStartMessage = "ПОВЕРХ РОЗПОЧАТО";
    [TextArea(1, 3)] public string bossFloorStartMessage = "УВАГА! ЛІГВО БОСА";

    [Header("References")]
    public BossTrigger bossTrigger;
    public Transform playerStartPoint;
    public GameObject player;
    public Camera mainCamera;

    private GameObject currentEnvInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (floorUIContainer != null) floorUIContainer.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;

        // Авто-пошук спавнера скринь, якщо не задано вручну
        if (chestSpawner == null) chestSpawner = FindObjectOfType<ChestSpawner>();
    }

    private void UpdateEnvironmentVisuals()
    {
        foreach (var tier in environmentTiers)
        {
            if (currentFloor >= tier.minFloor && currentFloor <= tier.maxFloor)
            {
                ApplyEnvironment(tier);
                return;
            }
        }
    }

    private void ApplyEnvironment(EnvironmentTier tier)
    {
        if (mainCamera != null)
            mainCamera.backgroundColor = tier.skyColor;

        if (tier.environmentPrefab != null)
        {
            if (currentEnvInstance != null) Destroy(currentEnvInstance);
            currentEnvInstance = Instantiate(tier.environmentPrefab, Vector3.zero, Quaternion.identity);
            currentEnvInstance.name = "Current_Environment_" + tier.themeName;
        }
    }

    public GameObject[] GetAvailablePrefabs()
    {
        List<GameObject> available = new List<GameObject>();
        foreach (var tier in enemyTiers)
        {
            if (currentFloor >= tier.minFloor && currentFloor <= tier.maxFloor)
            {
                available.AddRange(tier.prefabs);
            }
        }

        if (available.Count == 0 && enemyTiers.Length > 0)
            return enemyTiers[0].prefabs;

        return available.ToArray();
    }

    public int GetEnemiesCountForCurrentFloor()
    {
        if (IsBossFloor()) return 0;
        return baseEnemyCount + (currentFloor - 1) * enemiesIncrementPerFloor;
    }

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
        TeleportPlayer();
        UpdateFloorText();
        StartSpawners();

        if (TowerUIManager.Instance != null)
        {
            string msg = IsBossFloor() ? bossFloorStartMessage : normalFloorStartMessage;
            TowerUIManager.Instance.ShowNotification(msg);
        }
    }

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

        // Очищаємо скрині при повному скиданні
        if (chestSpawner != null) chestSpawner.ClearChests();

        if (currentEnvInstance != null) Destroy(currentEnvInstance);
    }

    private void PrepareLevel()
    {
        StopSpawners();
        ClearEnemies();
        ClearLoot();

        // Очищаємо скрині перед генерацією нового поверху
        if (chestSpawner != null) chestSpawner.ClearChests();

        UpdateEnvironmentVisuals();
        if (bossTrigger != null) bossTrigger.ResetTrigger();
    }

    private void StartSpawners()
    {
        if (bossTrigger == null) return;

        TowerSpawner ts = null;
        if (bossTrigger.linkedSpawner != null)
            ts = bossTrigger.linkedSpawner.GetComponent<TowerSpawner>();

        if (IsBossFloor())
        {
            if (ts != null) ts.isSpawningActive = false;
            bossTrigger.SpawnBoss();
            // Зазвичай на поверсі боса скрині не спавняться рандомно
        }
        else
        {
            // СПАВН СКРИНЬ на звичайному поверсі
            if (chestSpawner != null)
            {
                chestSpawner.SpawnChestsForFloor();
            }

            if (ts != null)
            {
                ts.enabled = true;
                ts.isSpawningActive = true;
                ts.RestartSpawner();
            }
        }
    }

    private void StopSpawners()
    {
        if (bossTrigger != null && bossTrigger.linkedSpawner != null)
        {
            var ts = bossTrigger.linkedSpawner.GetComponent<TowerSpawner>();
            if (ts != null) ts.enabled = false;
        }
    }

    private void TeleportPlayer()
    {
        if (player != null && playerStartPoint != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            player.transform.position = playerStartPoint.position;
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

    private void UpdateFloorText()
    {
        if (floorText != null)
            floorText.text = "FLOOR: " + currentFloor;
    }

    public void ShowTowerUI()
    {
        if (floorUIContainer != null)
        {
            floorUIContainer.SetActive(true);
            UpdateFloorText();
        }
    }

    public void HideTowerUI()
    {
        if (floorUIContainer != null)
            floorUIContainer.SetActive(false);
    }

    public float GetDifficultyMultiplier() => 1f + ((currentFloor - 1) * enemyMultiplierPerFloor);
    public float GetBossDifficultyMultiplier() => 1f + ((currentFloor - 1) * bossMultiplierPerFloor);
    public bool IsBossFloor() => currentFloor % bossEveryXFloors == 0;
}