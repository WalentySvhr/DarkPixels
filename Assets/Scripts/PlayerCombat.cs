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

    // === ДОДАНО: Посилання на аніматор гравця ===
    private Animator playerAnim;

    [Header("Start Weapon")]
    public WeaponData startingWeapon;

    [Header("Налаштування бою руками (Unarmed)")]
    public int unarmedDamage = 5;
    public float unarmedRange = 1.0f;
    public float unarmedCooldown = 0.5f;
    public float unarmedDamageDelay = 0.1f;
    public float unarmedKnockback = 5f;

    // === ДОДАНО: Посилання на скрипт звуку ===
    [Header("Звук бою руками")]
    public WeaponSound unarmedSoundScript;

    [Header("Бонуси від екіпіровки")]
    [HideInInspector] public int extraAmuletDamage = 0;
    [HideInInspector] public float extraAttackSpeed = 0f;
    [HideInInspector] public int extraRingDamage = 0;
    [HideInInspector] public float extraRingAttackSpeed = 0f;

    [Header("Механіка Критичного Удару")]
    [HideInInspector] public float critChance = 0f;       // 0.1 = 10%
    [HideInInspector] public float critMultiplier = 2f;   // х2 урон за замовчуванням

    void Start()
    {
        // === ДОДАНО: Отримуємо аніматор на старті ===
        playerAnim = GetComponent<Animator>();

        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
    }

    public void EquipWeapon(WeaponData newData)
    {
        // Видаляємо стару зброю
        if (spawnedWeapon != null)
        {
            Destroy(spawnedWeapon);
            spawnedWeapon = null;
        }

        // Якщо нової зброї немає — виходимо
        if (newData == null)
        {
            currentWeaponData = null;
            return;
        }

        currentWeaponData = newData;

        // Перевіряємо, чи є префаб і чи заданий weaponHolder
        if (newData.visualPrefab != null && weaponHolder != null)
        {
            spawnedWeapon = Instantiate(newData.visualPrefab, weaponHolder);
            spawnedWeapon.transform.localPosition = Vector3.zero;
            spawnedWeapon.transform.localRotation = Quaternion.identity;

            // Якщо у зброї є аніматор — оновлюємо швидкість
            Animator weaponAnim = spawnedWeapon.GetComponent<Animator>();
            if (weaponAnim != null)
            {
                newData.UpdateAnimationSpeed(weaponAnim);
            }
        }
        else
        {
            Debug.LogWarning("[PlayerCombat] Не знайдено visualPrefab або weaponHolder для зброї: " + newData.name);
        }
    }


    public void OnAttackButton()
    {
        if (Time.time < nextAttackTime) return;

        float totalSpeedBonus = extraAttackSpeed + extraRingAttackSpeed;
        float baseCooldown = (currentWeaponData != null) ? currentWeaponData.cooldown : unarmedCooldown;
        float finalCooldown = baseCooldown / (1f + totalSpeedBonus);

        StartCoroutine(PerformAttack());
        nextAttackTime = Time.time + finalCooldown;
    }

    IEnumerator PerformAttack()
    {
        float totalSpeedBonus = extraAttackSpeed + extraRingAttackSpeed;

        if (spawnedWeapon != null)
        {
            Animator anim = spawnedWeapon.GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = 1f + totalSpeedBonus;
                anim.SetTrigger("Attack");
            }
        }
        else // === ДОДАНО: Якщо зброї немає, граємо анімацію на самому гравці ===
        {
            if (playerAnim != null)
            {
                playerAnim.speed = 1f + totalSpeedBonus;
                playerAnim.SetTrigger("Attack");
            }
        }

        float baseDelay = (currentWeaponData != null) ? currentWeaponData.damageDelay : unarmedDamageDelay;
        float finalDelay = baseDelay / (1f + totalSpeedBonus);

        yield return new WaitForSeconds(finalDelay);

        if (currentWeaponData == null) UnarmedDamage();
        else
        {
            if (currentWeaponData.weaponType == WeaponType.Melee) MeleeDamage();
            else if (currentWeaponData.weaponType == WeaponType.Ranged) RangedShot();
        }
    }

    // --- Допоміжний метод для розрахунку фінального урону з урахуванням кріта ---
    private int CalculateFinalDamage(int baseDmg, out bool isCrit)
    {
        int totalBase = baseDmg + extraAmuletDamage + extraRingDamage;
        isCrit = Random.value < critChance;

        if (isCrit)
        {
            return Mathf.RoundToInt(totalBase * critMultiplier);
        }
        return totalBase;
    }

    void UnarmedDamage()
    {
        // === ДОДАНО: Викликаємо звук удару рукою ===
        if (unarmedSoundScript != null)
        {
            unarmedSoundScript.PlayHit();
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, unarmedRange, enemyLayers);
        int finalDamage = CalculateFinalDamage(unarmedDamage, out bool isCrit);

        foreach (Collider2D enemy in hitEnemies)
        {
            ApplyDamageToEnemy(enemy, finalDamage, isCrit, unarmedKnockback);
        }
    }

    void MeleeDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, currentWeaponData.attackRange, enemyLayers);
        int finalDamage = CalculateFinalDamage(currentWeaponData.damage, out bool isCrit);

        foreach (Collider2D enemy in hitEnemies)
        {
            // ТЕПЕР ПРАВИЛЬНО: Беремо силу відкидання з налаштувань самої зброї
            ApplyDamageToEnemy(enemy, finalDamage, isCrit, currentWeaponData.knockbackForce);
        }
    }

    void ApplyDamageToEnemy(Collider2D enemy, int damage, bool isCrit, float knockbackForce)
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;

            enemyHealth.TakeDamage(damage, knockbackDir, knockbackForce, isCrit);

            if (isCrit) Debug.Log("<color=yellow>CRITICAL HIT!</color> " + damage);
        }

        BossHealth bossHealth = enemy.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
        }
    }

    void RangedShot()
    {
        if (currentWeaponData.projectilePrefab != null)
        {
            // Визначаємо точку спавну стріли
            Vector3 spawnPosition = attackPoint.position; // Значення за замовчуванням

            // Намагаємося знайти FirePoint всередині префаба поточної зброї
            if (spawnedWeapon != null)
            {
                Transform customFirePoint = spawnedWeapon.transform.Find("FirePoint");
                if (customFirePoint != null)
                {
                    spawnPosition = customFirePoint.position; // Використовуємо точку з префаба лука
                }
            }

            Transform target = FindNearestEnemy();
            Vector2 shootDirection;

            if (target != null)
            {
                // Рахуємо напрямок від точки спавну до ворога
                shootDirection = ((Vector2)target.position - (Vector2)spawnPosition).normalized;
                FlipTowards(target.position.x);
            }
            else
            {
                shootDirection = new Vector2(Mathf.Sign(transform.localScale.x), 0);
            }

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

            // Створюємо стрілу в правильній точці spawnPosition
            GameObject proj = Instantiate(currentWeaponData.projectilePrefab, spawnPosition, Quaternion.AngleAxis(angle, Vector3.forward));

            Arrow arrowScript = proj.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.damage = CalculateFinalDamage(currentWeaponData.damage, out bool isCrit);
                arrowScript.isCrit = isCrit;

                if (isCrit) Debug.Log("<color=yellow>Ranged Crit!</color>");
            }

            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
            if (rbProj != null) rbProj.AddForce(shootDirection * currentWeaponData.shootForce, ForceMode2D.Impulse);
        }
    }

    void FlipTowards(float targetX)
    {
        Vector3 scale = transform.localScale;
        if (targetX > transform.position.x) scale.x = Mathf.Abs(scale.x);
        else scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
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