using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Змінну speed ми прибрали, бо тепер швидкість стріли залежить від самого Лука
    public int damage = 20;
    public float lifeTime = 3f;

    void Start()
    {
        // Стрілу вже штовхнув скрипт PlayerCombat, тому тут просто запускаємо таймер знищення
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1.ПЕРЕВІРКА НА БОСА
        BossHealth boss = collision.GetComponent<BossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. ПЕРЕВІРКА НА ЗВИЧАЙНОГО ВОРОГА
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // Напрямок відкидання тепер береться з того, куди розвернута стріла!
                Vector2 knockbackDir = new Vector2(Mathf.Sign(transform.localScale.x), 0);
                enemy.TakeDamage(damage, knockbackDir, 2f);
            }
            Destroy(gameObject);
            return;
        }

        // 3. ПЕРЕВІРКА НА ПЕРЕШКОДУ (Стіни тощо)
        if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}