using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1; // Скільки монет дає цей предмет

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо, чи це гравець торкнувся монетки
        if (collision.CompareTag("Player"))
        {
            // Звертаємося до нашого нового InventoryManager
            // Використовуємо Instance, щоб не шукати компонент щоразу
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ChangeCoins(coinValue);

                // Знищуємо монетку після підбору
                Destroy(gameObject);
            }
            else
            {
                // Якщо раптом забув додати скрипт на гравця, побачиш помилку в консолі
                Debug.LogError("InventoryManager не знайдено на сцені!");
            }
        }
    }
}