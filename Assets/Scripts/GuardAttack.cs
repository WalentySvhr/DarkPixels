using UnityEngine;

public class GuardAttack : MonoBehaviour
{
    [Header("Анімація")]
    [Tooltip("Перетягни сюди компонент Animator стража. Якщо порожньо — скрипт знайде його сам.")]
    public Animator animator;
    [Tooltip("Точна назва тригера атаки в твоєму вікні Animator (наприклад: Attack або Strike)")]
    public string attackTriggerName = "Attack";

    [Header("Налаштування атаки стража")]
    [Tooltip("Шкода, яку страж наносить мобу за один удар")]
    public int damageAmount = 30;
    [Tooltip("Радіус захисту (дальність огляду стража)")]
    public float attackRange = 5f;
    [Tooltip("Швидкість атаки (частота ударів у секундах)")]
    public float attackCooldown = 1f;

    [Header("Фізика удару")]
    [Tooltip("Сила, з якою страж відштовхує моба при ударі")]
    public float knockbackForce = 3f;

    private float nextAttackTime = 0f;

    void Start()
    {
        // Автоматично шукаємо Animator на цьому ж об'єкті, якщо забули перетягнути в Інспекторі
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            EnemyHealth targetEnemy = FindNearestEnemy();

            if (targetEnemy != null)
            {
                AttackEnemy(targetEnemy);
            }
        }
    }

    private EnemyHealth FindNearestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        EnemyHealth nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D col in colliders)
        {
            EnemyHealth enemy = col.GetComponent<EnemyHealth>();

            if (enemy == null && col.attachedRigidbody != null)
            {
                enemy = col.attachedRigidbody.GetComponent<EnemyHealth>();
            }

            if (enemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy;
    }

    private void AttackEnemy(EnemyHealth enemy)
    {
        // === ЗАПУСК АНІМАЦІЇ АТАКИ ===
        if (animator != null)
        {
            animator.SetTrigger(attackTriggerName);
        }

        // Вираховуємо напрямок відкидання моба
        Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;

        // Наносимо шкоду мобу
        enemy.TakeDamage(damageAmount, knockbackDirection, knockbackForce);

        Debug.Log($"<color=yellow>[Варта міста]</color> Захисник {gameObject.name} атакував {enemy.gameObject.name}!");

        // Ставимо кулдаун на наступний удар
        nextAttackTime = Time.time + attackCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}