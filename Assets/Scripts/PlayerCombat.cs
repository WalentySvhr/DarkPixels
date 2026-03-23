using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("General Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayers;
    private WeaponManager weaponManager;

    [Header("Melee Settings (Sword)")]
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public float knockbackForce = 10f;
    public float swordCooldown = 0.3f; // Затримка для меча
    private float lastMeleeTime;      // Час останнього удару мечем

    [Header("Ranged Settings (Bow)")]
    public GameObject arrowPrefab;
    public float arrowForce = 15f;

    [Header("Bow Attack Speed")]
    public float bowCooldown = 0.7f; // Затримка для лука
    private float lastShootTime;     // Час останнього пострілу

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    public void Attack()
    {
        if (weaponManager == null)
        {
            MeleeAttack();
            return;
        }

        // Перевірка на меч
        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Sword)
        {
            // ПЕРЕВІРКА КУЛДАУНУ ДЛЯ МЕЧА
            if (Time.time >= lastMeleeTime + swordCooldown)
            {
                MeleeAttack();
                lastMeleeTime = Time.time; // Оновлюємо час атаки
            }
        }
        // Перевірка на лук
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Bow)
        {
            // ПЕРЕВІРКА КУЛДАУНУ ДЛЯ ЛУКА
            if (Time.time >= lastShootTime + bowCooldown)
            {
                Shoot();
                lastShootTime = Time.time; // Оновлюємо час пострілу
            }
        }
    }

    void MeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // 1. Спочатку перевіряємо, чи це Бос
            BossHealth boss = enemy.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                continue;
            }

            // 2. Якщо не Бос, перевіряємо, чи це звичайний ворог
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector3 direction = enemy.transform.position - transform.position;
                direction.z = 0;
                Vector2 knockbackDirection = direction.normalized;

                enemyHealth.TakeDamage(attackDamage, knockbackDirection, knockbackForce);
            }
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null) return;

        // Створюємо стрілу
        GameObject arrow = Instantiate(arrowPrefab, attackPoint.position, Quaternion.identity);

        // Визначаємо напрямок
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // Повертаємо стрілу
        if (direction < 0)
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}