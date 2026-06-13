using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Назва тригера в Аніматорі для відкриття скрині")]
    [SerializeField] private string openTriggerName = "Open";

    [Header("Visuals (Запасний варіант)")]
    [Tooltip("Спрайт відкритої скрині (спрацює тільки якщо немає Аніматора)")]
    public Sprite openedSprite;

    [Header("Destroy Settings")]
    [Tooltip("Через скільки секунд скриня зникне після відкриття?")]
    public float destroyDelay = 60f;

    private bool isOpened = false;
    private Animator animator;

    void Awake()
    {
        // Дистанційно кешуємо аніматор при старті
        animator = GetComponent<Animator>();
    }

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

        // 1. Керування візуалом через Аніматор
        if (animator != null)
        {
            animator.SetTrigger(openTriggerName);
            Debug.Log($"<color=lime>ChestTrigger:</color> Запущено анімацію '{openTriggerName}'");
        }
        else if (openedSprite != null)
        {
            // Якщо аніматора немає, працює стара логіка зі спрайтом
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }

        // 2. Викликаємо випадіння луту (наша універсальна система)
        LootDropper dropper = GetComponent<LootDropper>();
        if (dropper != null)
        {
            dropper.DropLoot();
        }
        else
        {
            Debug.LogWarning($"На скрині {gameObject.name} не знайдено компонент LootDropper!");
        }

        // 3. Вимикаємо колайдер, щоб гравець більше не міг взаємодіяти
        Collider2D chestCollider = GetComponent<Collider2D>();
        if (chestCollider != null) chestCollider.enabled = false;

        // 4. Запускаємо таймер знищення об'єкта для оптимізації
        Destroy(gameObject, destroyDelay);

        Debug.Log($"Скриня відкрита! Вона зникне зі сцени через {destroyDelay} секунд.");
    }
}