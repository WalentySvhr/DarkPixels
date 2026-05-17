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

    private bool HasItemInInventory(Item itemToCheck)
    {
        foreach (var stack in InventoryManager.Instance.petItems)
        {
            if (stack.item == itemToCheck) return true;
        }
        return false;
    }

    // --- ОНОВЛЕНО: Тепер метод просто передає текст з твоїми тегами ---
    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}