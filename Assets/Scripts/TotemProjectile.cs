using UnityEngine;

public class TotemProjectile : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10; // Шкода від одного снаряда за замовчуванням

    public void Setup(Vector2 direction, float speed, int dmg)
    {
        damage = dmg;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Ігноруємо ворогів та самі тотеми
        if (collision.CompareTag("Enemy") || collision.GetComponent<TotemTrap>() != null)
        {
            return;
        }

        // 2. ВЛУЧАННЯ В ГРАВЦЯ
        if (collision.CompareTag("Player"))
        {
            // Шукаємо твій скрипт здоров'я на об'єкті гравця
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Передаємо шкоду снаряда у твій метод!
                playerHealth.TakeDamage(damage);
            }

            // Знищуємо снаряд, бо він уже виконав свою місію
            Destroy(gameObject);
            return;
        }

        // 3. Влучання в стіну
        if (collision.gameObject.layer == LayerMask.NameToLayer("Walls") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}