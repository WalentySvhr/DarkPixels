using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public Transform weaponHolder;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public WeaponData currentWeaponData;
    private GameObject spawnedWeapon;
    private float nextAttackTime = 0f;

    [Header("Start Weapon")]
    public WeaponData startingWeapon;

    [Header("Налаштування бою руками (Unarmed)")]
    public int unarmedDamage = 5;
    public float unarmedRange = 1.0f;
    public float unarmedCooldown = 0.5f;
    public float unarmedDamageDelay = 0.1f;
    public float unarmedKnockback = 5f;

    [Header("Бонуси від екіпіровки")]
    [HideInInspector]
    public int extraAmuletDamage = 0;   // Бонус до урону
    [HideInInspector]
    public float extraAttackSpeed = 0f; // Бонус до швидкості (наприклад, 0.2f для +20%)

    void Start()
    {
        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
    }

    public void EquipWeapon(WeaponData newData)
    {
        if (spawnedWeapon != null)
        {
            Destroy(spawnedWeapon);
            spawnedWeapon = null;
        }

        if (newData == null)
        {
            currentWeaponData = null;
            Debug.Log("Зброю знято! Персонаж б'ється руками.");
            return;
        }

        currentWeaponData = newData;

        if (newData.visualPrefab != null)
        {
            spawnedWeapon = Instantiate(newData.visualPrefab, weaponHolder);
            spawnedWeapon.transform.localPosition = Vector3.zero;
            spawnedWeapon.transform.localRotation = Quaternion.identity;

            if (spawnedWeapon.GetComponent<Animator>() != null)
                newData.UpdateAnimationSpeed(spawnedWeapon.GetComponent<Animator>());

            Debug.Log("Взято в руки: " + newData.itemName);
        }
    }

    public void OnAttackButton()
    {
        if (Time.time < nextAttackTime) return;

        float baseCooldown = (currentWeaponData != null) ? currentWeaponData.cooldown : unarmedCooldown;
        float finalCooldown = baseCooldown / (1f + extraAttackSpeed);

        StartCoroutine(PerformAttack());
        nextAttackTime = Time.time + finalCooldown;
    }

    IEnumerator PerformAttack()
    {
        if (spawnedWeapon != null)
        {
            Animator anim = spawnedWeapon.GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = 1f + extraAttackSpeed;
                anim.SetTrigger("Attack");
            }
        }

        float baseDelay = (currentWeaponData != null) ? currentWeaponData.damageDelay : unarmedDamageDelay;
        float finalDelay = baseDelay / (1f + extraAttackSpeed);

        yield return new WaitForSeconds(finalDelay);

        if (currentWeaponData == null)
        {
            UnarmedDamage();
        }
        else
        {
            if (currentWeaponData.weaponType == WeaponType.Melee)
            {
                MeleeDamage();
            }
            else if (currentWeaponData.weaponType == WeaponType.Ranged)
            {
                RangedShot();
            }
        }
    }

    void UnarmedDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, unarmedRange, enemyLayers);
        int totalDamage = unarmedDamage + extraAmuletDamage;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyHealth.TakeDamage(totalDamage, knockbackDir, unarmedKnockback);
            }

            BossHealth bossHealth = enemy.GetComponent<BossHealth>();
            if (bossHealth != null) bossHealth.TakeDamage(totalDamage);
        }
    }

    void MeleeDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, currentWeaponData.attackRange, enemyLayers);
        int totalDamage = currentWeaponData.damage + extraAmuletDamage;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyHealth.TakeDamage(totalDamage, knockbackDir, 10f);
            }

            BossHealth bossHealth = enemy.GetComponent<BossHealth>();
            if (bossHealth != null) bossHealth.TakeDamage(totalDamage);
        }
    }

    void RangedShot()
    {
        if (currentWeaponData.projectilePrefab != null)
        {
            Transform target = FindNearestEnemy();
            Vector2 shootDirection;

            if (target != null)
            {
                shootDirection = (target.position - attackPoint.position).normalized;

                Vector3 currentScale = transform.localScale;
                if (target.position.x > transform.position.x)
                    currentScale.x = Mathf.Abs(currentScale.x);
                else
                    currentScale.x = -Mathf.Abs(currentScale.x);

                transform.localScale = currentScale;
            }
            else
            {
                float facingDirection = Mathf.Sign(transform.localScale.x);
                shootDirection = new Vector2(facingDirection, 0);
            }

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject proj = Instantiate(currentWeaponData.projectilePrefab, attackPoint.position, rotation);

            // --- ДОДАНО: Передача урону снаряду ---
            Arrow arrowScript = proj.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.damage = currentWeaponData.damage + extraAmuletDamage;
            }

            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(shootDirection * currentWeaponData.shootForce, ForceMode2D.Impulse);
            }
        }
    }

    Transform FindNearestEnemy()
    {
        float detectionRadius = currentWeaponData.attackRange * 2f;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayers);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 center = (attackPoint != null) ? attackPoint.position : transform.position;
        float radius = (currentWeaponData != null) ? currentWeaponData.attackRange : unarmedRange;
        Gizmos.DrawWireSphere(center, radius);
    }
}