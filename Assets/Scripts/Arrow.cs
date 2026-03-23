using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 20;
    public float lifeTime = 3f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // Важливо: переконайся, що в інспекторі стріли Rigidbody2D не має гравітації (Gravity Scale = 0)
        rb.linearVelocity = transform.right * speed;

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
        // Перевіряємо за тегом або за скриптом
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Vector2 knockbackDir = transform.right;
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