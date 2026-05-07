using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage = 20;
    public float lifeTime = 3f;

    // === ДОДАНО: Змінна, яка запам'ятовує, чи стріла вилетіла критичною ===
    public bool isCrit = false;

    void Start()
    {
        // Стрілу вже штовхнув скрипт PlayerCombat
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. ПЕРЕВІРКА НА БОСА
        BossHealth boss = collision.GetComponent<BossHealth>();
        if (boss != null)
        {
            // Якщо ти в майбутньому оновиш скрипт боса для підтримки попапів крита,
            // зможеш замінити цей рядок на: boss.TakeDamage(damage, isCrit);
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. ПЕРЕВІРКА НА ЗВИЧАЙНОГО ВОРОГА
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            // ПРАВИЛЬНЕ ВІДКИДАННЯ: 
            // Беремо вектор швидкості стріли (куди вона летить), щоб відкинути ворога саме в ту сторону
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector2 knockbackDir = (rb != null) ? rb.linearVelocity.normalized : (Vector2)transform.right;

            // === ОНОВЛЕНО: Передаємо isCrit у EnemyHealth ===
            enemy.TakeDamage(damage, knockbackDir, 2f, isCrit);

            Destroy(gameObject);
            return;
        }

        // 3. ПЕРЕВІРКА НА ПЕРЕШКОДУ
        // Додай сюди тег стін, якщо вони у тебе просто мають колайдери без скриптів
        if (collision.CompareTag("Obstacle") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}