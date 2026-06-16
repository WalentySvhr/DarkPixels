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

    // --- ДОДАНО ДЛЯ СЛОТІВ ---
    [Header("Налаштування Покупки Слоту")]
    [Tooltip("Ціна розширення інвентарю в золоті")]
    public int slotGoldPrice = 5000;
    [Tooltip("Ціна розширення інвентарю в діамантах")]
    public int slotDiamondPrice = 10;
    [Tooltip("На скільки слотів збільшується інвентар за один раз")]
    public int slotsPerUpgrade = 1;
    [Tooltip("Максимальний розмір інвентарю, вище якого не можна купувати")]
    public int maxInventorySpace = 40;

    [Header("UI Посилання")]
    public GameObject windowPanel;
    public TextMeshProUGUI statusText;

    [Header("Тексти сповіщень (Можна використовувати XML/HTML теги)")]
    [Tooltip("Текст при успішному обміні")]
    public string successMessage = "<color=green>Обмін успішний!</color>";
    [Tooltip("Текст, коли не вистачає грошей")]
    public string notEnoughGoldMessage = "<color=red>Недостатньо золота!</color>";

    [Space]
    [Tooltip("Текст при успішній покупці пета")]
    public string petSuccessMessage = "<color=green>Помічника успішно придбано!</color>";
    [Tooltip("Текст, коли не вистачає діамантів на пета")]
    public string notEnoughDiamondsMessage = "<color=red>Недостатньо діамантів!</color>";
    [Tooltip("Текст, якщо такий пет вже є в рюкзаку")]
    public string alreadyOwnedMessage = "<color=yellow>У вас вже є цей помічник!</color>";

    // --- ДОДАНО ДЛЯ СЛОТІВ ---
    [Space]
    [Tooltip("Текст при успішній покупці слоту")]
    public string slotSuccessMessage = "<color=green>Інвентар успішно розширено!</color>";
    [Tooltip("Текст, якщо досягнуто максимального ліміту інвентарю")]
    public string maxSlotsReachedMessage = "<color=yellow>Досягнуто максимального розміру інвентарю!</color>";

    private void Awake()
    {
        Instance = this;
        Close();
    }

    public void Open()
    {
        if (statusText != null) statusText.text = "";
        if (windowPanel != null) windowPanel.SetActive(true);
    }

    public void Close()
    {
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

            ShowStatus(successMessage);
        }
        else
        {
            ShowStatus(notEnoughGoldMessage);
        }
    }

    // ==========================================
    // 2. КУПІВЛЯ ПЕТОМЦЯ ЗА ДІАМАНТИ
    // ==========================================
    public void BuyPet()
    {
        if (InventoryManager.Instance == null || petItem == null) return;

        int price = petItem.diamondPrice;

        if (InventoryManager.Instance.diamonds >= price)
        {
            if (HasItemInInventory(petItem))
            {
                ShowStatus(alreadyOwnedMessage);
                return;
            }

            InventoryManager.Instance.ChangeDiamonds(-price);
            InventoryManager.Instance.Add(petItem);

            ShowStatus(petSuccessMessage);
        }
        else
        {
            ShowStatus(notEnoughDiamondsMessage);
        }
    }

    // ==========================================
    // 3. КУПІВЛЯ СЛОТУ ЗА ЗОЛОТО
    // ==========================================
    public void BuySlotWithGold()
    {
        if (InventoryManager.Instance == null) return;

        // Перевірка на максимальний ліміт інвентарю
        if (InventoryManager.Instance.space >= maxInventorySpace)
        {
            ShowStatus(maxSlotsReachedMessage);
            return;
        }

        if (InventoryManager.Instance.coins >= slotGoldPrice)
        {
            InventoryManager.Instance.ChangeCoins(-slotGoldPrice);

            // Збільшуємо змінну місткості в InventoryManager
            InventoryManager.Instance.space += slotsPerUpgrade;

            // Оновлюємо інтерфейс
            InventoryManager.Instance.UpdateUI();

            if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
            ShowStatus(slotSuccessMessage);
        }
        else
        {
            ShowStatus(notEnoughGoldMessage);
        }
    }

    // ==========================================
    // 4. КУПІВЛЯ СЛОТУ ЗА ДІАМАНТИ
    // ==========================================
    public void BuySlotWithDiamonds()
    {
        if (InventoryManager.Instance == null) return;

        // Перевірка на максимальний ліміт інвентарю
        if (InventoryManager.Instance.space >= maxInventorySpace)
        {
            ShowStatus(maxSlotsReachedMessage);
            return;
        }

        if (InventoryManager.Instance.diamonds >= slotDiamondPrice)
        {
            InventoryManager.Instance.ChangeDiamonds(-slotDiamondPrice);

            // Збільшуємо змінну місткості в InventoryManager
            InventoryManager.Instance.space += slotsPerUpgrade;

            // Оновлюємо інтерфейс
            InventoryManager.Instance.UpdateUI();

            if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
            ShowStatus(slotSuccessMessage);
        }
        else
        {
            ShowStatus(notEnoughDiamondsMessage);
        }
    }

    private bool HasItemInInventory(Item itemToCheck)
    {
        foreach (var stack in InventoryManager.Instance.petItems)
        {
            if (stack.item == itemToCheck) return true;
        }
        return false;
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}