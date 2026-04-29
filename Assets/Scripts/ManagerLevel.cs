using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Щоб легко звертатися з будь-якого скрипта
    public GameObject bossPrefab; // Префаб боса
    public Transform bossSpawnPoint; // Місце появи боса
    
    private int enemyCount = 0;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy() => enemyCount++;

    public void UnregisterEnemy()
    {
        enemyCount--;
        if (enemyCount <= 0)
        {
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            Debug.Log("Всі вороги вбиті! Бос з'явився!");
        }
    }
}