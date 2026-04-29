using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 15;
    public float lifeTime = 5f;        // Час життя снаряда, щоб не захаращувати сцену
    public GameObject impactEffect;   // Ефект при влучанні (опціонально)

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Знищити снаряд автоматично через lifeTime секунд
        Destroy(gameObject, lifeTime);
    }

    // Метод для налаштування польоту (викликається з BossCombat)
    public void Launch(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Встановлюємо швидкість один раз — снаряд летить по прямій
        rb.linearVelocity = direction * speed;

        // Повертаємо снаряд «обличчям» до напрямку польоту
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Перевірка влучання в гравця
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            Explode();
        }

        // 2. Перевірка влучання в стіни (Tilemap або шари перешкод)
        // Додай сюди перевірку на шар стін, якщо хочеш, щоб снаряд розбивався об них
        if (collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}