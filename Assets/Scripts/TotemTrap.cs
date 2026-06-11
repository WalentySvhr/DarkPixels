using System.Collections.Generic;
using UnityEngine;

public class TotemTrap : MonoBehaviour
{
    public enum TotemMode { Projectile, AoEPuddle }

    [Header("Головні налаштування")]
    [SerializeField] private TotemMode totemMode = TotemMode.Projectile;

    [Header("Налаштування стрільби (Режим Projectile)")]
    [SerializeField] private float projectileFireRate = 2.0f; // Інтервал між пострілами снарядів
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int projectileCount = 4;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private int damage = 10;

    [Header("Налаштування калюжі (Режим AoE Puddle)")]
    [SerializeField] private float puddleFireRate = 4.5f;     // Інтервал між спавном нових калюж
    [SerializeField] private GameObject puddlePrefab;
    [SerializeField] private int puddleCount = 6;              // Скільки всього калюж спробувати заспавнити
    [Tooltip("Максимальний радіус зони навколо тотема, де можуть з'являтися калюжі")]
    [SerializeField] private float puddleZoneRadius = 2.5f;
    [Tooltip("Мінімальна відстань між калюжами, щоб вони не ліпилися в одну точку")]
    [SerializeField] private float minDistanceBetweenPuddles = 0.8f;
    [Tooltip("Зміщення центру зони калюж відносно тотема")]
    [SerializeField] private Vector2 puddleOffset = Vector2.zero;

    [Header("Анімація")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";

    private float fireCountdown = 0f;

    private void Start()
    {
        if (firePoint == null) firePoint = transform;
        if (animator == null) animator = GetComponent<Animator>();

        // Рандомізуємо початковий таймер на основі КД саме ТОГО режиму, який зараз увімкнено
        fireCountdown = Random.Range(0f, GetCurrentFireRate());
    }

    private void Update()
    {
        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            ExecuteAttack();

            // Оновлюємо таймер актуальним КД для поточного режиму
            fireCountdown = GetCurrentFireRate();
        }
    }

    // Зручний допоміжний метод, який повертає потрібне КД залежно від режиму
    private float GetCurrentFireRate()
    {
        return totemMode == TotemMode.Projectile ? projectileFireRate : puddleFireRate;
    }

    private void ExecuteAttack()
    {
        if (animator != null) animator.SetTrigger(shootTriggerName);

        if (totemMode == TotemMode.Projectile)
        {
            ShootBurst();
        }
        else if (totemMode == TotemMode.AoEPuddle)
        {
            SpawnPuddleZone();
        }
    }

    // Кругова стрільба снарядами
    private void ShootBurst()
    {
        if (projectilePrefab == null) return;

        float angleStep = 360f / projectileCount;
        float currentAngle = 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 targetDirection = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            TotemProjectile projectileScript = proj.GetComponent<TotemProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(targetDirection, projectileSpeed, damage);
            }

            currentAngle += angleStep;
        }
    }

    // Спавн калюж у випадкових точках зони із дотриманням дистанції N
    private void SpawnPuddleZone()
    {
        if (puddlePrefab == null) return;

        Vector3 centerPosition = transform.position + (Vector3)puddleOffset;

        // Список для зберігання координат уже успішно створених калюж у цьому залпі
        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < puddleCount; i++)
        {
            Vector3 targetPosition = Vector3.zero;
            bool validPointFound = false;

            // Робимо до 50 спроб знайти підходящу точку для кожної калюжі. 
            // Це захистить Unity від нескінченного циклу і зависання, якщо вільне місце закінчиться.
            for (int attempt = 0; attempt < 50; attempt++)
            {
                // Геруємо випадкову точку всередині одиничного кола і множимо на радіус нашої зони
                Vector2 randomCirclePoint = Random.insideUnitCircle * puddleZoneRadius;
                Vector3 potentialPos = centerPosition + new Vector3(randomCirclePoint.x, randomCirclePoint.y, 0f);

                // Перевіряємо відстань до кожної вже заспавненої калюжі
                bool tooCloseToOthers = false;
                foreach (Vector3 existingPos in spawnedPositions)
                {
                    if (Vector3.Distance(potentialPos, existingPos) < minDistanceBetweenPuddles)
                    {
                        tooCloseToOthers = true;
                        break; // Крапка не підходить, далі перевіряти цей attempt немає сенсу
                    }
                }

                // Якщо точка пройшла перевірку — фіксуємо її
                if (!tooCloseToOthers)
                {
                    targetPosition = potentialPos;
                    validPointFound = true;
                    break; // Виходимо з циклу спроб (attempt) для цієї калюжі
                }
            }

            // Якщо підходящу точку знайдено — спавнимо калюжу
            if (validPointFound)
            {
                spawnedPositions.Add(targetPosition);
                Instantiate(puddlePrefab, targetPosition, Quaternion.identity);
            }
        }

        Debug.Log($"<color=cyan>[Тотем] Атака AoE! Сформовано зону з {spawnedPositions.Count} калюж.</color>");
    }

    // Візуалізація меж зони в Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 centerPosition = transform.position + (Vector3)puddleOffset;

        // Малюємо коло, яке показує максимальні межі розльоту калюж
        Gizmos.DrawWireSphere(centerPosition, puddleZoneRadius);
    }
}