using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyExchangeUI : MonoBehaviour
{
    public static CurrencyExchangeUI Instance;

    [Header("Налаштування")]
    public int exchangeRate = 1000;

    [Header("UI Посилання")]
    public GameObject windowPanel;
    public TextMeshProUGUI statusText;

    [Header("Тексти сповіщень (Інспектор)")]
    [Tooltip("Текст при успішному обміні")]
    public string successMessage = "Обмін успішний!";
    [Tooltip("Текст, коли не вистачає грошей")]
    public string notEnoughGoldMessage = "Недостатньо золота!";

    private Image backgroundBlocker;

    private void Awake()
    {
        Instance = this;
        backgroundBlocker = GetComponent<Image>();
        Close();
    }

    public void Open()
    {
        if (statusText != null) statusText.text = "";

        if (windowPanel != null) windowPanel.SetActive(true);
        if (backgroundBlocker != null) backgroundBlocker.raycastTarget = true;
    }

    public void Close()
    {
        if (windowPanel != null) windowPanel.SetActive(false);
        if (backgroundBlocker != null) backgroundBlocker.raycastTarget = false;
    }

    public void Trade()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.coins >= exchangeRate)
        {
            InventoryManager.Instance.ChangeCoins(-exchangeRate);
            InventoryManager.Instance.ChangeDiamonds(1);

            // Використовуємо змінну з Інспектора
            ShowStatus(successMessage, Color.green);
        }
        else
        {
            // Використовуємо змінну з Інспектора
            ShowStatus(notEnoughGoldMessage, Color.red);
        }
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