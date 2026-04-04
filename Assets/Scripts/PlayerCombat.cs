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
    public int unarmedDamage = 5;          // Урон кулаком
    public float unarmedRange = 1.0f;      // Дальність (менша за меч)
    public float unarmedCooldown = 0.5f;   // Швидкість ударів руками
    public float unarmedDamageDelay = 0.1f; // Затримка до удару
    public float unarmedKnockback = 5f;    // Сила відштовхування руками

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
        // Тепер кнопка не повертає null, якщо зброї немає
        if (Time.time < nextAttackTime) return;

        // Визначаємо кулдаун (або зброї, або кулаків)
        float cooldown = (currentWeaponData != null) ? currentWeaponData.cooldown : unarmedCooldown;

        StartCoroutine(PerformAttack());
        nextAttackTime = Time.time + cooldown;
    }

    IEnumerator PerformAttack()
    {
        // 1. Анімація
        if (spawnedWeapon != null)
        {
            Animator anim = spawnedWeapon.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Attack");
        }
        else
        {
            // Тут можна запустити анімацію самого гравця (якщо є аніматор на тілі)
            // GetComponent<Animator>().SetTrigger("Punch");
            Debug.Log("Атака кулаками!");
        }

        // 2. Пауза до моменту удару
        float delay = (currentWeaponData != null) ? currentWeaponData.damageDelay : unarmedDamageDelay;
        yield return new WaitForSeconds(delay);

        // 3. Логіка нанесення урону
        if (currentWeaponData == null)
        {
            // Б'ємо руками
            UnarmedDamage();
        }
        else
        {
            // Б'ємо зброєю
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
        // Використовуємо unarmedRange
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, unarmedRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                // Використовуємо unarmedDamage та unarmedKnockback
                enemyHealth.TakeDamage(unarmedDamage, knockbackDir, unarmedKnockback);
            }
        }
    }

    void MeleeDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, currentWeaponData.attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyHealth.TakeDamage(currentWeaponData.damage, knockbackDir, 10f);
            }
        }
    }

    void RangedShot()
    {
        if (currentWeaponData.projectilePrefab != null)
        {
            GameObject proj = Instantiate(currentWeaponData.projectilePrefab, attackPoint.position, attackPoint.rotation);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 1. Визначаємо напрямок: беремо знак від масштабу гравця (1 = вправо, -1 = вліво)
                float facingDirection = Mathf.Sign(transform.localScale.x);
                Vector2 shootDirection = new Vector2(facingDirection, 0);

                // 2. Штовхаємо стрілу у правильному напрямку
                rb.AddForce(shootDirection * currentWeaponData.shootForce, ForceMode2D.Impulse);

                // 3. Розвертаємо саму картинку стріли, щоб вона не летіла "хвостом уперед"
                if (facingDirection < 0)
                {
                    Vector3 projScale = proj.transform.localScale;
                    projScale.x = -1;
                    proj.transform.localScale = projScale;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 center = (attackPoint != null) ? attackPoint.position : transform.position;

        // Гізмо тепер показує радіус кулаків, якщо зброї немає
        float radius = (currentWeaponData != null) ? currentWeaponData.attackRange : unarmedRange;

        Gizmos.DrawWireSphere(center, radius);
    }
}