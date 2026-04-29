using UnityEngine;

public class Arrow_Enemy : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifetime = 5f;

    void Start()
    {
        // Знищити стрілу через певний час, якщо вона нікуди не влучила
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Рух стріли вперед
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    // Цей метод спрацьовує, коли активовано Is Trigger на колайдері
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо, чи влучили ми в гравця за тегом
        if (collision.CompareTag("Player"))
        {
            // Шукаємо скрипт здоров'я на гравці
            // (Заміни "PlayerHealth" на назву свого скрипта здоров'я)
            PlayerHealth player = collision.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage); // Викликаємо метод отримання урону
            }

            // Знищуємо стрілу після влучання
            Destroy(gameObject);
        }

        // Можна додати руйнування об стіни
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}