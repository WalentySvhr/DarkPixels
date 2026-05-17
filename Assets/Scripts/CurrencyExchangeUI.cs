using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyExchangeUI : MonoBehaviour
{
    public static CurrencyExchangeUI Instance;

    [Header("Налаштування Обміну (Золото -> Діаманти)")]
    public int exchangeRate = 1000;

    [Header("Покупка Петомця (Діаманти ->)")]
    public PetData petItem; // Сюди перетягни файл твого пета (напр. Pet_Dragon)

    [Header("UI Посилання")]
    public GameObject windowPanel;
    public TextMeshProUGUI statusText;

    [Header("Тексти сповіщень (Інспектор)")]
    [Tooltip("Текст при успішному обміні")]
    public string successMessage = "Обмін успішний!";
    [Tooltip("Текст, коли не вистачає грошей")]
    public string notEnoughGoldMessage = "Недостатньо золота!";

    [Space]
    [Tooltip("Текст при успішній покупці пета")]
    public string petSuccessMessage = "Помічника успішно придбано!";
    [Tooltip("Текст, коли не вистачає діамантів на пета")]
    public string notEnoughDiamondsMessage = "Недостатньо діамантів!";
    [Tooltip("Текст, якщо такий пет вже є в рюкзаку")]
    public string alreadyOwnedMessage = "У вас вже є цей помічник!";

    private void Awake()
    {
        Instance = this;
        Close();
    }

    public void Open()
    {
        if (statusText != null) statusText.text = "";

        // Просто вмикаємо всю панель вікна
        if (windowPanel != null) windowPanel.SetActive(true);
    }

    public void Close()
    {
        // Просто вимикаємо всю панель вікна
        if (windowPanel != null) windowPanel.SetActive(false);
    }

    // ==========================================
    // 1. ОБМІН ЗОЛОТА НА ДІАМАНТИ
    // ==========================================
    public void Trade()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.coins >= exchangeRate)
        {
            InventoryManager.Instance.ChangeCoins(-exchangeRate);
            InventoryManager.Instance.ChangeDiamonds(1);

            ShowStatus(successMessage, Color.green);
        }
        else
        {
            ShowStatus(notEnoughGoldMessage, Color.red);
        }
    }

    // ==========================================
    // 2. КУПІВЛЯ ПЕТОМЦЯ ЗА ДІАМАНТИ
    // ==========================================
    public void BuyPet()
    {
        if (InventoryManager.Instance == null || petItem == null) return;

        int price = petItem.diamondPrice; // Беремо ціну з файлу PetData

        // Перевіряємо баланс діамантів
        if (InventoryManager.Instance.diamonds >= price)
        {
            // Перевіряємо, чи вже є такий пет в інвентарі петів
            if (HasItemInInventory(petItem))
            {
                ShowStatus(alreadyOwnedMessage, Color.yellow);
                return;
            }

            // Забираємо діаманти та викликаємо правильний метод Add()
            InventoryManager.Instance.ChangeDiamonds(-price);
            InventoryManager.Instance.Add(petItem);

            ShowStatus(petSuccessMessage, Color.green);
        }
        else
        {
            ShowStatus(notEnoughDiamondsMessage, Color.red);
        }
    }

    // Допоміжний метод: перевіряє колекцію петів, щоб не купити двічі
    private bool HasItemInInventory(Item itemToCheck)
    {
        // --- ОНОВЛЕНО: Тепер скануємо саме petItems замість звичайних items ---
        foreach (var stack in InventoryManager.Instance.petItems)
        {
            if (stack.item == itemToCheck) return true;
        }
        return false;
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }
}