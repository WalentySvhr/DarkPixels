using UnityEngine;
using System.Collections.Generic;

public class BossTrigger : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public GameObject spawnEffect;

    [Header("Spawner Link")]
    public MonoBehaviour linkedSpawner;

    [Header("Door Settings")]
    public GameObject nextFloorDoorPrefab;
    public Transform nextFloorSpawnPoint;

    [Header("Exit World Settings")]
    public GameObject exitWorldDoorPrefab;
    public Transform exitWorldSpawnPoint;
    public Transform playerWorldExitPoint;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool bossSpawned = false;
    private bool waitingForEnemies = false;

    private GameObject activeBoss;
    private bool doorSpawned = false;

    void Update()
    {
        // Додаємо doorSpawned у перевірку, щоб Update перестав працювати, коли рівень пройдено
        if (waitingForEnemies && !doorSpawned)
        {
            spawnedEnemies.RemoveAll(item => item == null);

            // Якщо звичайні вороги закінчилися ТА активного боса немає (він null)
            if (spawnedEnemies.Count == 0 && activeBoss == null)
            {
                CheckRoomProgress();
            }
        }
    }

    private void CheckRoomProgress()
    {
        if (doorSpawned) return;

        if (TowerManager.Instance != null && TowerManager.Instance.IsBossFloor() && !bossSpawned)
        {
            SpawnBoss();
        }
        else
        {
            // Поверх зачищено
            waitingForEnemies = false;
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnFloorCleared();
            }
            else
            {
                SpawnAllDoors();
            }
        }
    }

    public void SpawnBoss()
    {
        if (bossSpawned || bossPrefab == null) return;
        bossSpawned = true;

        // Фікс координат: беремо позицію точки або поточного об'єкта, скидаючи Z на 0
        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        spawnPos.z = 0f;

        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);
        }

        activeBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        // Гарантуємо, що бос не має "батька", який міг би його змістити
        activeBoss.transform.SetParent(null);

        RegisterEnemy(activeBoss);

        BossHealth bh = activeBoss.GetComponent<BossHealth>();
        if (bh != null && TowerManager.Instance != null)
        {
            float multiplier = TowerManager.Instance.GetBossDifficultyMultiplier();
            bh.SetHealth(multiplier);
            Debug.Log($"<color=red>БОС З'ЯВИВСЯ!</color> Позиція: {spawnPos}");
        }
    }

    public void ActivateExitDoor()
    {
        SpawnAllDoors();
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        if (!spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Add(enemy);
        }
        waitingForEnemies = true;
    }

    public void ResetTrigger()
    {
        bossSpawned = false;
        doorSpawned = false;
        waitingForEnemies = false;
        spawnedEnemies.Clear();

        if (activeBoss != null)
        {
            Destroy(activeBoss);
            activeBoss = null;
        }

        // Очищення старих дверей (краще через Tag, як у тебе)
        GameObject[] oldDoors = GameObject.FindGameObjectsWithTag("ExitDoor");
        foreach (var d in oldDoors) Destroy(d);

        if (linkedSpawner != null)
        {
            linkedSpawner.StopAllCoroutines();
            if (linkedSpawner is TowerSpawner ts) ts.RestartSpawner();
        }
    }

    public void SpawnAllDoors()
    {
        if (doorSpawned) return;
        doorSpawned = true;
        waitingForEnemies = false;

        // Двері далі
        if (nextFloorDoorPrefab != null)
        {
            Vector3 pos = nextFloorSpawnPoint != null ? nextFloorSpawnPoint.position : transform.position + Vector3.right * 2f;
            pos.z = 0f;
            Instantiate(nextFloorDoorPrefab, pos, Quaternion.identity);
        }

        // Двері додому
        if (exitWorldDoorPrefab != null)
        {
            Vector3 pos = exitWorldSpawnPoint != null ? exitWorldSpawnPoint.position : transform.position + Vector3.left * 2f;
            pos.z = 0f;
            GameObject exitDoor = Instantiate(exitWorldDoorPrefab, pos, Quaternion.identity);

            // Надаємо тег, щоб Reset міг їх видалити
            exitDoor.tag = "ExitDoor";

            LocalTeleport lt = exitDoor.GetComponent<LocalTeleport>();
            if (lt != null)
            {
                lt.targetLocation = playerWorldExitPoint;
                lt.isActive = true;
                lt.resetTowerOnExit = true;
            }
        }
    }
}