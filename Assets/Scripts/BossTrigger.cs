using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossTrigger : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;    // Префаб боса
    public Transform bossSpawnPoint; // Точка спавну боса
    public GameObject spawnEffect;   // Ефект появи (дим/вибух)

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool bossSpawned = false;
    private bool waitingForEnemies = false;

    // 1. Цей метод треба викликати у вашому скрипті спавнера щоразу, коли з'являється моб
    public void RegisterEnemy(GameObject enemy)
    {
        spawnedEnemies.Add(enemy);
        waitingForEnemies = true;
    }

    void Update()
    {
        // Якщо ми ще не спавнили боса і вже закінчили реєструвати ворогів
        if (!bossSpawned && waitingForEnemies)
        {
            // Видаляємо зі списку тих, хто вже знищений (null)
            spawnedEnemies.RemoveAll(item => item == null);

            // Якщо список порожній — значить усі вбиті
            if (spawnedEnemies.Count == 0)
            {
                SpawnBoss();
            }
        }
    }

    void SpawnBoss()
    {
        bossSpawned = true;
        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;

        // Ефект появи
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);
        }

        // Спавн боса
        if (bossPrefab != null)
        {
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            Debug.Log("<color=red>Усі моби вбиті. БОС З'ЯВИВСЯ!</color>");
        }
    }
}