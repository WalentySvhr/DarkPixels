using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("General Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayers;
    private WeaponManager weaponManager;

    [Header("Animation Settings")]
    public Animator swordAnimator;
    public Animator bowAnimator;
    public Animator playerAnimator;

    [Header("Melee Settings (Sword)")]
    public float attackRange = 2f;
    public int attackDamage = 100;
    public float knockbackForce = 10f;
    public float swordCooldown = 0.3f;
    private float lastMeleeTime;

    [Header("Ranged Settings (Bow)")]
    public GameObject arrowPrefab;
    public float arrowForce = 20f;
    public float bowCooldown = 1f;
    private float lastShootTime;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    // ГОЛОВНИЙ МЕТОД АТАКИ
    public void Attack()
    {
        if (weaponManager == null)
        {
            MeleeAttack();
            return;
        }

        // ЛОГІКА ДЛЯ МЕЧА
        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Sword)
        {
            if (Time.time >= lastMeleeTime + swordCooldown)
            {
                if (swordAnimator != null)
                {
                    // Підлаштовуємо швидкість анімації меча під swordCooldown
                    float animationLength = GetAnimationLength(swordAnimator, "Sword_Animanion_0");
                    float speedValue = animationLength / swordCooldown;

                    swordAnimator.SetFloat("AnimSpeed", speedValue);
                    swordAnimator.SetTrigger("SwordAttack");
                }

                MeleeAttack();
                lastMeleeTime = Time.time;
            }
        }
        // ЛОГІКА ДЛЯ ЛУКА
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Bow)
        {
            if (Time.time >= lastShootTime + bowCooldown)
            {
                if (bowAnimator != null)
                {
                    // Налаштування швидкості анімації під кулдаун
                    float animationLength = GetAnimationLength(bowAnimator, "Bow_animation");
                    float speedValue = animationLength / bowCooldown;

                    bowAnimator.SetFloat("AnimSpeed", speedValue);
                    bowAnimator.SetTrigger("BowAttack");
                }

                // Затримка вильоту стріли (наприклад, 0.2 секунди)
                // Підберіть цей час так, щоб стріла з'являлася саме на кадрі пострілу
                Invoke("Shoot", 0.3f * bowCooldown);
                lastShootTime = Time.time;
            }
        }
    }

    // Допоміжна функція для отримання точної довжини анімації
    float GetAnimationLength(Animator animator, string clipName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0.5f; // Значення за замовчуванням, якщо не знайдено
    }

    void MeleeAttack()
    {
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

        if (direction < 0)
            arrow.transform.rotation = Quaternion.Euler(0, 0, 180);
        else
            arrow.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Додаємо фізику стрілі
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * arrowForce, 0);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}