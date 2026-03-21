using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1; // Скільки монет дає цей предмет

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо, чи це гравець торкнувся монетки
        if (collision.CompareTag("Player"))
        {
            // Додаємо монетки гравцеві (створимо цей метод наступним кроком)
            PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCoins(coinValue);
            }

            // Знищуємо монетку після підбору
            Destroy(gameObject);
        }
    }
}