using UnityEngine;

public class RegenerationZone : MonoBehaviour
{
    [Header("Налаштування лікування")]
    public int healAmount = 5;
    public float interval = 1f;

    private float timer;
    private PlayerHealth playerHealth;

    void Update()
    {
        if (playerHealth != null)
        {
            // Перевіряємо, чи потрібно взагалі лікувати (якщо ХП менше макс)
            if (playerHealth.currentHealth < playerHealth.maxHealth)
            {
                timer += Time.deltaTime;
                if (timer >= interval)
                {
                    // Викликаємо тільки лікування. 
                    // Текст спавниться сам всередині методу ApplyHeal/Heal
                    playerHealth.ApplyHeal(healAmount);

                    timer = 0;
                }
            }
            else
            {
                timer = 0; // Скидаємо таймер, якщо гравець повністю здоровий
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = null;
            timer = 0; // Скидаємо таймер при виході
        }
    }
}