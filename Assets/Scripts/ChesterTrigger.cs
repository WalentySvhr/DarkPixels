using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite openedSprite; // Спрайт відкритої скрині

    [Header("Destroy Settings")]
    [Tooltip("Через скільки секунд скриня зникне після відкриття?")]
    public float destroyDelay = 60f;

    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            Open();
        }
    }

    void Open()
    {
        isOpened = true;

        // 1. Змінюємо спрайт
        if (openedSprite != null)
            GetComponent<SpriteRenderer>().sprite = openedSprite;

        // 2. Викликаємо випадіння луту
        LootDropper dropper = GetComponent<LootDropper>();
        if (dropper != null) dropper.DropLoot();

        // 3. Вимикаємо колайдер
        GetComponent<Collider2D>().enabled = false;

        // --- НОВИЙ ПУНКТ ---
        // 4. Запускаємо таймер знищення об'єкта
        // Destroy(gameObject, t) — видалить об'єкт через t секунд
        Destroy(gameObject, destroyDelay);

        Debug.Log($"Скриня відкрита! Вона зникне через {destroyDelay} секунд.");
    }
}