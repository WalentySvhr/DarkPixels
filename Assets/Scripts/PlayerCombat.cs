using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("General Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayers;
    private WeaponManager weaponManager;

    // ДОДАЄМО ПОСИЛАННЯ НА АНІМАТОРИ
    [Header("Animation Settings")]
    public Animator swordAnimator; // Перетягніть сюди об'єкт Sword_Animation_0
    public Animator bowAnimator;   // Перетягніть сюди об'єкт Bow (якщо є)
    public Animator playerAnimator; // Сам гравець (якщо він має напружуватись при атаці)

    [Header("Melee Settings (Sword)")]
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public float knockbackForce = 10f;
    public float swordCooldown = 0.3f;
    private float lastMeleeTime;

    [Header("Ranged Settings (Bow)")]
    public GameObject arrowPrefab;
    public float arrowForce = 15f;
    public float bowCooldown = 0.7f;
    private float lastShootTime;

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

        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Sword)
        {
            if (Time.time >= lastMeleeTime + swordCooldown)
            {
                // ЗАПУСКАЄМО АНІМАЦІЮ МЕЧА
                if (swordAnimator != null) swordAnimator.SetTrigger("AttackTrigger");

                MeleeAttack();
                lastMeleeTime = Time.time;
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Bow)
        {
            if (Time.time >= lastShootTime + bowCooldown)
            {
                // ЗАПУСКАЄМО АНІМАЦІЮ ЛУКА
                if (bowAnimator != null) bowAnimator.SetTrigger("ShootTrigger");

                Shoot();
                lastShootTime = Time.time;
            }
        }
    }

    void MeleeAttack()
    {
        // Можна додати невелику затримку через Invoke, 
        // щоб урон проходив саме в момент помаху, а не миттєво
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            BossHealth boss = enemy.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                continue;
            }

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
        GameObject arrow = Instantiate(arrowPrefab, attackPoint.position, Quaternion.identity);
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        arrow.transform.rotation = direction < 0 ? Quaternion.Euler(0, 0, 180) : Quaternion.Euler(0, 0, 0);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}