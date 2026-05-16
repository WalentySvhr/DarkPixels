using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfflineProfitWindow : MonoBehaviour
{
    public static OfflineProfitWindow Instance { get; private set; }

    [Header("UI Елементи")]
    public GameObject windowPanel;
    public TextMeshProUGUI coinsText; // Текст із сумою монет
    public TextMeshProUGUI messageText; // НОВЕ: Текст повідомлення
    public TextMeshProUGUI claimButtonText; // НОВЕ: Текст на самій кнопці

    [Header("Налаштування текстів")]
    [TextArea(2, 4)]
    public string rewardMessage = "З поверненням, Герою!\nМістяни зібрали для тебе податки:";
    [TextArea(2, 4)]
    public string emptyMessage = "Скарбниця поки що порожня.\nПодатки ще не зібрані. Зачекай трохи!";

    public string buttonClaimText = "Забрати скарб";
    public string buttonCloseText = "Зрозуміло";

    private int pendingCoins = 0;

    private void Awake()
    {
        Instance = this;
        if (windowPanel != null) windowPanel.SetActive(false);
    }

    public void OpenWindow(int amount)
    {
        pendingCoins = amount;

        // Перевіряємо, чи є нагорода
        if (amount > 0)
        {
            messageText.text = rewardMessage;           // Пишемо "З поверненням..."
            coinsText.text = amount.ToString();         // Вказуємо суму
            coinsText.transform.parent.gameObject.SetActive(true); // Показуємо блок з іконкою та сумою
            claimButtonText.text = buttonClaimText;     // Кнопка каже "Забрати скарб"
        }
        else
        {
            messageText.text = emptyMessage;            // Пишемо "Скарбниця порожня..."
            coinsText.transform.parent.gameObject.SetActive(false); // ХОВАЄМО іконку з нулем, щоб було красиво
            claimButtonText.text = buttonCloseText;     // Кнопка каже "Зрозуміло"
        }

        windowPanel.SetActive(true);
    }

    public void OnClaimButton()
    {
        // Якщо монети були, віддаємо їх гравцю
        if (pendingCoins > 0 && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ChangeCoins(pendingCoins);
            OfflineProfitManager.Instance.ClaimCoins();
        }

        // В обох випадках просто закриваємо вікно
        windowPanel.SetActive(false);
    }
}