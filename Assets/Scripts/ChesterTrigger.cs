using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite openedSprite; // Спрайт відкритої скрині

    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Перевіряємо тег гравця і чи скриня ще закрита
        if (other.CompareTag("Player") && !isOpened)
        {
            Open();
        }
    }

    void Open()
    {
        isOpened = true;

        // 1. Змінюємо спрайт на відкритий
        if (openedSprite != null)
            GetComponent<SpriteRenderer>().sprite = openedSprite;

        // 2. Викликаємо випадіння луту (твій існуючий скрипт)
        LootDropper dropper = GetComponent<LootDropper>();
        if (dropper != null) dropper.DropLoot();

        // 3. Вимикаємо колайдер, щоб не спрацьовувало вдруге
        GetComponent<Collider2D>().enabled = false;

        Debug.Log("Скриня відкрита торканням!");
    }
}